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
		public string[] GetAllDependencies(string assetBundleName) {
			var index = GetAssetBundleIndex(assetBundleName);
			return m_AssetBundleDependencies[index].all.Select(x=>m_AllAssetBundleNames[x]).ToArray();
		}

		/// <summary>
		/// 直接依存関係の取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>直接依存関係</returns>
		public string[] GetDirectDependencies(string assetBundleName) {
			var index = GetAssetBundleIndex(assetBundleName);
			return m_AssetBundleDependencies[index].direct.Select(x=>m_AllAssetBundleNames[x]).ToArray();
		}

		/// <summary>
		/// ハッシュ取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>ハッシュ</returns>
		public Hash128 GetAssetBundleHash(string assetBundleName) {
			var index = GetAssetBundleIndex(assetBundleName);
			return m_AssetBundleHashes[index];
		}

		/// <summary>
		/// CRC取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>CRC</returns>
		public uint GetAssetBundleCrc(string assetBundleName) {
			var index = GetAssetBundleIndex(assetBundleName);
			return m_AssetBundleCrcs[index];
		}

		/// <summary>
		/// ファイルサイズ取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>ファイルサイズ</returns>
		public uint GetAssetBundleFileSize(string assetBundleName) {
			var index = GetAssetBundleIndex(assetBundleName);
			return m_AssetBundleFileSizes[index];
		}

		/// <summary>
		/// 暗号化ハッシュ取得
		/// </summary>
		/// <param name="assetBundleName">アセットバンドル名</param>
		/// <returns>暗号化ハッシュ</returns>
		public int GetAssetBundleCryptoHash(string assetBundleName) {
			var index = GetAssetBundleIndex(assetBundleName);
			return m_AssetBundleCryptoHashes[index];
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

		/// <summary>
		/// 公開JSON出力
		/// </summary>
		/// <param name="fullPath">出力先</param>
		public virtual void SavePublicJson(string fullPath) {
			var publicInfo = new PublicInfo();
			publicInfo.contentHash = GetContentHash();
			var publicJsonString = JsonUtility.ToJson(publicInfo);
			System.IO.File.WriteAllText(fullPath, publicJsonString);
		}

		/// <summary>
		/// 詳細JSON出力
		/// </summary>
		/// <param name="fullPath">出力先</param>
		public virtual void SaveDetailJson(string fullPath) {
			var hashAlgorithm = new HashAlgorithm();
			var platformString = AssetBundleUtility.GetPlatformString();

			var detailJson = new DetailJson();
			detailJson.contentHash = GetContentHash();
			var contents = new List<DetailJson.Content>(allAssetBundleNames.Length);
			for (int i = 0, iMax = allAssetBundleNames.Length; i < iMax; ++i) {
				var assetBundleNameWithVariant = allAssetBundleNames[i];
				string fileName;
				if (AssetBundleUtility.IsDeliveryStreamingAsset(assetBundleNameWithVariant)) {
					//配信ストリーミングアセット
					fileName = hashAlgorithm.GetAssetBundleFileName(null, assetBundleNameWithVariant);
				} else {
					//アセットバンドル
					fileName = hashAlgorithm.GetAssetBundleFileName(platformString, assetBundleNameWithVariant);
				}
				var dependencies = m_AssetBundleDependencies[i];
				var content = new DetailJson.Content{
					name = assetBundleNameWithVariant,
					fileName = fileName,
					hash = m_AssetBundleHashes[i].ToString(),
					crc = m_AssetBundleCrcs[i],
					fileSize = m_AssetBundleFileSizes[i],
					cryptoHash = m_AssetBundleCryptoHashes[i],
					allDependencies = dependencies.all.Select(x=>m_AllAssetBundleNames[x]).ToArray(),
					directDependencies = dependencies.direct.Select(x=>m_AllAssetBundleNames[x]).ToArray(),
				};
				contents.Add(content);
			}
			detailJson.contents = contents;
			var detailJsonString = JsonUtility.ToJson(detailJson);
			System.IO.File.WriteAllText(fullPath, detailJsonString);
		}
#endif

		#endregion
		#region Protected types

#if UNITY_EDITOR
		/// <summary>
		/// 公開情報
		/// </summary>
		protected class PublicInfo {
			public uint contentHash;
		}

		/// <summary>
		/// 公開情報
		/// </summary>
		protected class DetailJson : PublicInfo {
			[System.Serializable]
			public class Content {
				public string name;
				public string fileName;
				public string hash;
				public uint crc;
				public uint fileSize;
				public int cryptoHash;
				public string[] allDependencies;
				public string[] directDependencies;
			}
			public List<Content> contents;
		}
#endif

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
