// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using UnityEngine.Networking;

	public class DownloadHandlerDeliveryStreamingAsset : DownloadHandlerScript {
		#region Public fields and properties

		/// <summary>
		/// 配信ストリーミングアセットのパス
		/// </summary>
		public string fullPath {get{return m_FullPath;}}

		/// <summary>
		/// ハッシュ
		/// </summary>
		public Hash128 hash {get{return m_Hash;}}

		/// <summary>
		/// CRC
		/// </summary>
		public uint crc {get{return m_Crc;}}


		#endregion
		#region Public methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="fullPath">保存先</param>
		/// <param name="hash">ハッシュ</param>
		/// <param name="crc">CRC</param>
		/// <param name="fileSize">ファイルサイズ</param>
		public DownloadHandlerDeliveryStreamingAsset(string fullPath, Hash128 hash, uint crc) : base(new byte[kPreallocatedBufferLength]) {
			m_FullPath = fullPath;
			m_Hash = hash;
			m_Crc = crc;
		}
		public DownloadHandlerDeliveryStreamingAsset(string fullPath, Hash128 hash, uint crc, int fileSize) : this(fullPath, hash, crc) {
			m_ContentLength = fileSize;
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// ダウンロード完了時イベント
		/// </summary>
		protected override void CompleteContent() {
			if (m_file != null) {
				m_file.Close();
			}
		}

		/// <summary>
		/// 進捗取得
		/// </summary>
		protected override float GetProgress() {
			var result = 0.5f;
			if (0 < m_ContentLength) {
				result = m_ReceivedLength / m_ContentLength;
			}
			return result;
		}

		/// <summary>
		/// コンテンツサイズ受信イベント
		/// </summary>
		/// <param name="contentLength">コンテンツサイズ</param>
		protected override void ReceiveContentLength(int contentLength) {
			m_ContentLength = contentLength;
		}

		/// <summary>
		/// データ受信イベント
		/// </summary>
		/// <param name="data">データ</param>
		/// <param name="dataLength">データサイズ</param>
		/// <returns></returns>
		protected override bool ReceiveData(byte[] data, int dataLength) {
			var success = true;
			try {
				if (m_file == null) {
					AssetBundleUtility.CreateDirectory(m_FullPath, true);
					m_file = File.Create(m_FullPath);
				}
				m_file.Write(data, 0, dataLength);
			} catch {
				success = false;
			}
			m_ReceivedLength += dataLength;
			return success;
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields

		/// <summary>
		/// 作業バッファサイズ
		/// </summary>
		private const int kPreallocatedBufferLength = 10 * 1024;

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 配信ストリーミングアセットパス
		/// </summary>
		private string m_FullPath;

		/// <summary>
		/// ハッシュ
		/// </summary>
		private Hash128 m_Hash;

		/// <summary>
		/// CRC
		/// </summary>
		private uint m_Crc;

		/// <summary>
		/// コンテンツサイズ
		/// </summary>
		private int m_ContentLength;

		/// <summary>
		/// 受信済みサイズ
		/// </summary>
		private int m_ReceivedLength;

		/// <summary>
		/// ファイル
		/// </summary>
		private FileStream m_file;

		#endregion
		#region Private methods
		#endregion
	}
}
