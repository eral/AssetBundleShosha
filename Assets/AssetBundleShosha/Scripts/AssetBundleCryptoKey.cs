// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {

	[System.AttributeUsage(System.AttributeTargets.Property)]
	public class AssetBundleCryptoKeyAttribute : System.Attribute {
		#region Public fields and properties

		/// <summary>
		/// 暗号化ハッシュ
		/// </summary>
		public int hash {get{return m_Hash;}}

		#endregion
		#region Public methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="hash">暗号化ハッシュ</param>
		public AssetBundleCryptoKeyAttribute(int hash) {
			if (hash == 0) {
				throw new System.ArgumentOutOfRangeException("hash");
			}
			m_Hash = hash;
		}

		#endregion
		#region Private fields and properties
		
		/// <summary>
		/// インデックス
		/// </summary>
		private int m_Hash = int.MinValue;

		#endregion
	}
}
