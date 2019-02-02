// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaToolchain.Editor {
	using UnityEngine;
	using UnityEditor;
	using System.IO;
	using System.Reflection;

	public static class ToolchainMenuItems {
		#region Public methods

		/// <summary>
		/// AssetBundleShoshaのエクスポート
		/// </summary>
		[MenuItem("AssetBundleShoshaToolchain/Export Package/AssetBundleShosha", false, 30010)]
		public static void ExportPackageAssetBundleShosha() {
			ExportPackage("Assets/AssetBundleShosha"
						, "Release/AssetBundleShosha-{0}.unitypackage"
						, "AssetBundleShosha"
						, "AssetBundleShosha.Editor"
						);
		}

		/// <summary>
		/// AssetBundleShoshaDemoのエクスポート
		/// </summary>
		[MenuItem("AssetBundleShoshaToolchain/Export Package/AssetBundleShoshaDemo", false, 30020)]
		public static void ExportPackageAssetBundleShoshaDemo() {
			ExportPackage("Assets/AssetBundleShoshaDemo"
						, "Release/AssetBundleShoshaDemo-{0}.unitypackage"
						, "AssetBundleShoshaDemo"
						, "AssetBundleShoshaDemo.Editor"
						);
		}

		#endregion
		#region Private methods

		/// <summary>
		/// ディレクトリ作成
		/// </summary>
		/// <param name="fullPath">作成するディレクトリのフルパス</param>
		/// <param name="excludeLastName">最後の名前を除外</param>
		public static void CreateDirectory(string fullPath, bool excludeLastName = false) {
			if (excludeLastName) {
				fullPath = Path.GetDirectoryName(fullPath);
			}
			if (Directory.Exists(fullPath)) {
				//ディレクトリが在るなら
				//empty.
			} else {
				//ディレクトリが無いなら
				var parentFullPath = Path.GetDirectoryName(fullPath);
				if (Directory.Exists(parentFullPath)) {
					//親ディレクトリが無いなら
					CreateDirectory(parentFullPath, false);
				}
				Directory.CreateDirectory(fullPath);
			}
		}

		/// <summary>
		/// アセンブリバージョンの取得
		/// </summary>
		/// <param name="assemblyName">アセンブリ名</param>
		/// <returns>バージョン</returns>
		private static System.Version GetAssemblyVersion(string assemblyName) {
			return Assembly.Load(assemblyName).GetName().Version;
		}

		/// <summary>
		/// パッケージエクスポート
		/// </summary>
		/// <param name="assetPathName">エクスポートするアセットのパス</param>
		/// <param name="filePath">エクスポート先(「{0}」にバージョンが挿入される)</param>
		/// <param name="runtimeAssemblyName">ランタイムアセンブリ名</param>
		/// <param name="editorAssemblyName">エディタアセンブリ名</param>
		/// <returns>true:成功、false:失敗</returns>
		private static bool ExportPackage(string assetPathName, string filePath, string runtimeAssemblyName, string editorAssemblyName) {
			var result = true;
			var version = GetAssemblyVersion(runtimeAssemblyName);
			var editorVersion = GetAssemblyVersion(editorAssemblyName);
			if (version != editorVersion) {
				var message = "ランタイムアセンブリとエディタアセンブリのバージョンが違います。\nエクスポートしますか?\n\nランタイムアセンブリバージョン:" + version + "\nエディタアセンブリバージョン　:" + editorVersion;
				if (!EditorUtility.DisplayDialog("アセンブリバージョンの不一致", message, "Export", "Cancel")) {
					result = false;
				}
			}
			if (result) {
				var fileName = string.Format(filePath, version);
				var fullPath = Application.dataPath + "/../" + fileName;
				CreateDirectory(fullPath, true);
				var flags = ExportPackageOptions.Recurse;
				AssetDatabase.ExportPackage(assetPathName, fileName, flags);
			}
			return result;
		}

		#endregion
	}
}
