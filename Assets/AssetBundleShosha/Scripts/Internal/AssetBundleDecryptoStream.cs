// (C) 2019 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.IO;
	using System.Security.Cryptography;

	public class AssetBundleDecryptoStream : Stream {
		#region Public fields and properties

		/// <summary>
		/// 現在位置
		/// </summary>
		public override long Position {get{
			return m_DecryptoStreamPosition;
		}set{
			if (value < 0) {
				throw new System.IO.IOException();
			}
			if (value < m_DecryptoStreamPosition) {
				ResetDecryptoStream();
			}
			var offset = (int)(value - m_DecryptoStreamPosition);
			ReadDecryptoStream(offset);
		}}

		/// <summary>
		/// ストリーム長
		/// </summary>
		public override long Length {get{
			return m_DecryptoStream.Length;
		}}

		/// <summary>
		/// 書き込み可能確認
		/// </summary>
		public override bool CanWrite {get{
			return m_DecryptoStream.CanWrite;
		}}

		/// <summary>
		/// シーク可能確認
		/// </summary>
		public override bool CanSeek {get{
			return true;
		}}

		/// <summary>
		/// 読み出し可能確認
		/// </summary>
		public override bool CanRead {get{
			return m_DecryptoStream.CanRead;
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// クローズ
		/// </summary>
		public override void Close() {
			m_DecryptoStream.Close();
			m_Source.Close();
		}

		/// <summary>
		/// フラッシュ
		/// </summary>
		public override void Flush() {
			m_DecryptoStream.Flush();
			m_Source.Flush();
		}

		/// <summary>
		/// 読み込み
		/// </summary>
		/// <param name="buffer">読み込み先</param>
		/// <param name="offset">読み込み先のオフセット</param>
		/// <param name="count">読み込みサイズ</param>
		/// <returns></returns>
		public override int Read(byte[] buffer, int offset, int count) {
			var result = m_DecryptoStream.Read(buffer, offset, count);
			m_DecryptoStreamPosition += result;
			return result;
		}

		/// <summary>
		/// シーク
		/// </summary>
		/// <param name="offset">オフセットサイズ</param>
		/// <param name="origin">基準点</param>
		/// <returns>現在位置</returns>
		public override long Seek(long offset, SeekOrigin origin) {
			long position = 0;
			switch (origin) {
			case SeekOrigin.Begin:
				position = offset;
				break;
			case SeekOrigin.Current:
				position = Position + offset;
				break;
			case SeekOrigin.End:
				position = Length + offset;
				break;
			}
			Position = position;
			return position;
		}

		/// <summary>
		/// ストリーム長設定
		/// </summary>
		/// <param name="value">ストリーム長</param>
		public override void SetLength(long value) {
			m_DecryptoStream.SetLength(value);
		}

		/// <summary>
		/// 書き込み
		/// </summary>
		/// <param name="buffer">書き込み元</param>
		/// <param name="offset">書き込み元のオフセット</param>
		/// <param name="count">書き込みサイズ</param>
		public override void Write(byte[] buffer, int offset, int count) {
			m_DecryptoStream.Write(buffer, offset, count);
			m_DecryptoStreamPosition += count;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="source">元データ</param>
		/// <param name="cryptoHash">暗号化ハッシュ</param>
		public AssetBundleDecryptoStream(Stream source, int cryptoHash) {
			m_Source = source;
			m_Rijndael = GetAES128Rijndael();

			var iv = new byte[AssetBundleCrypto.kIVSize];
			source.Read(iv, 0, iv.Length);
			var fileSizeBytes = new byte[sizeof(int)];
			source.Read(fileSizeBytes, 0, fileSizeBytes.Length);
			var key = AssetBundleCrypto.GetCryptoKey(cryptoHash);

			m_CreateDecryptoStream = ()=>{
				var decryptor = m_Rijndael.CreateDecryptor(key, iv);
				m_Source.Position = kCryptoHeaderSize;
				return new CryptoStream(m_Source, decryptor, CryptoStreamMode.Read);
			};

			m_ReadBuffer = new byte[kReadBufferSize];

			ResetDecryptoStream();
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// Disposeパターン
		/// </summary>
		/// <param name="disposing">true:Dispose, false:Finalizer</param>
		protected override void Dispose(bool disposing) {
			m_CreateDecryptoStream = null;
			((System.IDisposable)m_Rijndael).Dispose();
			m_DecryptoStream.Dispose();
			m_Source.Dispose();
			base.Dispose(disposing);
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// 暗号化ヘッダーサイズ
		/// </summary>
		private int kCryptoHeaderSize = sizeof(byte) * AssetBundleCrypto.kIVSize + sizeof(int);

		/// <summary>
		/// 読み込みバッファサイズ
		/// </summary>
		private int kReadBufferSize = 10 * 1024;

		#endregion
		#region Private fields and properties

		/// <summary>
		/// ソース
		/// </summary>
		private Stream m_Source;

		/// <summary>
		/// 復号ストリーム
		/// </summary>
		private Stream m_DecryptoStream;

		/// <summary>
		/// 復号ストリーム現在位置
		/// </summary>
		private long m_DecryptoStreamPosition;

		/// <summary>
		/// 復号ストリーム作成
		/// </summary>
		private System.Func<Stream> m_CreateDecryptoStream;

		/// <summary>
		/// Rijndael
		/// </summary>
		private RijndaelManaged m_Rijndael;

		/// <summary>
		/// 読み込みバッファ
		/// </summary>
		private byte[] m_ReadBuffer;

		#endregion
		#region Private methods

		/// <summary>
		/// 復号ストリームの初期化
		/// </summary>
		private void ResetDecryptoStream() {
			m_DecryptoStream = m_CreateDecryptoStream();
			m_DecryptoStreamPosition = 0;;
		}

		/// <summary>
		/// 復号ストリームの読み込み
		/// </summary>
		/// <param name="count">読み込み料</param>
		private void ReadDecryptoStream(int count) {
			if (0 < count) {
				do {
					var readSize = ((count < m_ReadBuffer.Length)? count: m_ReadBuffer.Length);
					readSize = m_DecryptoStream.Read(m_ReadBuffer, 0, readSize);
					if (readSize == 0) {
						break;
					}
					count -= readSize;
					m_DecryptoStreamPosition += readSize;
				} while (0 < count);
			}
		}

		/// <summary>
		/// AES128設定のRijndael取得
		/// </summary>
		/// <returns>Rijndael</returns>
		private static RijndaelManaged GetAES128Rijndael() {
			var result = new RijndaelManaged();
			result.BlockSize = 128;
			result.KeySize = 128;
			result.Mode = CipherMode.CBC;
			result.Padding = PaddingMode.PKCS7;
			return result;
		}

		#endregion
	}
}
