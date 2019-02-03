// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

#if UNITY_EDITOR
namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.Networking;
	using UnityEditor;

	[DisallowMultipleComponent]
	public class AssetBundleManagerEditor : MonoBehaviour {
		#region Public types
		#endregion
		#region Public const fields
		#endregion
		#region Public fields and properties
		#endregion
		#region Public methods
		#endregion
		#region Protected methods

#if UNITY_EDITOR
		/// <summary>
		/// 初期化
		/// </summary>
		protected virtual void Reset() {
			hideFlags |= HideFlags.DontSave;
		}
#endif

		#endregion
		#region Internal fields and properties

		/// <summary>
		/// カタログ
		/// </summary>
		internal AssetBundleCatalog catalog {get{
			AssetBundleCatalog result;
			switch (AssetBundleUtility.serverEmulation) {
			case AssetBundleUtility.ServerEmulation.LoadAssetsDirect:
				SyncDatabase();
				result = m_Catalog;
				break;
			default:
				result = null;
				break;
			}
			return result;
		}}

		#endregion
		#region Internal methods

		/// <summary>
		/// アセットバンドルインスタンスの探索
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>アセットバンドルインスタンス</returns>
		internal AssetBundleBase CreateAssetBundleInstance(string assetBundleName) {
			AssetBundleBase result;
			switch (AssetBundleUtility.serverEmulation) {
			case AssetBundleUtility.ServerEmulation.LoadAssetsDirect:
				if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleName)) {
					//配信ストリーミングアセット
					result = new DeliveryStreamingAssetEditor();
				} else {
					//アセットバンドル
					result = new AssetBundleEditor();
				}
				break;
			default:
				result = null;
				break;
			}
			return result;
		}

		/// <summary>
		/// キャッシュクリア
		/// </summary>
		/// <returns>成功確認</returns>
		public bool ClearCache() {
			switch (AssetBundleUtility.serverEmulation) {
			case AssetBundleUtility.ServerEmulation.LoadAssetsDirect:
				if (m_DirectAssetsLoadCacheList != null) {
					m_DirectAssetsLoadCacheList.Clear();
				}
				break;
			default:
				//empty.
				break;
			}
			return true;
		}

		/// <summary>
		/// アセットバンドルのキャッシュ確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:キャッシュ所持、false:キャッシュ未所持</returns>
		internal bool? HasCacheForAssetBundle(string assetBundleNameWithVariant) {
			bool? result = null;
			switch (AssetBundleUtility.serverEmulation) {
			case AssetBundleUtility.ServerEmulation.LoadAssetsDirect:
				result = directAssetsLoadCachedList.ContainsKey(assetBundleNameWithVariant);
				break;
			default:
				//empty.
				break;
			}
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットのキャッシュ確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:キャッシュ所持、false:キャッシュ未所持</returns>
		internal bool? HasCacheForDeliveryStreamingAsset(string assetBundleNameWithVariant) {
			bool? result = null;
			switch (AssetBundleUtility.serverEmulation) {
			case AssetBundleUtility.ServerEmulation.LoadAssetsDirect:
				result = directAssetsLoadCachedList.ContainsKey(assetBundleNameWithVariant);
				break;
			default:
				//empty.
				break;
			}
			return result;
		}

		/// <summary>
		/// 直接読み込みに於けるキャッシュ
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		internal void CachedInDirectAssetsLoad(string assetBundleNameWithVariant) {
			switch (AssetBundleUtility.serverEmulation) {
			case AssetBundleUtility.ServerEmulation.LoadAssetsDirect:
				directAssetsLoadCachedList[assetBundleNameWithVariant] = null;
				break;
			default:
				//empty.
				break;
			}
		}

		/// <summary>
		/// 配信ストリーミングアセットフルパスの取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>配信ストリーミングアセットフルパス</returns>
		internal string GetDeliveryStreamingAssetFullPath(string assetBundleNameWithVariant) {
			string result = null;
			switch (AssetBundleUtility.serverEmulation) {
			case AssetBundleUtility.ServerEmulation.LoadAssetsDirect:
				SyncDatabase();
				string path;
				if (m_DeliveryStreamingAssetPathsCatalog.TryGetValue(assetBundleNameWithVariant, out path)) {
					result = Application.dataPath + "/../" + path;
				}
				break;
			default:
				//empty.
				break;
			}
			return result;
		}

		/// <summary>
		/// 非同期エミュレーション
		/// </summary>
		/// <param name="action">進捗反映アクション</param>
		/// <returns>コルーチン</returns>
		internal Coroutine AsyncEmulation(UnityAction<float> action) {
			return StartCoroutine(AsyncEmulationCoroutine(action, kAsyncEmulationFrames));
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields

		/// <summary>
		/// 非同期エミュレーションフレーム数
		/// </summary>
		private const int kAsyncEmulationFrames = 2;

		#endregion
		#region Private fields and properties

		/// <summary>
		/// カタログ
		/// </summary>
		[SerializeField]
		private AssetBundleCatalog m_Catalog;

		/// <summary>
		/// 配信ストリーミングアセットパスカタログ
		/// </summary>
		/// <remarks>この値がnullの場合はスクリプトホットリロードが発生しているのでデータベース再構築が必要</remarks>
		[System.NonSerialized]
		private Dictionary<string, string> m_DeliveryStreamingAssetPathsCatalog;

		/// <summary>
		/// 直接読み込み用キャッシュ済みリスト
		/// </summary>
		[System.NonSerialized]
		private Dictionary<string, object> m_DirectAssetsLoadCacheList;
		private Dictionary<string, object> directAssetsLoadCachedList {get{
			if (m_DirectAssetsLoadCacheList == null) {
				m_DirectAssetsLoadCacheList = new Dictionary<string, object>();
			}
			return m_DirectAssetsLoadCacheList;
		}}

		#endregion
		#region Private methods

		/// <summary>
		/// データベースの同期
		/// </summary>
		private void SyncDatabase() {
			if (m_DeliveryStreamingAssetPathsCatalog == null) {
				if (m_Catalog == null) {
					m_Catalog = ScriptableObject.CreateInstance<AssetBundleCatalog>();
				}
				m_DeliveryStreamingAssetPathsCatalog = new Dictionary<string, string>();
				SyncCatalog(m_Catalog, m_DeliveryStreamingAssetPathsCatalog);
				CatalogPostprocess(m_Catalog);
			}
		}

		/// <summary>
		/// カタログの同期
		/// </summary>
		/// <param name="catalog"></param>
		private static void SyncCatalog(AssetBundleCatalog catalog, Dictionary<string, string> deliveryStreamingAssetPathsCatalog) {
			var allDeliveryStreamingAssetInfos = AssetBundleUtility.GetAllDeliveryStreamingAssetInfos();
			deliveryStreamingAssetPathsCatalog.Clear();
			foreach (var deliveryStreamingAssetInfo in allDeliveryStreamingAssetInfos ) {
				deliveryStreamingAssetPathsCatalog.Add(deliveryStreamingAssetInfo.deliveryStreamingAssetNameWithVariant, deliveryStreamingAssetInfo.path);
			}

			var allAssetBundles = AssetDatabase.GetAllAssetBundleNames();
			var allDeliveryStreamingAssets = allDeliveryStreamingAssetInfos.Select(x=>x.deliveryStreamingAssetNameWithVariant);
			allAssetBundles = allAssetBundles.Concat(allDeliveryStreamingAssets).ToArray();
			System.Array.Sort(allAssetBundles);

			var allAssetBundlesWithVariant = allAssetBundles.Where(x=>0 <= x.IndexOf('.')).ToArray();
			var allDeliveryStreamingAssetsWithVariant = allDeliveryStreamingAssetInfos.Where(x=>x.deliveryStreamingAssetName != x.deliveryStreamingAssetNameWithVariant)
																				.Select(x=>x.deliveryStreamingAssetNameWithVariant);
			allAssetBundlesWithVariant = allAssetBundlesWithVariant.Concat(allDeliveryStreamingAssetsWithVariant).ToArray();
			System.Array.Sort(allAssetBundlesWithVariant);
				
			catalog.SetAllAssetBundles(allAssetBundles, allAssetBundlesWithVariant);
			var dependencies = new string[0];
			var fileSize = 100u * 1024u; //10KiB
			for (int i = 0, iMax = allAssetBundles.Length; i < iMax; ++i) {
				catalog.SetDependencies(i, dependencies, dependencies);
				catalog.SetAssetBundleFileSize(i, fileSize);
			}
		}

		/// <summary>
		/// カタログポストプロセス
		/// </summary>
		/// <param name="catalog">カタログ</param>
		private static void CatalogPostprocess(AssetBundleCatalog catalog) {
			var assetBundleShoshaEditorAssembly = System.AppDomain.CurrentDomain
																	.GetAssemblies()
																	.Where(x=>x.FullName.StartsWith("AssetBundleShosha.Editor,"))
																	.FirstOrDefault();
			if (assetBundleShoshaEditorAssembly == null) {
				return;
			}
			var assetBundleCatalogPostprocessorAttributeType = assetBundleShoshaEditorAssembly.GetType("AssetBundleShosha.Editor.AssetBundleCatalogPostprocessorAttribute");
			if (assetBundleCatalogPostprocessorAttributeType == null) {
				return;
			}
			var assetBundleCatalogPostprocessorArgType = assetBundleShoshaEditorAssembly.GetType("AssetBundleShosha.Editor.AssetBundleCatalogPostprocessorArg");
			if (assetBundleCatalogPostprocessorArgType == null) {
				return;
			}
			var assetBundleCatalogPostprocessorAttributeOrderPropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
			var assetBundleCatalogPostprocessorAttributeOrderProperty = assetBundleCatalogPostprocessorAttributeType.GetProperty("order", assetBundleCatalogPostprocessorAttributeOrderPropertyBindingFlags);

			const BindingFlags kCatalogPostprocessMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var postProcess = System.AppDomain.CurrentDomain
											.GetAssemblies()
											.SelectMany(x=>x.GetTypes())
											.SelectMany(x=>x.GetMethods(kCatalogPostprocessMethodBindingFlags))
											.SelectMany(x=>System.Attribute.GetCustomAttributes(x, assetBundleCatalogPostprocessorAttributeType)
																			.Select(y=>new{method = x, order = (int)assetBundleCatalogPostprocessorAttributeOrderProperty.GetValue(y, null)}))
											.ToList();
			postProcess.Sort((x,y)=>x.order - y.order);
			const BindingFlags kAssetBundleCatalogPostprocessorArgCreateInstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var assetBundleCatalogPostprocessorArgInstance = System.Activator.CreateInstance(assetBundleCatalogPostprocessorArgType, kAssetBundleCatalogPostprocessorArgCreateInstanceBindingFlags, null, new[]{catalog}, System.Globalization.CultureInfo.CurrentUICulture);
			var invokeParameters = new[]{assetBundleCatalogPostprocessorArgInstance};
			postProcess.ForEach(x=>{
				x.method.Invoke(null, kCatalogPostprocessMethodBindingFlags, null, invokeParameters, null);
			});
		}

		/// <summary>
		/// 非同期エミュレーション
		/// </summary>
		/// <param name="action">進捗反映アクション</param>
		/// <param name="frames">非同期エミュレーションフレーム数</param>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> AsyncEmulationCoroutine(UnityAction<float> action, int frames) {
			var weight = 1.0f / frames;
			for (int i = 0; i < frames; ++i) {
				action(i * weight);
				yield return null;
			}
			action(1.0f);
			yield break;
		}

		#endregion
	}
}
#endif
