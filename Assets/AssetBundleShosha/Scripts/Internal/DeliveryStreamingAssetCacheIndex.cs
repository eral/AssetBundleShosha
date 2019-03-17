// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	using System.Text;
	using UnityEngine;

	public class DeliveryStreamingAssetCacheIndex {
		#region Public types

		/// <summary>
		/// 情報
		/// </summary>
		[System.Serializable]
		public class Info {
			public string nameWithVariant {get{return n;} set{n = value;}}
			public Hash128 hash {get{if (!g.isValid) {g = h;} return g;} set{if ((value != g) || !g.isValid) {h = g = value;}}}
			public uint crc {get{return c;} set{c = value;}}
			public uint fileSize {get{return s;} set{s = value;}}
			public uint accessIndex {get{return i;} set{i = value;}}

			public Info() {
			}
			public Info(string nameWithVariant, Hash128 hash, uint crc, uint fileSize) {
				n = nameWithVariant;
				h = g = hash;
				c = crc;
				s = fileSize;
			}
			public Info(string nameWithVariant, Hash128 hash, uint crc, uint fileSize, uint accessIndex) : this(nameWithVariant, hash, crc, fileSize) {
				i = accessIndex;
			}

			[SerializeField]
			private string n;
			[System.NonSerialized]
			private Hash128 g;
			[SerializeField]
			private SerializableHash128 h;
			[SerializeField]
			private uint c;
			[SerializeField]
			private uint s;
			[SerializeField]
			private uint i;
		}

		#endregion
		#region Public methods

		/// <summary>
		/// 情報取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>情報</returns>
		public Info GetInfo(string assetBundleNameWithVariant) {
			Info result = null;
			int index;
			if (m_Indices.TryGetValue(assetBundleNameWithVariant, out index)) {
				if (m_data.infos[index].accessIndex < m_MaxAccessIndex) {
					++m_MaxAccessIndex;
					m_data.infos[index].accessIndex = m_MaxAccessIndex;
				}
				result = m_data.infos[index];
			}
			return result;
		}

		/// <summary>
		/// 情報設定
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <param name="hash">ハッシュ</param>
		/// <param name="crc">CRC</param>
		/// <param name="fileSize">ファイルサイズ</param>
		/// <returns>情報</returns>
		public Info SetInfo(string assetBundleNameWithVariant, Hash128 hash, uint crc, uint fileSize) {
			Info result = null;
			int index;
			if (m_Indices.TryGetValue(assetBundleNameWithVariant, out index)) {
				if (m_data.infos[index].accessIndex < m_MaxAccessIndex) {
					++m_MaxAccessIndex;
					m_data.infos[index].accessIndex = m_MaxAccessIndex;
				}
				result = m_data.infos[index];
				result.hash = hash;
				result.crc = crc;
				result.fileSize = fileSize;
			} else {
				m_Indices.Add(assetBundleNameWithVariant, m_data.infos.Count);
				++m_MaxAccessIndex;
				m_data.infos.Add(new Info(assetBundleNameWithVariant, hash, crc, fileSize, m_MaxAccessIndex));
			}
			return result;
		}

		/// <summary>
		/// 情報削除
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>削除確認(true:削除した, false:元々無かった)</returns>
		public bool RemoveInfo(string assetBundleNameWithVariant) {
			var result = false;
			int index;
			if (m_Indices.TryGetValue(assetBundleNameWithVariant, out index)) {
				m_data.infos.RemoveAt(index);
				m_Indices.Remove(assetBundleNameWithVariant);
				foreach (var IndicesKey in m_Indices.Keys) {
					if (index < m_Indices[IndicesKey]) {
						--m_Indices[IndicesKey];
					}
				}
				result = true;
			}
			return result;
		}

		/// <summary>
		/// ハッシュ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>ハッシュ</returns>
		public Hash128? GetHash(string assetBundleNameWithVariant) {
			Hash128? result = null;
			int index;
			if (m_Indices.TryGetValue(assetBundleNameWithVariant, out index)) {
				if (m_data.infos[index].accessIndex < m_MaxAccessIndex) {
					++m_MaxAccessIndex;
					m_data.infos[index].accessIndex = m_MaxAccessIndex;
				}

				result = m_data.infos[index].hash;
			}
			return result;
		}

		/// <summary>
		/// CRC取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>CRC</returns>
		public uint? GetCrc(string assetBundleNameWithVariant) {
			uint? result = null;
			int index;
			if (m_Indices.TryGetValue(assetBundleNameWithVariant, out index)) {
				if (m_data.infos[index].accessIndex < m_MaxAccessIndex) {
					++m_MaxAccessIndex;
					m_data.infos[index].accessIndex = m_MaxAccessIndex;
				}

				result = m_data.infos[index].crc;
			}
			return result;
		}

		/// <summary>
		/// ファイルサイズ取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>ファイルサイズ</returns>
		public uint? GetFileSize(string assetBundleNameWithVariant) {
			uint? result = null;
			int index;
			if (m_Indices.TryGetValue(assetBundleNameWithVariant, out index)) {
				if (m_data.infos[index].accessIndex < m_MaxAccessIndex) {
					++m_MaxAccessIndex;
					m_data.infos[index].accessIndex = m_MaxAccessIndex;
				}

				result = m_data.infos[index].fileSize;
			}
			return result;
		}

		/// <summary>
		/// アクセスインデックス取得
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アクセスインデックス</returns>
		public uint? GetAccessIndex(string assetBundleNameWithVariant) {
			uint? result = null;
			int index;
			if (m_Indices.TryGetValue(assetBundleNameWithVariant, out index)) {
				result = m_data.infos[index].accessIndex;
			}
			return result;
		}

		/// <summary>
		/// ファイル読み込み
		/// </summary>
		/// <param name="fullPath">ファイルパス</param>
		public void Load(string fullPath) {
			var datajson = File.ReadAllText(fullPath, Encoding.UTF8);
			var data = JsonUtility.FromJson<Data>(datajson);
			m_data = data;
			m_MaxAccessIndex = m_data.infos.Max(x=>x.accessIndex);
			CreateIndices();
		}

		/// <summary>
		/// ファイル書き出し
		/// </summary>
		/// <param name="fullPath">ファイルパス</param>
		public void Save(string fullPath) {
			var datajson = JsonUtility.ToJson(m_data);
			File.WriteAllText(fullPath, datajson, Encoding.UTF8);
		}

		/// <summary>
		/// クリア
		/// </summary>
		public void Clear() {
			m_data.infos.Clear();
			m_Indices.Clear();
			m_MaxAccessIndex = 0;
		}

		#endregion
		#region Protected fields and properties
		#endregion
		#region Private types
		#endregion
		#region Protected methods
		#endregion
		#region Private types

		/// <summary>
		/// データ
		/// </summary>
		/// <remarks>リストをJsonUtilityに掛けられる様に包む</remarks>
		[System.Serializable]
		private struct Data {
			public List<Info> infos;
		}

		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// データ
		/// </summary>
		private Data m_data = new Data{infos = new List<Info>()};

		/// <summary>
		/// 名称インデックス辞書
		/// </summary>
		private Dictionary<string, int> m_Indices = new Dictionary<string, int>();

		/// <summary>
		/// 最大アクセスインデックス
		/// </summary>
		private uint m_MaxAccessIndex;

		#endregion
		#region Private methods

		/// <summary>
		/// 名称インデックス辞書
		/// </summary>
		private void CreateIndices() {
			m_Indices.Clear();
			for (int i = 0, iMax = m_data.infos.Count; i < iMax; ++i) {
				m_Indices.Add(m_data.infos[i].nameWithVariant, i);
			}
		}

		#endregion
	}
}
