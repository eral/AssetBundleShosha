// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor {
	using System.Collections.Generic;

	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class AssetBundlePackerAttribute : System.Attribute {
		#region Public types

		/// <summary>
		/// 関数の型
		/// </summary>
		/// <param name="arg">梱包情報</param>
		public delegate void MethodFormat(AssetBundlePackerArg arg);

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
		public AssetBundlePackerAttribute(int order) {
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

	public class AssetBundlePackerArg {
		#region Public types

		/// <summary>
		/// アセットバンドルフラグ
		/// </summary>
		[System.Flags]
		public enum AssetBundleFlags {
			Null		= 0,
			Exclude		= 1 << 0, //除外
			UnusedName	= 1 << 1, //未使用名(梱包するアセットが無い為基本的に除外対象となる、しかし梱包関数に依ってアセットが追加されると有効として扱われる)(これはUnityEditorの従来の挙動に似せる為のものである)
		}

		/// <summary>
		/// アセットフラグ
		/// </summary>
		[System.Flags]
		public enum AssetFlags {
			Null	= 0,
			Exclude	= 1 << 0, //除外
		}

		/// <summary>
		/// アセット
		/// </summary>
		public class Asset {
			public string assetPath;
			public AssetFlags option;
		}

		#endregion
		#region Public fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		public string assetBundleName {get{
			CreateNameCache();
			return m_AssetBundleName;
		}}

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		public string assetBundleNameWithVariant {get{return m_AssetBundleNameWithVariant;}}

		/// <summary>
		/// バリアント
		/// </summary>
		public string variant {get{
			CreateNameCache();
			return m_Variant;
		}}

		/// <summary>
		/// オプション
		/// </summary>
		public AssetBundleFlags options {get{return m_Options;} set{m_Options = value;}}

		/// <summary>
		/// 暗号化ハッシュ
		/// </summary>
		public int cryptoHash {get{return m_CryptoHash;} set{m_CryptoHash = value;}}

		/// <summary>
		/// 梱包アセット群
		/// </summary>
		public List<Asset> assets {get{return m_Assets;} set{m_Assets = value;}}

		#endregion
		#region Internal methods

		/// <summary>
		/// セットアップ
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <param name="options">オプション</param>
		/// <param name="cryptoHash">暗号化ハッシュ</param>
		/// <param name="assets">梱包アセット群</param>
		internal void Setup(string assetBundleNameWithVariant, AssetBundleFlags options, int cryptoHash, IEnumerable<Asset> assets) {
			m_AssetBundleNameWithVariant = assetBundleNameWithVariant;
			m_Options = options;
			m_CryptoHash = cryptoHash;
			DestroyNameCache();
			m_Assets.Clear();
			m_Assets.AddRange(assets);
		}

		#endregion
		#region Private fields and properties
		
		/// <summary>
		/// アセットバンドル名
		/// </summary>
		private string m_AssetBundleName = null;
		
		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		private string m_AssetBundleNameWithVariant = null;
		
		/// <summary>
		/// バリアント
		/// </summary>
		private string m_Variant = null;

		/// <summary>
		/// オプション
		/// </summary>
		public AssetBundleFlags m_Options = (AssetBundleFlags)0;

		/// <summary>
		/// 暗号化ハッシュ
		/// </summary>
		public int m_CryptoHash = 0;

		/// <summary>
		/// 梱包アセット群
		/// </summary>
		public List<Asset> m_Assets = new List<Asset>();

		#endregion
		#region Private methods

		/// <summary>
		/// 名前キャッシュ作成
		/// </summary>
		private void CreateNameCache() {
			if (m_AssetBundleName == null) {
				var variantPair = m_AssetBundleNameWithVariant.Split('.');
				m_AssetBundleName = variantPair[0];
				m_Variant = ((variantPair.Length != 2)? string.Empty: variantPair[1]);
			}
		}

		/// <summary>
		/// 名前キャッシュ破棄
		/// </summary>
		private void DestroyNameCache() {
			m_AssetBundleName = null;
			m_Variant = null;
		}

		#endregion
	}
}
