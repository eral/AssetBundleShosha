// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaTest {
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEngine.TestTools;
	using NUnit.Framework;
	using System.Collections;
	using AssetBundleShosha;
	using AssetBundleShosha.Internal;

	public class AssetBundleLoadTest : LoadTestCommon {
		#region Public methods

		/// <summary>
		/// ワンタイム準備
		/// </summary>
		[OneTimeSetUp]
		public new void OneTimeSetUp() {
			base.OneTimeSetUp();

#if UNITY_EDITOR
			m_ServerEmulationOld = AssetBundleUtility.serverEmulation;
			AssetBundleUtility.serverEmulation = AssetBundleUtility.ServerEmulation.None;
#endif
		}

		/// <summary>
		/// ワンタイム破棄
		/// </summary>
		[OneTimeTearDown]
		public new void OneTimeTearDown() {
#if UNITY_EDITOR
			AssetBundleUtility.serverEmulation = m_ServerEmulationOld;
#endif

			base.OneTimeTearDown();
		}

		/// <summary>
		/// アセットバンドルマネージャー初期化
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator Initialize() {
			if (CreateAssetBundleManagerInstance()) {
				yield return InitializeAssetBundleManager();
			}
			yield break;
		}

		/// <summary>
		/// カタログ内ユーザーデータ値
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator UserDataValueTestInCatalog() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return UserDataValueTestInCatalog(assetBundleManager);
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

		/// <summary>
		/// アセットバンドル読み込みエラー(共通)
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator CommonErrorAssetBundle() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return CommonErrorAssetBundle(assetBundleManager);
		}

		/// <summary>
		/// アセットバンドル読み込みエラー(カスタム)
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator CustomErrorAssetBundle() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return CustomErrorAssetBundle(assetBundleManager);
		}

		/// <summary>
		/// 配信ストリーミングアセット読み込みエラー(共通)
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator CommonErrorDeliveryStreamingAssets() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return CommonErrorDeliveryStreamingAssets(assetBundleManager);
		}

		/// <summary>
		/// 配信ストリーミングアセット読み込みエラー(カスタム)
		/// </summary>
		/// <returns>コルーチン</returns>
		[UnityTest] [Timeout(4000)]
		public IEnumerator CustomErrorDeliveryStreamingAssets() {
			yield return SetupAssetBundleManager();
			var assetBundleManager = AssetBundleManager.Instance;

			yield return CustomErrorDeliveryStreamingAssets(assetBundleManager);
		}

		#endregion
		#region Private fields and properties

#if UNITY_EDITOR
		/// <summary>
		/// テスト実行前サーバーエミュレーション値
		/// </summary>
		AssetBundleUtility.ServerEmulation m_ServerEmulationOld;
#endif

		#endregion
		#region Private methods
		#endregion
	}
}
