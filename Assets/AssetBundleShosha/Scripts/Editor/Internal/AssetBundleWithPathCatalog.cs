// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using AssetBundleShosha.Internal;

	public class AssetBundleWithPathCatalog : AssetBundleCatalog {
		#region Public methods

		/// <summary>
		/// パス取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>パス</returns>
		public string GetAssetBundlePath(string assetBundleName) {
			var index = GetAssetBundleIndex(assetBundleName);
			return m_AssetBundlePaths[index];
		}

#if UNITY_EDITOR
		/// <summary>
		/// 全てのアセットバンドル名設定後イベント
		/// </summary>
		public override void OnSetAllAssetBundles() {
			m_AssetBundlePaths = new string[allAssetBundleNames.Length];
		}

		/// <summary>
		/// パス設定
		/// </summary>
		/// <param name="assetBundleIndex">アセットバンドルインデックス</param>
		/// <param name="path">パス</param>
		public override void SetAssetBundlePath(int assetBundleIndex, string path) {
			m_AssetBundlePaths[assetBundleIndex] = path;
		}
#endif

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// パス
		/// </summary>
		[SerializeField]
		private string[] m_AssetBundlePaths;

		#endregion
		#region Private methods
		#endregion
	}
}
