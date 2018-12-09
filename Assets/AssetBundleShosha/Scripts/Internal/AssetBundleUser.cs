// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using UnityEngine;

	public class AssetBundleUser : IAssetBundle {
		#region Public fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		public string name {get{return referenceCountingInstance.name;}}

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		public string nameWithVariant {get{return referenceCountingInstance.nameWithVariant;}}

		/// <summary>
		/// 終了確認
		/// </summary>
		public bool isDone {get{return referenceCountingInstance.isDone;}}

		/// <summary>
		/// エラーコード
		/// </summary>
		public AssetBundleErrorCode errorCode {get{return referenceCountingInstance.errorCode;}}

		/// <summary>
		/// エラーハンドラー
		/// </summary>
		public IErrorHandler errorHandler {get{return referenceCountingInstance.errorHandler;} set{referenceCountingInstance.errorHandler = value;}}

		/// <summary>
		/// アセットバンドルをビルドするときに、必ず使うアセットを設定します（読み取り専用）
		/// </summary>
		public Object mainAsset {get{return referenceCountingInstance.mainAsset;}}

		/// <summary>
		/// アセットバンドルがストリーミングされたシーンのアセットバンドルならば、true を返します。
		/// </summary>
		public bool isStreamedSceneAssetBundle {get{return referenceCountingInstance.isStreamedSceneAssetBundle;}}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		public bool isDeliveryStreamingAsset {get{return referenceCountingInstance.isDeliveryStreamingAsset;}}

		/// <summary>
		/// 配信ストリーミングアセットのパスを返します。
		/// </summary>
		public string deliveryStreamingAssetPath {get{return referenceCountingInstance.deliveryStreamingAssetPath;}}

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public object Current {get {return referenceCountingInstance.Current;}}

		#endregion
		#region Public methods

		/// <summary>
		/// 特定のオブジェクトがアセットバンドルに含まれているか確認します。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>true:含まれる、false:含まれない</returns>
		public bool Contains(string name) {
			return referenceCountingInstance.Contains(name);
		}

		/// <summary>
		/// アセットバンドルにあるすべてのアセット名を返します。
		/// </summary>
		/// <returns>すべてのアセット名</returns>
		public string[] GetAllAssetNames() {
			return referenceCountingInstance.GetAllAssetNames();
		}

		/// <summary>
		/// アセットバンドルにあるすべてのシーンアセットのパス( *.unity アセットへのパス)を返します。
		/// </summary>
		/// <returns>すべてのシーンアセットのパス</returns>
		public string[] GetAllScenePaths() {
			return referenceCountingInstance.GetAllScenePaths();
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するすべてのアセット</returns>
		public T[] LoadAllAssets<T>() where T : Object {
			return referenceCountingInstance.LoadAllAssets<T>();
		}
		public Object[] LoadAllAssets(System.Type type) {
			return referenceCountingInstance.LoadAllAssets(type);
		}
		public Object[] LoadAllAssets() {
			return referenceCountingInstance.LoadAllAssets();
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public IAssetBundleRequest LoadAllAssetsAsync<T>() {
			return referenceCountingInstance.LoadAllAssetsAsync<T>();
		}
		public IAssetBundleRequest LoadAllAssetsAsync(System.Type type) {
			return referenceCountingInstance.LoadAllAssetsAsync(type);
		}
		public IAssetBundleRequest LoadAllAssetsAsync() {
			return referenceCountingInstance.LoadAllAssetsAsync();
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public T LoadAsset<T>(string name) where T : Object {
			return referenceCountingInstance.LoadAsset<T>(name);
		}
		public Object LoadAsset(string name, System.Type type) {
			return referenceCountingInstance.LoadAsset(name, type);
		}
		public Object LoadAsset(string name) {
			return referenceCountingInstance.LoadAsset(name);
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public IAssetBundleRequest LoadAssetAsync<T>(string name) {
			return referenceCountingInstance.LoadAssetAsync<T>(name);
		}
		public IAssetBundleRequest LoadAssetAsync(string name, System.Type type) {
			return referenceCountingInstance.LoadAssetAsync(name, type);
		}
		public IAssetBundleRequest LoadAssetAsync(string name) {
			return referenceCountingInstance.LoadAssetAsync(name);
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public T[] LoadAssetWithSubAssets<T>(string name) where T : Object {
			return referenceCountingInstance.LoadAssetWithSubAssets<T>(name);
		}
		public Object[] LoadAssetWithSubAssets(string name, System.Type type) {
			return referenceCountingInstance.LoadAssetWithSubAssets(name, type);
		}
		public Object[] LoadAssetWithSubAssets(string name) {
			return referenceCountingInstance.LoadAssetWithSubAssets(name);
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public IAssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name) {
			return referenceCountingInstance.LoadAssetWithSubAssetsAsync<T>(name);
		}
		public IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name, System.Type type) {
			return referenceCountingInstance.LoadAssetWithSubAssetsAsync(name, type);
		}
		public IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name) {
			return referenceCountingInstance.LoadAssetWithSubAssetsAsync(name);
		}

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public bool MoveNext() {
			return referenceCountingInstance.MoveNext();
		}

		/// <summary>
		/// IEnumeratorインターフェース
		/// </summary>
		public void Reset() {
			referenceCountingInstance.Reset();
		}

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public void Dispose() {
			Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// Disposeパターン
		/// </summary>
		/// <param name="disposing">true:Dispose, false:Finalizer</param>
		protected virtual void Dispose(bool disposing) {
			if (m_ReferenceCountingInstance == null) {
				return;
			}

			if (disposing) {
				//Dispose
				m_ReferenceCountingInstance.Dispose();
			} else {
				//Finalizer
				m_ReferenceCountingInstance.DisposeInFinalize();
			}
			m_ReferenceCountingInstance = null;
		}

		#endregion
		#region Internal fields and properties

		/// <summary>
		/// 参照カウントインスタンス
		/// </summary>
		internal AssetBundleBase referenceCountingInstanceInternal {get{return m_ReferenceCountingInstance;} set{m_ReferenceCountingInstance = value;}}

		#endregion
		#region Internal methods

		/// <summary>
		/// コンストラタク
		/// </summary>
		internal AssetBundleUser() {
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 参照カウントインスタンス
		/// </summary>
		[SerializeField]
		private AssetBundleBase m_ReferenceCountingInstance;

		/// <summary>
		/// 参照カウントインスタンス(Disposed確認あり)
		/// </summary>
		private IAssetBundle referenceCountingInstance {get{
			if (m_ReferenceCountingInstance == null) {
				throw new System.ObjectDisposedException("AssetBundleUser");
			}
			return m_ReferenceCountingInstance;
		}}

		#endregion
		#region Private methods

		/// <summary>
		/// ファイナライザー
		/// </summary>
		~AssetBundleUser() {
			Dispose(false);
		}

		#endregion
	}
}
