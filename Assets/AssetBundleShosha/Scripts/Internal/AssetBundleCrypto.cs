// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	using System.Reflection;
	using System.Security.Cryptography;

	public static class AssetBundleCrypto {
		#region Public types
		#endregion
		#region Public const fields
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

		#endregion
		#region Internal const fields

		/// <summary>
		/// IVサイズ
		/// </summary>
		internal const int kIVSize = 16;

		#endregion
		#region Internal methods

		/// <summary>
		/// 暗号化キー取得
		/// </summary>
		/// <param name="cryptoHash">暗号化ハッシュ</param>
		/// <returns>暗号化キー</returns>
		internal static byte[] GetCryptoKey(int cryptoHash) {
			var result = cryptoKeys[cryptoHash]();
			return result;
		}

		/// <summary>
		/// 先頭の暗号化キー取得
		/// </summary>
		/// <returns>暗号化キー</returns>
		internal static int GetFirstCryptoHash() {
			var result = cryptoKeys.First().Key;
			return result;
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// 暗号化キー辞書
		/// </summary>
		private static Dictionary<int, System.Func<byte[]>> cryptoKeys {get{if (s_CryptoKeys == null) {s_CryptoKeys = GetCryptoKeys();} return s_CryptoKeys;}}
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
