// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaTest {
	using UnityEngine;
	using UnityEditor;
	using AssetBundleShosha.Editor;

	public static class TestAssetBundleCatalogPostprocessor {
		[AssetBundleCatalogPostprocessor(0)]
		private static void Postprocessor(AssetBundleCatalogPostprocessorArg arg) {
			arg.catalog.userData = new byte[]{0, 1, 2, 3, 4, 5, 6, 7};
		}
	}

	public static class TestAssetBundlePostprocessor {
		[AssetBundlePostprocessor(0)]
		private static void Postprocessor(AssetBundlePostprocessorArg arg) {
			//Missingファイルをダウンロードエラーにする為に削除する
			var missingAssetBundlePath = arg.GetAssetBundlePath("AssetBundleShoshaTest/MissingAssetBundle");
			if (AssetBundleEditorUtility.Exists(missingAssetBundlePath)) {
				FileUtil.DeleteFileOrDirectory(missingAssetBundlePath);
			}

			var missingDeliveryStreamingAssetPath = arg.GetAssetBundlePath("DeliveryStreamingAssets:AssetBundleShoshaTest/MissingDeliveryStreamingAssets");
			if (AssetBundleEditorUtility.Exists(missingDeliveryStreamingAssetPath)) {
				FileUtil.DeleteFileOrDirectory(missingDeliveryStreamingAssetPath);
			}
		}
	}
}
