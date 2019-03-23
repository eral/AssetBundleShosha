// (C) 2019 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.IO;
	using System.Security.Cryptography;

	public class AssetBundleDecryptStream : Stream {
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
			FastForwardDecryptoStream(offset);
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
		public AssetBundleDecryptStream(Stream source, int cryptoHash) {
			m_Source = source;
			m_FastForwardBuffer = new byte[kFastForwardBufferSize];

			var key = AssetBundleCrypto.GetCryptoKey(cryptoHash);
			var iv = new byte[AssetBundleCrypto.kIVSize];
			source.Read(iv, 0, iv.Length);
			source.Read(m_FastForwardBuffer, 0, kFileSizeBytesSize);

			m_CreateDecryptoStream = ()=>{
				m_Source.Position = kCryptoHeaderSize;
				return AssetBundleCrypto.GetDecryptStream(m_Source, key, iv);
			};

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
			m_DecryptoStream.Dispose();
			m_Source.Dispose();
			base.Dispose(disposing);
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// ファイルサイズデータサイズ
		/// </summary>
		private const int kFileSizeBytesSize = sizeof(int);

		/// <summary>
		/// 暗号化ヘッダーサイズ
		/// </summary>
		private const int kCryptoHeaderSize = sizeof(byte) * AssetBundleCrypto.kIVSize + kFileSizeBytesSize;

		/// <summary>
		/// 早送りバッファサイズ
		/// </summary>
		/// <remarks>kFileSizeBytesSize以上必須</remarks>
		private const int kFastForwardBufferSize = 10 * 1024;

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
		/// 早送りバッファ
		/// </summary>
		private byte[] m_FastForwardBuffer;

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
		/// 復号ストリームの早送り
		/// </summary>
		/// <param name="count">早送り量</param>
		private void FastForwardDecryptoStream(int count) {
			if (0 < count) {
				do {
					var readSize = ((count < m_FastForwardBuffer.Length)? count: m_FastForwardBuffer.Length);
					readSize = m_DecryptoStream.Read(m_FastForwardBuffer, 0, readSize);
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
