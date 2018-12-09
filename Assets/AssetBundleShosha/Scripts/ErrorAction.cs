// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {
	using UnityEngine.Events;

	public class ErrorAction : IErrorHandler {
		#region Public methods

		/// <summary>
		/// エラー発生
		/// </summary>
		/// <param name="handle">エラーハンドル</param>
		public void Error(IErrorHandle handle) {
			m_OnError(handle);
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="onError">エラー時イベント</param>
		public ErrorAction(UnityAction<IErrorHandle> onError) {
			m_OnError = onError;
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// エラー時イベント
		/// </summary>
		private UnityAction<IErrorHandle> m_OnError;

		#endregion
	}
}
