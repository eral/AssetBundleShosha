// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {
	using System.Collections.Generic;
	using UnityEngine;

	public interface IProgressReceiver {
		#region Public methods

		/// <summary>
		/// 進捗開始
		/// </summary>
		/// <remarks>
		/// 	この関数1回に付きProgressFinished()が1回呼ばれる
		/// 	この関数呼び出し後、同一フレームで最低1回ProgressUpdate()が呼び出される
		/// </remarks>
		void ProgressStart();

		/// <summary>
		/// 進捗更新
		/// </summary>
		/// <remarks>ProgressStart()とProgressFinished()の間でのみ呼ばれる</remarks>
		void ProgressUpdate(float progress);

		/// <summary>
		/// 進捗終了
		/// </summary>
		/// <remarks>この関数が呼ばれる前には必ずProgressStart()が呼ばれている</remarks>
		void ProgressFinished();

		/// <summary>
		/// エラー開始
		/// </summary>
		/// <remarks>
		/// 	ProgressStart()とProgressFinished()の間でのみ呼ばれる
		/// 	この関数1回に付きProgressErrorFinished()が1回呼ばれる
		/// </remarks>
		void ProgressErrorStart();

		/// <summary>
		/// エラー終了
		/// </summary>
		/// <remarks>
		/// 	ProgressStart()とProgressFinished()の間でのみ呼ばれる
		/// 	この関数が呼ばれる前には必ずProgressErrorStart()が呼ばれている
		/// </remarks>
		void ProgressErrorFinished();

		#endregion
	}
}
