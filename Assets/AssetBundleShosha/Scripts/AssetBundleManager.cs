// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.Networking;
	using AssetBundleShosha.Internal;
	using AssetBundleShosha.Utility;

	[DisallowMultipleComponent]
	public class AssetBundleManager : SingletonMonoBehaviour<AssetBundleManager> {
		#region Public types

		/// <summary>
		/// 全ダウンロードフラグ
		/// </summary>
		[System.Flags]
		public enum DownloadAllAssetBundlesFlags {
			Null							= 0,
			ExcludeAssetBundles				= 1 << 0, //アセットバンドルを除外する
			ExcludeDeliveryStreamingAssets	= 1 << 1, //配信ストリーミングアセットを除外する
			IncludeOutOfVariants			= 1 << 2, //範囲外バリアントを含める
		}

		#endregion
		#region Public const fields
		#endregion
		#region Public fields and properties

		/// <summary>
		/// ベースURL('/'終端)
		/// </summary>
		public string baseURL {get{return m_BaseURL;}}

		/// <summary>
		/// バリアント
		/// </summary>
		public string[] variants {get{return m_Variants;}}

		/// <summary>
		/// カタログ
		/// </summary>
		public AssetBundleCatalog catalog {get{
			AssetBundleCatalog result = null;
#if UNITY_EDITOR
			if (editor != null) {
				result = editor.catalog;
			}
			if (result != null) {} else //下に続く
#endif
			{
				result = m_Catalog;
			}
			return result;
		}}

		/// <summary>
		/// コンテンツハッシュ
		/// </summary>
		public uint contentHash {get{
			var result = 0u;
			if (catalog != null) {
				result = catalog.GetContentHash();
			}
			return result;
		}}

		/// <summary>
		/// ダウンロードタイムアウト秒
		/// </summary>
		public int downloadTimeoutSeconds {get{return m_DownloadTimeoutSeconds;} set{m_DownloadTimeoutSeconds = value;}}

		/// <summary>
		/// 並列ダウンロード数
		/// </summary>
		public int parallelDownloadsCount {get{return m_ParallelDownloadsCount;} set{m_ParallelDownloadsCount = value;}}

		/// <summary>
		/// 進捗(0.0f～1.0f)
		/// </summary>
		public float progress {get{return m_Progress;}}

		/// <summary>
		/// 進捗レシーバー
		/// </summary>
		public IProgressReceiver progressReceiver {get{
			if ((m_ProgressReceiverCache == null) && (m_ProgressReceiver != null)) {
				m_ProgressReceiverCache = (IProgressReceiver)m_ProgressReceiver;
			}
			return m_ProgressReceiverCache;
		} set{
			m_ProgressReceiverCache = value;
			m_ProgressReceiver = value as Component;
		}}

		/// <summary>
		/// エラーハンドラー
		/// </summary>
		public IErrorHandler errorHandler {get{
			if ((m_ErrorHandlerCache == null) && (m_ErrorHandler != null)) {
				m_ErrorHandlerCache = (IErrorHandler)m_ErrorHandler;
			}
			return m_ErrorHandlerCache;
		} set{
			m_ErrorHandlerCache = value;
			m_ErrorHandler = value as Component;
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="baseURL">ベースURL('/'終端問わず)</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <param name="errorHandlerForInitialize">初期化用エラーハンドラー</param>
		/// <returns>コルーチン</returns>
		/// <remarks>
		///		アセットバンドル読み込み時に汎用的に使用されるエラーハンドラーはerrorHandlerプロパティに設定する
		///		初期化用エラーハンドラーが未設定に於いてエラーが発生した場合は(汎用)エラーハンドラーに委譲される
		///		初期化用エラーハンドラー・(汎用)エラーハンドラーが共に未設定に於いてエラーが発生した場合、初期化に失敗する
		/// </remarks>
		public Coroutine Initialize(string baseURL, UnityAction onFinished = null, IErrorHandler errorHandlerForInitialize = null) {
			return StartCoroutine(InitializeCoroutine(baseURL, onFinished, errorHandler));
		}
		public Coroutine Initialize(string baseURL, IErrorHandler errorHandlerForInitialize) {
			return Initialize(baseURL, null, errorHandler);
		}

		/// <summary>
		/// アセットバンドル読み込み
		/// </summary>
		/// <param name="assetBundleName">読み込むアセットバンドル名</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <returns>アセットバンドル</returns>
		public IAssetBundle LoadAssetBundle(string assetBundleName, UnityAction<IAssetBundle> onFinished = null) {
			var result = new AssetBundleUser();
			UnityAction<AssetBundleBase> onFinishedWithDependencies = null;
			if (onFinished != null) {
				onFinishedWithDependencies = x=>{
					if (result.referenceCountingInstanceInternal == null) {
						result.referenceCountingInstanceInternal = x;
					}
					onFinished(result);
				};
			}
			var assetBundle = LoadAssetBundleWithDependencies(assetBundleName, onFinishedWithDependencies, true);
			if (result.referenceCountingInstanceInternal == null) {
				result.referenceCountingInstanceInternal = assetBundle;
			}
			return result;
		}

		/// <summary>
		/// バリアント設定
		/// </summary>
		/// <param name="variants"></param>
		public void SetVariants(string[] variants) {
			m_Variants = variants;
		}
		public void SetVariants(IEnumerable<string> variants) {
			if (variants != null) {
				m_Variants = variants.ToArray();
			} else {
				m_Variants = null;
			}
		}

		/// <summary>
		/// キャッシュクリア
		/// </summary>
		/// <returns>成功確認</returns>
		public bool ClearCache() {
			var resultOfClearDeliveryStreamingAssetsCache = false;
			var clearDeliveryStreamingAssetsCache = AssetBundleUtility.ClearDeliveryStreamingAssetsCacheThread(x=>resultOfClearDeliveryStreamingAssetsCache = x);
			clearDeliveryStreamingAssetsCache.Start();
			m_DeliveryStreamingAssetCacheIndex.Clear();
			var resultOfClearAssetBundlesCache = Caching.ClearCache();
			clearDeliveryStreamingAssetsCache.Join();
			
			var result = resultOfClearAssetBundlesCache && resultOfClearDeliveryStreamingAssetsCache;
#if UNITY_EDITOR
			if (editor != null) {
				var editorResult = editor.ClearCache();
				result = result && editorResult;
			}
#endif
			return result;
		}

		/// <summary>
		/// キャッシュ確認
		/// </summary>
		/// <param name="assetBundleNames">アセットバンドル名群</param>
		/// <returns>true:キャッシュ所持、false:キャッシュ未所持</returns>
		public bool HasCache(string assetBundleName) {
			assetBundleName = assetBundleName.ToLower();
			var assetBundleNameWithVariant = ApplyVariant(assetBundleName);
			var result = HasCacheFromAssetBundleNameWithVariant(assetBundleNameWithVariant);
			if (result) {
				var allDependencies = GetAllDependencies(assetBundleName);
				foreach (var dependency in allDependencies) {
					var dependencyAssetBundleNameWithVariant = ApplyVariant(dependency);
					result = HasCacheFromAssetBundleNameWithVariant(dependencyAssetBundleNameWithVariant);
					if (!result) {
						break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// アセットバンドル全クリア
		/// </summary>
		public void ClearAllAssetBundle() {
			var assetBundlesCount = m_Downloaded.Count + m_Progressing.Count;
			var assetBundles = new List<AssetBundleBase>(assetBundlesCount);

			//ダウンロードキュー
			assetBundles.AddRange(m_DownloadQueue);
			foreach (var assetBundle in assetBundles) {
				assetBundle.Dispose();
			}

			//ダウンロード中探索
			assetBundles.AddRange(m_Downloading);
			foreach (var assetBundle in assetBundles) {
				assetBundle.Dispose();
			}

			//ダウンロード済み探索
			assetBundles.AddRange(m_Downloaded.Values);
			foreach (var assetBundle in assetBundles) {
				assetBundle.Dispose();
			}
		}

		/// <summary>
		/// ダウンロードサイズ取得
		/// </summary>
		/// <param name="assetBundleNames">アセットバンドル名群</param>
		/// <returns>ダウンロードサイズ</returns>
		public long GetDownloadSize(string assetBundleName) {
			assetBundleName = assetBundleName.ToLower();
			var assetBundleNameWithVariant = ApplyVariant(assetBundleName);
			var result = GetDownloadSizeFromAssetBundleNameWithVariant(assetBundleNameWithVariant);
			var allDependencies = GetAllDependencies(assetBundleName);
			foreach (var dependency in allDependencies) {
				var dependencyAssetBundleNameWithVariant = ApplyVariant(dependency);
				result += GetDownloadSizeFromAssetBundleNameWithVariant(dependencyAssetBundleNameWithVariant);
			}
			return result;
		}
		public long GetDownloadSize(params string[] assetBundleNames) {
			return GetDownloadSize((IEnumerable<string>)assetBundleNames);
		}
		public long GetDownloadSize(IEnumerable<string> assetBundleNames) {
			//読み込むアセットバンドルの列挙
			var allAssetBundleNamesMultiple = new List<string>();
			foreach (var assetBundleNameCaseInsensitive in assetBundleNames) {
				var assetBundleName = assetBundleNameCaseInsensitive.ToLower();
				allAssetBundleNamesMultiple.Add(assetBundleName);
				var allDependencies = GetAllDependencies(assetBundleName);
				allAssetBundleNamesMultiple.AddRange(allDependencies);
			}
			//重複除去とバリアント適用
			allAssetBundleNamesMultiple.Sort(); //重複除去用ソート
			List<string> assetBundleNameWithVariants;
			if (allAssetBundleNamesMultiple.Count <= 1) {
				//重複なし
				for (int i = 0; i < allAssetBundleNamesMultiple.Count; ++i) {
					allAssetBundleNamesMultiple[i] = ApplyVariant(allAssetBundleNamesMultiple[i]);
				}
				assetBundleNameWithVariants = allAssetBundleNamesMultiple;
			} else {
				//重複の可能性がある
				assetBundleNameWithVariants = new List<string>(allAssetBundleNamesMultiple.Count);
				assetBundleNameWithVariants.Add(ApplyVariant(allAssetBundleNamesMultiple[0]));
				for (int i = 1; i < allAssetBundleNamesMultiple.Count; ++i) {
					if (allAssetBundleNamesMultiple[i - 1] != allAssetBundleNamesMultiple[i]) {
						assetBundleNameWithVariants.Add(ApplyVariant(allAssetBundleNamesMultiple[i]));
					}
				}
			}
			var result = 0L;
			foreach (var assetBundleNameWithVariant in assetBundleNameWithVariants) {
				result += GetDownloadSizeFromAssetBundleNameWithVariant(assetBundleNameWithVariant);
			}
			return result;
		}

		/// <summary>
		/// アセットバンドル全ダウンロード
		/// </summary>
		/// <param name="options">ダウンロードオプション</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <returns>コルーチン</returns>
		public Coroutine DownloadAllAssetBundles(DownloadAllAssetBundlesFlags options, UnityAction onFinished = null) {
			return StartCoroutine(DownloadAllAssetBundlesCoroutine(options, onFinished));
		}
		public Coroutine DownloadAllAssetBundles(UnityAction onFinished = null) {
			return DownloadAllAssetBundles(DownloadAllAssetBundlesFlags.Null, onFinished);
		}

		/// <summary>
		/// アセットバンドル全ダウンロードに依るダウンロードサイズ取得
		/// </summary>
		/// <param name="options">ダウンロードオプション</param>
		/// <returns>ダウンロードサイズ</returns>
		public long GetDownloadSizeByDownloadAllAssetBundles(DownloadAllAssetBundlesFlags options) {
			var allAssetBundleNameWithVariants = GetAllAssetBundleNameWithVariants(options);
			var result = GetDownloadSize(allAssetBundleNameWithVariants);
			return result;
		}
		public long GetDownloadSizeByDownloadAllAssetBundles() {
			return GetDownloadSizeByDownloadAllAssetBundles(DownloadAllAssetBundlesFlags.Null);
		}

		#endregion
		#region Protected methods

#if UNITY_EDITOR
		/// <summary>
		/// 破棄
		/// </summary>
		protected override void OnDestroy() {
			if (m_Editor != null) {
				Destroy(m_Editor);
				m_Editor = null;
			}
			base.OnDestroy();
		}
#endif

		/// <summary>
		/// 更新
		/// </summary>
		protected virtual void Update() {
			UpdateRequestUnload();
			UpdateDownloading();
			UpdateProgress();
			UpdateDownloadingClear();
		}

#if UNITY_EDITOR
		/// <summary>
		/// バリデーション
		/// </summary>
		protected virtual void OnValidate() {
			m_ProgressReceiverCache = m_ProgressReceiver as IProgressReceiver;
			m_ErrorHandlerCache = m_ErrorHandler as IErrorHandler;
		}
#endif

		#endregion
		#region Internal types

		/// <summary>
		/// ファイル名とURL
		/// </summary>
		internal struct FileNameAndURL {
			public string fileName;
			public string url;
		}

		#endregion
		#region Internal fields and properties

		/// <summary>
		/// ハッシュ計算機
		/// </summary>
		internal HashAlgorithm hashAlgorithm {get{return m_HashAlgorithm;}}

#if UNITY_EDITOR
		/// <summary>
		/// ダウンロードキュー
		/// </summary>
		internal Queue<AssetBundleBase> downloadQueue {get{return m_DownloadQueue;}}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// ダウンロード中
		/// </summary>
		internal Queue<AssetBundleBase> downloading {get{return m_Downloading;}}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// ダウンロード済み
		/// </summary>
		internal Dictionary<string, AssetBundleBase> downloaded {get{return m_Downloaded;}}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// 進捗中
		/// </summary>
		internal Dictionary<string, AssetBundleBase> progressing {get{return m_Progressing;}}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// エディタ用マネージャー
		/// </summary>
		internal AssetBundleManagerEditor editor {get{
			if ((m_Editor == null) && (this != null)) {
				m_Editor = GetComponent<AssetBundleManagerEditor>();
				if ((m_Editor == null) && Application.isPlaying) {
					m_Editor = gameObject.AddComponent<AssetBundleManagerEditor>();
				}
			}
			return m_Editor;
		}}
#endif

		#endregion
		#region Internal methods

		/// <summary>
		/// アセットバンドルのキャッシュ確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:キャッシュ所持、false:キャッシュ未所持</returns>
		internal bool HasCacheForAssetBundle(string assetBundleNameWithVariant) {
#if UNITY_EDITOR
			if (editor != null) {
				var editorResult = editor.HasCacheForAssetBundle(assetBundleNameWithVariant);
				if (editorResult.HasValue) {
					return editorResult.Value;
				}
			}
#endif
			var url = GetAssetBundleFileNameAndURL(assetBundleNameWithVariant).url;
			var hash = catalog.GetAssetBundleHash(assetBundleNameWithVariant);
			var result = Caching.IsVersionCached(url, hash);
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットのキャッシュ確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:キャッシュ所持、false:キャッシュ未所持</returns>
		internal bool HasCacheForDeliveryStreamingAsset(string assetBundleNameWithVariant) {
#if UNITY_EDITOR
			if (editor != null) {
				var editorResult = editor.HasCacheForDeliveryStreamingAsset(assetBundleNameWithVariant);
				if (editorResult.HasValue) {
					return editorResult.Value;
				}
			}
#endif
			var result = false;
			var info = GetDeliveryStreamingAssetCache(assetBundleNameWithVariant);
			if (info != null) {
				//キャッシュがある
				var hash = catalog.GetAssetBundleHash(assetBundleNameWithVariant);
				var crc = catalog.GetAssetBundleCrc(assetBundleNameWithVariant);
				var fileSize = catalog.GetAssetBundleFileSize(assetBundleNameWithVariant);

				var vaildCache = true;
				vaildCache = vaildCache && (hash == info.hash);
				vaildCache = vaildCache && (crc == info.crc);
				vaildCache = vaildCache && (fileSize == info.fileSize);

				result = vaildCache;
			}
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットキャッシュフルパス取得
		/// </summary>
		/// <param name="fileName">ファイル名</param>
		/// <returns>配信ストリーミングアセットキャッシュフルパス</returns>
		internal string GetDeliveryStreamingAssetsCacheFullPath(string fileName) {
			var result = m_HashAlgorithm.JoinString(AssetBundleUtility.temporaryCacheBasePath, AssetBundleUtility.kDeliveryStreamingAssetsCacheMiddlePath, fileName);
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットキャッシュ情報取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>配信ストリーミングアセットキャッシュ情報</returns>
		internal DeliveryStreamingAssetCacheIndex.Info GetDeliveryStreamingAssetCache(string assetBundleNameWithVariant) {
			var result = m_DeliveryStreamingAssetCacheIndex.GetInfo(assetBundleNameWithVariant);
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットキャッシュ情報設定
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <param name="hash">ハッシュ</param>
		/// <param name="crc">CRC</param>
		/// <param name="fileSize">ファイルサイズ</param>
		/// <returns>情報</returns>
		internal DeliveryStreamingAssetCacheIndex.Info SetDeliveryStreamingAssetCache(string assetBundleNameWithVariant, Hash128 hash, uint crc, uint fileSize) {
			var result = m_DeliveryStreamingAssetCacheIndex.SetInfo(assetBundleNameWithVariant, hash, crc, fileSize);
			SaveDeliveryStreamingAssetCacheIndex();
			return result;
		}

		/// <summary>
		/// アセットバンドルのファイル名とURL取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>ファイル名とURL</returns>
		internal FileNameAndURL GetAssetBundleFileNameAndURL(string assetBundleNameWithVariant) {
			var result = new FileNameAndURL();
			var platformString = AssetBundleUtility.GetPlatformString();
			result.fileName = m_HashAlgorithm.GetAssetBundleFileName(platformString, assetBundleNameWithVariant);
			result.url = m_BaseURL + result.fileName;
			return result;
		}

		/// <summary>
		/// アセットバンドル破棄
		/// </summary>
		/// <param name="assetBundle">破棄するアセットバンドル</param>
		internal void UnloadAssetBundle(AssetBundleBase assetBundle) {
			UnloadAssetBundleWithDependencies(assetBundle, true);
		}

		/// <summary>
		/// ファイナライザー内アセットバンドル破棄
		/// </summary>
		/// <param name="assetBundle">破棄するアセットバンドル</param>
		internal void UnloadAssetBundleInFinalize(AssetBundleBase assetBundle) {
			m_RequestUnloadAssetBundles.Enqueue(assetBundle);
		}

		/// <summary>
		/// エラー無視
		/// </summary>
		/// <param name="assetBundle">アセットバンドル</param>
		internal void IgnoreError(AssetBundleBase assetBundle) {
			assetBundle.state = AssetBundleBase.State.ErrorIgnore;
			FinishedError();
		}

		/// <summary>
		/// エラーリトライ
		/// </summary>
		/// <param name="assetBundle">アセットバンドル</param>
		internal void RetryError(AssetBundleBase assetBundle) {
			assetBundle.state = AssetBundleBase.State.ErrorRetry;
			FinishedError();
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// ベースURL('/'終端)
		/// </summary>
		[SerializeField]
		private string m_BaseURL;

		/// <summary>
		/// バリアント
		/// </summary>
		[SerializeField]
		public string[] m_Variants;

		/// <summary>
		/// キャッシュ
		/// </summary>
		private DeliveryStreamingAssetCacheIndex m_DeliveryStreamingAssetCacheIndex = new DeliveryStreamingAssetCacheIndex();

		/// <summary>
		/// カタログ
		/// </summary>
		[SerializeField]
		private AssetBundleCatalog m_Catalog;

		/// <summary>
		/// ダウンロードキュー
		/// </summary>
		private Queue<AssetBundleBase> m_DownloadQueue = new Queue<AssetBundleBase>();

		/// <summary>
		/// ダウンロード中
		/// </summary>
		private Queue<AssetBundleBase> m_Downloading = new Queue<AssetBundleBase>();

		/// <summary>
		/// 進捗(0.0f～1.0f)
		/// </summary>
		[System.NonSerialized]
		private float m_Progress;

		/// <summary>
		/// ダウンロード済み
		/// </summary>
		private Dictionary<string, AssetBundleBase> m_Downloaded = new Dictionary<string, AssetBundleBase>();

		/// <summary>
		/// 進捗中
		/// </summary>
		private Dictionary<string, AssetBundleBase> m_Progressing = new Dictionary<string, AssetBundleBase>();

		/// <summary>
		/// アセットバンドル破棄リクエスト
		/// </summary>
		private Queue<AssetBundleBase> m_RequestUnloadAssetBundles = new Queue<AssetBundleBase>();

		/// <summary>
		/// ダウンロードタイムアウト秒
		/// </summary>
		[SerializeField]
		private int m_DownloadTimeoutSeconds = 30;

		/// <summary>
		/// 並列ダウンロード数
		/// </summary>
		[SerializeField]
		private int m_ParallelDownloadsCount = 2;

		/// <summary>
		/// 進捗レシーバー
		/// </summary>
		[SerializeField][RestrictInterface(typeof(IProgressReceiver))]
		private Component m_ProgressReceiver;
		[System.NonSerialized]
		private IProgressReceiver m_ProgressReceiverCache;

		/// <summary>
		/// エラーハンドラー
		/// </summary>
		[SerializeField][RestrictInterface(typeof(IErrorHandler))]
		private Component m_ErrorHandler;
		[System.NonSerialized]
		private IErrorHandler m_ErrorHandlerCache;

		/// <summary>
		/// ハッシュ計算機
		/// </summary>
		private HashAlgorithm m_HashAlgorithm = new HashAlgorithm();

#if UNITY_EDITOR
		/// <summary>
		/// エディタ用マネージャー
		/// </summary>
		private AssetBundleManagerEditor m_Editor;
#endif

		#endregion
		#region Private methods

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="baseURL">ベースURL('/'終端問わず)</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <param name="errorHandlerForInitialize">初期化用エラーハンドラー</param>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> InitializeCoroutine(string baseURL, UnityAction onFinished, IErrorHandler errorHandlerForInitialize) {
			//白紙化
			m_BaseURL = string.Empty;
			m_Catalog = null;
			//m_Variantsは初期化しない

			//末端'/'確認と付与
			if (!baseURL.EndsWith("/")) {
				baseURL += '/';
			}

			//カタログ読み込み
			AssetBundleCatalog catalog = null;
			var errorHandler = errorHandlerForInitialize ?? this.errorHandler;
			var downloadCatalog = DownloadCatalog(baseURL, c=>{
				catalog = c;
			}, errorHandler);

			//配信ストリーミングアセットキャッシュインデックス読み込み
			LoadDeliveryStreamingAssetCacheIndex();

			//カタログ読み込み待ち
			yield return downloadCatalog;

			if (catalog != null) {
				//カタログ読み込み成功
				m_BaseURL = baseURL;
				m_Catalog = catalog;
				//m_Variantsは初期化しない
#if UNITY_IOS
				//配信ストリーミングアセットキャッシュディレクトリ作成とバックアップ除外設定
				if (HasDeliveryStreamingAssets(m_Catalog)) {
					CreateShoshaCacheDirectory();
				}
#endif
			}

			//キャッシュ準備完了待ち
			while(!Caching.ready) {
				yield return null;
			}
			
			if (onFinished != null) onFinished();
			yield break;
		}

#if UNITY_IOS
		/// <summary>
		/// 配信ストリーミングアセット所持確認
		/// </summary>
		/// <param name="catalog">カタログ</param>
		/// <returns>true:配信ストリーミングアセット所持, false:未所持</returns>
		private static bool HasDeliveryStreamingAssets(AssetBundleCatalog catalog) {
			var result = false;
			var allAssetBundles = catalog.GetAllAssetBundles();
			if (allAssetBundles != null) {
				var firstIndexOfDeliveryStreamingAsset = 0;
				var binarySearchResult = System.Array.BinarySearch(allAssetBundles, AssetBundleUtility.kDeliveryStreamingAssetsPrefix);
				if (0 <= binarySearchResult) {
					Debug.LogAssertion("Logical Error!"); //無名配信ストリーミングアセットは格納されている筈がない
					firstIndexOfDeliveryStreamingAsset = binarySearchResult;
				} else {
					firstIndexOfDeliveryStreamingAsset = ~binarySearchResult;
				}
				if (firstIndexOfDeliveryStreamingAsset < allAssetBundles.Length) {
					//配信ストリーミングアセット名の先頭インデックスが見つかったなら
					var firstNameOfDeliveryStreamingAsset = allAssetBundles[firstIndexOfDeliveryStreamingAsset];
					if (AssetBundleUtility.IsDeliveryStreamingAsset(firstNameOfDeliveryStreamingAsset)) {
						//配信ストリーミングアセット名が見つかったなら
						result = true;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// キャッシュディレクトリ作成
		/// </summary>
		/// <remarks>iOSではバックアップ除外設定を行う関係上、ディレクトリのみ先に作る</remarks>
		private static void CreateShoshaCacheDirectory() {
			var temporaryCachePath = AssetBundleUtility.temporaryCacheBasePath;
			AssetBundleUtility.CreateDirectory(temporaryCachePath);
			UnityEngine.iOS.Device.SetNoBackupFlag(temporaryCachePath);
		}
#endif

		/// <summary>
		/// 配信ストリーミングアセットキャッシュインデックスフルパス取得
		/// </summary>
		/// <returns>配信ストリーミングアセットキャッシュインデックスフルパス</returns>
		private string GetDeliveryStreamingAssetsCacheIndexFullPath() {
			var fileName = m_HashAlgorithm.GetAssetBundleFileName(null, AssetBundleUtility.kDeliveryStreamingAssetsPrefix); //プレフィックスのみのファイルは存在しないのでキャッシュファイルとして使用
			var result = GetDeliveryStreamingAssetsCacheFullPath(fileName);
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットキャッシュインデックス読み込み
		/// </summary>
		private void LoadDeliveryStreamingAssetCacheIndex() {
			var fullPath = GetDeliveryStreamingAssetsCacheIndexFullPath();
			try {
				m_DeliveryStreamingAssetCacheIndex.Load(fullPath);
			} catch {
				//読み込みミスなら
				//empty.
				//初回はファイルが無いのでそれに起因する例外が投げられる事は想定内だが
				//この時に投げられる例外がプラットフォームに依ってバラバラな為、全部受け取る
			}
		}

		/// <summary>
		/// 配信ストリーミングアセットキャッシュインデックス書き出し
		/// </summary>
		private void SaveDeliveryStreamingAssetCacheIndex() {
			var fullPath = GetDeliveryStreamingAssetsCacheIndexFullPath();
			m_DeliveryStreamingAssetCacheIndex.Save(fullPath);
		}

		/// <summary>
		/// カタログURL取得
		/// </summary>
		/// <param name="baseURL">ベースURL('/'終端)</param>
		/// <returns>カタログURL</returns>
		private string GetCatalogURL(string baseURL) {
			var result = baseURL;
			var platformString = AssetBundleUtility.GetPlatformString();
			result += m_HashAlgorithm.GetAssetBundleFileName(platformString, null);
			return result;
		}

		/// <summary>
		/// カタログダウンロード
		/// </summary>
		/// <param name="baseURL">ベースURL('/'終端問わず)</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <param name="errorHandler">エラーハンドラー</param>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> DownloadCatalog(string baseURL, UnityAction<AssetBundleCatalog> onFinished, IErrorHandler errorHandler) {
			var catalogURL = GetCatalogURL(baseURL);
			while (true) {
				//カタログダウンロード
				var catalogRequest = UnityWebRequest.GetAssetBundle(catalogURL);
				yield return catalogRequest.SendWebRequest();
				if (catalogRequest.isHttpError || catalogRequest.isNetworkError) {
					//ダウンロードエラーなら
					var state = AssetBundleBase.State.ErrorThinking;
					yield return DownloadCatalogError(catalogRequest, errorHandler, x=>{
						state = x;
					});
					if (state == AssetBundleBase.State.ErrorRetry) {
						//リトライなら
						continue;
					}
					if (onFinished != null) onFinished(null);
					yield break;
				}

				//カタログ読み込み
				var catalogDownloadHandlerAssetBundle = (DownloadHandlerAssetBundle)catalogRequest.downloadHandler;
				var catalogAssetBundle = catalogDownloadHandlerAssetBundle.assetBundle;
				catalogRequest.Dispose();
				if (catalogAssetBundle == null) {
					//アセットバンドル取得エラーなら
					var state = AssetBundleBase.State.ErrorThinking;
					yield return DownloadCatalogError(catalogURL, AssetBundleErrorCode.InitializeNotFoundAssetBundle, errorHandler, x=>{
						state = x;
					});
					if (state == AssetBundleBase.State.ErrorRetry) {
						//リトライなら
						continue;
					}
					if (onFinished != null) onFinished(null);
					yield break;
				}
				var catalogLoad = catalogAssetBundle.LoadAllAssetsAsync<AssetBundleCatalog>();
				yield return catalogLoad;
				var catalog = (AssetBundleCatalog)catalogLoad.asset;
				catalogAssetBundle.Unload(false);
				if (catalog == null) {
					//アセット取得エラーなら
					var state = AssetBundleBase.State.ErrorThinking;
					yield return DownloadCatalogError(catalogURL, AssetBundleErrorCode.InitializeNotFoundAsset, errorHandler, x=>{
						state = x;
					});
					if (state == AssetBundleBase.State.ErrorRetry) {
						//リトライなら
						continue;
					}
					yield break;
				}
				if (onFinished != null) onFinished(catalog);
				yield break;
			}
		}

		/// <summary>
		/// ダウンロードカタログエラー
		/// </summary>
		/// <param name="request">エラーが発生したWebリクエスト</param>
		/// <param name="url">エラーが発生したダウンロードURL</param>
		/// <param name="errorCode">エラーコード</param>
		/// <param name="errorHandler">エラーハンドラー</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <returns></returns>
		private IEnumerator<object> DownloadCatalogError(string url, AssetBundleErrorCode errorCode, IErrorHandler errorHandler, UnityAction<AssetBundleBase.State> onFinished) {
			if (errorHandler == null) {
				if (onFinished != null) onFinished(AssetBundleBase.State.ErrorIgnore);
			} else {
				var state = AssetBundleBase.State.ErrorThinking;
				var handle = InitializeErrorHandle.CreateAssetBundle(errorCode, url
																	, ignoreEvent:()=>state = AssetBundleBase.State.ErrorIgnore
																	, retryEvent:()=>state = AssetBundleBase.State.ErrorRetry
																	);
				errorHandler.Error(handle);
				while (state == AssetBundleBase.State.ErrorThinking) {
					yield return null;
				}
				if (onFinished != null) onFinished(state);
			}
		}
		private IEnumerator<object> DownloadCatalogError(UnityWebRequest request, IErrorHandler errorHandler, UnityAction<AssetBundleBase.State> onFinished) {
			if (errorHandler == null) {
				if (onFinished != null) onFinished(AssetBundleBase.State.ErrorIgnore);
			} else {
				AssetBundleErrorCode errorCode;
				if (AssetBundleErrorCodeUtility.TryParseForInitialize(request, out errorCode)) {
					yield return DownloadCatalogError(request.url, errorCode, errorHandler, onFinished);
				}
			}
		}

		/// <summary>
		/// アセットバンドル読み込み
		/// </summary>
		/// <param name="assetBundleName">読み込むアセットバンドル名</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <param name="loadDependencies">依存読み込み</param>
		/// <returns>アセットバンドル</returns>
		private AssetBundleBase LoadAssetBundleWithDependencies(string assetBundleName, UnityAction<AssetBundleBase> onFinished, bool loadDependencies) {
			assetBundleName = assetBundleName.ToLower();

			if (loadDependencies) {
				var allDependencies = GetAllDependencies(assetBundleName);
				foreach (var dependence in allDependencies) {
					LoadAssetBundleWithDependencies(dependence, null, false);
				}
			}

			var result = FindAssetBundleInstance(assetBundleName);
			if (result == null) {
				//既存インスタンス無し
				result = CreateAssetBundleInstance(assetBundleName);
				if (result.isDone) {
					//ダウンロード完了
					 onFinished(result);
					m_Downloaded[result.name] = result;
				} else {
					//ダウンロードに時間がかかる
					if (onFinished != null) {
						result.AddDownloadFinishedListener(x=>onFinished(x));
					}
					if (m_Progressing.Count == 0) {
						//進捗管理開始
						m_Progress = 0.0f;
						if (progressReceiver != null) {
							progressReceiver.ProgressStart();
							progressReceiver.ProgressUpdate(m_Progress);
						}
					}
					EntryDownloadQueue(result);
				}
			} else {
				//既存インスタンス有り
				++result.referenceCount;
				if (onFinished != null) {
					if (m_Downloaded.ContainsKey(result.name)) {
						//ダウンロード済み
						 onFinished(result);
					} else {
						//未ダウンロード
						result.AddDownloadFinishedListener(onFinished);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// 間接含む全依存関係の取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>間接含む全依存関係</returns>
		private string[] GetAllDependencies(string assetBundleName) {
			var assetBundleNameWithVariant = ApplyVariant(assetBundleName);
			var result = catalog.GetAllDependencies(assetBundleNameWithVariant);
			return result;
		}

		/// <summary>
		/// アセットバンドルインスタンスの探索
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>アセットバンドルインスタンス</returns>
		private AssetBundleBase FindAssetBundleInstance(string assetBundleName) {
			//ダウンロード済み探索
			AssetBundleBase downloaded = null;
			if (m_Downloaded.TryGetValue(assetBundleName, out downloaded)) {
				return downloaded;
			}

			//ダウンロード中探索
			foreach (var downloading in m_Downloading) {
				if (assetBundleName == downloading.name) {
					return downloading;
				}
			}

			//ダウンロードキュー探索
			foreach (var downloadQueue in m_DownloadQueue) {
				if (assetBundleName == downloadQueue.name) {
					return downloadQueue;
				}
			}

			return null;
		}

		/// <summary>
		/// アセットバンドルインスタンスの探索
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アセットバンドルインスタンス</returns>
		private AssetBundleBase FindAssetBundleInstanceWithAssetBundleNameWithVariant(string assetBundleNameWithVariant) {
			var assetBundleName = AssetBundleUtility.DeleteVariant(assetBundleNameWithVariant);

			//ダウンロード済み探索
			AssetBundleBase downloaded = null;
			if (m_Downloaded.TryGetValue(assetBundleName, out downloaded)) {
				if (downloaded.nameWithVariant == assetBundleNameWithVariant) {
					return downloaded;
				} else {
					return null;
				}
			}

			//ダウンロード中探索
			foreach (var downloading in m_Downloading) {
				if (assetBundleNameWithVariant == downloading.nameWithVariant) {
					return downloading;
				}
			}

			//ダウンロードキュー探索
			foreach (var downloadQueue in m_DownloadQueue) {
				if (assetBundleNameWithVariant == downloadQueue.nameWithVariant) {
					return downloadQueue;
				}
			}

			return null;
		}

		/// <summary>
		/// アセットバンドルインスタンスの作成
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>アセットバンドルインスタンス</returns>
		private AssetBundleBase CreateAssetBundleInstance(string assetBundleName) {
			AssetBundleBase result = null;
			var assetBundleNameWithVariant = ApplyVariant(assetBundleName);
#if UNITY_EDITOR
			if (editor != null) {
				result = editor.CreateAssetBundleInstance(assetBundleName);
			}
			if (result != null) {} else //下に続く
#endif
			if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleName)) {
				//配信ストリーミングアセット
				result = new DeliveryStreamingAssetPlayer();
			} else if (AssetBundleCrypto.IsCrypto(catalog, assetBundleNameWithVariant)) {
				//暗号化アセットバンドル
				result = new AssetBundlePlayerCrypto();
			} else {
				//平文アセットバンドル
				result = new AssetBundlePlayer();
			}
			var fileSize = catalog.GetAssetBundleFileSize(assetBundleNameWithVariant);
			result.Create(this, assetBundleName, assetBundleNameWithVariant, fileSize);
			return result;
		}

		/// <summary>
		/// バリアント適用
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>バリアント付きアセットバンドル名</returns>
		private string ApplyVariant(string assetBundleName) {
			if ((variants != null) && (0 < m_Variants.Length)) {
				var catalogVariantInfos = catalog.GetAssetBundleVariantInfos(assetBundleName);
				if (catalogVariantInfos != null) {
					foreach (var variant in m_Variants) {
						foreach (var catalogVariantInfo in catalogVariantInfos) {
							if (variant == catalogVariantInfo.Variant) {
								return catalogVariantInfo.assetBundleWithVariant;
							}
						}
					}
				}
			}
			return assetBundleName;
		}

		/// <summary>
		/// ダウンロードキューへの登録
		/// </summary>
		/// <param name="assetBundle">アセットバンドルインスタンス</param>
		private void EntryDownloadQueue(AssetBundleBase assetBundle) {
			//進捗中
			if (!m_Progressing.ContainsKey(assetBundle.nameWithVariant)) {
				m_Progressing[assetBundle.nameWithVariant] = assetBundle;
			}

			if (m_Downloading.Count < m_ParallelDownloadsCount) {
				//ダウンロードに空きがある
				//オンラインプロセス開始
				assetBundle.StartOnlineProcess();
			}
			if (assetBundle.state == AssetBundleBase.State.OnlineProcessed) {
				//オンラインプロセス終了なら
				//オフラインプロセス開始
				assetBundle.StartOfflineProcess();
			}
			if (assetBundle.state == AssetBundleBase.State.OfflineProcessed) {
				//オフラインプロセス終了なら
				if (m_Downloading.Count == 0) {
					//ダウンロード中が無いなら
					//完了処理
					if (!assetBundle.errorCode.IsError()) {
						//エラー無しなら
						//完了
						assetBundle.ProcessFinished();
					} else {
						//エラー発生なら
						//エラー処理開始
						StartError(assetBundle);
					}
				}
			}
			
			switch (assetBundle.state) {
			case AssetBundleBase.State.Wait:
				//ダウンロード待ち
				m_DownloadQueue.Enqueue(assetBundle);
				break;
			case AssetBundleBase.State.Done:
				//完了
				m_Downloaded[assetBundle.name] = assetBundle;
				break;
			default:
				m_Downloading.Enqueue(assetBundle);
				break;
			}
		}

		/// <summary>
		/// アセットバンドル破棄
		/// </summary>
		/// <param name="assetBundle">破棄するアセットバンドル</param>
		/// <param name="unloadDependencies">依存破棄</param>
		private void UnloadAssetBundleWithDependencies(AssetBundleBase assetBundle, bool unloadDependencies) {
			--assetBundle.referenceCount;
			if (0 == assetBundle.referenceCount) {
				var assetBundleName = assetBundle.name;
				var assetBundleNameWithVariant = assetBundle.nameWithVariant;
				m_Downloaded.Remove(assetBundleName);
				m_Progressing.Remove(assetBundleName);
				AssetBundleUtility.QueueRemoveAll(m_Downloading, x=>x.nameWithVariant == assetBundleNameWithVariant);
				AssetBundleUtility.QueueRemoveAll(m_DownloadQueue, x=>x.nameWithVariant == assetBundleNameWithVariant);
				assetBundle.Destroy();

				if (unloadDependencies) {
					var allDependencies = GetAllDependencies(assetBundleName);
					for (int i = allDependencies.Length - 1; 0 <= i; --i) {
						var dependence = allDependencies[i];
						AssetBundleBase dependenceAssetBundle;
						if (m_Downloaded.TryGetValue(dependence, out dependenceAssetBundle)) {
							UnloadAssetBundleWithDependencies(dependenceAssetBundle, false);
						}
					}
				}
			}
		}

		/// <summary>
		/// 破棄リクエスト更新
		/// </summary>
		private void UpdateRequestUnload() {
			if (0 < m_RequestUnloadAssetBundles.Count) {
				foreach (var requestUnloadAssetBundle in m_RequestUnloadAssetBundles) {
					requestUnloadAssetBundle.Dispose();
				}
				m_RequestUnloadAssetBundles.Clear();
			}
		}

		/// <summary>
		/// ダウンロード中更新
		/// </summary>
		private void UpdateDownloading() {
			bool redownloading;
			do {
				redownloading = false;

				//ダウンロード中処理
				while (0 < m_Downloading.Count) {
					var firstDownloading = m_Downloading.Peek();
					switch (firstDownloading.state) {
					case AssetBundleBase.State.OnlineProcessing:
					case AssetBundleBase.State.OfflineProcessing:
					case AssetBundleBase.State.ErrorThinking:
						//処理中なら
						//次に進む
						break;
					case AssetBundleBase.State.Wait:
						//処理前なら
						Debug.LogAssertion("Logical Error!"); //m_Downloadingが処理前になる事は無い
						//オンラインプロセス開始
						firstDownloading.StartOnlineProcess();
						continue;
					case AssetBundleBase.State.OnlineProcessed:
						//オンラインプロセスが終了しているなら
						//オフラインプロセス開始
						firstDownloading.StartOfflineProcess();
						continue;
					case AssetBundleBase.State.OfflineProcessed:
						//オフラインプロセスが終了しているなら
						//完了処理
						if (!firstDownloading.errorCode.IsError()) {
							//エラー無しなら
							//完了
							m_Downloading.Dequeue();
							firstDownloading.ProcessFinished();
							m_Downloaded[firstDownloading.name] = firstDownloading;
						} else {
							//エラー発生なら
							//エラー処理開始
							StartError(firstDownloading);
						}
						continue;
					case AssetBundleBase.State.ErrorIgnore:
						//エラー無視なら
						//完了
						m_Downloading.Dequeue();
						firstDownloading.ProcessFinished();
						m_Downloaded[firstDownloading.name] = firstDownloading;
						continue;
					case AssetBundleBase.State.ErrorRetry:
						//エラーリトライ無視なら
						//ダウンロード再開
						firstDownloading.StartOnlineProcess();
						continue;
					default:
						Debug.LogAssertion("Logical Error!");
						break;
					}
					break;
				}

				//ダウンロードキュー処理
				if (0 < m_DownloadQueue.Count) {
					while (m_Downloading.Count < m_ParallelDownloadsCount) {
						if (0 == m_DownloadQueue.Count) {
							break;
						}
						var queue = m_DownloadQueue.Dequeue();
						queue.StartOnlineProcess();
						m_Downloading.Enqueue(queue);
						if (queue.state == AssetBundleBase.State.OnlineProcessed) {
							//即時終了なら
							//ダウンロード中処理の再試行
							redownloading = true;
						}
					}
				}
			} while (redownloading);
		}

		/// <summary>
		/// 進捗更新
		/// </summary>
		private void UpdateProgress() {
			var total = 0u;
			var downloaded = 0u;
			foreach (var progress in m_Progressing) {
				var s = progress.Value.fileSize;
				var p = progress.Value.progress;
				total += s;
				if (1.0f <= p) {
					downloaded += s;
				} else {
					downloaded += (uint)(s * p);
				}
			}
			if (0 < total) {
				m_Progress = (float)downloaded / (float)total;

				if (progressReceiver != null) {
					progressReceiver.ProgressUpdate(m_Progress);
				}
			}
		}

		/// <summary>
		/// ダウンロード中クリア更新
		/// </summary>
		private void UpdateDownloadingClear() {
			if ((0 == m_Downloading.Count) && (0 == m_DownloadQueue.Count)) {
				//全てのダウンロードが終了
				m_Progressing.Clear();

				if (progressReceiver != null) {
					progressReceiver.ProgressFinished();
				}
			}
		}

		/// <summary>
		/// エラー処理開始
		/// </summary>
		/// <param name="assetBundle">アセットバンドル</param>
		private void StartError(AssetBundleBase assetBundle) {
			if ((errorHandler == null) && (assetBundle.errorHandler == null)) {
				IgnoreError(assetBundle);
				return;
			}

			var url = GetAssetBundleFileNameAndURL(assetBundle.nameWithVariant).url;
			var errorHandle = new ErrorHandle(assetBundle, url);
			assetBundle.state = AssetBundleBase.State.ErrorThinking;
			if (progressReceiver != null) {
				progressReceiver.ProgressErrorStart();
			}
			if (assetBundle.errorHandler != null) {
				assetBundle.errorHandler.Error(errorHandle);
			} else {
				errorHandler.Error(errorHandle);
			}
		}

		/// <summary>
		/// エラー処理終了
		/// </summary>
		private void FinishedError() {
			if (progressReceiver == null) {
				return;
			}

			var finishedAllError = m_Downloading.All(x=>x.state != AssetBundleBase.State.ErrorThinking);
			if (finishedAllError) {
				progressReceiver.ProgressErrorFinished();
			}
		}

		/// <summary>
		/// キャッシュ確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:キャッシュ所持、false:キャッシュ未所持</returns>
		private bool HasCacheFromAssetBundleNameWithVariant(string assetBundleNameWithVariant) {
			var result = false;
			if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleNameWithVariant)) {
				//配信ストリーミングアセット
				result = HasCacheForDeliveryStreamingAsset(assetBundleNameWithVariant);
			} else {
				//アセットバンドル
				result = HasCacheForAssetBundle(assetBundleNameWithVariant);
			}
			return result;
		}

		/// <summary>
		/// バリアント付きアセットバンドル名からダウンロードサイズ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>ダウンロードサイズ</returns>
		private uint GetDownloadSizeFromAssetBundleNameWithVariant(string assetBundleNameWithVariant) {
			var result = 0u;
			if (!HasCacheFromAssetBundleNameWithVariant(assetBundleNameWithVariant)) {
				result = catalog.GetAssetBundleFileSize(assetBundleNameWithVariant);
			}
			return result;
		}


		/// <summary>
		/// アセットバンドル全ダウンロード
		/// </summary>
		/// <param name="options">ダウンロードオプション</param>
		/// <param name="onFinished">終了時イベント</param>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> DownloadAllAssetBundlesCoroutine(DownloadAllAssetBundlesFlags options, UnityAction onFinished) {
			var allAssetBundleNameWithVariants = GetAllAssetBundleNameWithVariants(options);
			var temporaryAssetBundles = new List<IAssetBundle>(allAssetBundleNameWithVariants.Count);
			foreach (var assetBundleNameWithVariant in allAssetBundleNameWithVariants) {
				var queue = FindAssetBundleInstanceWithAssetBundleNameWithVariant(assetBundleNameWithVariant);
				if (queue == null) {
					//既存インスタンス無し
#if UNITY_EDITOR
					if (editor != null) {
						queue = editor.CreateAssetBundleInstance(assetBundleNameWithVariant);
					}
					if (queue != null) {} else //下に続く
#endif
					if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleNameWithVariant)) {
						//配信ストリーミングアセット
						queue = new DeliveryStreamingAssetPlayer();
					} else {
						//アセットバンドル
						queue = new AssetBundleDownloadOnly();
					}
					var fileSize = catalog.GetAssetBundleFileSize(assetBundleNameWithVariant);
					queue.Create(this, "[DL]" + assetBundleNameWithVariant, assetBundleNameWithVariant, fileSize);
					EntryDownloadQueue(queue);

					temporaryAssetBundles.Add(queue);
				}
			}
			temporaryAssetBundles.Reverse();
			while (0 < temporaryAssetBundles.Count) {
				yield return null;
				var index = temporaryAssetBundles.Count - 1;
				var assetBundle = temporaryAssetBundles[index];
				if (assetBundle.isDone) {
					temporaryAssetBundles.RemoveAt(index);
					UnloadAssetBundleWithDependencies((AssetBundleBase)assetBundle, false);
				}
			}
			yield break;
		}

		/// <summary>
		/// 全バリアント付きアセットバンドル名取得
		/// </summary>
		/// <param name="options"></param>
		/// <returns>バリアント付きアセットバンドル名群</returns>
		private List<string> GetAllAssetBundleNameWithVariants(DownloadAllAssetBundlesFlags options) {
			var allAssetBundles = catalog.GetAllAssetBundles();
			if (options == DownloadAllAssetBundlesFlags.IncludeOutOfVariants) {
				//除外する項目がなく範囲外バリアントを含めるなら
				//カタログ全掲載分を返す
				return allAssetBundles.ToList();
			}

			//バリアント無しアセットバンドル名群
			IEnumerable<string> allAssetBundlesWithoutVariant = allAssetBundles;
			var allAssetBundlesWithVariant = catalog.GetAllAssetBundlesWithVariant();
			if (0 < allAssetBundlesWithVariant.Length) {
				//カタログにバリアント付きが1つ以上ある
				//バリアントの最大文字長を探す
				var variantWithDotMaxLength = 0;
				foreach (var assetBundleWithVariant in allAssetBundlesWithVariant) {
					var variantStartIndex = assetBundleWithVariant.IndexOf('.');
					if (0 <= variantStartIndex) {
						var variantWithDotLength = assetBundleWithVariant.Length - variantStartIndex;
						if (variantWithDotMaxLength < variantWithDotLength) {
							variantWithDotMaxLength = variantWithDotLength;
						}
					}
				}
				//バリアント付きを除外
				allAssetBundlesWithoutVariant = allAssetBundlesWithoutVariant.Where(n=>{
					if (variantWithDotMaxLength < n.Length) {
						//バリアントの最大文字長分最後尾から区切り文字を探す
						return n.IndexOf('.', n.Length - variantWithDotMaxLength) < 0;
					} else {
						return false;
					}
				});
			}

			//バリアント無しアセットバンドル名を戻り値へ
			var result = new List<string>(allAssetBundles.Length);
			foreach (var assetBundleName in allAssetBundlesWithoutVariant) {
				var isAdd = true;
				if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleName)) {
					//配信ストリーミングアセット
					if ((options & DownloadAllAssetBundlesFlags.ExcludeDeliveryStreamingAssets) != 0) {
						isAdd = false;
					}
				} else {
					//アセットバンドル
					if ((options & DownloadAllAssetBundlesFlags.ExcludeAssetBundles) != 0) {
						isAdd = false;
					}
				}
				if (isAdd) {
					result.Add(assetBundleName);
				}
			}

			//バリアント付きアセットバンドル名群
			var prevAssetBundleWithVariant = string.Empty;
			var appliedVariantAssetBundleWithVariant = string.Empty;
			foreach (var assetBundleWithVariant in allAssetBundlesWithVariant) {
				var assetBundleName = AssetBundleUtility.DeleteVariant(assetBundleWithVariant);
				if (assetBundleName != prevAssetBundleWithVariant) {
					//(前回と同じアセットバンドル名のバリアント違いではなく)新規アセットバンドル名なら
					//バリアント適用後のバリアント付きアセットバンドル名を取得
					appliedVariantAssetBundleWithVariant = ApplyVariant(assetBundleName);
					prevAssetBundleWithVariant = assetBundleName;
				}
				var isAdd = true;
				if (assetBundleWithVariant != appliedVariantAssetBundleWithVariant) {
					//範囲外アセットバンドル
					if ((options & DownloadAllAssetBundlesFlags.IncludeOutOfVariants) == 0) {
						isAdd = false;
					}
				}
				if (isAdd) {
					if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleWithVariant)) {
						//配信ストリーミングアセット
						if ((options & DownloadAllAssetBundlesFlags.ExcludeDeliveryStreamingAssets) != 0) {
							isAdd = false;
						}
					} else {
						//アセットバンドル
						if ((options & DownloadAllAssetBundlesFlags.ExcludeAssetBundles) != 0) {
							isAdd = false;
						}
					}
				}
				if (isAdd) {
					result.Add(assetBundleWithVariant);
				}
			}

			return result;
		}

		#endregion
	}
}
