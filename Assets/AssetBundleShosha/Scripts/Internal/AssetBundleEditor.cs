// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

#if UNITY_EDITOR
namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Networking;
	using UnityEditor;
	using AssetBundleShosha.Internal;

	public class AssetBundleEditor : AssetBundleBase {
		#region Public fields and properties

		/// <summary>
		/// 進捗(0.0f～1.0f)
		/// </summary>
		public override float progress {get{
			float result;
			if (m_DownloadWork != null) {
				result = m_DownloadWork.progress;
			} else if (!m_IsDone) {
				result = 0.0f;
			} else {
				result = 1.0f;
			}
			return result;
		}}

		/// <summary>
		/// エラーコード
		/// </summary>
		public override AssetBundleErrorCode errorCode {get{return m_ErrorCode;}}

		/// <summary>
		/// アセットバンドルをビルドするときに、必ず使うアセットを設定します（読み取り専用）
		/// </summary>
		public override Object mainAsset {get{
			if (!m_IsDone) throw new System.NullReferenceException();

			Object result = null;
#if false
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (1 == assetPaths.Length)) {
				result = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
			}
#endif
			return result;
		}}

		/// <summary>
		/// アセットバンドルがストリーミングされたシーンのアセットバンドルならば、true を返します。
		/// </summary>
		public override bool isStreamedSceneAssetBundle {get{
			if (!m_IsDone) throw new System.NullReferenceException();

			return false;
		}}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		public override bool isDeliveryStreamingAsset {get{
			return false;
		}}

		/// <summary>
		/// 配信ストリーミングアセットのパスを返します。
		/// </summary>
		public override string deliveryStreamingAssetPath {get{
			throw new System.InvalidOperationException();
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// 特定のオブジェクトがアセットバンドルに含まれているか確認します。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>true:含まれる、false:含まれない</returns>
		public override bool Contains(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			var result = false;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(nameWithVariant, name);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = true;
			}
			return result;
		}

		/// <summary>
		/// アセットバンドルにあるすべてのアセット名を返します。
		/// </summary>
		/// <returns>すべてのアセット名</returns>
		public override string[] GetAllAssetNames() {
			if (!m_IsDone) throw new System.NullReferenceException();

			string[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.Select(x=>x.ToLower())
									.ToArray();
			}
			if (result == null) result = new string[0];
			return result;
		}

		/// <summary>
		/// アセットバンドルにあるすべてのシーンアセットのパス( *.unity アセットへのパス)を返します。
		/// </summary>
		/// <returns>すべてのシーンアセットのパス</returns>
		public override string[] GetAllScenePaths() {
			if (!m_IsDone) throw new System.NullReferenceException();

			string[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>x.EndsWith(".unity"))
									.Select(x=>x.ToLower())
									.ToArray();
			}
			if (result == null) result = new string[0];
			return result;
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <returns>該当するすべてのアセット</returns>
		public override T[] LoadAllAssets<T>() {
			if (!m_IsDone) throw new System.NullReferenceException();

			T[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.SelectMany(x=>AssetDatabase.LoadAllAssetsAtPath(x))
									.Where(x=>x is T)
									.Select(x=>(T)x)
									.ToArray();
			}
			if (result == null) result = new T[0];
			return result;
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets(System.Type type) {
			if (!m_IsDone) throw new System.NullReferenceException();

			Object[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.SelectMany(x=>AssetDatabase.LoadAllAssetsAtPath(x))
									.Where(x=>type.IsAssignableFrom(x.GetType()))
									.ToArray();
			}
			if (result == null) result = new Object[0];
			return result;
		}

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <returns>該当するすべてのアセット</returns>
		public override UnityEngine.Object[] LoadAllAssets() {
			if (!m_IsDone) throw new System.NullReferenceException();

			Object[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.SelectMany(x=>AssetDatabase.LoadAllAssetsAtPath(x))
									.ToArray();
			}
			if (result == null) result = new Object[0];
			return result;
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync<T>() {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAllAssets(typeof(T))
												);
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync(System.Type type) {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAllAssets(type)
												);
		}

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAllAssetsAsync() {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAllAssets()
												);
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T LoadAsset<T>(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			T result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(nameWithVariant, name);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.Select(x=>AssetDatabase.LoadAssetAtPath<T>(x))
									.FirstOrDefault();
			}
			return result;
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name, System.Type type) {
			if (!m_IsDone) throw new System.NullReferenceException();

			Object result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(nameWithVariant, name);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.Select(x=>AssetDatabase.LoadAssetAtPath(x, type))
									.FirstOrDefault();
			}
			return result;
		}

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object LoadAsset(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			Object result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(nameWithVariant, name);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.Select(x=>AssetDatabase.LoadMainAssetAtPath(x))
									.FirstOrDefault();
			}
			return result;
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync<T>(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAsset(name, typeof(T))
												);
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name, System.Type type) {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAsset(name, type)
												);
		}

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetAsync(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAsset(name)
												);
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override T[] LoadAssetWithSubAssets<T>(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			T[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(nameWithVariant, name);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.SelectMany(x=>AssetDatabase.LoadAllAssetRepresentationsAtPath(x))
									.Where(x=>x is T)
									.Select(x=>(T)x)
									.ToArray();
			}
			return result;
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name, System.Type type) {
			if (!m_IsDone) throw new System.NullReferenceException();

			Object[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.SelectMany(x=>AssetDatabase.LoadAllAssetRepresentationsAtPath(x))
									.Where(x=>type.IsAssignableFrom(x.GetType()))
									.ToArray();
			}
			if (result == null) result = new Object[0];
			return result;
		}

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>該当するアセット</returns>
		public override UnityEngine.Object[] LoadAssetWithSubAssets(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			Object[] result = null;
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(nameWithVariant);
			if ((assetPaths != null) && (0 < assetPaths.Length)) {
				result = assetPaths.Where(x=>!x.EndsWith(".unity"))
									.SelectMany(x=>AssetDatabase.LoadAllAssetRepresentationsAtPath(x))
									.ToArray();
			}
			if (result == null) result = new Object[0];
			return result;
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAssetWithSubAssets(name, typeof(T))
												);
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name, System.Type type) {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAssetWithSubAssets(name, type)
												);
		}

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>アセットバンドルリクエスト</returns>
		public override IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name) {
			if (!m_IsDone) throw new System.NullReferenceException();

			return new AssetBundleRequestEditor(manager
												, this
												, LoadAssetWithSubAssets(name)
												);
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// オンラインプロセス開始イベント
		/// </summary>
		/// <returns>コルーチン</returns>
		protected override System.Collections.IEnumerator OnStartedOnlineProcess() {
			m_ErrorCode = AssetBundleErrorCode.Null;

			m_DownloadWork = new DownloadWork{
				progress = 0.0f
			};
			yield return manager.editor.AsyncEmulation(x=>m_DownloadWork.progress = x);
			manager.editor.CachedInDirectAssetsLoad(nameWithVariant);
			
			yield return base.OnStartedOnlineProcess();
		}

		/// <summary>
		/// ダウンロード終了イベント
		/// </summary>
		protected override void OnDownloadFinished() {
			m_IsDone = true;
			m_DownloadWork = null;

			base.OnDownloadFinished();
		}

		/// <summary>
		/// 破棄イベント
		/// </summary>
		protected override void OnDestroy() {
			m_IsDone = false;

			base.OnDestroy();
		}

		#endregion
		#region Private types

		/// <summary>
		/// ダウンロード作業領域
		/// </summary>
		private class DownloadWork {
			public float progress;
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 完了確認
		/// </summary>
		private bool m_IsDone;

		/// <summary>
		/// ダウンロード作業領域
		/// </summary>
		private DownloadWork m_DownloadWork;

		/// <summary>
		/// エラーコード
		/// </summary>
		private AssetBundleErrorCode m_ErrorCode = AssetBundleErrorCode.Null;

		#endregion
	}
}
#endif
