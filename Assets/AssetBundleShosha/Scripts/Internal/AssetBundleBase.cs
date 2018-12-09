// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;

	public abstract class AssetBundleBase : IAssetBundle {
		#region Public fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		public string name {get{return m_Name;}}

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		public string nameWithVariant {get{return m_NameWithVariant;}}

		/// <summary>
		/// 進捗(0.0f～1.0f)
		/// </summary>
		public abstract float progress {get;}

		/// <summary>
		/// 終了確認
		/// </summary>
		public bool isDone {get{return state == State.Done;}}

		/// <summary>
		/// エラーコード
		/// </summary>
		public abstract AssetBundleErrorCode errorCode {get;}

		/// <summary>
		/// エラーハンドラー
		/// </summary>
		public IErrorHandler errorHandler {get{return m_ErrorHandler;} set{m_ErrorHandler = value;}}

		/// <summary>
		/// アセットバンドルをビルドするときに、必ず使うアセットを設定します（読み取り専用）
		/// </summary>
		public abstract Object mainAsset {get;}

		/// <summary>
		/// アセットバンドルがストリーミングされたシーンのアセットバンドルならば、true を返します。
		/// </summary>
		public abstract bool isStreamedSceneAssetBundle {get;}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		public abstract bool isDeliveryStreamingAsset {get;}

		/// <summary>
		/// 配信ストリーミングアセットのパスを返します。
		/// </summary>
		public abstract string deliveryStreamingAssetPath {get;}

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public object Current {get {return null;}}

		#endregion
		#region Public methods

		/// <summary>
		/// 特定のオブジェクトがアセットバンドルに含まれているか確認します。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>true:含まれる、false:含まれない</returns>
		public abstract bool Contains(string name);

		/// <summary>
		/// アセットバンドルにあるすべてのアセット名を返します。
		/// </summary>
		/// <returns>すべてのアセット名</returns>
		public abstract string[] GetAllAssetNames();

		/// <summary>
		/// アセットバンドルにあるすべてのシーンアセットのパス( *.unity アセットへのパス)を返します。
		/// </summary>
		/// <returns>すべてのシーンアセットのパス</returns>
		public abstract string[] GetAllScenePaths();

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するすべてのアセット</returns>
		public abstract T[] LoadAllAssets<T>() where T : Object;
		public abstract Object[] LoadAllAssets(System.Type type);
		public abstract Object[] LoadAllAssets();

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public abstract IAssetBundleRequest LoadAllAssetsAsync<T>();
		public abstract IAssetBundleRequest LoadAllAssetsAsync(System.Type type);
		public abstract IAssetBundleRequest LoadAllAssetsAsync();

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public abstract T LoadAsset<T>(string name) where T : Object;
		public abstract Object LoadAsset(string name, System.Type type);
		public abstract Object LoadAsset(string name);

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public abstract IAssetBundleRequest LoadAssetAsync<T>(string name);
		public abstract IAssetBundleRequest LoadAssetAsync(string name, System.Type type);
		public abstract IAssetBundleRequest LoadAssetAsync(string name);

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public abstract T[] LoadAssetWithSubAssets<T>(string name) where T : Object;
		public abstract Object[] LoadAssetWithSubAssets(string name, System.Type type);
		public abstract Object[] LoadAssetWithSubAssets(string name);

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public abstract IAssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name);
		public abstract IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name, System.Type type);
		public abstract IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name);

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public bool MoveNext() {
			return !isDone;
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
		public void Dispose() {
			m_Manager.UnloadAssetBundle(this);
		}

		#endregion
		#region Internal types

		/// <summary>
		/// 状態
		/// </summary>
		internal enum State {
			Wait,				//処理前
			OnlineProcessing,	//オンラインプロセス中
			OnlineProcessed,	//オンラインプロセス完了
			OfflineProcessing,	//オフラインプロセス中
			OfflineProcessed,	//オフラインプロセス完了
			ErrorThinking,		//エラー処理選択中
			ErrorIgnore,		//エラー無視
			ErrorRetry,			//エラーリトライ
			Done,				//処理完了
		}

		#endregion
		#region Internal fields and properties

		/// <summary>
		/// 管轄のアセットバンドルマネージャー
		/// </summary>
		internal AssetBundleManager manager {get{return m_Manager;}}

		/// <summary>
		/// 状態
		/// </summary>
		internal State state {get{return m_State;} set{m_State = value;}}

		#endregion
		#region Protected methods

		/// <summary>
		/// 構築後イベント
		/// </summary>
		protected virtual void OnCreated() {
			//empty.
		}

		/// <summary>
		/// オンラインプロセス開始イベント
		/// </summary>
		/// <returns>コルーチン</returns>
		protected virtual System.Collections.IEnumerator OnStartedOnlineProcess() {
			m_State = State.OnlineProcessed;
			yield break;
		}

		/// <summary>
		/// オフラインプロセス開始イベント
		/// </summary>
		/// <returns>コルーチン</returns>
		protected virtual System.Collections.IEnumerator OnStartedOfflineProcess() {
			m_State = State.OfflineProcessed;
			yield break;
		}

		/// <summary>
		/// ダウンロード終了イベント
		/// </summary>
		/// <remarks>関数を抜ける前にisDoneプロパティはtrueにならなければならない</remarks>
		protected virtual void OnDownloadFinished() {
			//empty.
		}

		/// <summary>
		/// 破棄イベント
		/// </summary>
		protected virtual void OnDestroy() {
			//empty.
		}

		#endregion
		#region Internal fields and properties

		/// <summary>
		/// ファイルサイズ
		/// </summary>
		internal uint fileSize {get{return m_FileSize;}}

		/// <summary>
		/// 参照カウンター
		/// </summary>
		internal int referenceCount {get{return m_ReferenceCount;} set{m_ReferenceCount = value;}}

		#endregion
		#region Internal methods

		/// <summary>
		/// 構築
		/// </summary>
		/// <param name="manager">管轄のアセットバンドルマネージャー</param>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <param name="fileSize">ファイルサイズ</param>
		internal void Create(AssetBundleManager manager, string assetBundleName, string assetBundleNameWithVariant, uint fileSize) {
			m_Manager = manager;
			m_Name = assetBundleName;
			m_NameWithVariant = assetBundleNameWithVariant;
			m_FileSize = fileSize;
			m_State = State.Wait;
			m_ReferenceCount = 1;

			OnCreated();
		}

		/// <summary>
		/// 破棄
		/// </summary>
		internal void Destroy() {
			if (m_ProcessCoroutine != null) {
				manager.StopCoroutine(m_ProcessCoroutine);
				m_ProcessCoroutine = null;
			}
			OnDestroy();
		}

		/// <summary>
		/// ダウンロード完了イベント登録
		/// </summary>
		/// <param name="action">アクション</param>
		internal void AddDownloadFinishedListener(UnityAction<AssetBundleBase> action) {
			if (m_DownloadFinishedEvent == null) {
				m_DownloadFinishedEvent = new DownloadFinishedEvent();
			}
			m_DownloadFinishedEvent.AddListener(action);
		}

		/// <summary>
		/// オンラインプロセス開始
		/// </summary>
		internal void StartOnlineProcess() {
			m_State = State.OnlineProcessing;
			m_ProcessCoroutine = manager.StartCoroutine(OnlineProcessing()); //TODO:コルーチンが即時に終了した場合m_DownloadCoroutineはnullが望ましいが、null以外の値が設定されてしまう様に思うので要調査
		}

		/// <summary>
		/// オフラインプロセス開始
		/// </summary>
		internal void StartOfflineProcess() {
			m_State = State.OfflineProcessing;
			m_ProcessCoroutine = manager.StartCoroutine(OfflineProcessing()); //TODO:コルーチンが即時に終了した場合m_DownloadCoroutineはnullが望ましいが、null以外の値が設定されてしまう様に思うので要調査
		}

		/// <summary>
		/// 処理完了
		/// </summary>
		internal void ProcessFinished() {
			OnDownloadFinished();

			m_State = State.Done;
			if (m_DownloadFinishedEvent != null) {
				m_DownloadFinishedEvent.Invoke(this);
				m_DownloadFinishedEvent = null;
			}
		}

		/// <summary>
		/// ファイナライザー内Dispose
		/// </summary>
		internal void DisposeInFinalize() {
			m_Manager.UnloadAssetBundleInFinalize(this);
		}

		#endregion
		#region Private types

		/// <summary>
		/// ダウンロード完了イベント
		/// </summary>
		private class DownloadFinishedEvent : UnityEvent<AssetBundleBase> {
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 管轄のアセットバンドルマネージャー
		/// </summary>
		[SerializeField]
		private AssetBundleManager m_Manager;

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		[SerializeField]
		private string m_Name;

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		[SerializeField]
		private string m_NameWithVariant;

		/// <summary>
		/// 状態
		/// </summary>
		[SerializeField]
		private State m_State = State.Wait;

		/// <summary>
		/// ダウンロード完了イベント
		/// </summary>
		[SerializeField]
		private DownloadFinishedEvent m_DownloadFinishedEvent;

		/// <summary>
		/// プロセスコルーチン
		/// </summary>
		private Coroutine m_ProcessCoroutine;

		/// <summary>
		/// ファイルサイズ
		/// </summary>
		private uint m_FileSize = 0;

		/// <summary>
		/// エラーハンドラー
		/// </summary>
		[SerializeField]
		private IErrorHandler m_ErrorHandler = null;

		/// <summary>
		/// 参照カウンター
		/// </summary>
		private int m_ReferenceCount = 0;

		#endregion
		#region Private methods

		/// <summary>
		/// オンラインプロセス中
		/// </summary>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> OnlineProcessing() {
			yield return OnStartedOnlineProcess();
			m_ProcessCoroutine = null;
		}

		/// <summary>
		/// オフラインプロセス中
		/// </summary>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> OfflineProcessing() {
			yield return OnStartedOfflineProcess();
			m_ProcessCoroutine = null;
		}

		#endregion
	}
}
