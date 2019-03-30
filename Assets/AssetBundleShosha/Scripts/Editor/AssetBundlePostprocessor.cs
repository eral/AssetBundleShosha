// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor {
	using AssetBundleShosha.Internal;
	using UnityEngine;

	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class AssetBundlePostprocessorAttribute : System.Attribute {
		#region Public types

		/// <summary>
		/// 関数の型
		/// </summary>
		/// <param name="arg">ビルド後情報</param>
		public delegate void MethodFormat(AssetBundlePostprocessorArg arg);

		#endregion
		#region Public fields and properties

		/// <summary>
		/// 実行順
		/// </summary>
		public int order {get{return m_Order;}}

		#endregion
		#region Public methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="order">実行順</param>
		public AssetBundlePostprocessorAttribute(int order) {
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

	public class AssetBundlePostprocessorArg {
		#region Public fields and properties

		/// <summary>
		/// カタログ
		/// </summary>
		public AssetBundleCatalog catalog {get{return m_Catalog;}}

		#endregion
		#region Public methods

		/// <summary>
		/// 全てのアセットバンドル名取得
		/// </summary>
		/// <returns>全てのアセットバンドル名</returns>
		public string[] GetAllAssetBundles() {
			return m_Catalog.GetAllAssetBundles();
		}

		/// <summary>
		/// 全てのバリアント付きアセットバンドル名取得
		/// </summary>
		/// <returns>全てのバリアント付きアセットバンドル名</returns>
		public string[] GetAllAssetBundlesWithVariant() {
			return m_Catalog.GetAllAssetBundlesWithVariant();
		}

		/// <summary>
		/// バリアント情報取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>バリアント情報</returns>
		/// <remarks>登録されていないアセットバンドル名の場合はnullを返す</remarks>
		public AssetBundleCatalog.VariantInfo[] GetAssetBundleVariantInfos(string assetBundleNameWithVariant) {
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			return m_Catalog.GetAssetBundleVariantInfos(assetBundleNameWithVariant);
		}

		/// <summary>
		/// 間接含む全依存関係の取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>間接含む全依存関係</returns>
		/// <remarks>登録されていないアセットバンドル名の場合はnullを返す</remarks>
		public string[] GetAllDependencies(string assetBundleNameWithVariant) {
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			return m_Catalog.GetAllDependencies(assetBundleNameWithVariant);
		}

		/// <summary>
		/// 直接依存関係の取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>直接依存関係</returns>
		/// <remarks>登録されていないアセットバンドル名の場合はnullを返す</remarks>
		public string[] GetDirectDependencies(string assetBundleNameWithVariant) {
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			return m_Catalog.GetDirectDependencies(assetBundleNameWithVariant);
		}

		/// <summary>
		/// ハッシュ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>ハッシュ</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は空ハッシュを返す</remarks>
		public Hash128 GetAssetBundleHash(string assetBundleNameWithVariant) {
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			return m_Catalog.GetAssetBundleHash(assetBundleNameWithVariant);
		}

		/// <summary>
		/// CRC取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>CRC</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は0を返す</remarks>
		public uint GetAssetBundleCrc(string assetBundleNameWithVariant) {
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			return m_Catalog.GetAssetBundleCrc(assetBundleNameWithVariant);
		}

		/// <summary>
		/// ファイルサイズ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>ファイルサイズ</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は0を返す</remarks>
		public uint GetAssetBundleFileSize(string assetBundleNameWithVariant) {
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			return m_Catalog.GetAssetBundleFileSize(assetBundleNameWithVariant);
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は0を返す</remarks>
		public int GetAssetBundleCryptoHash(string assetBundleNameWithVariant) {
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			return m_Catalog.GetAssetBundleCryptoHash(assetBundleNameWithVariant);
		}

		/// <summary>
		/// パス取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>パス</returns>
		public string GetAssetBundlePath(string assetBundleNameWithVariant) {
			string result;
			assetBundleNameWithVariant = assetBundleNameWithVariant.ToLower();
			if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleNameWithVariant)) {
				//配信ストリーミングアセット
				result = m_OutputPath + "/" + m_HashAlgorithm.GetAssetBundleFileName(null, assetBundleNameWithVariant);
			} else {
				//アセットバンドル
				result = m_OutputPath + "/" + m_HashAlgorithm.GetAssetBundleFileName(m_PlatformString, assetBundleNameWithVariant);
			}
			return result;
		}

		#endregion
		#region Internal methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="catalog">カタログ</param>
		/// <param name="outputPath">出力パス</param>
		/// <param name="platformString">プラットフォーム文字列</param>
		internal AssetBundlePostprocessorArg(AssetBundleCatalog catalog, string outputPath, string platformString) {
			m_Catalog = catalog;
			m_OutputPath = outputPath;
			m_PlatformString = platformString;
		}

		#endregion
		#region Private fields and properties
		
		/// <summary>
		/// カタログ
		/// </summary>
		private AssetBundleCatalog m_Catalog;

		/// <summary>
		/// 出力パス
		/// </summary>
		private string m_OutputPath;

		/// <summary>
		/// プラットフォーム文字列
		/// </summary>
		private string m_PlatformString;

		/// <summary>
		/// ハッシュ計算機
		/// </summary>
		private HashAlgorithm m_HashAlgorithm = new HashAlgorithm();

		#endregion
	}
}
