// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;

	public class AssetBundleRequestPlayer : AssetBundleRequestBase {
		#region Public fields and properties

		/// <summary>
		/// 動作が終了したか確認します（読み取り専用）
		/// </summary>
		public override bool isDone {get{
			return m_AssetBundleRequest.isDone;
		}}

		/// <summary>
		/// 進捗状況を表示します（読み取り専用）
		/// </summary>
		public override float progress {get{
			return m_AssetBundleRequest.progress;
		}}

		/// <summary>
		/// 非同期で読み込む際の優先順位を設定します。
		/// </summary>
		public override int priority {get{
			return m_AssetBundleRequest.priority;
		} set{
			m_AssetBundleRequest.priority = value;
		}}

		/// <summary>
		/// シーンが準備完了となったタイミングですぐにシーンが有効化されることを許可します。
		/// </summary>
		public override bool allowSceneActivation {get{
			return m_AssetBundleRequest.allowSceneActivation;
		} set{
			m_AssetBundleRequest.allowSceneActivation = value;
		}}

		/// <summary>
		/// 完了時イベント
		/// </summary>
		public override event System.Action<IAsyncOperation> completed {add{
			if (m_Completed == null) {
				m_AssetBundleRequest.completed += OnCompleted;
			}
			m_Completed += value;
		} remove{
			m_Completed -= value;
			if (m_Completed == null) {
				m_AssetBundleRequest.completed -= OnCompleted;
			}
		}}

		/// <summary>
		/// 読み込まれているアセットバンドルを返します（読み取り専用）
		/// </summary>
		public override Object asset {get{
			return m_AssetBundleRequest.asset;
		}}

		/// <summary>
		/// 読み込まれたサブアセットを返します（読み取り専用）
		/// </summary>
		public override Object[] allAssets {get{
			return m_AssetBundleRequest.allAssets;
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public override void Dispose() {
			m_AssetBundleRequest = null;
			base.Dispose();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="manager">管轄のアセットバンドルマネージャー</param>
		/// <param name="assetBundle">管轄のアセットバンドル</param>
		/// <param name="assetBundleRequest">アセットバンドルリクエスト</param>
		public AssetBundleRequestPlayer(AssetBundleManager manager, AssetBundleBase assetBundle, AssetBundleRequest assetBundleRequest) : base(manager, assetBundle) {
			m_AssetBundleRequest = assetBundleRequest;
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// アセットバンドルリクエスト
		/// </summary>
		private AssetBundleRequest m_AssetBundleRequest;

		/// <summary>
		/// 完了時イベント
		/// </summary>
		private event System.Action<IAsyncOperation> m_Completed;

		#endregion
		#region Private methods

		/// <summary>
		/// アセットバンドルの完了時イベント
		/// </summary>
		/// <param name="asyncOperation">非同期オペレーション</param>
		private void OnCompleted(AsyncOperation asyncOperation) {
			if (m_Completed != null) m_Completed(this);
		}

		#endregion
	}
}
