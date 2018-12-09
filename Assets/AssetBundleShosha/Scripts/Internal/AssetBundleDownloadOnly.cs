// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Networking;
	using AssetBundleShosha.Internal;

	public class AssetBundleDownloadOnly : AssetBundleBase {
		#region Public fields and properties

		/// <summary>
		/// 進捗(0.0f～1.0f)
		/// </summary>
		public override float progress {get{
			float result;
			if (m_DownloadWork != null) {
				result = m_DownloadWork.request.downloadProgress;
			} else if (state == State.Done) {
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
			return false;
		}}

		/// <summary>
		/// 配信ストリーミングアセットのパスを返します。
		/// </summary>
		public override string deliveryStreamingAssetPath {get{
			throw null;
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// 特定のオブジェクトがアセットバンドルに含まれているか確認します。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>true:含まれる、false:含まれない</returns>
		public override bool Contains(string name) {
			return false;
		}

		/// <summary>
		/// アセットバンドルにあるすべてのアセット名を返します。
		/// </summary>
		/// <returns>すべてのアセット名</returns>
		public override string[] GetAllAssetNames() {
			return null;
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
			return null;
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets(System.Type type) {
			return null;
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets() {
			return null;
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync<T>() {
			return null;
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync(System.Type type) {
			return null;
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync() {
			return null;
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T LoadAsset<T>(string name) {
			return null;
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name, System.Type type) {
			return null;
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name) {
			return null;
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync<T>(string name) {
			return null;
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name, System.Type type) {
			return null;
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name) {
			return null;
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T[] LoadAssetWithSubAssets<T>(string name) {
			return null;
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name, System.Type type) {
			return null;
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name) {
			return null;
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name) {
			return null;
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name, System.Type type) {
			return null;
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name) {
			return null;
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// オンラインプロセス開始イベント
		/// </summary>
		/// <returns>コルーチン</returns>
		protected override System.Collections.IEnumerator OnStartedOnlineProcess() {
			m_ErrorCode = AssetBundleErrorCode.Null;

			var fileNameAndURL = manager.GetAssetBundleFileNameAndURL(nameWithVariant);
			m_DownloadWork = new DownloadWork{
				url = fileNameAndURL.url,
				hash = manager.catalog.GetAssetBundleHash(nameWithVariant),
				crc = manager.catalog.GetAssetBundleCrc(nameWithVariant)
			};
			m_DownloadWork.request = UnityWebRequest.GetAssetBundle(m_DownloadWork.url, m_DownloadWork.hash, m_DownloadWork.crc);
			var sendWebRequest = m_DownloadWork.request.SendWebRequest();
			var progress = -1.0f;
			var startTime = Time.realtimeSinceStartup;
			while (!sendWebRequest.isDone) {
				yield return null;
				if (progress != sendWebRequest.progress) {
					//進行
					progress = sendWebRequest.progress;
					startTime = Time.realtimeSinceStartup;
				} else if (manager.downloadTimeoutSeconds < (Time.realtimeSinceStartup - startTime)) {
					//タイムアウト時間の停滞
					break;
				}
			}

			if (m_DownloadWork.request.isNetworkError || m_DownloadWork.request.isHttpError) {
				AssetBundleErrorCodeUtility.TryParse(m_DownloadWork.request, out m_ErrorCode);
			} else {
				Caching.ClearOtherCachedVersions(fileNameAndURL.fileName, m_DownloadWork.hash);
			}

			yield return base.OnStartedOnlineProcess();
		}

		/// <summary>
		/// ダウンロード終了イベント
		/// </summary>
		protected override void OnDownloadFinished() {
			if (!m_DownloadWork.request.isNetworkError && !m_DownloadWork.request.isHttpError) {
				var requestDownloadHandlerAssetBundle = (DownloadHandlerAssetBundle)m_DownloadWork.request.downloadHandler;
				var assetBundle = requestDownloadHandlerAssetBundle.assetBundle;
				assetBundle.Unload(true);
			}
			m_DownloadWork.request.Dispose();
			m_DownloadWork = null;

			base.OnDownloadFinished();
		}

		/// <summary>
		/// 破棄イベント
		/// </summary>
		protected override void OnDestroy() {
			if (m_DownloadWork != null) {
				if (m_DownloadWork.request != null) {
					m_DownloadWork.request.Dispose();
				}
				m_DownloadWork = null;
			}

			base.OnDestroy();
		}

		#endregion
		#region Private types

		/// <summary>
		/// ダウンロード作業領域
		/// </summary>
		private class DownloadWork {
			public string url;
			public Hash128 hash;
			public uint crc;
			public UnityWebRequest request;
		}

		#endregion
		#region Private fields and properties

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
