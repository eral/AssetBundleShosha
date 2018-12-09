// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using UnityEngine;

	/// <summary>
	/// シリアライズ可能Hash128
	/// </summary>
	[System.Serializable]
	public struct SerializableHash128 {
		#region Public methods

		public static implicit operator SerializableHash128(Hash128 src) {
			var hashStr = src.ToString();
			var data = new uint[4];
			for (int i = 0, iMax = data.Length; i < iMax; ++i) {
				var v = System.Convert.ToUInt32(hashStr.Substring(i * 8, 8), 16);
				data[i] = ByteSwapping(v);
			}
			return new SerializableHash128{m_data = data};
		}

		public static implicit operator Hash128(SerializableHash128 src) {
			if (src.m_data != null) {
				return new Hash128(src.m_data[0], src.m_data[1], src.m_data[2], src.m_data[3]);
			} else {
				return new Hash128();
			}
		}

		public override string ToString() {
			var result = "00000000000000000000000000000000";
			if (m_data != null) {
				result = ByteSwapping(m_data[0]).ToString("x8")
						+ ByteSwapping(m_data[1]).ToString("x8")
						+ ByteSwapping(m_data[2]).ToString("x8")
						+ ByteSwapping(m_data[3]).ToString("x8")
						;
			}
			return result;
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// データ
		/// </summary>
		[SerializeField]
		private uint[] m_data;

		#endregion
		#region Private methods

		/// <summary>
		/// エンディアン変換
		/// </summary>
		/// <param name="src">値</param>
		/// <returns>変換後値</returns>
		private static uint ByteSwapping(uint src) {
			var result = ((src >> 24) & 0x000000FF) | ((src >> 8) & 0x0000FF00) | ((src << 8) & 0x00FF0000) | ((src << 24) & 0xFF000000);
			return result;
		}

		#endregion
	}
}
