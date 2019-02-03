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

	public static class AssetBundleBuilder {
		#region Public types

		/// <summary>
		/// ビルドフラク
		/// </summary>
		[System.Flags]
		public enum BuildFlags {
			Null										= 0,
			OutputDetailJson							= 1 << 0,	//詳細JSONを出力する
			SkipFileDeploymentOfDeliveryStreamingAssets	= 1 << 1,	//配信ストリーミングアセットのファイルデプロイを省略する
			ForceCrypto									= 1 << 2,	//強制暗号化
			NonDeterministicCrypto						= 1 << 3,	//非決定性暗号化
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
			if (catalog == null) {
				catalog = ScriptableObject.CreateInstance<AssetBundleCatalog>();
			}

			Preprocess(catalog);

			var catalogAlreadyDontUnloadUnusedAsset = (catalog.hideFlags & HideFlags.DontUnloadUnusedAsset) != 0;
			if (!catalogAlreadyDontUnloadUnusedAsset) {
				catalog.hideFlags |= HideFlags.DontUnloadUnusedAsset; //BuildAssetBundlesの実行と共にインスタンスが破棄される為、破棄されない様にする
			}

			var packerHelper = new AssetBundlePackerHelper();
			var assetBundlesCatalog = BuildAssetBundles(kOutputPath, targetPlatform, packerHelper, options);
			var deliveryStreamingAssetsCatalog = BuildDeliveryStreamingAssets(kOutputPath, packerHelper);
			MergeAssetBundleCatalog(ref catalog, assetBundlesCatalog, deliveryStreamingAssetsCatalog);

			if (!catalogAlreadyDontUnloadUnusedAsset) {
				catalog.hideFlags -= HideFlags.DontUnloadUnusedAsset;
			}
			catalog.OnBuildFinished();
			CatalogPostprocess(catalog);

			CreateAssetBundleCatalogAssetBundle(catalog, kOutputPath, targetPlatform, options);

			Postprocess(catalog, kOutputPath, targetPlatform);
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields

		/// <summary>
		/// 出力事前パス
		/// </summary>
		private const string kPreOutputBasePath = "AssetBundleShoshaWork/Cache";

		/// <summary>
		/// 暗号化作業パス
		/// </summary>
		private const string kCryptoWorkBasePath = "Assets/AssetBundleShoshaWork/Crypto";

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

		#endregion
		#region Private fields and properties
		#endregion
		#region Private methods

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
		/// アセットバンドルの構築
		/// </summary>
		/// <param name="outputPath">出力パス</param>
		/// <param name="targetPlatform">ターゲットプラットフォーム</param>
		/// <param name="packerHelper">梱包呼び出しヘルパー</param>
		/// <param name="options">ビルドオプション</param>
		/// <returns>カタログ</returns>
		private static AssetBundleWithPathCatalog BuildAssetBundles(string outputPath, BuildTarget targetPlatform, AssetBundlePackerHelper packerHelper, BuildFlags options) {
			var assetBundleBuilds = AssetDatabase.GetAllAssetBundleNames()
												.Select(x=>packerHelper.PackAssetBundle(x))
												.Where(x=>!string.IsNullOrEmpty(x.assetBundleName))
												.ToArray();

			const BuildAssetBundleOptions kAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;

			var platformString = AssetBundleEditorUtility.GetPlatformString(targetPlatform);
			var preOutputPath = kPreOutputBasePath + "/" + platformString;
			CreateDirectory(preOutputPath);
			var manifest = BuildPipeline.BuildAssetBundles(preOutputPath, assetBundleBuilds, kAssetBundleOptions, targetPlatform);
			var filePaths = manifest.GetAllAssetBundles().ToDictionary(x=>x, x=>preOutputPath + "/" + x);

			//暗号化
			{
				var cryptoFilePaths = filePaths.Where(x=>packerHelper.IsCustomizedCrypto(x.Key))
												.ToArray();
				if (0 < cryptoFilePaths.Length) {
					manifest.hideFlags |= HideFlags.DontUnloadUnusedAsset; //BuildAssetBundlesの実行と共にインスタンスが破棄される為、破棄されない様にする

					bool isNonDeterministic = (options & BuildFlags.NonDeterministicCrypto) != 0;
					const BuildAssetBundleOptions kCryptoAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
					var cryptoBuilds = new[]{new AssetBundleBuild{assetNames = new[]{string.Empty}}};
					var cryptoWorkBasePath = kCryptoWorkBasePath + "/" + platformString;
					var cryptoPreOutputBasePath = kCryptoPreOutputBasePath + "/" + platformString;
					CreateDirectory(cryptoPreOutputBasePath);
					using (var crypto = new AssetBundleCryptoEditor()) {
						foreach (var path in cryptoFilePaths) {
							var assetBundleName = ReplaceExtension(path.Key, string.Empty);
							var cryptoWorkPath = cryptoWorkBasePath + "/" + assetBundleName + ".bytes";
							if (!IsSkippable(path.Value, cryptoWorkPath)) {
								CreateDirectory(cryptoWorkPath, true);
								var cryptoHash = packerHelper.GetCustomizedCryptoHash(path.Key);
								crypto.Encrypt(path.Value, cryptoWorkPath, cryptoHash, isNonDeterministic);
								AssetDatabase.ImportAsset(cryptoWorkPath, ImportAssetOptions.Default);
							}
							var abb = cryptoBuilds[0];
							abb.assetBundleName = assetBundleName;
							abb.assetBundleVariant = string.Empty;
							var abbans = abb.assetNames;
							abbans[0] = cryptoWorkPath;
							abb.assetNames = abbans;
							cryptoBuilds[0] = abb;
							BuildPipeline.BuildAssetBundles(cryptoPreOutputBasePath, cryptoBuilds, kCryptoAssetBundleOptions, targetPlatform);
							filePaths[path.Key] = cryptoPreOutputBasePath + "/" + assetBundleName;
						}
					}

					manifest.hideFlags -= HideFlags.DontUnloadUnusedAsset;
				}
			}

			var result = CreateAssetBundleCatalog(manifest, filePaths, packerHelper);

			CreateDirectory(outputPath);
			var hashAlgorithm = new AssetBundleShosha.Internal.HashAlgorithm();
			foreach (var path in filePaths) {
				var fileName = outputPath + "/" + hashAlgorithm.GetAssetBundleFileName(platformString, path.Key);
				CopyFileSkippable(path.Value, fileName);
			}

			return result;
		}

		private static AssetBundlePackerArg SetPackerArg(string assetBundleName, AssetBundlePackerArg packerArg) {
			return packerArg;
		}

		/// <summary>
		/// アセットバンドル用カタログ構築
		/// </summary>
		/// <param name="manifest">マニュフェスト</param>
		/// <param name="filePaths">ファイルパス</param>
		/// <param name="packerHelper">梱包呼び出しヘルパー</param>
		/// <returns>カタログ</returns>
		private static AssetBundleWithPathCatalog CreateAssetBundleCatalog(AssetBundleManifest manifest, Dictionary<string, string> filePaths, AssetBundlePackerHelper packerHelper) {
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
				result.SetAssetBundleCryptoHash(assetBundleIndex
											, packerHelper.GetCustomizedCryptoHash(assetBundle)
											);
				var path = filePaths[assetBundle];
				result.SetAssetBundleHash(assetBundleIndex
										, GetHashFromAssetBundleFile(path)
										);
				result.SetAssetBundleCrc(assetBundleIndex
										, GetCRCFromAssetBundleFile(path)
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
		/// 配信ストリーミングアセット名・パスの列挙
		/// </summary>
		/// <param name="outputPath">出力パス</param>
		/// <param name="packerHelper">梱包呼び出しヘルパー</param>
		/// <returns>カタログ</returns>
		private static AssetBundleWithPathCatalog BuildDeliveryStreamingAssets(string outputPath, AssetBundlePackerHelper packerHelper) {
			var allDeliveryStreamingAssetInfos = AssetBundleUtility.GetAllDeliveryStreamingAssetInfos()
																	.Select(x=>packerHelper.PackDeliveryStreamingAsset(x))
																	.Where(x=>!string.IsNullOrEmpty(x.deliveryStreamingAssetNameWithVariant))
																	.ToList();

			var result = CreateDeliveryStreamingAssetCatalog(allDeliveryStreamingAssetInfos);

			if (!AssetBundleEditorUtility.buildOptionSkipFileDeploymentOfDeliveryStreamingAssets) {
				CreateDirectory(outputPath);
				var hashAlgorithm = new AssetBundleShosha.Internal.HashAlgorithm();
				foreach (var deliveryStreamingAssetInfo in allDeliveryStreamingAssetInfos) {
					var destPath = outputPath + "/" + hashAlgorithm.GetAssetBundleFileName(null, deliveryStreamingAssetInfo.deliveryStreamingAssetNameWithVariant);
					if (packerHelper.IsCustomizedExclude(deliveryStreamingAssetInfo.path)) {
						//0バイト配信ストリーミングアセット
						var isDestAssetExists = File.Exists(destPath);
						if (isDestAssetExists) {
							if (GetFileSizeFromFile(destPath) != 0) {
								File.Delete(destPath);
								isDestAssetExists = false;
							}
						}
						if (!isDestAssetExists) {
							CreateEmptyAsset(destPath);
						}
					} else {
						CopyFileSkippable(deliveryStreamingAssetInfo.path, destPath);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセット用カタログ構築
		/// </summary>
		/// <param name="deliveryStreamingAssetInfos">配信ストリーミングアセット情報</param>
		/// <returns>カタログ</returns>
		private static AssetBundleWithPathCatalog CreateDeliveryStreamingAssetCatalog(List<AssetBundleUtility.DeliveryStreamingAssetInfo> deliveryStreamingAssetInfos) {
			var result = ScriptableObject.CreateInstance<AssetBundleWithPathCatalog>();
			var allAssetBundles = deliveryStreamingAssetInfos.Select(x=>x.deliveryStreamingAssetNameWithVariant)
														.ToArray();
			System.Array.Sort(allAssetBundles);

			var allAssetBundlesWithVariant = deliveryStreamingAssetInfos.Where(x=>x.deliveryStreamingAssetName != x.deliveryStreamingAssetNameWithVariant)
																	.Select(x=>x.deliveryStreamingAssetNameWithVariant)
																	.ToArray();
			result.SetAllAssetBundles(allAssetBundles, allAssetBundlesWithVariant);
			
			var emptyDependencies = new string[0];
			foreach (var deliveryStreamingAssetInfo in deliveryStreamingAssetInfos) {
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
		/// ポストプロセス
		/// </summary>
		/// <param name="catalog">カタログ</param>
		/// <param name="outputPath">出力パス</param>
		/// <param name="targetPlatform">ターゲットプラットフォーム</param>
		private static void Postprocess(AssetBundleCatalog catalog, string outputPath, BuildTarget targetPlatform) {
			const BindingFlags kPostprocessMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var platformString = AssetBundleEditorUtility.GetPlatformString(targetPlatform);
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
		/// カタログのアセットバンドル構築
		/// </summary>
		/// <param name="catalog">カタログ</param>
		/// <param name="outputPath">出力パス</param>
		/// <param name="targetPlatform">ターゲットプラットフォーム</param>
		/// <param name="options">ビルドオプション</param>
		private static void CreateAssetBundleCatalogAssetBundle(AssetBundleCatalog catalog, string outputPath, BuildTarget targetPlatform, BuildFlags options) {
			var platformString = AssetBundleEditorUtility.GetPlatformString(targetPlatform);
			var inAssetsCatalogOutputPath = kInAssetsCatalogOutputBasePath + "/" + platformString + "/catalog.asset";
			CreateDirectory(inAssetsCatalogOutputPath, true);
			AssetDatabase.CreateAsset(catalog, inAssetsCatalogOutputPath);

			const BuildAssetBundleOptions kAssetBundleOptions = BuildAssetBundleOptions.None;
			var assetBundleBuilds = new[]{new AssetBundleBuild{assetBundleName = "catalog", assetNames = new[]{inAssetsCatalogOutputPath}}};
			var preCatalogOutput = kPreCatalogOutputBasePath + "/" + platformString;
			CreateDirectory(preCatalogOutput);
			BuildPipeline.BuildAssetBundles(preCatalogOutput, assetBundleBuilds, kAssetBundleOptions, targetPlatform);

			CreateDirectory(outputPath);
			{
				var path = preCatalogOutput + "/" + assetBundleBuilds[0].assetBundleName;
				var fileName = outputPath + "/" + new AssetBundleShosha.Internal.HashAlgorithm().GetAssetBundleFileName(platformString, null);
				CopyFileSkippable(path, fileName);
			}
			var publicJsonFullPath = Application.dataPath + "/../" + outputPath + "/" + platformString + ".json";
			catalog.SavePublicJson(publicJsonFullPath);
			if ((options & BuildFlags.OutputDetailJson) != 0) {
				var detailJsonFullPath = Application.dataPath + "/../" + kPreOutputBasePath + "/" + platformString + ".json";
				catalog.SaveDetailJson(detailJsonFullPath);
			}
		}

		/// <summary>
		/// ディレクトリ作成
		/// </summary>
		/// <param name="path">作成するディレクトリのパス</param>
		/// <param name="excludeLastName">最後の名前を除外</param>
		private static void CreateDirectory(string path, bool excludeLastName = false) {
			path = ConvertToPOSIXPath(path);
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
				System.IO.Directory.CreateDirectory(fullPath);
			} else {
				var parentDirectory = path.Substring(0, separateIndex);
				if (parentDirectory == "Assets") {
					var targetDirectory = path.Substring(separateIndex + 1);
					AssetDatabase.CreateFolder(parentDirectory, targetDirectory);
				} else {
					var fullPath = Application.dataPath + "/../" + path;
					System.IO.Directory.CreateDirectory(fullPath);
				}
			}
		}

		/// <summary>
		/// POSIXパスへ変換
		/// </summary>
		/// <param name="path">パス</param>
		/// <returns>POSIXパス</returns>
		private static string ConvertToPOSIXPath(string path) {
#if UNITY_EDITOR_WIN
			path = path.Replace('\\', '/');
#endif
			return path;
		}

		/// <summary>
		/// 間接含む全依存関係を被参照数降順で取得
		/// </summary>
		/// <param name="path">アセットバンドルファイルパス</param>
		/// <param name="assetBundle">アセットバンドル名</param>
		/// <returns>被参照数降順の間接含む全依存関係</returns>
		private static string[] GetSortedAllDependencies(AssetBundleManifest manifest, string assetBundle) {
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
			var fileInfo = new System.IO.FileInfo(fullPath);
			return fileInfo.Length;
		}

		/// <summary>
		/// 空アセット作成
		/// </summary>
		/// <param name="path"></param>
		private static void CreateEmptyAsset(string path) {
			var fullPath = Application.dataPath + "/../" + path;
			File.Create(fullPath).Close();;
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

		#endregion
	}
}
