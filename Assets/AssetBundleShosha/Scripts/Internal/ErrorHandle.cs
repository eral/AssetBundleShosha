// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using UnityEngine;

	public class ErrorHandle : IErrorHandle {
		#region Public fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		public string assetBundleName {get{return m_AssetBundleName;}}

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		public string assetBundleNameWithVariant {get{return m_AssetBundleNameWithVariant;}}

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
		public uint fileSize {get{return m_FileSize;}}

		/// <summary>
		/// エラーコード
		/// </summary>
		public AssetBundleErrorCode errorCode {get{return m_ErrorCode;}}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		public bool isDeliveryStreamingAsset {get{return m_IsDeliveryStreamingAsset;}}

		/// <summary>
		/// 初期化ならば、true を返します。
		/// </summary>
		public bool isInitialize {get{return false;}}

		#endregion
		#region Public methods

		/// <summary>
		/// 無視
		/// </summary>
		public void Ignore() {
			if (m_AssetBundle != null) {
				m_AssetBundle.manager.IgnoreError(m_AssetBundle);
				m_AssetBundle = null;
				System.GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// リトライ
		/// </summary>
		public void Retry() {
			if (m_AssetBundle != null) {
				m_AssetBundle.manager.RetryError(m_AssetBundle);
				m_AssetBundle = null;
				System.GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="assetBundle">アセットバンドル</param>
		/// <param name="url">URL</param>
		public ErrorHandle(AssetBundleBase assetBundle, string url) {
			m_AssetBundle = assetBundle;
			m_AssetBundleName = assetBundle.name;
			m_AssetBundleNameWithVariant = assetBundle.nameWithVariant;
			m_URL = url;
			m_FileName = null;
			m_FileSize = m_AssetBundle.fileSize;
			m_ErrorCode = m_AssetBundle.errorCode;
			m_IsDeliveryStreamingAsset = m_AssetBundle.isDeliveryStreamingAsset;
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// アセットバンドル
		/// </summary>
		[SerializeField]
		private AssetBundleBase m_AssetBundle;

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
		/// ファイルサイズ
		/// </summary>
		[SerializeField]
		private uint m_FileSize;

		/// <summary>
		/// エラーコード
		/// </summary>
		[SerializeField]
		private AssetBundleErrorCode m_ErrorCode;

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		/// <remarks>無視・リトライ後でも値が取得出来る様にm_AssetBundleNameへ委譲せずに独自に持つ</remarks>
		[SerializeField]
		public bool m_IsDeliveryStreamingAsset;

		#endregion
		#region Private methods

		/// <summary>
		/// ファイナライザー
		/// </summary>
		~ErrorHandle() {
			if (m_AssetBundle != null) {
				m_AssetBundle.manager.IgnoreError(m_AssetBundle);
				m_AssetBundle = null;
			}
		}

		#endregion
	}
}
