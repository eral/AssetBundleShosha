// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;

	public abstract class AssetBundleRequestBase : IAssetBundleRequest {
		#region Public fields and properties

		/// <summary>
		/// 終了確認
		/// </summary>
		public abstract bool isDone {get;}

		/// <summary>
		/// 進捗状況を表示します（読み取り専用）
		/// </summary>
		public abstract float progress {get;}

		/// <summary>
		/// 非同期で読み込む際の優先順位を設定します。
		/// </summary>
		public abstract int priority {get; set;}

		/// <summary>
		/// シーンが準備完了となったタイミングですぐにシーンが有効化されることを許可します。
		/// </summary>
		public abstract bool allowSceneActivation {get; set;}

		/// <summary>
		/// 完了時イベント
		/// </summary>
		public abstract event System.Action<IAsyncOperation> completed;

		/// <summary>
		/// 読み込まれているアセットバンドルを返します（読み取り専用）
		/// </summary>
		public abstract Object asset {get;}

		/// <summary>
		/// 読み込まれたサブアセットを返します（読み取り専用）
		/// </summary>
		public abstract Object[] allAssets {get;}

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public object Current {get {return null;}}

		#endregion
		#region Public methods

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public bool MoveNext() {
			return isDone;
		}

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public void Reset() {
			//empty.
		}

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public virtual void Dispose() {
			m_Manager = null;
			m_AssetBundle = null;
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="manager">管轄のアセットバンドルマネージャー</param>
		/// <param name="assetBundle">管轄のアセットバンドル</param>
		public AssetBundleRequestBase(AssetBundleManager manager, AssetBundleBase assetBundle) {
			m_Manager = manager;
			m_AssetBundle = assetBundle;
		}

		#endregion
		#region Protected fields and properties

		/// <summary>
		/// 管轄のアセットバンドルマネージャー
		/// </summary>
		protected AssetBundleManager manager {get{return m_Manager;}}

		/// <summary>
		/// 管轄のアセットバンドル名
		/// </summary>
		protected AssetBundleBase assetBundle {get{return m_AssetBundle;}}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 管轄のアセットバンドルマネージャー
		/// </summary>
		[SerializeField]
		private AssetBundleManager m_Manager;

		/// <summary>
		/// 管轄のアセットバンドル名
		/// </summary>
		[SerializeField]
		private AssetBundleBase m_AssetBundle;

		#endregion
	}
}
