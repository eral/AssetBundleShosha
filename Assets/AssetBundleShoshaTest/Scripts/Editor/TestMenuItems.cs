// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaTest.Editor.Internal {
	using UnityEngine;
	using UnityEditor;
	using AssetBundleShoshaTest.Editor;

	public static class TestMenuItems {
		#region Public methods

		/// <summary>
		/// 現在のプラットフォーム向けアセットバンドル作成
		/// </summary>
		[MenuItem("AssetBundleShoshaTest/Build AssetBundles/Current Platform", false, 30010)]
		public static void BuildAssetBundlesForCurrentPlatform() {
			AssetBundleBuildTest.testBuildEnable = true;
			try {
				EditorApplication.ExecuteMenuItem("Assets/AssetBundles/Build AssetBundles/Current Platform");
			} catch (System.Exception e) {
				throw e;
			} finally {
				AssetBundleBuildTest.testBuildEnable = false;
			}
		}

		/// <summary>
		/// Windows向けアセットバンドル作成
		/// </summary>
		[MenuItem("AssetBundleShoshaTest/Build AssetBundles/Windows", false, 30110)]
		public static void BuildAssetBundlesForWindows() {
			AssetBundleBuildTest.testBuildEnable = true;
			try {
				EditorApplication.ExecuteMenuItem("Assets/AssetBundles/Build AssetBundles/Windows");
			} catch (System.Exception e) {
				throw e;
			} finally {
				AssetBundleBuildTest.testBuildEnable = false;
			}
		}

		/// <summary>
		/// MacOSX向けアセットバンドル作成
		/// </summary>
		[MenuItem("AssetBundleShoshaTest/Build AssetBundles/MacOSX", false, 30120)]
		public static void BuildAssetBundlesForMacOSX() {
			AssetBundleBuildTest.testBuildEnable = true;
			try {
				EditorApplication.ExecuteMenuItem("Assets/AssetBundles/Build AssetBundles/MacOSX");
			} catch (System.Exception e) {
				throw e;
			} finally {
				AssetBundleBuildTest.testBuildEnable = false;
			}
		}

		/// <summary>
		/// Linux向けアセットバンドル作成
		/// </summary>
		[MenuItem("AssetBundleShoshaTest/Build AssetBundles/Linux", false, 30130)]
		public static void BuildAssetBundlesForLinux() {
			AssetBundleBuildTest.testBuildEnable = true;
			try {
				EditorApplication.ExecuteMenuItem("Assets/AssetBundles/Build AssetBundles/Linux");
			} catch (System.Exception e) {
				throw e;
			} finally {
				AssetBundleBuildTest.testBuildEnable = false;
			}
		}

		/// <summary>
		/// Android向けアセットバンドル作成
		/// </summary>
		[MenuItem("AssetBundleShoshaTest/Build AssetBundles/Android", false, 30140)]
		public static void BuildAssetBundlesForAndroid() {
			AssetBundleBuildTest.testBuildEnable = true;
			try {
				EditorApplication.ExecuteMenuItem("Assets/AssetBundles/Build AssetBundles/Android");
			} catch (System.Exception e) {
				throw e;
			} finally {
				AssetBundleBuildTest.testBuildEnable = false;
			}
		}

		/// <summary>
		/// iOS向けアセットバンドル作成
		/// </summary>
		[MenuItem("AssetBundleShoshaTest/Build AssetBundles/iOS", false, 30150)]
		public static void BuildAssetBundlesForIos() {
			AssetBundleBuildTest.testBuildEnable = true;
			try {
				EditorApplication.ExecuteMenuItem("Assets/AssetBundles/Build AssetBundles/iOS");
			} catch (System.Exception e) {
				throw e;
			} finally {
				AssetBundleBuildTest.testBuildEnable = false;
			}
		}

		/// <summary>
		/// WebGL向けアセットバンドル作成
		/// </summary>
		[MenuItem("AssetBundleShoshaTest/Build AssetBundles/WebGL", false, 30160)]
		public static void BuildAssetBundlesForWebGL() {
			AssetBundleBuildTest.testBuildEnable = true;
			try {
				EditorApplication.ExecuteMenuItem("Assets/AssetBundles/Build AssetBundles/WebGL");
			} catch (System.Exception e) {
				throw e;
			} finally {
				AssetBundleBuildTest.testBuildEnable = false;
			}
		}

		#endregion
		#region Private methods
		#endregion
	}
}
