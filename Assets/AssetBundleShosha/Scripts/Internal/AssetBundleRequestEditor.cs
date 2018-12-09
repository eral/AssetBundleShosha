// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

#if UNITY_EDITOR
namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;

	public class AssetBundleRequestEditor : AssetBundleRequestBase {
		#region Public fields and properties

		/// <summary>
		/// 動作が終了したか確認します（読み取り専用）
		/// </summary>
		public override bool isDone {get{
			return m_Progress == 1.0f;
		}}

		/// <summary>
		/// 進捗状況を表示します（読み取り専用）
		/// </summary>
		public override float progress {get{
			return m_Progress;
		}}

		/// <summary>
		/// 非同期で読み込む際の優先順位を設定します。
		/// </summary>
		public override int priority {get{
			return m_Priority;
		} set{
			m_Priority = value;
		}}

		/// <summary>
		/// シーンが準備完了となったタイミングですぐにシーンが有効化されることを許可します。
		/// </summary>
		public override bool allowSceneActivation {get{
			return m_AllowSceneActivation;
		} set{
			m_AllowSceneActivation = value;
		}}

		/// <summary>
		/// 完了時イベント
		/// </summary>
		public override event System.Action<IAsyncOperation> completed {add{
			m_Completed += value;
		} remove{
			m_Completed -= value;
		}}

		/// <summary>
		/// 読み込まれているアセットバンドルを返します（読み取り専用）
		/// </summary>
		public override Object asset {get{
			return m_Assets[0];
		}}

		/// <summary>
		/// 読み込まれたサブアセットを返します（読み取り専用）
		/// </summary>
		public override Object[] allAssets {get{
			return m_Assets;
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public override void Dispose() {
			m_Assets = null;
			base.Dispose();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="manager">管轄のアセットバンドルマネージャー</param>
		/// <param name="assetBundle">管轄のアセットバンドル</param>
		/// <param name="asset">アセット</param>
		public AssetBundleRequestEditor(AssetBundleManager manager, AssetBundleBase assetBundle, Object asset) : base(manager, assetBundle) {
			m_Assets = new[]{asset};
			manager.editor.AsyncEmulation(OnAsyncEmulation);
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="manager">管轄のアセットバンドルマネージャー</param>
		/// <param name="assetBundle">管轄のアセットバンドル</param>
		/// <param name="assets">アセット群</param>
		public AssetBundleRequestEditor(AssetBundleManager manager, AssetBundleBase assetBundle, Object[] assets) : base(manager, assetBundle) {
			m_Assets = assets;
			manager.editor.AsyncEmulation(OnAsyncEmulation);
		}

		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// アセットバンドルリクエスト
		/// </summary>
		private Object[] m_Assets;

		/// <summary>
		/// 進捗
		/// </summary>
		private float m_Progress = 0.0f;

		/// <summary>
		/// 完了時イベント
		/// </summary>
		private event System.Action<IAsyncOperation> m_Completed;

		/// <summary>
		/// priorityダミー
		/// </summary>
		private int m_Priority = 0;

		/// <summary>
		/// allowSceneActivationダミー
		/// </summary>
		private bool m_AllowSceneActivation = true;

		#endregion
		#region Private methods

		/// <summary>
		/// アセットバンドルの完了時イベント
		/// </summary>
		/// <param name="progress">進捗</param>
		private void OnAsyncEmulation(float progress) {
			m_Progress = progress;
			if (progress == 1.0f) {
				if (m_Completed != null) m_Completed(this);
			}
		}

		#endregion
	}
}
#endif
