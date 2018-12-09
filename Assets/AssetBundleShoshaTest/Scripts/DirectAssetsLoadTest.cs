// Created by ERAL
// This is free and unencumbered software released into the public domain.

#if UNITY_EDITOR
namespace AssetBundleShoshaTest {
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEngine.TestTools;
	using NUnit.Framework;
	using System.Collections;
	using AssetBundleShosha;
	using AssetBundleShosha.Internal;

	public class DirectAssetsLoadTest : LoadTestCommon {
		#region Public methods

		/// <summary>
		/// ワンタイム準備
		/// </summary>
		[OneTimeSetUp]
		public new void OneTimeSetUp() {
			base.OneTimeSetUp();

			m_ServerEmulationOld = AssetBundleUtility.serverEmulation;
			AssetBundleUtility.serverEmulation = AssetBundleUtility.ServerEmulation.LoadAssetsDirect;
		}

		/// <summary>
		/// ワンタイム破棄
		/// </summary>
		[OneTimeTearDown]
		public new void OneTimeTearDown() {
			AssetBundleUtility.serverEmulation = m_ServerEmulationOld;

			base.OneTimeTearDown();
		}

		/// <summary>
		/// アセットバンドルマネージャー初期化
		/// </summary>
		/// <returns></returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator Initialize() {
			if (CreateAssetBundleManagerInstance()) {
				yield return InitializeAssetBundleManager();
			}
			yield break;
		}

		/// <summary>
		/// 単体アセットバンドル読み込み
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator LoadIndependentAssetBundle() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return LoadIndependentAssetBundle(assetBundleManager);
		}

		/// <summary>
		/// 依存アセットバンドル読み込み
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator LoadDependenciesAssetBundle() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return LoadDependenciesAssetBundle(assetBundleManager);
		}

		/// <summary>
		/// 暗号化アセットバンドル読み込み
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator LoadCryptoAssetBundle() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return LoadCryptoAssetBundle(assetBundleManager);
		}

		/// <summary>
		/// 配信ストリーミングアセット読み込み
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator LoadDeliveryStreamingAssets() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return LoadDeliveryStreamingAssets(assetBundleManager);
		}

		/// <summary>
		/// ダウンロードサイズ取得
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator DownloadSize() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return DownloadSize(assetBundleManager);
		}

		/// <summary>
		/// 全ダウンロード
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(20000)]
		public IEnumerator DownloadAll() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return DownloadAll(assetBundleManager);
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// テスト実行前サーバーエミュレーション値
		/// </summary>
		AssetBundleUtility.ServerEmulation m_ServerEmulationOld;

		#endregion
		#region Private methods
		#endregion
	}
}
#endif
