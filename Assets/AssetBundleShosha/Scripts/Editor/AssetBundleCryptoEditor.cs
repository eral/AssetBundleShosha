// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor {
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.IO;
	using System.Security.Cryptography;
	using UnityEngine;
	using UnityEditor;
	using AssetBundleShosha.Internal;
	using HashAlgorithm = AssetBundleShosha.Internal.HashAlgorithm;
	using AssetBundleShosha.Editor.Internal;

	public class AssetBundleCryptoEditor : AssetBundleCrypto {
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
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:暗号化, false:平文</returns>
		public bool IsCrypto(string assetBundleNameWithVariant) {
			var result = cryptoAssetBundleNamesWithVariant.ContainsKey(assetBundleNameWithVariant);
			if (!result && !AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleNameWithVariant)) {
				//アセットバンドル
				result = AssetBundleEditorUtility.buildOptionForceCrypto;
			}
			return result;
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		public int GetCryptoHash(string assetBundleNameWithVariant) {
			var result = 0;
			if (IsCrypto(assetBundleNameWithVariant)) {
				result = cryptoKeys.First().Key;
			}
			return result;
		}

		/// <summary>
		/// 暗号化
		/// </summary>
		/// <param name="source">元ファイル</param>
		/// <param name="dest">暗号化ファイル(元ファイルと同一は不可)</param>
		/// <param name="cryptoHash">暗号化ハッシュ</param>
		/// <param name="isNonDeterministic">非決定性暗号化</param>
		/// <remarks>非スレッドセーフ</remarks>
		public void Encrypt(string source, string dest, int cryptoHash, bool isNonDeterministic) {
			var sourceFullPath = Application.dataPath + "/../" + source;
			var destFullPath = Application.dataPath + "/../" + dest;

			byte[] iv;
			if (isNonDeterministic) {
				//非決定性暗号化
				iv = CreateRandomIV();
			} else {
				//決定性暗号化
				iv = hashAlgorithm.GetHashFromFile(sourceFullPath).Take(kIVSize).ToArray();
			}

			using (var destStream = File.Open(destFullPath, FileMode.Create, FileAccess.Write)) {
				destStream.Write(iv, 0, iv.Length);
				using (var sourceStream = File.Open(sourceFullPath, FileMode.Open, FileAccess.Read)) {
					var fileSizeBytes = System.BitConverter.GetBytes((int)sourceStream.Length);
					destStream.Write(fileSizeBytes, 0, fileSizeBytes.Length);

					var key = cryptoKeys[cryptoHash]();
					var encryptor = rijndael.CreateEncryptor(key, iv);
					var buffer = cryptoBuffer;
					using (CryptoStream cryptoStream = new CryptoStream(destStream, encryptor, CryptoStreamMode.Write)) {
						while (true) {
							var readed = sourceStream.Read(buffer, 0, buffer.Length);
							if (0 == readed) {
								break;
							}
							cryptoStream.Write(buffer, 0, readed);
						}
					}
				}
			}
		}

		/// <summary>
		/// IDisposableインターフェース
		/// </summary>
		public new void Dispose() {
			m_CryptoBuffer = null;
			m_CryptoAssetBundleNamesWithVariant = null;
			base.Dispose();
		}

		/// <summary>
		/// 暗号化キー作成
		/// </summary>
		/// <returns>暗号化キー(C#ソースコードファイル)のパス</returns>
		public static string CreateCryptoKey() {
			if (!HasCryptoKey()) {
				//新規作成
				var hashLength = Random.Range(kCryptoKeyRandomValueLengthMin, kCryptoKeyRandomValueLengthMax + 1);
				var hash = new byte[hashLength];
				for (var i = 0; i < hash.Length; ++i) {
					hash[i] = (byte)Random.Range((int)byte.MinValue, ((int)byte.MaxValue) + 1);
				}
				var password = System.Convert.ToBase64String(hash);
				var contents = AssetBundleCryptoKeyTemplate.kCryptoKeyTemplate.Replace("{0}", password);

				var fullPath = Application.dataPath + "/../" + kCryptoKeyPath;
				AssetBundleUtility.CreateDirectory(fullPath, true);
				File.WriteAllText(fullPath, contents, Encoding.UTF8);
				AssetDatabase.ImportAsset(kCryptoKeyPath, ImportAssetOptions.Default);
			}
			return kCryptoKeyPath;
		}

		/// <summary>
		/// 暗号化キー所持確認
		/// </summary>
		/// <returns>true:所持, false:未所持</returns>
		public static bool HasCryptoKey() {
			return AssetBundleEditorUtility.Exists(kCryptoKeyPath);
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields

		/// <summary>
		/// 暗号化バッファサイズ
		/// </summary>
		private const int kCryptoBufferSize = 1 * 1024 * 1024;

		/// <summary>
		/// 暗号化キーパス
		/// </summary>
		private const string kCryptoKeyPath = "Assets/AssetBundleShoshaUserData/Scripts/AssetBundleCryptoKey.cs";

		/// <summary>
		/// 暗号化キー乱数値最小長
		/// </summary>
		private const int kCryptoKeyRandomValueLengthMin = 16;

		/// <summary>
		/// 暗号化キー乱数値最大長
		/// </summary>
		private const int kCryptoKeyRandomValueLengthMax = 40;

		/// <summary>
		/// UnityのデフォルトバンドルID
		/// </summary>
		private const string kDefaultBundleIdentifier = "com.Company.ProductName";

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 暗号化バッファ
		/// </summary>
		private byte[] cryptoBuffer {get{if (m_CryptoBuffer == null) {m_CryptoBuffer = new byte[kCryptoBufferSize];} return m_CryptoBuffer;}}
		private byte[] m_CryptoBuffer = null;

		/// <summary>
		/// 暗号化バリアント付きアセットバンドル名辞書
		/// </summary>
		private Dictionary<string, object> cryptoAssetBundleNamesWithVariant {get{if (m_CryptoAssetBundleNamesWithVariant == null) {m_CryptoAssetBundleNamesWithVariant = GetCryptoAssetBundleNamesWithVariant();} return m_CryptoAssetBundleNamesWithVariant;}}
		private Dictionary<string, object> m_CryptoAssetBundleNamesWithVariant = null;

		/// <summary>
		/// 暗号化バッファ
		/// </summary>
		private HashAlgorithm hashAlgorithm {get{if (m_HashAlgorithm == null) {m_HashAlgorithm = new HashAlgorithm();} return m_HashAlgorithm;}}
		private HashAlgorithm m_HashAlgorithm = null;

		#endregion
		#region Private methods

		/// <summary>
		/// ランダムなIVの作成
		/// </summary>
		/// <returns>ランダムなIV</returns>
		private static byte[] CreateRandomIV() {
			var result = new byte[kIVSize];
			for (var i = 0; i < result.Length; ++i) {
				result[i] = (byte)Random.Range((int)byte.MinValue, ((int)byte.MaxValue) + 1);
			}
			return result;
		}

		/// <summary>
		/// 暗号化バリアント付きアセットバンドル名取得
		/// </summary>
		/// <returns>暗号化バリアント付きアセットバンドル名</returns>
		private static Dictionary<string, object> GetCryptoAssetBundleNamesWithVariant() {
			var result = AssetDatabase.FindAssets("l:CryptoAssetBundle")
									.Select(x=>AssetDatabase.GUIDToAssetPath(x))
									.Select(x=>AssetDatabase.GetImplicitAssetBundleName(x))
									.Where(x=>!string.IsNullOrEmpty(x))
									.ToDictionary(x=>x, y=>(object)null);
			return result;
		}

		#endregion
	}
}
