// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaToolchain.Editor {
	using UnityEngine;
	using UnityEditor;
	using System.IO;

	public static class ToolchainMenuItems {
		#region Public methods

		/// <summary>
		/// AssetBundleShoshaのエクスポート
		/// </summary>
		[MenuItem("AssetBundleShoshaToolchain/Export Package/AssetBundleShosha", false, 30010)]
		public static void ExportPackageAssetBundleShosha() {
			CreateReleaseDirectory();

			var assetPathName = "Assets/AssetBundleShosha";
			var version = AssetDatabase.LoadAssetAtPath<Version>("Assets/AssetBundleShoshaToolchain/Assets/AssetBundleShoshaVersion.asset").version;
			var fileName = "Release/AssetBundleShosha-" + version + ".unitypackage";
			var flags = ExportPackageOptions.Recurse;
			AssetDatabase.ExportPackage(assetPathName, fileName, flags);
		}

		/// <summary>
		/// AssetBundleShoshaDemoのエクスポート
		/// </summary>
		[MenuItem("AssetBundleShoshaToolchain/Export Package/AssetBundleShoshaDemo", false, 30020)]
		public static void ExportPackageAssetBundleShoshaDemo() {
			CreateReleaseDirectory();

			var assetPathName = "Assets/AssetBundleShoshaDemo";
			var version = AssetDatabase.LoadAssetAtPath<Version>("Assets/AssetBundleShoshaToolchain/Assets/AssetBundleShoshaDemoVersion.asset").version;
			var fileName = "Release/AssetBundleShoshaDemo-" + version + ".unitypackage";
			var flags = ExportPackageOptions.Recurse;
			AssetDatabase.ExportPackage(assetPathName, fileName, flags);
		}

		#endregion
		#region Private methods

		/// <summary>
		/// Releaseディレクトリ作成
		/// </summary>
		private static void CreateReleaseDirectory() {
			var fullPath = Application.dataPath + "/../Release";
			if (!Directory.Exists(fullPath)) {
				Directory.CreateDirectory(fullPath);
			}
		}
		#endregion
	}
}
