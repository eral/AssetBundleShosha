// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

#if UNITY_EDITOR
namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Networking;
	using AssetBundleShosha.Internal;

	public class DeliveryStreamingAssetEditor : AssetBundleBase {
		#region Public fields and properties

		/// <summary>
		/// 進捗(0.0f～1.0f)
		/// </summary>
		public override float progress {get{
			float result;
			if (m_DownloadWork != null) {
				result = m_DownloadWork.progress;
			} else if (string.IsNullOrEmpty(m_DeliveryStreamingAssetPath)) {
				result = 0.0f;
			} else {
				result = 1.0f;
			}
			return result;
		}}

		/// <summary>
		/// エラーコード
		/// </summary>
		public override AssetBundleErrorCode errorCode {get{return m_ErrorCode;}}

		/// <summary>
		/// アセットバンドルをビルドするときに、必ず使うアセットを設定します（読み取り専用）
		/// </summary>
		public override Object mainAsset {get{
			return null;
		}}

		/// <summary>
		/// アセットバンドルがストリーミングされたシーンのアセットバンドルならば、true を返します。
		/// </summary>
		public override bool isStreamedSceneAssetBundle {get{
			return false;
		}}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		public override bool isDeliveryStreamingAsset {get{
			return true;
		}}

		/// <summary>
		/// 配信ストリーミングアセットのパスを返します。
		/// </summary>
		public override string deliveryStreamingAssetPath {get{
			return m_DeliveryStreamingAssetPath;
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// 特定のオブジェクトがアセットバンドルに含まれているか確認します。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>true:含まれる、false:含まれない</returns>
		public override bool Contains(string name) {
			return ((isDone)? this.name == name: false);
		}

		/// <summary>
		/// アセットバンドルにあるすべてのアセット名を返します。
		/// </summary>
		/// <returns>すべてのアセット名</returns>
		public override string[] GetAllAssetNames() {
			return ((isDone)? new[]{this.name}: null);
		}

		/// <summary>
		/// アセットバンドルにあるすべてのシーンアセットのパス( *.unity アセットへのパス)を返します。
		/// </summary>
		/// <returns>すべてのシーンアセットのパス</returns>
		public override string[] GetAllScenePaths() {
			return null;
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <returns>該当するすべてのアセット</returns>
		public override T[] LoadAllAssets<T>() {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets(System.Type type) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets() {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync<T>() {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync(System.Type type) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync() {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T LoadAsset<T>(string name) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name, System.Type type) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync<T>(string name) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name, System.Type type) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T[] LoadAssetWithSubAssets<T>(string name) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name, System.Type type) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name, System.Type type) {
			throw new System.InvalidOperationException();
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name) {
			throw new System.InvalidOperationException();
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// オンラインプロセス開始イベント
		/// </summary>
		/// <returns>コルーチン</returns>
		protected override System.Collections.IEnumerator OnStartedOnlineProcess() {
			m_ErrorCode = AssetBundleErrorCode.Null;

			m_DownloadWork = new DownloadWork{
				progress = 0.0f,
				deliveryStreamingAssetPath = manager.editor.GetDeliveryStreamingAssetFullPath(nameWithVariant)
			};
			yield return manager.editor.AsyncEmulation(x=>m_DownloadWork.progress = x);
			manager.editor.CachedInDirectAssetsLoad(nameWithVariant);
			
			yield return base.OnStartedOnlineProcess();
		}

		/// <summary>
		/// ダウンロード終了イベント
		/// </summary>
		protected override void OnDownloadFinished() {
			m_DeliveryStreamingAssetPath = m_DownloadWork.deliveryStreamingAssetPath;
			m_DownloadWork = null;

			base.OnDownloadFinished();
		}

		/// <summary>
		/// 破棄イベント
		/// </summary>
		protected override void OnDestroy() {
			m_DeliveryStreamingAssetPath = null;

			base.OnDestroy();
		}

		#endregion
		#region Private types

		/// <summary>
		/// ダウンロード作業領域
		/// </summary>
		private class DownloadWork {
			public float progress;
			public string deliveryStreamingAssetPath;
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 配信ストリーミングアセットパス
		/// </summary>
		private string m_DeliveryStreamingAssetPath;

		/// <summary>
		/// ダウンロード作業領域
		/// </summary>
		private DownloadWork m_DownloadWork;

		/// <summary>
		/// エラーコード
		/// </summary>
		private AssetBundleErrorCode m_ErrorCode = AssetBundleErrorCode.Null;

		#endregion
	}
}
#endif
