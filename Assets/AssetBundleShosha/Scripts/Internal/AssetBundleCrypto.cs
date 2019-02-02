// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	using System.Reflection;
	using System.Security.Cryptography;

	public class AssetBundleCrypto : System.IDisposable {
		#region Public types
		#endregion
		#region Public const fields

		/// <summary>
		/// IVサイズ
		/// </summary>
		public const int kIVSize = 16;

		#endregion
		#region Public fields and properties
		#endregion
		#region Public methods

		/// <summary>
		/// 暗号化確認
		/// </summary>
		/// <param name="catalog">カタログ</param>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:暗号化, false:平文</returns>
		public static bool IsCrypto(AssetBundleCatalog catalog, string assetBundleNameWithVariant) {
			var hash = GetCryptoHash(catalog, assetBundleNameWithVariant);
			return 0 != hash;
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="catalog">カタログ</param>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		public static int GetCryptoHash(AssetBundleCatalog catalog, string assetBundleNameWithVariant) {
			var result = catalog.GetAssetBundleCryptoHash(assetBundleNameWithVariant);
			return result;
		}

		/// <summary>
		/// 復号
		/// </summary>
		/// <param name="source">元データ</param>
		/// <param name="cryptoHash">暗号化ハッシュ</param>
		/// <returns>復号データ</returns>
		/// <remarks>スレッドセーフ</remarks>
		public byte[] Decrypt(byte[] source, int cryptoHash) {
			byte[] result = null;
			using (var sourceStream = new MemoryStream(source, false)) {
				var iv = new byte[kIVSize];
				sourceStream.Read(iv, 0, iv.Length);
				var fileSizeBytes = new byte[sizeof(int)];
				sourceStream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
				var fileSize = System.BitConverter.ToInt32(fileSizeBytes, 0);
				using (var destStream = new MemoryStream(fileSize)) {
					var key = cryptoKeys[cryptoHash]();
					var decryptor = rijndael.CreateDecryptor(key, iv);
					var buffer = new byte[kCryptoBufferSize];
					using (CryptoStream cryptoStream = new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read)) {
						while (true) {
							var readed = cryptoStream.Read(buffer, 0, buffer.Length);
							if (0 == readed) {
								break;
							}
							destStream.Write(buffer, 0, readed);
						}
					}

					result = destStream.GetBuffer();
				}
			}
			return result;
		}

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public void Dispose() {
			((System.IDisposable)m_Rijndael).Dispose();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public AssetBundleCrypto() {
			m_Rijndael = new RijndaelManaged();
			m_Rijndael.BlockSize = 128;
			m_Rijndael.KeySize = 128;
			m_Rijndael.Mode = CipherMode.CBC;
			m_Rijndael.Padding = PaddingMode.PKCS7;
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields

		/// <summary>
		/// Rijndael計算機
		/// </summary>
#if UNITY_EDITOR
		internal 
#else
		private 
#endif
		RijndaelManaged rijndael {get{return m_Rijndael;}}
		private RijndaelManaged m_Rijndael = null;

		/// <summary>
		/// 暗号化バッファサイズ
		/// </summary>
		private const int kCryptoBufferSize = 16 * 1024;

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 暗号化キー辞書
		/// </summary>
#if UNITY_EDITOR
		internal 
#else
		private 
#endif
		static Dictionary<int, System.Func<byte[]>> cryptoKeys {get{if (s_CryptoKeys == null) {s_CryptoKeys = GetCryptoKeys();} return s_CryptoKeys;}}
		private static Dictionary<int, System.Func<byte[]>> s_CryptoKeys = null;

		#endregion
		#region Private methods

		/// <summary>
		/// 暗号化キー取得
		/// </summary>
		/// <returns>暗号化キー</returns>
		private static Dictionary<int, System.Func<byte[]>> GetCryptoKeys() {
			const BindingFlags kCryptoKeyMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var result = System.AppDomain.CurrentDomain
										.GetAssemblies()
										.SelectMany(x=>x.GetTypes())
										.SelectMany(x=>x.GetProperties(kCryptoKeyMethodBindingFlags))
										.SelectMany(x=>System.Attribute.GetCustomAttributes(x, typeof(AssetBundleCryptoKeyAttribute))
																		.Select(y=>new{property = x, attribute = y as AssetBundleCryptoKeyAttribute}))
										.ToDictionary(x=>x.attribute.hash
													,x=>(System.Func<byte[]>)(()=>x.property.GetValue(null, kCryptoKeyMethodBindingFlags, null, null, null) as byte[]));
			return result;
		}

		#endregion
	}
}
