// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using UnityEngine.Networking;
	using AssetBundleShosha.Internal;

	public class AssetBundlePlayerCrypto : AssetBundleBase {
		#region Public fields and properties

		/// <summary>
		/// 進捗(0.0f～1.0f)
		/// </summary>
		public override float progress {get{
			float result;
			if (m_DownloadWork != null) {
				if (m_DownloadWork.createRequest != null) {
					result = Mathf.Lerp(kDownloadedProgress, 1.0f, m_DownloadWork.createRequest.progress);
				} else if (m_DownloadWork.request != null) {
					result = m_DownloadWork.request.downloadProgress * kDownloadedProgress;
				} else {
					result = kDownloadedProgress;
				}
			} else if (m_AssetBundle == null) {
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
			return m_AssetBundle.mainAsset;
		}}

		/// <summary>
		/// アセットバンドルがストリーミングされたシーンのアセットバンドルならば、true を返します。
		/// </summary>
		public override bool isStreamedSceneAssetBundle {get{
			return m_AssetBundle.isStreamedSceneAssetBundle;
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
			throw new System.InvalidOperationException();
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// 特定のオブジェクトがアセットバンドルに含まれているか確認します。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>true:含まれる、false:含まれない</returns>
		public override bool Contains(string name) {
			return m_AssetBundle.Contains(name);
		}

		/// <summary>
		/// アセットバンドルにあるすべてのアセット名を返します。
		/// </summary>
		/// <returns>すべてのアセット名</returns>
		public override string[] GetAllAssetNames() {
			return m_AssetBundle.GetAllAssetNames();
		}

		/// <summary>
		/// アセットバンドルにあるすべてのシーンアセットのパス( *.unity アセットへのパス)を返します。
		/// </summary>
		/// <returns>すべてのシーンアセットのパス</returns>
		public override string[] GetAllScenePaths() {
			return m_AssetBundle.GetAllScenePaths();
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <returns>該当するすべてのアセット</returns>
		public override T[] LoadAllAssets<T>() {
			return m_AssetBundle.LoadAllAssets<T>();
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets(System.Type type) {
			return m_AssetBundle.LoadAllAssets(type);
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets() {
			return m_AssetBundle.LoadAllAssets();
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync<T>() {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAllAssetsAsync<T>()
												);
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync(System.Type type) {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAllAssetsAsync(type)
												);
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync() {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAllAssetsAsync()
												);
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T LoadAsset<T>(string name) {
			return m_AssetBundle.LoadAsset<T>(name);
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name, System.Type type) {
			return m_AssetBundle.LoadAsset(name, type);
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name) {
			return m_AssetBundle.LoadAsset(name);
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync<T>(string name) {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAssetAsync<T>(name)
												);
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name, System.Type type) {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAssetAsync(name, type)
												);
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name) {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAssetAsync(name)
												);
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T[] LoadAssetWithSubAssets<T>(string name) {
			return m_AssetBundle.LoadAssetWithSubAssets<T>(name);
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name, System.Type type) {
			return m_AssetBundle.LoadAssetWithSubAssets(name, type);
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name) {
			return m_AssetBundle.LoadAssetWithSubAssets(name);
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name) {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAssetWithSubAssetsAsync<T>(name)
												);
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name, System.Type type) {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAssetWithSubAssetsAsync(name, type)
												);
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name) {
			return new AssetBundleRequestPlayer(manager
												, this
												, m_AssetBundle.LoadAssetWithSubAssetsAsync(name)
												);
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
				fullPath = manager.GetDeliveryStreamingAssetsCacheFullPath(fileNameAndURL.fileName),
				hash = manager.catalog.GetAssetBundleHash(nameWithVariant),
				crc = manager.catalog.GetAssetBundleCrc(nameWithVariant),
				fileSize = manager.catalog.GetAssetBundleFileSize(nameWithVariant)
			};

			var hasCache = manager.HasCacheForDeliveryStreamingAsset(nameWithVariant);
			if (!hasCache) {
				//キャッシュ無効
				m_DownloadWork.request = new UnityWebRequest(m_DownloadWork.url
															, UnityWebRequest.kHttpVerbGET
															, new DownloadHandlerDeliveryStreamingAsset(m_DownloadWork.fullPath, m_DownloadWork.hash, 0, (int)m_DownloadWork.fileSize)
															, null
															);
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

				if (!m_DownloadWork.request.isDone || m_DownloadWork.request.isNetworkError || m_DownloadWork.request.isHttpError) {
					AssetBundleErrorCodeUtility.TryParse(m_DownloadWork.request, out m_ErrorCode);
				}
			}

			yield return base.OnStartedOnlineProcess();
		}

		/// <summary>
		/// オフラインプロセス開始イベント
		/// </summary>
		/// <returns>コルーチン</returns>
		protected override System.Collections.IEnumerator OnStartedOfflineProcess() {
			if (m_DownloadWork.request != null) {
				if (!m_DownloadWork.request.isNetworkError && !m_DownloadWork.request.isHttpError) {
					m_DownloadWork.request.Dispose();
					m_DownloadWork.request = null;
				}
			}

			var cryptoHash = AssetBundleCrypto.GetCryptoHash(manager.catalog, nameWithVariant);
			var cryptoFile = File.Open(m_DownloadWork.fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var decryptoStream = new AssetBundleDecryptoStream(cryptoFile, cryptoHash);
			m_DownloadWork.createRequest = AssetBundle.LoadFromStreamAsync(decryptoStream, m_DownloadWork.crc);
			yield return m_DownloadWork.createRequest;

			try {
				m_AssetBundle = m_DownloadWork.createRequest.assetBundle;
			} catch {
				m_ErrorCode = AssetBundleErrorCode.DecryptFailed;
			}
			
			yield return base.OnStartedOfflineProcess();
		}

		/// <summary>
		/// ダウンロード終了イベント
		/// </summary>
		protected override void OnDownloadFinished() {
			manager.SetDeliveryStreamingAssetCache(nameWithVariant, m_DownloadWork.hash, m_DownloadWork.crc, m_DownloadWork.fileSize);
			if (m_DownloadWork.request != null) {
				m_DownloadWork.request.Dispose();
			}
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
			if (m_AssetBundle != null) {
				m_AssetBundle.Unload(false);
				m_AssetBundle = null;
			}

			base.OnDestroy();
		}

		#endregion
		#region Private types

		/// <summary>
		/// ダウンロード作業領域
		/// </summary>
		private class DownloadWork {
			public string fullPath;
			public string url;
			public Hash128 hash;
			public uint crc;
			public uint fileSize;
			public UnityWebRequest request;
			public AssetBundleCreateRequest createRequest;
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// ダウンロード済みの進捗率
		/// </summary>
		private float kDownloadedProgress = 0.75f;

		#endregion
		#region Private fields and properties

		/// <summary>
		/// アセットバンドル
		/// </summary>
		private AssetBundle m_AssetBundle;

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
