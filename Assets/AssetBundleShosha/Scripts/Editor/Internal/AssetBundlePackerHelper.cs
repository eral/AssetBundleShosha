// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using UnityEditor;
	using AssetBundleShosha.Internal;

	public class AssetBundlePackerHelper : System.IDisposable {
		#region Public types
		#endregion
		#region Public const fields
		#endregion
		#region Public fields and properties
		#endregion
		#region Public methods

		/// <summary>
		/// アセットバンドル梱包
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アセットバンドルビルド</returns>
		public AssetBundleBuild PackAssetBundle(string assetBundleNameWithVariant) {
			var variantPair = assetBundleNameWithVariant.Split('.');
			var result = new AssetBundleBuild{
							assetBundleName = variantPair[0],
							assetBundleVariant = ((variantPair.Length != 2)? string.Empty: variantPair[1]),
							assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNameWithVariant),
							addressableNames = null,
						};
			if (0 < m_Packing.Count) {
				var option = GetOptionFromAssetBundleNameWithVariant(assetBundleNameWithVariant);
				var cryptoHash = GetCryptoHash(assetBundleNameWithVariant);
				var assets = result.assetNames.Select(x=>new AssetBundlePackerArg.Asset{assetPath = x, option = GetAssetOption(x)});
				m_InvokeParameters[0].Setup(assetBundleNameWithVariant, option, cryptoHash, assets);
				m_Packing.ForEach(x=>{
					x.Invoke(null, kPackerMethodBindingFlags, null, m_InvokeParameters, null);
				});
				if ((m_InvokeParameters[0].options & AssetBundlePackerArg.AssetBundleFlags.Exclude) != 0) {
					result.assetBundleName = null;
					result.assetBundleVariant = null;
				}
				result.assetNames = m_InvokeParameters[0].assets.Where(x=>(x.option & AssetBundlePackerArg.AssetFlags.Exclude) == 0)
																.Select(x=>x.assetPath)
																.ToArray();
				if (result.assetNames.Length == 0) {
					//梱包アセットが無い場合はダミーウェイトを乗せる
					result.assetNames = kDummyWeightPaths; 
				}
				var customizedCryptoHash = m_InvokeParameters[0].cryptoHash;
				if (customizedCryptoHash != 0) {
					//暗号化アセットバンドル
					m_CustomizedCryptoHashFromAssetBundleNameWithVariant.Add(assetBundleNameWithVariant, customizedCryptoHash);
				}
			}
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセット梱包
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アセットバンドルビルド</returns>
		public AssetBundleUtility.DeliveryStreamingAssetInfo PackDeliveryStreamingAsset(AssetBundleUtility.DeliveryStreamingAssetInfo deliveryStreamingAssetInfo) {
			var result = deliveryStreamingAssetInfo;
			if (0 < m_Packing.Count) {
				var option = GetOptionFromPath(result.path);
				var cryptoHash = 0;
				var assets = new[]{new AssetBundlePackerArg.Asset{assetPath = result.path, option = GetAssetOption(result.path)}};
				m_InvokeParameters[0].Setup(result.deliveryStreamingAssetNameWithVariant, option, cryptoHash, assets);
				m_Packing.ForEach(x=>{
					x.Invoke(null, kPackerMethodBindingFlags, null, m_InvokeParameters, null);
				});
				if ((m_InvokeParameters[0].options & AssetBundlePackerArg.AssetBundleFlags.Exclude) != 0) {
					result.deliveryStreamingAssetName = null;
					result.deliveryStreamingAssetNameWithVariant = null;
					result.variant = null;
				}
				result.path = m_InvokeParameters[0].assets[0].assetPath;
				if ((m_InvokeParameters[0].assets[0].option & AssetBundlePackerArg.AssetFlags.Exclude) != 0) {
					//除外アセット
					m_CustomizedExcludeFromAssetPath.Add(result.path, null);
				}
			}
			return result ;
		}

		/// <summary>
		/// 暗号化確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:暗号化, false:平文</returns>
		public bool IsCustomizedCrypto(string assetBundleNameWithVariant) {
			var result = m_CustomizedCryptoHashFromAssetBundleNameWithVariant.ContainsKey(assetBundleNameWithVariant);
			return result;
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		public int GetCustomizedCryptoHash(string assetBundleNameWithVariant) {
			int result;
			if (!m_CustomizedCryptoHashFromAssetBundleNameWithVariant.TryGetValue(assetBundleNameWithVariant, out result)) {
				result = 0;
			}
			return result;
		}

		/// <summary>
		/// 除外アセット確認
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns>true:除外アセット, false:梱包アセット</returns>
		public bool IsCustomizedExclude(string assetPath) {
			var result = m_CustomizedExcludeFromAssetPath.ContainsKey(assetPath);
			return result;
		}

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public void Dispose() {
			m_CustomizedCryptoHashFromAssetBundleNameWithVariant.Clear();
			m_CustomizedExcludeFromAssetPath.Clear();
			m_AssetBundleCryptoEditor.Dispose();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public AssetBundlePackerHelper() {
			var packing = System.AppDomain.CurrentDomain
											.GetAssemblies()
											.SelectMany(x=>x.GetTypes())
											.SelectMany(x=>x.GetMethods(kPackerMethodBindingFlags))
											.SelectMany(x=>System.Attribute.GetCustomAttributes(x, typeof(AssetBundlePackerAttribute))
																			.Select(y=>new{method = x, order = ((AssetBundlePackerAttribute)y).order}))
											.ToList();
			packing.Sort((x,y)=>x.order - y.order);
			m_Packing = packing.Select(x=>x.method)
								.ToList();
			if (0 < m_Packing.Count) {
				m_InvokeParameters = new[]{new AssetBundlePackerArg()};
			}
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields

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
		/// 除外アセットバンドルラベル辞書
		/// </summary>
		private Dictionary<string, object> excldueAssetBundleLabels {get{if (m_ExcludeAssetBundleLabels == null) {m_ExcludeAssetBundleLabels = GetExcludeAssetBundleLabels();} return m_ExcludeAssetBundleLabels;}}
		private Dictionary<string, object> m_ExcludeAssetBundleLabels;

		/// <summary>
		/// 梱包関数群
		/// </summary>
		private List<MethodInfo> m_Packing;

		/// <summary>
		/// 梱包関数呼び出し用引数
		/// </summary>
		private AssetBundlePackerArg[] m_InvokeParameters = null;

		/// <summary>
		/// 暗号化アセットバンドル(バリアント付きアセットバンドル名)
		/// </summary>
		private Dictionary<string, int> m_CustomizedCryptoHashFromAssetBundleNameWithVariant = new Dictionary<string, int>();

		/// <summary>
		/// 除外アセット(アセットパス)
		/// </summary>
		/// <remarks>valueは未使用</remarks>
		private Dictionary<string, object> m_CustomizedExcludeFromAssetPath = new Dictionary<string, object>();

		/// <summary>
		/// アセットバンドル暗号化クラス
		/// </summary>
		private AssetBundleCryptoEditor m_AssetBundleCryptoEditor = new AssetBundleCryptoEditor();

		#endregion
		#region Private methods

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
			var result = m_AssetBundleCryptoEditor.GetCryptoHash(assetBundleNameWithVariant);
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
