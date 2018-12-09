// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {
	using System.Collections.Generic;
	using UnityEngine;

	public interface IAsyncOperation : IEnumerator<object> {
		#region Public fields and properties

		/// <summary>
		/// 動作が終了したか確認します（読み取り専用）
		/// </summary>
		bool isDone {get;}

		/// <summary>
		/// 進捗状況を表示します（読み取り専用）
		/// </summary>
		float progress {get;}

		/// <summary>
		/// 非同期で読み込む際の優先順位を設定します。
		/// </summary>
		int priority {get; set;}

		/// <summary>
		/// シーンが準備完了となったタイミングですぐにシーンが有効化されることを許可します。
		/// </summary>
		bool allowSceneActivation {get; set;}

		/// <summary>
		/// 完了時イベント
		/// </summary>
		event System.Action<IAsyncOperation> completed;

		#endregion
	}
}
