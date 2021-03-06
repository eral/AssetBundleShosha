// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Text;
	using System.Linq;
	using UnityEngine;

	public class AssetBundleCatalog : ScriptableObject {
		#region Public types

		/// <summary>
		/// バリアント情報
		/// </summary>
		public struct VariantInfo {
			public string assetBundle;
			public string assetBundleWithVariant;
			public string Variant;
		}

		#endregion
		#region Public fields and properties

		/// <summary>
		/// ユーザーデータ
		/// </summary>
		public byte[] userData {
			get{return m_UserData;}
#if UNITY_EDITOR
			set{m_UserData = value;}
#endif
		}

		#endregion
		#region Public methods

		/// <summary>
		/// コンテンツハッシュ取得
		/// </summary>
		/// <returns>コンテンツハッシュ</returns>
		public uint GetContentHash() {
			if (m_ContentHashCache == 0) {
				var hashAlgorithm = new HashAlgorithm();
				m_ContentHashCache = hashAlgorithm.GetCRCFromBytes(GetContentHashBytes());
			}
			return m_ContentHashCache;
		}

		/// <summary>
		/// 全てのアセットバンドル名取得
		/// </summary>
		/// <returns>全てのアセットバンドル名</returns>
		public string[] GetAllAssetBundles() {
			return m_AllAssetBundleNames;
		}

		/// <summary>
		/// 全てのバリアント付きアセットバンドル名取得
		/// </summary>
		/// <returns>全てのバリアント付きアセットバンドル名</returns>
		public string[] GetAllAssetBundlesWithVariant() {
			return m_AllAssetBundleWithVariantIndices.Select(x=>m_AllAssetBundleNames[x]).ToArray();
		}

		/// <summary>
		/// バリアント情報取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>バリアント情報</returns>
		/// <remarks>登録されていないアセットバンドル名の場合はnullを返す</remarks>
		public VariantInfo[] GetAssetBundleVariantInfos(string assetBundleName) {
			VariantInfo[] result;
			if (m_VariantsCache == null) {
				PrepareVariantCache();
			}
			m_VariantsCache.TryGetValue(assetBundleName, out result);
			return result;
		}

		/// <summary>
		/// 間接含む全依存関係の取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>間接含む全依存関係</returns>
		/// <remarks>登録されていないアセットバンドル名の場合はnullを返す</remarks>
		public string[] GetAllDependencies(string assetBundleName) {
			string[] result = null;
			var index = GetAssetBundleIndex(assetBundleName);
			if ((0 <= index) && (index < m_AssetBundleDependencies.Length)) {
				result = m_AssetBundleDependencies[index].all.Select(x=>m_AllAssetBundleNames[x]).ToArray();
			}
			return result;
		}

		/// <summary>
		/// 直接依存関係の取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>直接依存関係</returns>
		/// <remarks>登録されていないアセットバンドル名の場合はnullを返す</remarks>
		public string[] GetDirectDependencies(string assetBundleName) {
			string[] result = null;
			var index = GetAssetBundleIndex(assetBundleName);
			if ((0 <= index) && (index < m_AssetBundleDependencies.Length)) {
				result = m_AssetBundleDependencies[index].direct.Select(x=>m_AllAssetBundleNames[x]).ToArray();
			}
			return result;
		}

		/// <summary>
		/// ハッシュ取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>ハッシュ</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は空ハッシュを返す</remarks>
		public Hash128 GetAssetBundleHash(string assetBundleName) {
			var result = new Hash128();
			var index = GetAssetBundleIndex(assetBundleName);
			if ((0 <= index) && (index < m_AssetBundleHashes.Length)) {
				result = m_AssetBundleHashes[index];
			}
			return result;
		}

		/// <summary>
		/// CRC取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>CRC</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は0を返す</remarks>
		public uint GetAssetBundleCrc(string assetBundleName) {
			var result = 0u;
			var index = GetAssetBundleIndex(assetBundleName);
			if ((0 <= index) && (index < m_AssetBundleCrcs.Length)) {
				result = m_AssetBundleCrcs[index];
			}
			return result;
		}

		/// <summary>
		/// ファイルサイズ取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>ファイルサイズ</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は0を返す</remarks>
		public uint GetAssetBundleFileSize(string assetBundleName) {
			var result = 0u;
			var index = GetAssetBundleIndex(assetBundleName);
			if ((0 <= index) && (index < m_AssetBundleFileSizes.Length)) {
				result = m_AssetBundleFileSizes[index];
			}
			return result;
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		/// <remarks>登録されていないアセットバンドル名の場合は0を返す</remarks>
		public int GetAssetBundleCryptoHash(string assetBundleName) {
			var result = 0;
			var index = GetAssetBundleIndex(assetBundleName);
			if ((0 <= index) && (index < m_AssetBundleCryptoHashes.Length)) {
				result = m_AssetBundleCryptoHashes[index];
			}
			return result;
		}

#if UNITY_EDITOR
		/// <summary>
		/// アセットバンドルインデックスの取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>アセットバンドルインデックス</returns>
		public int GetAssetBundleIndexOnEditor(string assetBundleName) {
			return GetAssetBundleIndex(assetBundleName);
		}

		/// <summary>
		/// 全てのアセットバンドル名設定
		/// </summary>
		/// <param name="allAssetBundles">全てのアセットバンドル名</param>
		/// <param name="allAssetBundlesWithVariant">全てのバリアント付きアセットバンドル名</param>
		public void SetAllAssetBundles(string[] allAssetBundles, string[] allAssetBundlesWithVariant) {
			m_AllAssetBundleNames = allAssetBundles;
			PrepareAllAssetBundleNamesInverseCache();
			m_AllAssetBundleWithVariantIndices = allAssetBundlesWithVariant.Select(x=>m_AllAssetBundleNamesInverseCache[x]).ToArray();
			var AssetBundlesLength = allAssetBundles.Length;
			m_AssetBundleDependencies = new Dependency[AssetBundlesLength];
			m_AssetBundleHashes = new SerializableHash128[AssetBundlesLength];
			m_AssetBundleCrcs = new uint[AssetBundlesLength];
			m_AssetBundleFileSizes = new uint[AssetBundlesLength];
			m_AssetBundleCryptoHashes = new int[AssetBundlesLength];

			OnSetAllAssetBundles();
		}

		/// <summary>
		/// 依存関係の設定
		/// </summary>
		/// <param name="assetBundleIndex">アセットバンドルインデックス</param>
		/// <param name="allDependencies">間接含む全依存関係</param>
		/// <param name="directDependencies">直接依存関係</param>
		public void SetDependencies(int assetBundleIndex, string[] allDependencies, string[] directDependencies) {
			m_AssetBundleDependencies[assetBundleIndex] = new Dependency{all = allDependencies.Select(x=>m_AllAssetBundleNamesInverseCache[x]).ToArray()
																		, direct = directDependencies.Select(x=>m_AllAssetBundleNamesInverseCache[x]).ToArray()
																		};
		}

		/// <summary>
		/// ハッシュ設定
		/// </summary>
		/// <param name="assetBundleIndex">アセットバンドルインデックス</param>
		/// <param name="hash">ハッシュ</param>
		public void SetAssetBundleHash(int assetBundleIndex, Hash128 hash) {
			m_AssetBundleHashes[assetBundleIndex] = hash;
		}

		/// <summary>
		/// CRC設定
		/// </summary>
		/// <param name="assetBundleIndex">アセットバンドルインデックス</param>
		/// <param name="crc">CRC</param>
		public void SetAssetBundleCrc(int assetBundleIndex, uint crc) {
			m_AssetBundleCrcs[assetBundleIndex] = crc;
		}

		/// <summary>
		/// ファイルサイズ設定
		/// </summary>
		/// <param name="assetBundleIndex">アセットバンドルインデックス</param>
		/// <param name="fileSize">ファイルサイズ</param>
		public void SetAssetBundleFileSize(int assetBundleIndex, uint fileSize) {
			m_AssetBundleFileSizes[assetBundleIndex] = fileSize;
		}

		/// <summary>
		/// 暗号化ハッシュ設定
		/// </summary>
		/// <param name="assetBundleIndex">アセットバンドルインデックス</param>
		/// <param name="hash">暗号化ハッシュ</param>
		public void SetAssetBundleCryptoHash(int assetBundleIndex, int hash) {
			m_AssetBundleCryptoHashes[assetBundleIndex] = hash;
		}

		/// <summary>
		/// 全てのアセットバンドル名設定後イベント
		/// </summary>
		public virtual void OnSetAllAssetBundles() {
			//empty.
		}

		/// <summary>
		/// パス設定
		/// </summary>
		/// <param name="assetBundleIndex">アセットバンドルインデックス</param>
		/// <param name="path">パス</param>
		public virtual void SetAssetBundlePath(int assetBundleIndex, string path) {
			//empty.
		}

		/// <summary>
		/// 構築完了後イベント
		/// </summary>
		public virtual void OnBuildFinished() {
			//empty.
		}
#endif

		#endregion
		#region Protected types
		#endregion
		#region Protected fields and properties

		/// <summary>
		/// 全アセットバンドル名
		/// </summary>
		protected string[] allAssetBundleNames {get{return m_AllAssetBundleNames;}}

		/// <summary>
		/// バリアント持ち全アセットバンドル名のインデックス
		/// </summary>
		protected int[] allAssetBundleWithVariantIndices {get{return m_AllAssetBundleWithVariantIndices;}}

		/// <summary>
		/// 依存関係
		/// </summary>
		protected Dependency[] assetBundleDependencies {get{return m_AssetBundleDependencies;}}

		#endregion
		#region Private types

		/// <summary>
		/// 依存関係
		/// </summary>
		[System.Serializable]
		protected struct Dependency {
			/// <summary>
			/// 間接含む全依存関係
			/// </summary>
			[SerializeField]
			public int[] all;

			/// <summary>
			/// 直接依存関係
			/// </summary>
			[SerializeField]
			public int[] direct;
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// アセットバンドルインデックスの取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>アセットバンドルインデックス</returns>
		protected int GetAssetBundleIndex(string assetBundleName) {
			if (m_AllAssetBundleNamesInverseCache == null) {
				PrepareAllAssetBundleNamesInverseCache();
			}
			int result;
			if (!m_AllAssetBundleNamesInverseCache.TryGetValue(assetBundleName, out result)) {
				result = -1;
			}
			return result;
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// コンテンツハッシュキャッシュ
		/// </summary>
		[System.NonSerialized]
		private uint m_ContentHashCache = 0;

		/// <summary>
		/// 全アセットバンドル名(昇順ソート済み)
		/// </summary>
		[SerializeField]
		private string[] m_AllAssetBundleNames;

		/// <summary>
		/// アセットバンドル逆引きキャッシュ
		/// </summary>
		[System.NonSerialized]
		private Dictionary<string, int> m_AllAssetBundleNamesInverseCache;

		/// <summary>
		/// バリアント持ち全アセットバンドル名のインデックス(昇順ソート済み)
		/// </summary>
		[SerializeField]
		private int[] m_AllAssetBundleWithVariantIndices;

		/// <summary>
		/// バリアントキャッシュ
		/// </summary>
		[System.NonSerialized]
		private Dictionary<string, VariantInfo[]> m_VariantsCache;

		/// <summary>
		/// 依存関係
		/// </summary>
		[SerializeField]
		private Dependency[] m_AssetBundleDependencies;

		/// <summary>
		/// ハッシュ値
		/// </summary>
		[SerializeField]
		private SerializableHash128[] m_AssetBundleHashes;

		/// <summary>
		/// CRC値
		/// </summary>
		[SerializeField]
		private uint[] m_AssetBundleCrcs;

		/// <summary>
		/// ファイルサイズ
		/// </summary>
		[SerializeField]
		private uint[] m_AssetBundleFileSizes;

		/// <summary>
		/// 暗号化ハッシュ
		/// </summary>
		[SerializeField]
		private int[] m_AssetBundleCryptoHashes;

		/// <summary>
		/// ユーザーデータ
		/// </summary>
		[SerializeField]
		private byte[] m_UserData;

		#endregion
		#region Private methods

		/// <summary>
		/// コンテンツハッシュ計算用バイト群取得
		/// </summary>
		/// <returns>コンテンツハッシュ計算用バイト群</returns>
		private IEnumerable<byte> GetContentHashBytes() {
			for (int i = 0, iMax = m_AllAssetBundleNames.Length; i < iMax; ++i) {
				var nameBytes = Encoding.UTF8.GetBytes(m_AllAssetBundleNames[i]);
				foreach (var nameByte in nameBytes) {
					yield return nameByte;
				}
				var crcBytes = System.BitConverter.GetBytes(m_AssetBundleCrcs[i]);
				foreach (var crcByte in crcBytes) {
					yield return crcByte;
				}
			}
		}

		/// <summary>
		/// アセットバンドル逆引きキャッシュ作成
		/// </summary>
		private void PrepareAllAssetBundleNamesInverseCache() {
			if (m_AllAssetBundleNamesInverseCache == null) {
				m_AllAssetBundleNamesInverseCache = new Dictionary<string, int>(m_AllAssetBundleNames.Length);
			} else {
				m_AllAssetBundleNamesInverseCache.Clear();
			}
			for (int i = 0, iMax = m_AllAssetBundleNames.Length; i < iMax; ++i) {
				m_AllAssetBundleNamesInverseCache.Add(m_AllAssetBundleNames[i], i);
			}
		}

		/// <summary>
		/// バリアントキャッシュ作成
		/// </summary>
		private void PrepareVariantCache() {
			var variantsInfoListDictionay = new Dictionary<string, List<VariantInfo>>();
			foreach (var assetBundleWithVariantIndex in m_AllAssetBundleWithVariantIndices) {
				var assetBundleWithVariant = m_AllAssetBundleNames[assetBundleWithVariantIndex];
				var variantPair = assetBundleWithVariant.Split('.');
				if (variantPair.Length != 2) {
					continue;
				}
				var info = new VariantInfo{assetBundle = variantPair[0]
										, assetBundleWithVariant = assetBundleWithVariant
										, Variant = variantPair[1]
										};
				List<VariantInfo> variantsInfoList;
				if (!variantsInfoListDictionay.TryGetValue(info.assetBundle, out variantsInfoList)) {
					variantsInfoList = new List<VariantInfo>();
					variantsInfoListDictionay[info.assetBundle] = variantsInfoList;
				}
				variantsInfoList.Add(info);
			}
			m_VariantsCache = variantsInfoListDictionay.ToDictionary(x=>x.Key, x=>x.Value.ToArray());
		}

		#endregion
	}
}
