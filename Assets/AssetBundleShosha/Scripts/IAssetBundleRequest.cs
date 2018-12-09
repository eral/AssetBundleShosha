// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {
	using System.Collections.Generic;
	using UnityEngine;

	public interface IAssetBundleRequest : IAsyncOperation {
		#region Public fields and properties

		/// <summary>
		/// 読み込まれているアセットバンドルを返します（読み取り専用）
		/// </summary>
		Object asset {get;}

		/// <summary>
		/// 読み込まれたサブアセットを返します（読み取り専用）
		/// </summary>
		Object[] allAssets {get;}

		#endregion
	}
}
