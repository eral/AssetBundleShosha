// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Utility {
	using UnityEngine;

	public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
		#region Public fields and properties

		/// <summary>
		/// インスタンス
		/// </summary>
		public static T Instance {get{
#if UNITY_EDITOR
			if (s_Instance == null) {
				s_Instance = FindObjectOfType<T>();
			}
#endif
			return s_Instance;
		}}

		#endregion
		#region Protected methods

		/// <summary>
		/// 構築
		/// </summary>
		protected virtual void Awake() {
			if (s_Instance == null) {
				s_Instance = this as T;
			}
		}

		/// <summary>
		/// 破棄
		/// </summary>
		protected virtual void OnDestroy() {
			if (s_Instance == this) {
				s_Instance = FindObjectOfType<T>();
			}
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// インスタンス
		/// </summary>
		private static T s_Instance;

		#endregion
	}
}
