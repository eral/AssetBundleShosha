// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaDemo {
	using UnityEngine;
	using UnityEditor;
	using AssetBundleShosha.Editor;

	public static class DemoAssetBundlePostprocessor {
		[AssetBundlePostprocessor(0)]
		private static void Postprocessor(AssetBundlePostprocessorArg arg) {
			//Missingファイルをダウンロードエラーにする為に削除する
			var missingAssetBundlePath = arg.GetAssetBundlePath("AssetBundleShoshaDemo/Missing");
			if (AssetBundleEditorUtility.Exists(missingAssetBundlePath)) {
				FileUtil.DeleteFileOrDirectory(missingAssetBundlePath);
			}
		}
	}
}
