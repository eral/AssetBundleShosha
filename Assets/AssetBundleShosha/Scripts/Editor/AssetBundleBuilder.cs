// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor {
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	using System.Reflection;
	using System.Security.Cryptography;
	using UnityEngine;
	using UnityEditor;
	using AssetBundleShosha.Internal;
	using AssetBundleShosha.Editor.Internal;

	public class AssetBundleBuilder : System.IDisposable {
		#region Public types

		/// <summary>
		/// ビルドフラク
		/// </summary>
		[System.Flags]
		public enum BuildFlags {
			Null										= 0,
			OutputDetailJson							= 1 << 0,	//詳細JSONを出力する
			ForceRebuild								= 1 << 1,	//強制再ビルド
			SkipFileDeploymentOfDeliveryStreamingAssets	= 1 << 2,	//配信ストリーミングアセットのファイルデプロイを省略する
			ForceCrypto									= 1 << 3,	//強制暗号化
			NonDeterministicCrypto						= 1 << 4,	//非決定性暗号化
		}

		#endregion
		#region Public const fields

		/// <summary>
		/// 出力パス
		/// </summary>
		public const string kOutputPath = "AssetBundles";

		#endregion
		#region Public fields and properties
		#endregion
		#region Public methods

		/// <summary>
		/// 構築
		/// </summary>
		/// <param name="targetPlatform">ターゲットプラットフォーム</param>
		/// <param name="options">ビルドオプション</param>
		/// <param name="catalog">カタログ</param>
		public static void Build(BuildTarget targetPlatform, BuildFlags options, AssetBundleCatalog catalog = null) {
			using (var assetBundleBuilder = new AssetBundleBuilder(targetPlatform, options, catalog)) {
				assetBundleBuilder.BuildInternal();
			}
		}

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public void Dispose() {
			m_TargetPlatform = BuildTarget.NoTarget;
			m_TargetPlatformString = null;
			m_BuildOptions = BuildFlags.Null;
			m_Catalog = null;
			m_AssetBundleBuilds = null;
			m_DeliveryStreamingAssetBuilds = null;
			m_ExcludeAssetsAssetBundleName = null;
			m_ExcludeAssetBundleLabels = null;
			m_ExcludeAssetLabels = null;
			m_CustomizedExcludeFromAssetPath = null;
			m_PackingUserMethods = null;
			m_PackingUserMethodParameters = null;
			m_CustomizedCryptoHashFromAssetBundleNameWithVariant = null;
			if (m_AssetBundleCryptoEditor != null) {
				m_AssetBundleCryptoEditor.Dispose();
				m_AssetBundleCryptoEditor = null;
			}
		}

		#endregion
		#region Private types

		/// <summary>
		/// 除外アセット群を除いたアセットバンドルマニュフェスト
		/// </summary>
		private class AssetBundleManifestWithoutExcludeAssets {
			public string[] GetAllAssetBundles() {return m_AssetBundleBuilder.GetAllAssetBundlesWithoutExcludeAssets(m_Manifest);}
			public string[] GetAllAssetBundlesWithVariant() {return m_AssetBundleBuilder.GetAllAssetBundlesWithVariantWithoutExcludeAssets(m_Manifest);}
			public string[] GetAllDependencies(string assetBundleName) {return m_AssetBundleBuilder.GetAllDependenciesWithoutExcludeAssets(m_Manifest, assetBundleName);}
			public string[] GetDirectDependencies(string assetBundleName) {return m_AssetBundleBuilder.GetDirectDependenciesWithoutExcludeAssets(m_Manifest, assetBundleName);}
			public Hash128 GetAssetBundleHash(string assetBundleName) {return m_Manifest.GetAssetBundleHash(assetBundleName);}
			public AssetBundleManifestWithoutExcludeAssets(AssetBundleBuilder assetBundleBuilder, AssetBundleManifest manifest) {
				m_AssetBundleBuilder = assetBundleBuilder;
				m_Manifest = manifest;
			}
			private AssetBundleBuilder m_AssetBundleBuilder = null;
			private AssetBundleManifest m_Manifest = null;
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// 出力事前パス
		/// </summary>
		private const string kPreOutputBasePath = "AssetBundleShoshaWork/Cache";

		/// <summary>
		/// 暗号化出力事前パス
		/// </summary>
		private const string kCryptoPreOutputBasePath = "AssetBundleShoshaWork/Crypto";

		/// <summary>
		/// Assetsへのカタログ出力パス
		/// </summary>
		private const string kInAssetsCatalogOutputBasePath = "Assets/AssetBundleShoshaWork/Catalog";

		/// <summary>
		/// カタログ事前出力パス
		/// </summary>
		private const string kPreCatalogOutputBasePath = "AssetBundleShoshaWork/Catalog";

		/// <summary>
		/// AssetBundlePackerAttribute用バインディングフラグ
		/// </summary>
		private const BindingFlags kPackerMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		/// <summary>
		/// 除外アセットバンドルラベル
		/// </summary>
		private const string kExcludeAssetBundleLabel = "ExcludeAssetBundle";

		/// <summary>
		/// 除外アセットラベル
		/// </summary>
		private const string kExcludeAssetLabel = "ExcludeAsset";

		/// <summary>
		/// ダミーウェイトパス
		/// </summary>
		private readonly string[] kDummyWeightPaths = AssetDatabase.FindAssets("t:" + typeof(DummyWeight).FullName)
																	.Select(x=>AssetDatabase.GUIDToAssetPath(x))
																	.ToArray();

		#endregion
		#region Private fields and properties

		/// <summary>
		/// ターゲットプラットフォーム
		/// </summary>
		private BuildTarget m_TargetPlatform = BuildTarget.NoTarget;

		/// <summary>
		/// ターゲットプラットフォーム文字列
		/// </summary>
		private string m_TargetPlatformString = null;

		/// <summary>
		/// ビルドオプション
		/// </summary>
		private BuildFlags m_BuildOptions = BuildFlags.Null;

		/// <summary>
		/// カタログ
		/// </summary>
		private AssetBundleCatalog m_Catalog = null;

		/// <summary>
		/// アセットバンドルビルド
		/// </summary>
		private AssetBundleBuild[] m_AssetBundleBuilds = null;

		/// <summary>
		/// 配信ストリーミングビルド
		/// </summary>
		private List<AssetBundleUtility.DeliveryStreamingAssetInfo> m_DeliveryStreamingAssetBuilds = null;

		/// <summary>
		/// 除外アセット用アセットバンドル名
		/// </summary>
		private string m_ExcludeAssetsAssetBundleName = null;

		/// <summary>
		/// 除外アセットバンドルラベル辞書
		/// </summary>
		private Dictionary<string, object> excldueAssetBundleLabels {get{if (m_ExcludeAssetBundleLabels == null) {m_ExcludeAssetBundleLabels = GetExcludeAssetBundleLabels();} return m_ExcludeAssetBundleLabels;}}
		private Dictionary<string, object> m_ExcludeAssetBundleLabels = null;

		/// <summary>
		/// 除外アセットラベル辞書
		/// </summary>
		private Dictionary<string, object> excldueAssetLabels {get{if (m_ExcludeAssetLabels == null) {m_ExcludeAssetLabels = GetExcludeAssetLabelAssetPaths();} return m_ExcludeAssetLabels;}}
		private Dictionary<string, object> m_ExcludeAssetLabels = null;

		/// <summary>
		/// 除外アセット(アセットパス)
		/// </summary>
		/// <remarks>valueは未使用</remarks>
		private Dictionary<string, object> customizedExcludeFromAssetPath {get{if (m_CustomizedExcludeFromAssetPath == null) {m_CustomizedExcludeFromAssetPath = new Dictionary<string, object>();} return m_CustomizedExcludeFromAssetPath;}}
		private Dictionary<string, object> m_CustomizedExcludeFromAssetPath = null;

		/// <summary>
		/// 梱包ユーザー関数群
		/// </summary>
		private List<MethodInfo> packingUserMethods {get{if (m_PackingUserMethods == null) {m_PackingUserMethods = GetPackingUserMethods();} return m_PackingUserMethods;}}
		private List<MethodInfo> m_PackingUserMethods;

		/// <summary>
		/// 梱包ユーザー関数呼び出し用引数
		/// </summary>
		private AssetBundlePackerArg[] packingUserMethodParameters {get{if (m_PackingUserMethodParameters == null) {m_PackingUserMethodParameters = new[]{new AssetBundlePackerArg()};} return m_PackingUserMethodParameters;}}
		private AssetBundlePackerArg[] m_PackingUserMethodParameters = null;

		/// <summary>
		/// 暗号化アセットバンドル(バリアント付きアセットバンドル名)
		/// </summary>
		private Dictionary<string, int> customizedCryptoHashFromAssetBundleNameWithVariant {get{if (m_CustomizedCryptoHashFromAssetBundleNameWithVariant == null) {m_CustomizedCryptoHashFromAssetBundleNameWithVariant = new Dictionary<string, int>();} return m_CustomizedCryptoHashFromAssetBundleNameWithVariant;}}
		private Dictionary<string, int> m_CustomizedCryptoHashFromAssetBundleNameWithVariant = null;

		/// <summary>
		/// アセットバンドル暗号化クラス
		/// </summary>
		private AssetBundleCryptoEditor assetBundleCryptoEditor {get{if (m_AssetBundleCryptoEditor == null) {m_AssetBundleCryptoEditor = new AssetBundleCryptoEditor();} return m_AssetBundleCryptoEditor;}}
		private AssetBundleCryptoEditor m_AssetBundleCryptoEditor = null;

		#endregion
		#region Private methods
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="targetPlatform">ターゲットプラットフォーム</param>
		/// <param name="buildOptions">ビルドオプション</param>
		/// <param name="catalog">カタログ</param>
		/// <remarks>staticメソッド外からのインスタンス化禁止</remarks>
		private AssetBundleBuilder(BuildTarget targetPlatform, BuildFlags buildOptions, AssetBundleCatalog catalog) {
			m_TargetPlatform = targetPlatform;
			m_TargetPlatformString = AssetBundleEditorUtility.GetPlatformString(targetPlatform);
			m_BuildOptions = buildOptions;
			if (catalog != null) {
				m_Catalog = catalog;
			} else {
				m_Catalog = ScriptableObject.CreateInstance<AssetBundleCatalog>();
			}
		}
		private AssetBundleBuilder(BuildTarget targetPlatform, BuildFlags buildOptions) : this(targetPlatform, buildOptions, null) {
			//empty.
		}

		/// <summary>
		/// 構築
		/// </summary>
		private void BuildInternal() {
			Preprocess(m_Catalog);

			var catalogAlreadyDontUnloadUnusedAsset = (m_Catalog.hideFlags & HideFlags.DontUnloadUnusedAsset) != 0;
			if (!catalogAlreadyDontUnloadUnusedAsset) {
				m_Catalog.hideFlags |= HideFlags.DontUnloadUnusedAsset; //BuildAssetBundlesの実行と共にインスタンスが破棄される為、破棄されない様にする
			}

			m_AssetBundleBuilds = CreateAssetBundleBuilds();
			var assetBundlesCatalog = BuildAssetBundles(kOutputPath);
			m_DeliveryStreamingAssetBuilds = CreateDeliveryStreamingAssetBuilds();
			var deliveryStreamingAssetsCatalog = BuildDeliveryStreamingAssets(kOutputPath);
			MergeAssetBundleCatalog(ref m_Catalog, assetBundlesCatalog, deliveryStreamingAssetsCatalog);

			if (!catalogAlreadyDontUnloadUnusedAsset) {
				m_Catalog.hideFlags -= HideFlags.DontUnloadUnusedAsset;
			}
			m_Catalog.OnBuildFinished();
			CatalogPostprocess(m_Catalog);

			CreateAssetBundleCatalogAssetBundle(kOutputPath);

			Postprocess(m_Catalog, m_TargetPlatformString, kOutputPath);
		}

		/// <summary>
		/// プリプロセス
		/// </summary>
		/// <param name="catalog">カタログ</param>
		private static void Preprocess(AssetBundleCatalog catalog) {
			const BindingFlags kPreprocessMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var postProcess = System.AppDomain.CurrentDomain
											.GetAssemblies()
											.SelectMany(x=>x.GetTypes())
											.SelectMany(x=>x.GetMethods(kPreprocessMethodBindingFlags))
											.SelectMany(x=>System.Attribute.GetCustomAttributes(x, typeof(AssetBundlePreprocessorAttribute))
																			.Select(y=>new{method = x, order = ((AssetBundlePreprocessorAttribute)y).order}))
											.ToList();
			postProcess.Sort((x,y)=>x.order - y.order);
			var invokeParameters = new[]{new AssetBundlePreprocessorArg(catalog)};
			postProcess.ForEach(x=>{
				x.method.Invoke(null, kPreprocessMethodBindingFlags, null, invokeParameters, null);
			});
		}

		/// <summary>
		/// アセットバンドルビルド作成
		/// </summary>
		/// <returns>アセットバンドルビルド</returns>
		private AssetBundleBuild[] CreateAssetBundleBuilds() {
			var result = AssetDatabase.GetAllAssetBundleNames()
												.Select(x=>PackAssetBundle(x))
												.Where(x=>!string.IsNullOrEmpty(x.assetBundleName))
												.ToArray();
			AddExcludeAssetsBuild(ref result);
			return result;
		}

		/// <summary>
		/// アセットバンドル梱包
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アセットバンドルビルド</returns>
		private AssetBundleBuild PackAssetBundle(string assetBundleNameWithVariant) {
			var variantPair = assetBundleNameWithVariant.Split('.');
			var result = new AssetBundleBuild{
							assetBundleName = variantPair[0],
							assetBundleVariant = ((variantPair.Length != 2)? string.Empty: variantPair[1]),
							assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNameWithVariant),
							addressableNames = null,
						};
			if (0 < packingUserMethods.Count) {
				var option = GetOptionFromAssetBundleNameWithVariant(assetBundleNameWithVariant);
				if (0 == result.assetNames.Length) {
					option |= AssetBundlePackerArg.AssetBundleFlags.UnusedName;
				}
				var cryptoHash = GetCryptoHash(assetBundleNameWithVariant);
				var assets = result.assetNames.Select(x=>new AssetBundlePackerArg.Asset{assetPath = x, option = GetAssetOption(x)});
				packingUserMethodParameters[0].Setup(assetBundleNameWithVariant, option, cryptoHash, assets);
				packingUserMethods.ForEach(x=>{
					x.Invoke(null, kPackerMethodBindingFlags, null, packingUserMethodParameters, null);
				});
				if ((packingUserMethodParameters[0].options & AssetBundlePackerArg.AssetBundleFlags.Exclude) != 0) {
					result.assetBundleName = null;
					result.assetBundleVariant = null;
				}
				result.assetNames = packingUserMethodParameters[0].assets.Where(x=>(x.option & AssetBundlePackerArg.AssetFlags.Exclude) == 0)
																.Select(x=>x.assetPath)
																.ToArray();
				if ((result.assetNames.Length == 0) && ((packingUserMethodParameters[0].options & AssetBundlePackerArg.AssetBundleFlags.UnusedName) == 0)) {
					//梱包アセットが無く、未使用名で無い場合はダミーウェイトを乗せる
					result.assetNames = kDummyWeightPaths; 
				}
				var customizedCryptoHash = packingUserMethodParameters[0].cryptoHash;
				if (customizedCryptoHash != 0) {
					//暗号化アセットバンドル
					customizedCryptoHashFromAssetBundleNameWithVariant.Add(assetBundleNameWithVariant, customizedCryptoHash);
				}
			}
			return result;
		}

		/// <summary>
		/// アセットバンドルの構築
		/// </summary>
		/// <param name="outputPath">出力パス</param>
		/// <returns>カタログ</returns>
		private AssetBundleWithPathCatalog BuildAssetBundles(string outputPath) {
			var assetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
			if ((m_BuildOptions & BuildFlags.ForceRebuild) != 0) {
				assetBundleOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
			}

			var preOutputPath = kPreOutputBasePath + "/" + m_TargetPlatformString;
			CreateDirectory(preOutputPath);
			var manifestOrigin = BuildPipeline.BuildAssetBundles(preOutputPath, m_AssetBundleBuilds, assetBundleOptions, m_TargetPlatform);
			var manifest = new AssetBundleManifestWithoutExcludeAssets(this, manifestOrigin);
			var filePaths = manifest.GetAllAssetBundles().ToDictionary(x=>x, x=>new List<string>{preOutputPath + "/" + x});

			//暗号化
			{
				var cryptoFilePaths = filePaths.Where(x=>IsCustomizedCrypto(x.Key))
												.ToArray();
				if (0 < cryptoFilePaths.Length) {
					bool isNonDeterministic = (m_BuildOptions & BuildFlags.NonDeterministicCrypto) != 0;
					var cryptoPreOutputBasePath = kCryptoPreOutputBasePath + "/" + m_TargetPlatformString;
					CreateDirectory(cryptoPreOutputBasePath);
					using (var crypto = new AssetBundleCryptoEditor()) {
						foreach (var path in cryptoFilePaths) {
							var cryptoPreOutputPath = cryptoPreOutputBasePath + "/" + path.Key;
							if (!IsSkippable(path.Value.First(), cryptoPreOutputPath)) {
								CreateDirectory(cryptoPreOutputPath, true);
								var cryptoHash = GetCustomizedCryptoHash(path.Key);
								crypto.Encrypt(path.Value.First(), cryptoPreOutputPath, cryptoHash, isNonDeterministic);
							}
							filePaths[path.Key].Insert(0, cryptoPreOutputPath);
						}
					}
				}
			}

			var result = CreateAssetBundleCatalog(manifest, filePaths);

			CreateDirectory(outputPath);
			var hashAlgorithm = new AssetBundleShosha.Internal.HashAlgorithm();
			foreach (var path in filePaths) {
				var fileName = outputPath + "/" + hashAlgorithm.GetAssetBundleFileName(m_TargetPlatformString, path.Key);
				CopyFileSkippable(path.Value.First(), fileName);
			}

			return result;
		}

		/// <summary>
		/// アセットバンドル用カタログ構築
		/// </summary>
		/// <param name="manifest">マニュフェスト</param>
		/// <param name="filePaths">ファイルパス</param>
		/// <returns>カタログ</returns>
		private AssetBundleWithPathCatalog CreateAssetBundleCatalog(AssetBundleManifestWithoutExcludeAssets manifest, Dictionary<string, List<string>> filePaths) {
			var result = ScriptableObject.CreateInstance<AssetBundleWithPathCatalog>();
			var allAssetBundles = manifest.GetAllAssetBundles();
			System.Array.Sort(allAssetBundles);
			var allAssetBundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();
			result.SetAllAssetBundles(allAssetBundles, allAssetBundlesWithVariant);
			
			foreach (var assetBundle in allAssetBundles) {
				var assetBundleIndex = result.GetAssetBundleIndexOnEditor(assetBundle);

				
				result.SetDependencies(assetBundleIndex
									, GetSortedAllDependencies(manifest, assetBundle)
									, manifest.GetDirectDependencies(assetBundle)
									);
				var cryptoHash = GetCustomizedCryptoHash(assetBundle);
				result.SetAssetBundleCryptoHash(assetBundleIndex
											, cryptoHash
											);
				var deliveryPath = filePaths[assetBundle].First();
				var originPath = filePaths[assetBundle].Last();
				if (cryptoHash != 0) {
					//暗号化
					result.SetAssetBundleHash(assetBundleIndex
											, GetHashFromDeliveryStreamingAssetFile(deliveryPath)
											);
				} else {
					//平文
					result.SetAssetBundleHash(assetBundleIndex
											, GetHashFromAssetBundleFile(originPath)
											);
				}
				result.SetAssetBundleCrc(assetBundleIndex
										, GetCRCFromAssetBundleFile(originPath)
										);
				result.SetAssetBundleFileSize(assetBundleIndex
											, (uint)GetFileSizeFromFile(deliveryPath)
											);
				result.SetAssetBundlePath(assetBundleIndex
											, deliveryPath
											);
			}
			result.OnBuildFinished();
			return result;
		}

		/// <summary>
		/// 配信ストリーミングビルド作成
		/// </summary>
		/// <returns>配信ストリーミングビルド</returns>
		private List<AssetBundleUtility.DeliveryStreamingAssetInfo> CreateDeliveryStreamingAssetBuilds() {
			var deliveryStreamingAssetBuilds = AssetBundleUtility.GetAllDeliveryStreamingAssetInfos()
																	.Select(x=>PackDeliveryStreamingAsset(x))
																	.Where(x=>!string.IsNullOrEmpty(x.deliveryStreamingAssetNameWithVariant))
																	.ToList();
			return deliveryStreamingAssetBuilds;
		}

		/// <summary>
		/// 配信ストリーミングアセット梱包
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>配信ストリーミングビルド</returns>
		private AssetBundleUtility.DeliveryStreamingAssetInfo PackDeliveryStreamingAsset(AssetBundleUtility.DeliveryStreamingAssetInfo deliveryStreamingAssetInfo) {
			var result = deliveryStreamingAssetInfo;
			if (0 < packingUserMethods.Count) {
				var option = GetOptionFromPath(result.path);
				var cryptoHash = 0;
				var assets = new[]{new AssetBundlePackerArg.Asset{assetPath = result.path, option = GetAssetOption(result.path)}};
				packingUserMethodParameters[0].Setup(result.deliveryStreamingAssetNameWithVariant, option, cryptoHash, assets);
				packingUserMethods.ForEach(x=>{
					x.Invoke(null, kPackerMethodBindingFlags, null, packingUserMethodParameters, null);
				});
				if ((packingUserMethodParameters[0].options & AssetBundlePackerArg.AssetBundleFlags.Exclude) != 0) {
					result.deliveryStreamingAssetName = null;
					result.deliveryStreamingAssetNameWithVariant = null;
					result.variant = null;
				}
				result.path = packingUserMethodParameters[0].assets[0].assetPath;
				if ((packingUserMethodParameters[0].assets[0].option & AssetBundlePackerArg.AssetFlags.Exclude) != 0) {
					//除外アセット
					customizedExcludeFromAssetPath.Add(result.path, null);
				}
			}
			return result ;
		}

		/// <summary>
		/// 配信ストリーミングアセット名・パスの列挙
		/// </summary>
		/// <param name="outputPath">出力パス</param>
		/// <returns>カタログ</returns>
		private AssetBundleWithPathCatalog BuildDeliveryStreamingAssets(string outputPath) {
			var result = CreateDeliveryStreamingAssetCatalog();

			if (!AssetBundleEditorUtility.buildOptionSkipFileDeploymentOfDeliveryStreamingAssets) {
				CreateDirectory(outputPath);
				var isForceRebuild = (m_BuildOptions & BuildFlags.ForceRebuild) != 0;
				var hashAlgorithm = new AssetBundleShosha.Internal.HashAlgorithm();
				foreach (var deliveryStreamingAssetInfo in m_DeliveryStreamingAssetBuilds) {
					var destPath = outputPath + "/" + hashAlgorithm.GetAssetBundleFileName(null, deliveryStreamingAssetInfo.deliveryStreamingAssetNameWithVariant);
					if (IsCustomizedExclude(deliveryStreamingAssetInfo.path)) {
						//0バイト配信ストリーミングアセット
						var isDestAssetExists = File.Exists(destPath);
						if (isDestAssetExists) {
							if (isForceRebuild || (GetFileSizeFromFile(destPath) != 0)) {
								File.Delete(destPath);
								isDestAssetExists = false;
							}
						}
						if (!isDestAssetExists) {
							CreateEmptyAsset(destPath);
						}
					} else if (isForceRebuild) {
						//配信ストリーミングアセット・強制再ビルド
						FileUtil.ReplaceFile(deliveryStreamingAssetInfo.path, destPath);
					} else {
						//配信ストリーミングアセット・スキップ化
						CopyFileSkippable(deliveryStreamingAssetInfo.path, destPath);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセット用カタログ構築
		/// </summary>
		/// <returns>カタログ</returns>
		private AssetBundleWithPathCatalog CreateDeliveryStreamingAssetCatalog() {
			var result = ScriptableObject.CreateInstance<AssetBundleWithPathCatalog>();
			var allAssetBundles = m_DeliveryStreamingAssetBuilds.Select(x=>x.deliveryStreamingAssetNameWithVariant)
														.ToArray();
			System.Array.Sort(allAssetBundles);

			var allAssetBundlesWithVariant = m_DeliveryStreamingAssetBuilds.Where(x=>x.deliveryStreamingAssetName != x.deliveryStreamingAssetNameWithVariant)
																	.Select(x=>x.deliveryStreamingAssetNameWithVariant)
																	.ToArray();
			result.SetAllAssetBundles(allAssetBundles, allAssetBundlesWithVariant);
			
			var emptyDependencies = new string[0];
			foreach (var deliveryStreamingAssetInfo in m_DeliveryStreamingAssetBuilds) {
				var assetBundleIndex = result.GetAssetBundleIndexOnEditor(deliveryStreamingAssetInfo.deliveryStreamingAssetNameWithVariant);
				var path = deliveryStreamingAssetInfo.path;
				result.SetDependencies(assetBundleIndex
									, emptyDependencies
									, emptyDependencies
									);
				result.SetAssetBundleCryptoHash(assetBundleIndex
										, 0
										);
				result.SetAssetBundleHash(assetBundleIndex
										, GetHashFromDeliveryStreamingAssetFile(path)
										);
				result.SetAssetBundleCrc(assetBundleIndex
										, GetCRCFromDeliveryStreamingAssetFile(path)
										);
				result.SetAssetBundleFileSize(assetBundleIndex
											, (uint)GetFileSizeFromFile(path)
											);
				result.SetAssetBundlePath(assetBundleIndex
											, path
											);
			}
			result.OnBuildFinished();
			return result;
		}

		/// <summary>
		/// カタログの統合
		/// </summary>
		/// <param name="dst">統合先カタログ</param>
		/// <param name="src1">統合元カタログ1</param>
		/// <param name="src2">統合元カタログ2</param>
		private static void MergeAssetBundleCatalog(ref AssetBundleCatalog dst, AssetBundleWithPathCatalog src1, AssetBundleWithPathCatalog src2) {
			var allAssetBundles = src1.GetAllAssetBundles().Concat(src2.GetAllAssetBundles()).ToArray();
			System.Array.Sort(allAssetBundles);
			var allAssetBundlesWithVariant = src1.GetAllAssetBundlesWithVariant().Concat(src2.GetAllAssetBundlesWithVariant()).ToArray();
			System.Array.Sort(allAssetBundlesWithVariant);
			dst.SetAllAssetBundles(allAssetBundles, allAssetBundlesWithVariant);

			foreach (var AssetBundle in src1.GetAllAssetBundles()) {
				var assetBundleIndex = dst.GetAssetBundleIndexOnEditor(AssetBundle);
				dst.SetDependencies(assetBundleIndex, src1.GetAllDependencies(AssetBundle), src1.GetDirectDependencies(AssetBundle));
				dst.SetAssetBundleCryptoHash(assetBundleIndex, src1.GetAssetBundleCryptoHash(AssetBundle));
				dst.SetAssetBundleHash(assetBundleIndex, src1.GetAssetBundleHash(AssetBundle));
				dst.SetAssetBundleCrc(assetBundleIndex, src1.GetAssetBundleCrc(AssetBundle));
				dst.SetAssetBundleFileSize(assetBundleIndex, src1.GetAssetBundleFileSize(AssetBundle));
				dst.SetAssetBundlePath(assetBundleIndex, src1.GetAssetBundlePath(AssetBundle));
			}
			foreach (var AssetBundle in src2.GetAllAssetBundles()) {
				var assetBundleIndex = dst.GetAssetBundleIndexOnEditor(AssetBundle);
				dst.SetDependencies(assetBundleIndex, src2.GetAllDependencies(AssetBundle), src2.GetDirectDependencies(AssetBundle));
				dst.SetAssetBundleCryptoHash(assetBundleIndex, src2.GetAssetBundleCryptoHash(AssetBundle));
				dst.SetAssetBundleHash(assetBundleIndex, src2.GetAssetBundleHash(AssetBundle));
				dst.SetAssetBundleCrc(assetBundleIndex, src2.GetAssetBundleCrc(AssetBundle));
				dst.SetAssetBundleFileSize(assetBundleIndex, src2.GetAssetBundleFileSize(AssetBundle));
				dst.SetAssetBundlePath(assetBundleIndex, src2.GetAssetBundlePath(AssetBundle));
			}
		}

		/// <summary>
		/// カタログポストプロセス
		/// </summary>
		/// <param name="catalog">カタログ</param>
		private static void CatalogPostprocess(AssetBundleCatalog catalog) {
			const BindingFlags kCatalogPostprocessMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var postProcess = System.AppDomain.CurrentDomain
											.GetAssemblies()
											.SelectMany(x=>x.GetTypes())
											.SelectMany(x=>x.GetMethods(kCatalogPostprocessMethodBindingFlags))
											.SelectMany(x=>System.Attribute.GetCustomAttributes(x, typeof(AssetBundleCatalogPostprocessorAttribute))
																			.Select(y=>new{method = x, order = ((AssetBundleCatalogPostprocessorAttribute)y).order}))
											.ToList();
			postProcess.Sort((x,y)=>x.order - y.order);
			var invokeParameters = new[]{new AssetBundleCatalogPostprocessorArg(catalog)};
			postProcess.ForEach(x=>{
				x.method.Invoke(null, kCatalogPostprocessMethodBindingFlags, null, invokeParameters, null);
			});
		}

		/// <summary>
		/// カタログのアセットバンドル構築
		/// </summary>
		/// <param name="outputPath">出力パス</param>
		private void CreateAssetBundleCatalogAssetBundle(string outputPath) {
			var inAssetsCatalogOutputPath = kInAssetsCatalogOutputBasePath + "/" + m_TargetPlatformString + "/catalog.asset";
			CreateDirectory(inAssetsCatalogOutputPath, true);
			AssetDatabase.CreateAsset(m_Catalog, inAssetsCatalogOutputPath);

			const BuildAssetBundleOptions kAssetBundleOptions = BuildAssetBundleOptions.None;
			var assetBundleBuilds = new[]{new AssetBundleBuild{assetBundleName = "catalog", assetNames = new[]{inAssetsCatalogOutputPath}}};
			var preCatalogOutput = kPreCatalogOutputBasePath + "/" + m_TargetPlatformString;
			CreateDirectory(preCatalogOutput);
			BuildPipeline.BuildAssetBundles(preCatalogOutput, assetBundleBuilds, kAssetBundleOptions, m_TargetPlatform);

			CreateDirectory(outputPath);
			{
				var path = preCatalogOutput + "/" + assetBundleBuilds[0].assetBundleName;
				var fileName = outputPath + "/" + new AssetBundleShosha.Internal.HashAlgorithm().GetAssetBundleFileName(m_TargetPlatformString, null);
				CopyFileSkippable(path, fileName);
			}
			var publicJsonFullPath = Application.dataPath + "/../" + outputPath + "/" + m_TargetPlatformString + ".json";
			m_Catalog.SavePublicJson(publicJsonFullPath);
			if ((m_BuildOptions & BuildFlags.OutputDetailJson) != 0) {
				var detailJsonFullPath = Application.dataPath + "/../" + kPreOutputBasePath + "/" + m_TargetPlatformString + ".json";
				m_Catalog.SaveDetailJson(detailJsonFullPath);
			}
		}

		/// <summary>
		/// ポストプロセス
		/// </summary>
		/// <param name="catalog">カタログ</param>
		/// <param name="platformString">ターゲットプラットフォーム文字列</param>
		/// <param name="outputPath">出力パス</param>
		private static void Postprocess(AssetBundleCatalog catalog, string platformString, string outputPath) {
			const BindingFlags kPostprocessMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var postProcess = System.AppDomain.CurrentDomain
											.GetAssemblies()
											.SelectMany(x=>x.GetTypes())
											.SelectMany(x=>x.GetMethods(kPostprocessMethodBindingFlags))
											.SelectMany(x=>System.Attribute.GetCustomAttributes(x, typeof(AssetBundlePostprocessorAttribute))
																			.Select(y=>new{method = x, order = ((AssetBundlePostprocessorAttribute)y).order}))
											.ToList();
			postProcess.Sort((x,y)=>x.order - y.order);
			var invokeParameters = new[]{new AssetBundlePostprocessorArg(catalog, outputPath, platformString)};
			postProcess.ForEach(x=>{
				x.method.Invoke(null, kPostprocessMethodBindingFlags, null, invokeParameters, null);
			});
		}

		/// <summary>
		/// ディレクトリ作成
		/// </summary>
		/// <param name="path">作成するディレクトリのパス</param>
		/// <param name="excludeLastName">最後の名前を除外</param>
		private static void CreateDirectory(string path, bool excludeLastName = false) {
			ConvertToPOSIXPath(ref path);
			if (excludeLastName) {
				var excludeIndex = path.LastIndexOf('/');
				if (0 <= excludeIndex) {
					path = path.Substring(0, excludeIndex);
				}
			}
			var separateIndex = path.LastIndexOf('/');
			if (separateIndex == path.Length - 1) {
				//ディレクトリ区切り終わりのパスなら
				//末尾のディレクトリ区切りを除去して再試行
				path = path.Substring(0, separateIndex);
				CreateDirectory(path);
			} else if (AssetDatabase.IsValidFolder(path)) {
				//ディレクトリが既に在るなら
				//empty.
			} else if (separateIndex < 0) {
				//ディレクトリ区切りが無いなら
				//ディレクトリ作成
				var fullPath = Application.dataPath + "/../" + path;
				Directory.CreateDirectory(fullPath);
			} else {
				var parentDirectory = path.Substring(0, separateIndex);
				if (parentDirectory == "Assets") {
					var targetDirectory = path.Substring(separateIndex + 1);
					AssetDatabase.CreateFolder(parentDirectory, targetDirectory);
				} else {
					var fullPath = Application.dataPath + "/../" + path;
					Directory.CreateDirectory(fullPath);
				}
			}
		}

		/// <summary>
		/// POSIXパスへ変換
		/// </summary>
		/// <param name="path">パス</param>
		/// <returns>POSIXパス</returns>
		[System.Diagnostics.Conditional("UNITY_EDITOR_WIN")]
		private static void ConvertToPOSIXPath(ref string path) {
			path = path.Replace('\\', '/');
		}

		/// <summary>
		/// 間接含む全依存関係を被参照数降順で取得
		/// </summary>
		/// <param name="manifest">アセットバンドルマニュフェスト</param>
		/// <param name="assetBundle">アセットバンドル名</param>
		/// <returns>被参照数降順の間接含む全依存関係</returns>
		private static string[] GetSortedAllDependencies(AssetBundleManifestWithoutExcludeAssets manifest, string assetBundle) {
			var result = manifest.GetAllDependencies(assetBundle);
			var dependenceCount = result.ToDictionary(x=>x, x=>1);
			foreach (var targetAssetBundle in result) {
				foreach (var dependence in manifest.GetAllDependencies(targetAssetBundle)) {
					++dependenceCount[dependence];
				}
			}
			System.Array.Sort(result, (x,y)=>dependenceCount[y] - dependenceCount[x]);
			return result;
		}

		/// <summary>
		/// アセットバンドルファイルからハッシュ取得
		/// </summary>
		/// <param name="path">アセットバンドルファイルパス</param>
		/// <returns>ハッシュ</returns>
		private static Hash128 GetHashFromAssetBundleFile(string path) {
			Hash128 result;
			BuildPipeline.GetHashForAssetBundle(path, out result);
			return result;
		}

		/// <summary>
		/// アセットバンドルファイルからCRC取得
		/// </summary>
		/// <param name="path">アセットバンドルファイルパス</param>
		/// <returns>CRC</returns>
		private static uint GetCRCFromAssetBundleFile(string path) {
			uint result;
			BuildPipeline.GetCRCForAssetBundle(path, out result);
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットファイルからハッシュ取得
		/// </summary>
		/// <param name="path">配信ストリーミングアセットファイルパス</param>
		/// <returns>ハッシュ</returns>
		private static Hash128 GetHashFromDeliveryStreamingAssetFile(string path) {
			var fullPath = Application.dataPath + "/../" + path;
			byte[] hashBytes;
			using (var md5 = new MD5CryptoServiceProvider())
			using (var fileStream = File.OpenRead(fullPath)) {
				var buffer = new byte[128 * 1024];
				var readSize = 0;
				do {
					var read = fileStream.Read(buffer, 0, buffer.Length);
					hashBytes = md5.ComputeHash(buffer, 0, read);
					readSize += read;
				} while (readSize < fileStream.Length);
			}
			var hashUints = new uint[4];
			for (int i = 0, iMax = hashBytes.Length; i < iMax; ++i) {
				hashUints[i / 4] |= (uint)hashBytes[i] << ((i & 0x03) * 8);
			}
			return new Hash128(hashUints[0], hashUints[1], hashUints[2], hashUints[3]);
		}

		/// <summary>
		/// 配信ストリーミングアセットファイルからCRC取得
		/// </summary>
		/// <param name="path">配信ストリーミングアセットファイルパス</param>
		/// <returns>CRC</returns>
		private static uint GetCRCFromDeliveryStreamingAssetFile(string path) {
			var fullPath = Application.dataPath + "/../" + path;
			var result = new AssetBundleShosha.Internal.HashAlgorithm().GetCRCFromFile(fullPath);
			return result;
		}

		/// <summary>
		/// ファイルからファイルサイズ取得
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <returns>ファイルサイズ</returns>
		private static long GetFileSizeFromFile(string path) {
			var fullPath = Application.dataPath + "/../" + path;
			var fileInfo = new FileInfo(fullPath);
			return fileInfo.Length;
		}

		/// <summary>
		/// 空アセット作成
		/// </summary>
		/// <param name="path"></param>
		private static void CreateEmptyAsset(string path) {
			var fullPath = Application.dataPath + "/../" + path;
			File.Create(fullPath).Close();
		}

		/// <summary>
		/// スキップ可能確認
		/// </summary>
		/// <param name="source">元ファイル</param>
		/// <param name="dest">先ファイル</param>
		/// <remarks>先ファイルのファイル時刻が元ファイル依りも新しい場合スキップ可能</remarks>
		private static bool IsSkippable(string source, string dest) {
			var result = false;

			var destFullPath = Application.dataPath + "/../" + dest;
			if (File.Exists(destFullPath)) {
				var sourceFullPath = Application.dataPath + "/../" + source;
				var sourceFileInfo = new FileInfo(sourceFullPath);
				var destFileInfo = new FileInfo(destFullPath);
				result = sourceFileInfo.LastWriteTimeUtc <= destFileInfo.LastWriteTimeUtc;
			}
			return result;
		}

		/// <summary>
		/// スキップするかもしれないファイルコピー
		/// </summary>
		/// <param name="source">コピー元</param>
		/// <param name="dest">コピー先</param>
		/// <remarks>コピー先のファイル時刻・ファイルサイズが同じ場合スキップ</remarks>
		private static void CopyFileSkippable(string source, string dest) {
			var skip = false;

			var destFullPath = Application.dataPath + "/../" + dest;
			if (File.Exists(destFullPath)) {
				var sourceFullPath = Application.dataPath + "/../" + source;
				var sourceFileInfo = new FileInfo(sourceFullPath);
				var destFileInfo = new FileInfo(destFullPath);
				skip = true;
				skip = skip && (sourceFileInfo.LastWriteTimeUtc == destFileInfo.LastWriteTimeUtc);
				skip = skip && (sourceFileInfo.Length == destFileInfo.Length);

				if (!skip) {
					FileUtil.DeleteFileOrDirectory(dest);
				}
			}

			if (!skip) {
				FileUtil.CopyFileOrDirectory(source, dest);
			}
		}

		/// <summary>
		/// 拡張子置換
		/// </summary>
		/// <param name="path">バス</param>
		/// <param name="ext">置換後拡張子('.'含む)</param>
		/// <returns>パス</returns>
		private static string ReplaceExtension(string path, string ext) {
			var result = path;
			var dotIndex = result.IndexOf('.');
			if (0 <= dotIndex) {
				result = result.Substring(0, dotIndex) + ext;
			}
			return result;
		}

		/// <summary>
		/// 除外アセット群のアセットバンドルビルド追加
		/// </summary>
		/// <param name="assetBundleBuilds">アセットバンドルビルド</param>
		/// <returns>true:追加した, false:追加しなかった</returns>
		private bool AddExcludeAssetsBuild(ref AssetBundleBuild[] assetBundleBuilds) {
			var result = false;
			if (0 < excldueAssetLabels.Count()) {
#if false
				var includeAssetPaths = assetBundleBuilds.SelectMany(x=>x.assetNames)
														.ToArray();
				var excludeAssetPaths = excldueAssetLabels.Keys.Intersect(includeAssetPaths)
																.ToArray();
#else
				var excludeAssetPaths = excldueAssetLabels.Keys.ToArray();
#endif
				if (0 < excludeAssetPaths.Length) {
					var exclude = kExcludeAssetLabel.ToLower();
					var index = 0;
					while (assetBundleBuilds.Any(x=>x.assetBundleName == exclude)) {
						exclude = kExcludeAssetLabel.ToLower() + index.ToString();
						++index;
					}

					var excludeAssetsBuild = new AssetBundleBuild{
												assetBundleName = exclude,
												assetBundleVariant = string.Empty,
												assetNames = excludeAssetPaths,
												addressableNames = null
											};

					System.Array.Resize(ref assetBundleBuilds, assetBundleBuilds.Length + 1);
					assetBundleBuilds[assetBundleBuilds.Length - 1] = excludeAssetsBuild;
					m_ExcludeAssetsAssetBundleName = exclude;
					result = true;
				}
			}
			if (!result) {
				m_ExcludeAssetsAssetBundleName = null;
			}
			return result;
		}

		/// <summary>
		/// 除外アセット群を除いた全てのアセットバンドル名取得
		/// </summary>
		/// <param name="manifest">マニフェスト</param>
		/// <returns>全てのアセットバンドル名</returns>
		private string[] GetAllAssetBundlesWithoutExcludeAssets(AssetBundleManifest manifest) {
			var result = manifest.GetAllAssetBundles();
			if (!string.IsNullOrEmpty(m_ExcludeAssetsAssetBundleName)) {
				if (0 <= System.Array.IndexOf(result, m_ExcludeAssetsAssetBundleName)) {
					result = result.Where(x=>x != m_ExcludeAssetsAssetBundleName).ToArray();
				}
			}
			return result;
		}

		/// <summary>
		/// 除外アセット群を除いた全てのバリアント付きアセットバンドル名取得
		/// </summary>
		/// <param name="manifest">マニフェスト</param>
		/// <returns>全てのバリアント付きアセットバンドル名</returns>
		private string[] GetAllAssetBundlesWithVariantWithoutExcludeAssets(AssetBundleManifest manifest) {
			var result = manifest.GetAllAssetBundlesWithVariant();
			if (!string.IsNullOrEmpty(m_ExcludeAssetsAssetBundleName)) {
				if (0 <= System.Array.IndexOf(result, m_ExcludeAssetsAssetBundleName)) {
					result = result.Where(x=>x != m_ExcludeAssetsAssetBundleName).ToArray();
				}
			}
			return result;
		}

		/// <summary>
		/// 除外アセット群を除いた間接含む全依存関係の取得
		/// </summary>
		/// <param name="manifest">マニフェスト</param>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>間接含む全依存関係</returns>
		private string[] GetAllDependenciesWithoutExcludeAssets(AssetBundleManifest manifest, string assetBundleName) {
			var result = manifest.GetAllDependencies(assetBundleName);
			if (!string.IsNullOrEmpty(m_ExcludeAssetsAssetBundleName)) {
				if (0 <= System.Array.IndexOf(result, m_ExcludeAssetsAssetBundleName)) {
					result = result.Where(x=>x != m_ExcludeAssetsAssetBundleName).ToArray();
				}
			}
			return result;
		}

		/// <summary>
		/// 除外アセット群を除いた直接依存関係の取得
		/// </summary>
		/// <param name="manifest">マニフェスト</param>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>直接依存関係</returns>
		private string[] GetDirectDependenciesWithoutExcludeAssets(AssetBundleManifest manifest, string assetBundleName) {
			var result = manifest.GetDirectDependencies(assetBundleName);
			if (!string.IsNullOrEmpty(m_ExcludeAssetsAssetBundleName)) {
				if (0 <= System.Array.IndexOf(result, m_ExcludeAssetsAssetBundleName)) {
					result = result.Where(x=>x != m_ExcludeAssetsAssetBundleName).ToArray();
				}
			}
			return result;
		}

		/// <summary>
		/// 暗号化確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:暗号化, false:平文</returns>
		private bool IsCustomizedCrypto(string assetBundleNameWithVariant) {
			var result = customizedCryptoHashFromAssetBundleNameWithVariant.ContainsKey(assetBundleNameWithVariant);
			return result;
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		private int GetCustomizedCryptoHash(string assetBundleNameWithVariant) {
			int result;
			if (!customizedCryptoHashFromAssetBundleNameWithVariant.TryGetValue(assetBundleNameWithVariant, out result)) {
				result = 0;
			}
			return result;
		}

		/// <summary>
		/// 除外アセット確認
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns>true:除外アセット, false:梱包アセット</returns>
		private bool IsCustomizedExclude(string assetPath) {
			var result = customizedExcludeFromAssetPath.ContainsKey(assetPath);
			return result;
		}

		/// <summary>
		/// 除外アセットバンドルラベル辞書取得
		/// </summary>
		/// <returns>除外アセットバンドルラベル辞書</returns>
		private static Dictionary<string, object> GetExcludeAssetBundleLabels() {
			var result = AssetDatabase.FindAssets("l:" + kExcludeAssetBundleLabel)
									.Select(x=>AssetDatabase.GUIDToAssetPath(x))
									.Select(x=>AssetDatabase.GetImplicitAssetBundleName(x))
									.Where(x=>!string.IsNullOrEmpty(x))
									.ToDictionary(x=>x, y=>(object)null);
			return result;
		}

		/// <summary>
		/// 除外アセットラベル辞書取得
		/// </summary>
		/// <returns>除外アセットラベル辞書</returns>
		private static Dictionary<string, object> GetExcludeAssetLabelAssetPaths() {
			var result = AssetDatabase.FindAssets("l:" + kExcludeAssetLabel)
									.Select(x=>AssetDatabase.GUIDToAssetPath(x))
									.Where(x=>{
										var a = AssetDatabase.LoadMainAssetAtPath(x);
										var l = AssetDatabase.GetLabels(a);
										var r = 0 <= System.Array.IndexOf(l, kExcludeAssetLabel);
										return r;
									})
									.Distinct()
									.SelectMany(x=>{if (AssetDatabase.IsValidFolder(x)) {
											return AssetDatabase.FindAssets("t:Object", new[]{x})
																.Select(y=>AssetDatabase.GUIDToAssetPath(y))
																.Where(y=>!AssetDatabase.IsValidFolder(y));
										} else {
											return new[]{x};
										}
									})
									.Select(x=>{Debug.Log(x);return x;})
									.ToDictionary(x=>x, y=>(object)null);
			return result;
		}

		/// <summary>
		/// 梱包ユーザー関数群取得
		/// </summary>
		/// <returns>梱包ユーザー関数群</returns>
		private static List<MethodInfo> GetPackingUserMethods() {
			var result = System.AppDomain.CurrentDomain
										.GetAssemblies()
										.SelectMany(x=>x.GetTypes())
										.SelectMany(x=>x.GetMethods(kPackerMethodBindingFlags))
										.SelectMany(x=>System.Attribute.GetCustomAttributes(x, typeof(AssetBundlePackerAttribute))
																		.Select(y=>new{method = x, order = ((AssetBundlePackerAttribute)y).order}))
										.OrderBy(x=>x.order)
										.Select(x=>x.method)
										.ToList();
			return result;
		}

		/// <summary>
		/// アセットバンドルフラグ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アセットバンドルフラグ</returns>
		private int GetCryptoHashFromAssetBundleNameWithVariant(string assetBundleNameWithVariant) {
			var result = 0;
			if (excldueAssetBundleLabels.ContainsKey(assetBundleNameWithVariant)) {
			}
			return result;
		}

		/// <summary>
		/// アセットバンドルフラグ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アセットバンドルフラグ</returns>
		private AssetBundlePackerArg.AssetBundleFlags GetOptionFromAssetBundleNameWithVariant(string assetBundleNameWithVariant) {
			var result = AssetBundlePackerArg.AssetBundleFlags.Null;
			if (excldueAssetBundleLabels.ContainsKey(assetBundleNameWithVariant)) {
				result = AssetBundlePackerArg.AssetBundleFlags.Exclude;
			}
			return result;
		}

		/// <summary>
		/// アセットバンドルフラグ取得
		/// </summary>
		/// <param name="assetPath">アセットパス</param>
		/// <returns>アセットバンドルフラグ</returns>
		private AssetBundlePackerArg.AssetBundleFlags GetOptionFromPath(string assetPath) {
			var result = AssetBundlePackerArg.AssetBundleFlags.Null;
			var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
			if (AssetDatabase.GetLabels(asset).Contains(kExcludeAssetBundleLabel)) {
				result = AssetBundlePackerArg.AssetBundleFlags.Exclude;
			}
			return result;
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		private int GetCryptoHash(string assetBundleNameWithVariant) {
			var result = assetBundleCryptoEditor.GetCryptoHash(assetBundleNameWithVariant);
			return result;
		}

		/// <summary>
		/// アセットフラグ取得
		/// </summary>
		/// <param name="assetPath">アセットパス</param>
		/// <returns>アセットフラグ</returns>
		private static AssetBundlePackerArg.AssetFlags GetAssetOption(string assetPath) {
			var result = AssetBundlePackerArg.AssetFlags.Null;
			var path = assetPath;
			do {
				var asset = AssetDatabase.LoadMainAssetAtPath(path);
				if (AssetDatabase.GetLabels(asset).Contains(kExcludeAssetLabel)) {
					result = AssetBundlePackerArg.AssetFlags.Exclude;
					break;
				}
				if (!string.IsNullOrEmpty(AssetImporter.GetAtPath(path).assetBundleName)) {
					break;
				}
				path = Path.GetDirectoryName(path);
			} while ("assets/".Length < path.Length);
			return result;
		}

		#endregion
	}
}
