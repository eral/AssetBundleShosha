// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using System.Linq;
	using System.IO;
	using System.Text;
	using UnityEngine;

	public class HashAlgorithm {
		#region Public methods

		/// <summary>
		/// ハッシュ取得
		/// </summary>
		/// <param name="bytes">ハッシュ算出データ</param>
		/// <returns>ハッシュ</returns>
		public byte[] GetHash(byte[] bytes) {
			m_HashAlgorithm.Initialize();
			var result = m_HashAlgorithm.ComputeHash(bytes);
			return result;
		}

		/// <summary>
		/// 16進数表記ハッシュ取得
		/// </summary>
		/// <param name="bytes">ハッシュ算出データ</param>
		/// <returns>16進数表記ハッシュ</returns>
		public string GetHashHex(byte[] bytes) {
			var hash = GetHash(bytes);
			var result = ConvertHashHexFromBytes(hash);
			return result;
		}

		/// <summary>
		/// ハッシュ取得
		/// </summary>
		/// <param name="text">ハッシュ算出テキスト</param>
		/// <returns>ハッシュ</returns>
		public byte[] GetHash(string text) {
			var bytes = m_TextEncoding.GetBytes(text);
			return GetHash(bytes);
		}

		/// <summary>
		/// 16進数表記ハッシュ取得
		/// </summary>
		/// <param name="text">ハッシュ算出テキスト</param>
		/// <returns>16進数表記ハッシュ</returns>
		public string GetHashHex(string text) {
			var hash = GetHash(text);
			var result = ConvertHashHexFromBytes(hash);
			return result;
		}

		/// <summary>
		/// ファイルのハッシュ取得
		/// </summary>
		/// <param name="fullPath">ファイルフルパス</param>
		/// <returns>ハッシュ</returns>
		public byte[] GetHashFromFile(string fullPath) {
			byte[] result;
			m_HashAlgorithm.Initialize();
			using (var fileStream = File.OpenRead(fullPath)) {
				var buffer = new byte[128 * 1024];
				var readSize = 0;
				do {
					var read = fileStream.Read(buffer, 0, buffer.Length);
					result = m_HashAlgorithm.ComputeHash(buffer, 0, read);
					readSize += read;
				} while (readSize < fileStream.Length);
			}
			return result;
		}

		/// <summary>
		/// ファイルの16進数表記ハッシュ取得
		/// </summary>
		/// <param name="fullPath">ファイルフルパス</param>
		/// <returns>16進数表記ハッシュ</returns>
		public string GetHashHexFromFile(string fullPath) {
			var hash = GetHashFromFile(fullPath);
			var result = ConvertHashHexFromBytes(hash);
			return result;
		}

		/// <summary>
		/// バイト群のCRC
		/// </summary>
		/// <param name="data">データ</param>
		/// <returns>ハッシュ</returns>
		public uint GetCRCFromBytes(IEnumerable<byte> data) {
			var crcTable = GetCRCTable();
			uint result = 0xFFFFFFFF;
			foreach (var dat in data) {
				result = crcTable[(result ^ dat) & 0xFF] ^ (result >> 8);
			}
			result ^= 0xFFFFFFFF;
			return result;
		}

		/// <summary>
		/// ファイルのCRC
		/// </summary>
		/// <param name="fullPath">ファイルフルパス</param>
		/// <returns>ハッシュ</returns>
		public uint GetCRCFromFile(string fullPath) {
			var crcTable = GetCRCTable();
			uint result = 0xFFFFFFFF;
			using (var fileStream = File.OpenRead(fullPath)) {
				var buffer = new byte[128 * 1024];
				var readSize = 0;
				do {
					var read = fileStream.Read(buffer, 0, buffer.Length);
					for (int i = 0, iMax = read; i < iMax; ++i) {
						result = crcTable[(result ^ buffer[i]) & 0xFF] ^ (result >> 8);
					}
					readSize += read;
				} while (readSize < fileStream.Length);
			}
			result ^= 0xFFFFFFFF;
			return result;
		}

		/// <summary>
		/// ファイル名取得
		/// </summary>
		/// <param name="platform">プラットフォーム</param>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>ファイル名</returns>
		public string GetAssetBundleFileName(string platform, string assetBundleNameWithVariant) {
			var hasPlatform = !string.IsNullOrEmpty(platform);
			var hasAssetBundleNameWithVariant = !string.IsNullOrEmpty(assetBundleNameWithVariant);
			m_StringBuilder.Length = 0;
			if (hasPlatform) {
				m_StringBuilder.Append(platform);
			}
			if (hasPlatform && hasAssetBundleNameWithVariant) {
				m_StringBuilder.Append('/');
			}
			if (hasAssetBundleNameWithVariant) {
				m_StringBuilder.Append(assetBundleNameWithVariant);
			}
			var text = m_StringBuilder.ToString().ToLower();
			return GetHashHex(text);
		}

		/// <summary>
		/// 文字列連結
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public string JoinString(params string[] args) {
			m_StringBuilder.Length = 0;
			foreach (var arg in args) {
				m_StringBuilder.Append(arg);
			}
			return m_StringBuilder.ToString();
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields

		/// <summary>
		/// 16進数表記ハッシュの文字数
		/// </summary>
		private const int kHashHexLength = 160 / 4;

		#endregion
		#region Private fields and properties

		/// <summary>
		/// ハッシュ計算機
		/// </summary>
		private System.Security.Cryptography.HashAlgorithm m_HashAlgorithm = new SHA1CryptoServiceProvider();

		/// <summary>
		/// テキストエンコーダー
		/// </summary>
		private Encoding m_TextEncoding = new UTF8Encoding();

		/// <summary>
		/// ストリングビルダー
		/// </summary>
		private StringBuilder m_StringBuilder = new StringBuilder();

		/// <summary>
		/// CRCデーブル
		/// </summary>
		private uint[] m_CRCTable = null;

		#endregion
		#region Private methods

		/// <summary>
		/// 16進数表記ハッシュ変換
		/// </summary>
		/// <param name="bytes">バイトコード</param>
		/// <returns>16進数表記ハッシュ</returns>
		private string ConvertHashHexFromBytes(byte[] bytes) {
			m_StringBuilder.Length = 0;
			foreach(var b in bytes) {
				m_StringBuilder.AppendFormat("{0,0:x2}", b);
			}
			var result = m_StringBuilder.ToString().ToLower();
			return result;
		}

		/// <summary>
		/// CRCデーブルの取得
		/// </summary>
		/// <returns>CRCデーブル</returns>
		private uint[] GetCRCTable() {
			if (m_CRCTable == null) {
				var table = new uint[256];
				for (uint i = 0, iMax = (uint)table.Length; i < iMax; ++i) {
					var value = i;
					for (int k = 0, kMax = 8; k < kMax; ++k) {
						value = (((value & 1) != 0)? 0xEDB88320 ^ (value >> 1): value >> 1);
					}
					table[i] = value;
				}
				m_CRCTable = table;
			}
			return m_CRCTable;
		}

		#endregion
	}
}
