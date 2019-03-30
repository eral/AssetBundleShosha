// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaTest {
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEngine.TestTools;
	using NUnit.Framework;
	using System.Collections.Generic;
	using System.Collections;
	using System.Linq;
	using AssetBundleShosha;
	using AssetBundleShosha.Internal;

	public class LoadTestCommon {
		#region Protected methods

		/// <summary>
		/// ワンタイム準備
		/// </summary>
		protected void OneTimeSetUp() {
			m_TestScene = SceneManager.CreateScene(GetType().Name);
			SceneManager.SetActiveScene(m_TestScene);
		}

		/// <summary>
		/// ワンタイム破棄
		/// </summary>
		protected void OneTimeTearDown() {
#pragma warning disable 618
			SceneManager.UnloadScene(m_TestScene);
#pragma warning restore 618  
			AssetBundle.UnloadAllAssetBundles(true);
			Resources.UnloadUnusedAssets();
			Caching.ClearCache();
			System.GC.Collect();
		}

		/// <summary>
		/// カタログ内ユーザーデータ値
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator UserDataValueTestInCatalog(AssetBundleManager assetBundleManager) {
			Assert.AreEqual(new byte[]{0, 1, 2, 3, 4, 5, 6, 7}, assetBundleManager.catalog.userData);
			yield break;
		}

		/// <summary>
		/// 単体アセットバンドル読み込み
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator LoadIndependentAssetBundle(AssetBundleManager assetBundleManager) {
			var isloaded = false;
			var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/Primitive", x=>isloaded = true);
			Assert.IsTrue(assetBundle != null);
			yield return assetBundle;
			Assert.IsTrue(isloaded);

			Assert.IsTrue(assetBundle.mainAsset == null);
			Assert.IsTrue(assetBundle.Contains("Primitive"));
			Assert.IsFalse(assetBundle.Contains("test"));
			Assert.IsFalse(assetBundle.Contains("PrimitiveCircle"));
			var allAssetNames = assetBundle.GetAllAssetNames();
			Assert.IsTrue(allAssetNames != null);
			Assert.AreEqual(1, allAssetNames.Length);
			Assert.AreEqual("assets/assetbundleshoshatest/sprites/primitive.png", allAssetNames[0]);
			var allScenePaths = assetBundle.GetAllScenePaths();
			Assert.IsTrue(allScenePaths != null);
			Assert.AreEqual(0, allScenePaths.Length);

			IAssetBundleRequest assetBundleRequest;

			isloaded = false;
			assetBundleRequest = null;
			Assert.DoesNotThrow(()=>{
				assetBundleRequest = assetBundle.LoadAllAssetsAsync<Texture>();
			});
			Assert.IsTrue(assetBundleRequest != null);
			assetBundleRequest.completed += x=>{
				isloaded = true;
			};
			yield return assetBundleRequest;
			for (var i = 0; i < 2; ++i) {
				if (isloaded) {
					break;
				}
				yield return null; //コルーチンを抜けてからcompletedが呼ばれる迄に2f掛かる
			}
			Assert.IsTrue(isloaded);
			Assert.IsTrue(assetBundleRequest.asset != null);
			Assert.IsTrue(assetBundleRequest.allAssets != null);
			Assert.AreEqual(1, assetBundleRequest.allAssets.Length);

			assetBundleRequest = null;
			Assert.DoesNotThrow(()=>{
				assetBundleRequest = assetBundle.LoadAllAssetsAsync<Sprite>();
			});
			Assert.IsTrue(assetBundleRequest != null);
			yield return assetBundleRequest;
			Assert.IsTrue(assetBundleRequest.asset != null);
			Assert.IsTrue(assetBundleRequest.allAssets != null);
			Assert.AreEqual(4, assetBundleRequest.allAssets.Length);

			assetBundle.Dispose();

			yield break;
		}

		/// <summary>
		/// 依存アセットバンドル読み込み
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator LoadDependenciesAssetBundle(AssetBundleManager assetBundleManager) {
			assetBundleManager.SetVariants(new[]{"blue"});

			try {
				var isloaded = false;
				var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/ColorMaterials", x=>isloaded = true);
				Assert.IsTrue(assetBundle != null);
				yield return assetBundle;
				Assert.IsTrue(isloaded);

				IAssetBundleRequest assetBundleRequest;

				isloaded = false;
				assetBundleRequest = null;
				Assert.DoesNotThrow(()=>{
					assetBundleRequest = assetBundle.LoadAssetAsync<Material>("Circle");
				});
				Assert.IsTrue(assetBundleRequest != null);
				assetBundleRequest.completed += x=>isloaded = true;
				yield return assetBundleRequest;
				for (var i = 0; i < 2; ++i) {
					if (isloaded) {
						break;
					}
					yield return null; //コルーチンを抜けてからcompletedが呼ばれる迄に2f掛かる
				}
				Assert.IsTrue(isloaded);
				Assert.IsTrue(assetBundleRequest.asset != null);
				Assert.IsTrue(assetBundleRequest.allAssets != null);
				Assert.AreEqual(1, assetBundleRequest.allAssets.Length);
				var material = (Material)assetBundleRequest.asset;
				Assert.IsTrue(material != null);
				Assert.IsTrue(material.mainTexture != null);

				assetBundle.Dispose();
			} finally {
				assetBundleManager.SetVariants(null);
			}

			yield break;
		}

		/// <summary>
		/// 暗号化アセットバンドル読み込み
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator LoadCryptoAssetBundle(AssetBundleManager assetBundleManager) {
			var isloaded = false;
			var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/CryptoAssetBundle", x=>isloaded = true);
			Assert.IsTrue(assetBundle != null);
			yield return assetBundle;
			Assert.IsTrue(isloaded);

			Assert.IsTrue(assetBundle.mainAsset == null);
			Assert.IsTrue(assetBundle.Contains("CryptoAssetBundle"));
			var allAssetNames = assetBundle.GetAllAssetNames();
			Assert.IsTrue(allAssetNames != null);
			Assert.AreEqual(1, allAssetNames.Length);
			Assert.AreEqual("assets/assetbundleshoshatest/textures/cryptoassetbundle.png", allAssetNames[0]);
			var allScenePaths = assetBundle.GetAllScenePaths();
			Assert.IsTrue(allScenePaths != null);
			Assert.AreEqual(0, allScenePaths.Length);

			IAssetBundleRequest assetBundleRequest;

			isloaded = false;
			assetBundleRequest = null;
			Assert.DoesNotThrow(()=>{
				assetBundleRequest = assetBundle.LoadAllAssetsAsync<Texture>();
			});
			Assert.IsTrue(assetBundleRequest != null);
			assetBundleRequest.completed += x=>{
				isloaded = true;
			};
			yield return assetBundleRequest;
			for (var i = 0; i < 2; ++i) {
				if (isloaded) {
					break;
				}
				yield return null; //コルーチンを抜けてからcompletedが呼ばれる迄に2f掛かる
			}
			Assert.IsTrue(isloaded);
			Assert.IsTrue(assetBundleRequest.asset != null);
			Assert.IsTrue(assetBundleRequest.allAssets != null);
			Assert.AreEqual(1, assetBundleRequest.allAssets.Length);

			assetBundle.Dispose();

			yield break;
		}

		/// <summary>
		/// 直接参照系除外アセットバンドル読み込み
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator LoadDirectExcludeAssetBundle(AssetBundleManager assetBundleManager) {
			var downloadTimeoutSecondsOld = assetBundleManager.downloadTimeoutSeconds;
			assetBundleManager.downloadTimeoutSeconds = 4;

			var isloaded = false;
			AssetBundleErrorCode? errorCode = null;
			var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/ExcldeMatrials", x=>isloaded = true);
			assetBundle.errorHandler = new ErrorAction(x=>errorCode = x.errorCode);
			Assert.IsTrue(assetBundle != null);
			yield return assetBundle;
			Assert.IsTrue(isloaded);
			Assert.IsTrue(errorCode.HasValue);
			Assert.AreNotEqual(AssetBundleErrorCode.Null, assetBundle.errorCode);

			assetBundle.Dispose();

			yield break;
		}

		/// <summary>
		/// 間接参照系除外アセットバンドル読み込み
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator LoadIndirectExcludeAssetBundle(AssetBundleManager assetBundleManager) {
			var isloaded = false;
			var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/IndirectExcldeMatrials", x=>isloaded = true);
			Assert.IsTrue(assetBundle != null);
			yield return assetBundle;
			Assert.IsTrue(isloaded);
			Assert.AreEqual(AssetBundleErrorCode.Null, assetBundle.errorCode);

			Assert.IsTrue(assetBundle.Contains("Exclude"));
			var allAssetNames = assetBundle.GetAllAssetNames();
			Assert.IsTrue(allAssetNames != null);
			Assert.AreEqual(1, allAssetNames.Length);
			Assert.AreEqual("assets/assetbundleshoshatest/materials/indirectexcludematerials/exclude.mat", allAssetNames[0]);
			var allScenePaths = assetBundle.GetAllScenePaths();
			Assert.IsTrue(allScenePaths != null);
			Assert.AreEqual(0, allScenePaths.Length);

			IAssetBundleRequest assetBundleRequest;

			isloaded = false;
			assetBundleRequest = null;
			Assert.DoesNotThrow(()=>{
				assetBundleRequest = assetBundle.LoadAssetAsync<Material>("Exclude");
			});
			Assert.IsTrue(assetBundleRequest != null);
			assetBundleRequest.completed += x=>{
				isloaded = true;
			};
			yield return assetBundleRequest;
			for (var i = 0; i < 2; ++i) {
				if (isloaded) {
					break;
				}
				yield return null; //コルーチンを抜けてからcompletedが呼ばれる迄に2f掛かる
			}
			Assert.IsTrue(isloaded);
			Assert.IsTrue(assetBundleRequest.asset != null);
			Assert.IsTrue(assetBundleRequest.allAssets != null);
			Assert.AreEqual(1, assetBundleRequest.allAssets.Length);

			var material = (Material)assetBundleRequest.asset;
			Assert.IsTrue(material.mainTexture == null);

			assetBundle.Dispose();

			yield break;
		}

		/// <summary>
		/// 配信ストリーミングアセット読み込み
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator LoadDeliveryStreamingAssets(AssetBundleManager assetBundleManager) {
			var isloaded = false;
			var deliveryStreamingAsset = assetBundleManager.LoadAssetBundle("DeliveryStreamingAssets:AssetBundleShoshaTest/binary", x=>isloaded = true);
			Assert.IsTrue(deliveryStreamingAsset != null);
			yield return deliveryStreamingAsset;
			Assert.IsTrue(isloaded);

			var deliveryStreamingAssetPath = deliveryStreamingAsset.deliveryStreamingAssetPath;
			Assert.IsFalse(string.IsNullOrEmpty(deliveryStreamingAssetPath));

			byte[] deliveryStreamingAssetContents = null;
			Assert.DoesNotThrow(()=>{
				deliveryStreamingAssetContents = System.IO.File.ReadAllBytes(deliveryStreamingAssetPath);
			});
			Assert.IsTrue(deliveryStreamingAssetContents != null);
			Assert.AreEqual(256, deliveryStreamingAssetContents.Length);
			for (var i = 0; i < 256; ++i) {
				Assert.AreEqual(i, deliveryStreamingAssetContents[i]);
			}

			deliveryStreamingAsset.Dispose();

			yield break;
		}

		/// <summary>
		/// ダウンロードサイズ取得
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator DownloadSize(AssetBundleManager assetBundleManager) {
			Assert.IsTrue(assetBundleManager.ClearCache());
			assetBundleManager.SetVariants(new[]{"blue"});

			{
				var size = assetBundleManager.GetDownloadSize("AssetBundleShoshaTest/Primitive"
																, "AssetBundleShoshaTest/ColorMaterials"
																, "DeliveryStreamingAssets:AssetBundleShoshaTest/binary"
																);
				Assert.Greater(size, 0);
			}

			{
				var size = assetBundleManager.GetDownloadSize("AssetBundleShoshaTest/Primitive");
				Assert.Greater(size, 0);
				var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/Primitive");
				yield return assetBundle;
				assetBundle.Dispose();
				size = assetBundleManager.GetDownloadSize("AssetBundleShoshaTest/Primitive");
				Assert.AreEqual(0, size);
			}

			{
				var size = assetBundleManager.GetDownloadSize("AssetBundleShoshaTest/ColorMaterials");
				Assert.Greater(size, 0);
				var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/ColorMaterials");
				yield return assetBundle;
				assetBundle.Dispose();
				size = assetBundleManager.GetDownloadSize("AssetBundleShoshaTest/ColorMaterials");
				Assert.AreEqual(0, size);
			}

			{
				var size = assetBundleManager.GetDownloadSize("DeliveryStreamingAssets:AssetBundleShoshaTest/binary");
				Assert.Greater(size, 0);
				var assetBundle = assetBundleManager.LoadAssetBundle("DeliveryStreamingAssets:AssetBundleShoshaTest/binary");
				yield return assetBundle;
				assetBundle.Dispose();
				size = assetBundleManager.GetDownloadSize("DeliveryStreamingAssets:AssetBundleShoshaTest/binary");
				Assert.AreEqual(0, size);
			}

			var assetBundleNames = new List<string>{"AssetBundleShoshaTest/Primitive"
												, "AssetBundleShoshaTest/ColorMaterials"
												, "DeliveryStreamingAssets:AssetBundleShoshaTest/binary"
												};
			Assert.AreEqual(0, assetBundleManager.GetDownloadSize(assetBundleNames));

			assetBundleManager.SetVariants(null);
			yield break;
		}

		/// <summary>
		/// 全ダウンロード
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		public IEnumerator DownloadAll(AssetBundleManager assetBundleManager) {
			Assert.IsTrue(assetBundleManager.ClearCache());
			assetBundleManager.SetVariants(new[]{"blue", "variant1"});

			System.Func<AssetBundleManager.DownloadAllAssetBundlesFlags, long> GetMissingSize = f=>{
				var r = 0L;
				if ((f & AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeAssetBundles) == 0) {
					r += assetBundleManager.GetDownloadSize("AssetBundleShoshaTest/MissingAssetBundle");
				}
				if ((f & AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeDeliveryStreamingAssets) == 0) {
					r += assetBundleManager.GetDownloadSize("DeliveryStreamingAssets:AssetBundleShoshaTest/MissingDeliveryStreamingAssets");
				}
				return r;
			};
			long missingSize;

			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(), 0);

			yield return assetBundleManager.DownloadAllAssetBundles();

			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			missingSize = GetMissingSize(AssetBundleManager.DownloadAllAssetBundlesFlags.Null);
			Assert.AreEqual(missingSize, assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles());
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants), missingSize);

			assetBundleManager.SetVariants(new[]{"red", "variant2"});

			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));

			Assert.IsTrue(assetBundleManager.ClearCache());
			assetBundleManager.SetVariants(new[]{"blue", "variant1"});

			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(), 0);

			yield return assetBundleManager.DownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeAssetBundles);

			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			missingSize = GetMissingSize(AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeAssetBundles);
			Assert.AreEqual(missingSize, assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeAssetBundles));
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(), 0);
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants), 0);

			Assert.IsTrue(assetBundleManager.ClearCache());
			assetBundleManager.SetVariants(new[]{"blue", "variant1"});

			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(), 0);

			yield return assetBundleManager.DownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeDeliveryStreamingAssets);

			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			missingSize = GetMissingSize(AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeDeliveryStreamingAssets);
			Assert.AreEqual(missingSize, assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.ExcludeDeliveryStreamingAssets));
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(), 0);
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants), 0);

			Assert.IsTrue(assetBundleManager.ClearCache());
			assetBundleManager.SetVariants(new[]{"blue", "variant1"});

			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsFalse(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsFalse(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			Assert.Greater(assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(), 0);

			yield return assetBundleManager.DownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants);

			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/Primitive"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterialsCommon"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/CryptoAssetBundle"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/binary"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/DeliveryStreamingAssets"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			missingSize = GetMissingSize(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants);
			Assert.AreEqual(missingSize, assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants));
			Assert.AreEqual(missingSize, assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles());

			assetBundleManager.SetVariants(new[]{"red", "variant2"});

			Assert.IsTrue(assetBundleManager.HasCache("AssetBundleShoshaTest/ColorMaterials"));
			Assert.IsTrue(assetBundleManager.HasCache("DeliveryStreamingAssets:AssetBundleShoshaTest/VariantStream"));
			missingSize = GetMissingSize(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants);
			Assert.AreEqual(missingSize, assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles(AssetBundleManager.DownloadAllAssetBundlesFlags.IncludeOutOfVariants));
			Assert.AreEqual(missingSize, assetBundleManager.GetDownloadSizeByDownloadAllAssetBundles());
		}

		/// <summary>
		/// アセットバンドル読み込みエラー(共通)
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator CommonErrorAssetBundle(AssetBundleManager assetBundleManager) {
			IErrorHandler errorHandlerOld = assetBundleManager.errorHandler;
			IErrorHandle errorHandleCommon;
			assetBundleManager.errorHandler = new ErrorAction(x=>errorHandleCommon = x);

			try {
				errorHandleCommon = null;
				var isloaded = false;
				var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/MissingAssetBundle", x=>isloaded = true);
				Assert.IsTrue(assetBundle != null);
				while (!assetBundle.isDone) {
					if (errorHandleCommon != null) {
						break;
					}
					yield return null;
				}
				Assert.IsFalse(assetBundle.isDone);
				Assert.IsFalse(isloaded);
				Assert.IsTrue(errorHandleCommon != null);
				errorHandleCommon.Ignore();
				while (!assetBundle.isDone) {
					yield return null;
				}
				Assert.IsTrue(assetBundle.isDone);
				Assert.IsTrue(isloaded);
				Assert.Throws<System.NullReferenceException>(()=>{
					assetBundle.LoadAllAssets();
				});
				Assert.Throws<System.NullReferenceException>(()=>{
					assetBundle.LoadAllAssetsAsync();
				});

				assetBundle.Dispose();
			} finally {
				assetBundleManager.errorHandler = errorHandlerOld;
			}

			yield break;
		}

		/// <summary>
		/// アセットバンドル読み込みエラー(カスタム)
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator CustomErrorAssetBundle(AssetBundleManager assetBundleManager) {
			IErrorHandler errorHandlerOld = assetBundleManager.errorHandler;
			IErrorHandle errorHandleCommon;
			assetBundleManager.errorHandler = new ErrorAction(x=>errorHandleCommon = x);

			try {
				errorHandleCommon = null;
				var isloaded = false;
				var assetBundle = assetBundleManager.LoadAssetBundle("AssetBundleShoshaTest/MissingAssetBundle", x=>isloaded = true);
				IErrorHandle errorHandle = null;
				assetBundle.errorHandler = new ErrorAction(x=>errorHandle = x);
				Assert.IsTrue(assetBundle != null);
				while (!assetBundle.isDone) {
					if (errorHandle != null) {
						break;
					}
					if (errorHandleCommon != null) {
						break;
					}
					yield return null;
				}
				Assert.IsFalse(assetBundle.isDone);
				Assert.IsFalse(isloaded);
				Assert.IsTrue(errorHandle != null);
				Assert.IsTrue(errorHandleCommon == null);
				errorHandle.Ignore();
				while (!assetBundle.isDone) {
					yield return null;
				}
				Assert.IsTrue(assetBundle.isDone);
				Assert.IsTrue(isloaded);
				Assert.Throws<System.NullReferenceException>(()=>{
					assetBundle.LoadAllAssets();
				});
				Assert.Throws<System.NullReferenceException>(()=>{
					assetBundle.LoadAllAssetsAsync();
				});

				assetBundle.Dispose();
			} finally {
				assetBundleManager.errorHandler = errorHandlerOld;
			}

			yield break;
		}

		/// <summary>
		/// 配信ストリーミングアセット読み込みエラー(共通)
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator CommonErrorDeliveryStreamingAssets(AssetBundleManager assetBundleManager) {
			IErrorHandler errorHandlerOld = assetBundleManager.errorHandler;
			IErrorHandle errorHandleCommon;
			assetBundleManager.errorHandler = new ErrorAction(x=>errorHandleCommon = x);

			try {
				errorHandleCommon = null;
				var isloaded = false;
				var assetBundle = assetBundleManager.LoadAssetBundle("DeliveryStreamingAssets:AssetBundleShoshaTest/MissingDeliveryStreamingAssets", x=>isloaded = true);
				Assert.IsTrue(assetBundle != null);
				while (!assetBundle.isDone) {
					if (errorHandleCommon != null) {
						break;
					}
					yield return null;
				}
				Assert.IsFalse(assetBundle.isDone);
				Assert.IsFalse(isloaded);
				Assert.IsTrue(errorHandleCommon != null);
				errorHandleCommon.Ignore();
				while (!assetBundle.isDone) {
					yield return null;
				}
				Assert.IsTrue(assetBundle.isDone);
				Assert.IsTrue(isloaded);
				Assert.IsTrue(string.IsNullOrEmpty(assetBundle.deliveryStreamingAssetPath));

				assetBundle.Dispose();
			} finally {
				assetBundleManager.errorHandler = errorHandlerOld;
			}

			yield break;
		}

		/// <summary>
		/// 配信ストリーミングアセット読み込みエラー(カスタム)
		/// </summary>
		/// <param name="assetBundleManager">アセットバンドルマネージャー</param>
		/// <returns>コルーチン</returns>
		protected IEnumerator CustomErrorDeliveryStreamingAssets(AssetBundleManager assetBundleManager) {
			IErrorHandler errorHandlerOld = assetBundleManager.errorHandler;
			IErrorHandle errorHandleCommon;
			assetBundleManager.errorHandler = new ErrorAction(x=>errorHandleCommon = x);

			try {
				errorHandleCommon = null;
				var isloaded = false;
				var assetBundle = assetBundleManager.LoadAssetBundle("DeliveryStreamingAssets:AssetBundleShoshaTest/MissingDeliveryStreamingAssets", x=>isloaded = true);
				IErrorHandle errorHandle = null;
				assetBundle.errorHandler = new ErrorAction(x=>errorHandle = x);
				Assert.IsTrue(assetBundle != null);
				while (!assetBundle.isDone) {
					if (errorHandle != null) {
						break;
					}
					if (errorHandleCommon != null) {
						break;
					}
					yield return null;
				}
				Assert.IsFalse(assetBundle.isDone);
				Assert.IsFalse(isloaded);
				Assert.IsTrue(errorHandle != null);
				Assert.IsTrue(errorHandleCommon == null);
				errorHandle.Ignore();
				while (!assetBundle.isDone) {
					yield return null;
				}
				Assert.IsTrue(assetBundle.isDone);
				Assert.IsTrue(isloaded);
				Assert.IsTrue(string.IsNullOrEmpty(assetBundle.deliveryStreamingAssetPath));

				assetBundle.Dispose();
			} finally {
				assetBundleManager.errorHandler = errorHandlerOld;
			}

			yield break;
		}

		/// <summary>
		/// AssetBundleManager構築
		/// </summary>
		/// <returns>true:新規作成, false:既存</returns>
		protected bool CreateAssetBundleManagerInstance() {
			var result = false;
			if (AssetBundleManager.Instance == null) {
				new GameObject(typeof(AssetBundleManager).Name, typeof(AssetBundleManager));
				result = true;
			}
			Assert.IsTrue(AssetBundleManager.Instance != null);
			return result;
		}

		/// <summary>
		/// AssetBundleManager初期化
		/// </summary>
		/// <returns>待機用コルーチン</returns>
		protected virtual IEnumerator InitializeAssetBundleManager() {
			var assetBundleManager = AssetBundleManager.Instance;
			Assert.IsTrue(assetBundleManager.ClearCache());
			var isInitialized = false;
			var baseURL = "file:///" + Application.dataPath.Replace('\\', '/') + "/../AssetBundles/";
			yield return assetBundleManager.Initialize(baseURL, ()=>isInitialized = true);
			Assert.IsTrue(isInitialized);
		}

		/// <summary>
		/// AssetBundleManagerセットアップ
		/// </summary>
		/// <returns>待機用コルーチン</returns>
		protected IEnumerator SetupAssetBundleManager() {
			if (CreateAssetBundleManagerInstance()) {
				yield return InitializeAssetBundleManager();
			} else {
				var assetBundleManager = AssetBundleManager.Instance;
				assetBundleManager.downloadTimeoutSeconds = AssetBundleManager.kDownloadTimeoutSecondsDefault;
				assetBundleManager.parallelDownloadsCount = AssetBundleManager.kParallelDownloadsCountDefault;
				assetBundleManager.SetVariants(null);
			}
		}

		#endregion
		#region Private fields and properties

		/// <summary>
		/// テストシーン
		/// </summary>
		Scene m_TestScene;

		#endregion
		#region Private methods
		#endregion
	}
}
