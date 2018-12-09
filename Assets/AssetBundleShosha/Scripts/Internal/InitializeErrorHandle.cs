// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;

	public class InitializeErrorHandle : IErrorHandle {
		#region Public fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		public string assetBundleName {get{return m_AssetBundleName;}}

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		public string assetBundleNameWithVariant {get{
			if (string.IsNullOrEmpty(m_AssetBundleNameWithVariant)) {
				m_AssetBundleNameWithVariant = m_AssetBundleName + "." + AssetBundleUtility.GetPlatformString();
			}
			return m_AssetBundleNameWithVariant;
		}}

		/// <summary>
		/// URL
		/// </summary>
		public string url {get{return m_URL;}}

		/// <summary>
		/// ファイル名
		/// </summary>
		public string fileName {get{
			if (string.IsNullOrEmpty(m_FileName)) {
				m_FileName = System.IO.Path.GetFileName(m_URL);
			}
			return m_FileName;
		}}

		/// <summary>
		/// ファイルサイズ
		/// </summary>
		public uint fileSize {get{return 0;}}

		/// <summary>
		/// エラーコード
		/// </summary>
		public AssetBundleErrorCode errorCode {get{return m_ErrorCode;}}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		public bool isDeliveryStreamingAsset {get{return false;}}

		/// <summary>
		/// 初期化ならば、true を返します。
		/// </summary>
		public bool isInitialize {get{return true;}}

		#endregion
		#region Public methods

		/// <summary>
		/// 無視
		/// </summary>
		public void Ignore() {
			if (m_IgnoreEvent != null) {
				m_IgnoreEvent();
				m_IgnoreEvent = null;
				m_RetryEvent = null;
				System.GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// リトライ
		/// </summary>
		public void Retry() {
			if (m_RetryEvent != null) {
				m_RetryEvent();
				m_RetryEvent = null;
				m_IgnoreEvent = null;
				System.GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// アセットバンドル用作成
		/// </summary>
		/// <param name="errorCode">エラーコード</param>
		/// <param name="url">URL</param>
		/// <param name="ignoreEvent">無視イベント</param>
		/// <param name="retryEvent">リトライイベント</param>
		/// <returns>インスタンス</returns>
		public static InitializeErrorHandle CreateAssetBundle(AssetBundleErrorCode errorCode, string url, UnityAction ignoreEvent, UnityAction retryEvent) {
			return new InitializeErrorHandle(kAssetBundleNameForAssetBundle, errorCode, url, ignoreEvent, retryEvent);
		}

		/// <summary>
		/// アセット用作成
		/// </summary>
		/// <param name="errorCode">エラーコード</param>
		/// <param name="url">URL</param>
		/// <param name="ignoreEvent">無視イベント</param>
		/// <param name="retryEvent">リトライイベント</param>
		/// <returns>インスタンス</returns>
		public static InitializeErrorHandle CreateAsset(AssetBundleErrorCode errorCode, string url, UnityAction ignoreEvent, UnityAction retryEvent) {
			return new InitializeErrorHandle(kAssetBundleNameForAsset, errorCode, url, ignoreEvent, retryEvent);
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// アセットバンドルエラー用のアセットバンドル名
		/// </summary>
		private const string kAssetBundleNameForAssetBundle = "initialize:assetbundle";

		/// <summary>
		/// アセットエラー用のアセットバンドル名
		/// </summary>
		private const string kAssetBundleNameForAsset = "initialize:asset";

		#endregion
		#region Private fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		[SerializeField]
		private string m_AssetBundleName;

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		[SerializeField]
		private string m_AssetBundleNameWithVariant;

		/// <summary>
		/// URL
		/// </summary>
		[SerializeField]
		private string m_URL;

		/// <summary>
		/// ファイル名
		/// </summary>
		[SerializeField]
		private string m_FileName;

		/// <summary>
		/// エラーコード
		/// </summary>
		[SerializeField]
		private AssetBundleErrorCode m_ErrorCode;

		/// <summary>
		/// 無視イベント
		/// </summary>
		[SerializeField]
		private UnityAction m_IgnoreEvent;

		/// <summary>
		/// リトライイベント
		/// </summary>
		[SerializeField]
		private UnityAction m_RetryEvent;

		#endregion
		#region Private methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <param name="errorCode">エラーコード</param>
		/// <param name="url">URL</param>
		/// <param name="ignoreEvent">無視イベント</param>
		/// <param name="retryEvent">リトライイベント</param>
		private InitializeErrorHandle(string assetBundleName, AssetBundleErrorCode errorCode, string url, UnityAction ignoreEvent, UnityAction retryEvent) {
			m_AssetBundleName = assetBundleName;
			m_AssetBundleNameWithVariant = null;
			m_URL = url;
			m_FileName = null;
			m_ErrorCode = errorCode;
			m_IgnoreEvent = ignoreEvent;
			m_RetryEvent = retryEvent;
		}

		/// <summary>
		/// ファイナライザー
		/// </summary>
		~InitializeErrorHandle() {
			if (m_IgnoreEvent != null) {
				m_IgnoreEvent();
				m_IgnoreEvent = null;
				m_RetryEvent = null;
			}
		}

		#endregion
	}
}
