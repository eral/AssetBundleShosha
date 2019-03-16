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
			return m_Decrypto.Position;
		}set{
			m_Decrypto.Position = value;
		}}

		/// <summary>
		/// ストリーム長
		/// </summary>
		public override long Length {get{
			return m_Decrypto.Length;
		}}

		/// <summary>
		/// 書き込み可能確認
		/// </summary>
		public override bool CanWrite {get{
			return m_Decrypto.CanWrite;
		}}

		/// <summary>
		/// シーク可能確認
		/// </summary>
		public override bool CanSeek {get{
			return m_Decrypto.CanSeek;
		}}

		/// <summary>
		/// 読み出し可能確認
		/// </summary>
		public override bool CanRead {get{
			return m_Decrypto.CanRead;
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// クローズ
		/// </summary>
		public override void Close() {
			m_Decrypto.Close();
		}

		/// <summary>
		/// フラッシュ
		/// </summary>
		public override void Flush() {
			m_Decrypto.Flush();
		}

		/// <summary>
		/// 読み込み
		/// </summary>
		/// <param name="buffer">読み込み先</param>
		/// <param name="offset">読み込み先のオフセット</param>
		/// <param name="count">読み込みサイズ</param>
		/// <returns></returns>
		public override int Read(byte[] buffer, int offset, int count) {
			return m_Decrypto.Read(buffer, offset, count);
		}

		/// <summary>
		/// シーク
		/// </summary>
		/// <param name="offset">オフセットサイズ</param>
		/// <param name="origin">基準点</param>
		/// <returns>現在位置</returns>
		public override long Seek(long offset, SeekOrigin origin) {
			return m_Decrypto.Seek(offset, origin);
		}

		/// <summary>
		/// ストリーム長設定
		/// </summary>
		/// <param name="value">ストリーム長</param>
		public override void SetLength(long value) {
			m_Decrypto.SetLength(value);
		}

		/// <summary>
		/// 書き込み
		/// </summary>
		/// <param name="buffer">書き込み元</param>
		/// <param name="offset">書き込み元のオフセット</param>
		/// <param name="count">書き込みサイズ</param>
		public override void Write(byte[] buffer, int offset, int count) {
			m_Decrypto.Write(buffer, offset, count);
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="source">元データ</param>
		/// <param name="cryptoHash">暗号化ハッシュ</param>
		public AssetBundleDecryptoStream(Stream source, int cryptoHash) {
			var iv = new byte[AssetBundleCrypto.kIVSize];
			source.Read(iv, 0, iv.Length);
			var fileSizeBytes = new byte[sizeof(int)];
			source.Read(fileSizeBytes, 0, fileSizeBytes.Length);
			var fileSize = System.BitConverter.ToInt32(fileSizeBytes, 0);
			using (var destStream = new MemoryStream(fileSize)) {
				var rijndael = GetAES128Rijndael();
				const int kCryptoBufferSize = 16 * 1024;

				var key = AssetBundleCrypto.GetCryptoKey(cryptoHash);
				var decryptor = rijndael.CreateDecryptor(key, iv);
				var buffer = new byte[kCryptoBufferSize];
				using (var cryptoStream = new CryptoStream(source, decryptor, CryptoStreamMode.Read)) {
					while (true) {
						var readed = cryptoStream.Read(buffer, 0, buffer.Length);
						if (0 == readed) {
							break;
						}
						destStream.Write(buffer, 0, readed);
					}
				}

				m_Decrypto = new MemoryStream(destStream.GetBuffer(), false);
			}
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// Disposeパターン
		/// </summary>
		/// <param name="disposing">true:Dispose, false:Finalizer</param>
		protected override void Dispose(bool disposing) {
			m_Decrypto.Dispose();
			base.Dispose(disposing);
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// ソース
		/// </summary>
		private Stream m_Decrypto;

		/// <summary>
		/// 暗号化ハッシュ
		/// </summary>
		private int m_CryptoHash;

		#endregion
		#region Private methods

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
