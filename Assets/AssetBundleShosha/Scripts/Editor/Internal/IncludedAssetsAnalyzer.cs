// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;

	public class IncludedAssetsAnalyzer {
		#region Public types
		#endregion
		#region Public const fields
		#endregion
		#region Public fields and properties
		#endregion
		#region Public methods

		/// <summary>
		/// 関節依存アセット群の取得
		/// </summary>
		/// <param name="assetBundleBuilds">アセットバンドルビルド(除外アセット用アセットバンドル除く)</param>
		/// <param name="excludeAssetPaths">除外アセットパス群</param>
		/// <returns>Dictionary(バリアント付きアセットバンドル名, 関節依存アセット群)</returns>
		public Dictionary<string, string[]> GetAllIncludedAssets(AssetBundleBuild[] assetBundleBuilds, string[] excludeAssetPaths) {
			m_AssetBundleBuilds = assetBundleBuilds;
			m_IncludeRootAssetPaths = m_AssetBundleBuilds.SelectMany(x=>x.assetNames)
														.ToDictionary(x=>x, x=>(object)null);
			m_ExcludeRootAssetPaths = excludeAssetPaths.ToDictionary(x=>x, x=>(object)null);

			var result = new Dictionary<string, string[]>(m_AssetBundleBuilds.Length);
			{
				var queue = new Dictionary<string, object>(m_AssetBundleBuilds.Length);
				var include = new Dictionary<string, object>(m_AssetBundleBuilds.Length);
				foreach (var assetBundleBuild in m_AssetBundleBuilds) {
					include.Clear();
					queue.Clear();
					System.Array.ForEach(assetBundleBuild.assetNames, x=>include.Add(x, null));
					System.Array.ForEach(assetBundleBuild.assetNames, x=>queue.Add(x, null));
					while (0 < queue.Count) {
						var current = queue.Keys.First();
						var dependencies = AssetDatabase.GetDependencies(current, false);
						foreach (var dependency in dependencies) {
							if (dependency.EndsWith(".cs")) {
								//スクリプトファイルなら
								//対象外
								continue;
							} else if (include.ContainsKey(dependency)) {
								//既に登録済みなら
								//対象外
								continue;
							} else if (queue.ContainsKey(dependency)) {
								//既に解析対象なら
								//対象外
								continue;
							} else if (m_IncludeRootAssetPaths.ContainsKey(dependency)) {
								//他のアセットバンドルに含まれるなら
								//対象外
								continue;
							} else if (m_ExcludeRootAssetPaths.ContainsKey(dependency)) {
								//除外アセットなら
								//対象外
								continue;
							}
							//依存アセットなら
							include.Add(dependency, null);
							queue.Add(dependency, null);
						}
						queue.Remove(current);
					}
					var assetBundleBuildWithVariant = assetBundleBuild.assetBundleName + (string.IsNullOrEmpty(assetBundleBuild.assetBundleVariant)? string.Empty: "." + assetBundleBuild.assetBundleVariant);
					result.Add(assetBundleBuildWithVariant, include.Keys.ToArray());
				}
			}
			return result;
		}
		public Dictionary<string, string[]> GetAllIncludedAssets(AssetBundleBuild[] assetBundleBuilds) {
			return GetAllIncludedAssets(assetBundleBuilds, null);
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// アセットバンドルビルド (除外アセット用アセットバンドルは含まず)
		/// </summary>
		AssetBundleBuild[] m_AssetBundleBuilds = null;

		/// <summary>
		/// 梱包対象アセットの内、アセットバンドルのルートに存在するアセットのパス群
		/// </summary>
		Dictionary<string, object> m_IncludeRootAssetPaths = null;

		/// <summary>
		/// 除外アセットバンドルのルートに存在するアセットのパス群
		/// </summary>
		Dictionary<string, object> m_ExcludeRootAssetPaths = null;

		#endregion
		#region Private methods
		#endregion
	}
}
