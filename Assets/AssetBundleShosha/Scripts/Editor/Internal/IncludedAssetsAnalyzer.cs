// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;

	public class IncludedAssetsAnalyzer {
		#region Public types
		#endregion
		#region Public const fields
		#endregion
		#region Public fields and properties
		#endregion
		#region Public methods

		/// <summary>
		/// �֐߈ˑ��A�Z�b�g�Q�̎擾
		/// </summary>
		/// <param name="assetBundleBuilds">�A�Z�b�g�o���h���r���h(���O�A�Z�b�g�p�A�Z�b�g�o���h������)</param>
		/// <param name="excludeAssetPaths">���O�A�Z�b�g�p�X�Q</param>
		/// <returns>Dictionary(�o���A���g�t���A�Z�b�g�o���h����, �֐߈ˑ��A�Z�b�g�Q)</returns>
		public Dictionary<string, string[]> GetAllIncludedAssets(AssetBundleBuild[] assetBundleBuilds, string[] excludeAssetPaths) {
			m_AssetBundleBuilds = assetBundleBuilds;
			m_IncludeRootAssetPaths = m_AssetBundleBuilds.SelectMany(x=>x.assetNames)
														.ToDictionary(x=>x, x=>(object)null);
			m_ExcludeRootAssetPaths = excludeAssetPaths.ToDictionary(x=>x, x=>(object)null);

			var result = new Dictionary<string, string[]>(m_AssetBundleBuilds.Length);
			{
				var queue = new Dictionary<string, object>(m_AssetBundleBuilds.Length);
				var include = new Dictionary<string, object>(m_AssetBundleBuilds.Length);
				foreach (var assetBundleBuild in m_AssetBundleBuilds) {
					include.Clear();
					queue.Clear();
					System.Array.ForEach(assetBundleBuild.assetNames, x=>include.Add(x, null));
					System.Array.ForEach(assetBundleBuild.assetNames, x=>queue.Add(x, null));
					while (0 < queue.Count) {
						var current = queue.Keys.First();
						var dependencies = AssetDatabase.GetDependencies(current, false);
						foreach (var dependency in dependencies) {
							if (dependency.EndsWith(".cs")) {
								//�X�N���v�g�t�@�C���Ȃ�
								//�ΏۊO
								continue;
							} else if (include.ContainsKey(dependency)) {
								//���ɓo�^�ς݂Ȃ�
								//�ΏۊO
								continue;
							} else if (queue.ContainsKey(dependency)) {
								//���ɉ�͑ΏۂȂ�
								//�ΏۊO
								continue;
							} else if (m_IncludeRootAssetPaths.ContainsKey(dependency)) {
								//���̃A�Z�b�g�o���h���Ɋ܂܂��Ȃ�
								//�ΏۊO
								continue;
							} else if (m_ExcludeRootAssetPaths.ContainsKey(dependency)) {
								//���O�A�Z�b�g�Ȃ�
								//�ΏۊO
								continue;
							}
							//�ˑ��A�Z�b�g�Ȃ�
							include.Add(dependency, null);
							queue.Add(dependency, null);
						}
						queue.Remove(current);
					}
					var assetBundleBuildWithVariant = assetBundleBuild.assetBundleName + (string.IsNullOrEmpty(assetBundleBuild.assetBundleVariant)? string.Empty: "." + assetBundleBuild.assetBundleVariant);
					result.Add(assetBundleBuildWithVariant, include.Keys.ToArray());
				}
			}
			return result;
		}
		public Dictionary<string, string[]> GetAllIncludedAssets(AssetBundleBuild[] assetBundleBuilds) {
			return GetAllIncludedAssets(assetBundleBuilds, null);
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// �A�Z�b�g�o���h���r���h (���O�A�Z�b�g�p�A�Z�b�g�o���h���͊܂܂�)
		/// </summary>
		AssetBundleBuild[] m_AssetBundleBuilds = null;

		/// <summary>
		/// ����ΏۃA�Z�b�g�̓��A�A�Z�b�g�o���h���̃��[�g�ɑ��݂���A�Z�b�g�̃p�X�Q
		/// </summary>
		Dictionary<string, object> m_IncludeRootAssetPaths = null;

		/// <summary>
		/// ���O�A�Z�b�g�o���h���̃��[�g�ɑ��݂���A�Z�b�g�̃p�X�Q
		/// </summary>
		Dictionary<string, object> m_ExcludeRootAssetPaths = null;

		#endregion
		#region Private methods
		#endregion
	}
}
