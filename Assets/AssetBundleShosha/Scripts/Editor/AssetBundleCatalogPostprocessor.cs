// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor {
	using AssetBundleShosha.Internal;
	using AssetBundleShosha.Editor.Internal;

	public class AssetBundleCatalogPostprocessorAttribute : AssetBundleCustomProcessorAttribute {
		#region Public types

		/// <summary>
		/// 関数の型
		/// </summary>
		/// <param name="arg">ビルド前情報</param>
		public delegate void MethodFormat(AssetBundleCatalogPostprocessorArg arg);

		#endregion
		#region Public fields and properties

		/// <summary>
		/// 実行順
		/// </summary>
		public override int order {get{return m_Order;}}

		#endregion
		#region Public methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="order">実行順</param>
		public AssetBundleCatalogPostprocessorAttribute(int order) {
			m_Order = order;
		}

		#endregion
		#region Private fields and properties
		
		/// <summary>
		/// インデックス
		/// </summary>
		private int m_Order;

		#endregion
	}

	public class AssetBundleCatalogPostprocessorArg {
		#region Public fields and properties

		/// <summary>
		/// カタログ
		/// </summary>
		public AssetBundleCatalog catalog {get{return m_Catalog;}}

		#endregion
		#region Internal methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="catalog">カタログ</param>
		internal AssetBundleCatalogPostprocessorArg(AssetBundleCatalog catalog) {
			m_Catalog = catalog;
		}

		#endregion
		#region Private fields and properties
		
		/// <summary>
		/// カタログ
		/// </summary>
		private AssetBundleCatalog m_Catalog;

		#endregion
	}
}
