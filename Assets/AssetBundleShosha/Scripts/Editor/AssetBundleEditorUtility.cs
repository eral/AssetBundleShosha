// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor {
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Reflection;
	using System.IO;
	using AssetBundleShosha.Internal;

	public static class AssetBundleEditorUtility {
		#region Public types
		#endregion
		#region Public const fields

		/// <summary>
		/// OutputDetailJsonビルドオプションのEditorPrefsキー
		/// </summary>
		private const string kBuildOptionOutputDetailJsonEditorPrefsKey = "AssetBundleShosha/BuildOption/OutputDetailJson";

		/// <summary>
		/// SkipFileDeploymentOfDeliveryStreamingAssetsビルドオプションのEditorPrefsキー
		/// </summary>
		private const string kBuildOptionSkipFileDeploymentOfDeliveryStreamingAssetsEditorPrefsKey = "AssetBundleShosha/BuildOption/SkipFileDeploymentOfDeliveryStreamingAssets";

		/// <summary>
		/// ForceCryptoビルドオプションのEditorPrefsキー
		/// </summary>
		private const string kBuildOptionForceCryptoEditorPrefsKey = "AssetBundleShosha/BuildOption/ForceCrypto";

		/// <summary>
		/// NonDeterministicCryptoビルドオプションのEditorPrefsキー
		/// </summary>
		private const string kBuildOptionNonDeterministicCryptoEditorPrefsKey = "AssetBundleShosha/BuildOption/NonDeterministicCrypto";

		#endregion
		#region Public fields and properties

		/// <summary>
		/// OutputDetailJsonビルドオプション
		/// </summary>
		public static bool buildOptionOutputDetailJson {get{
			if (s_BuildOptionOutputDetailJson == -1) {
				s_BuildOptionOutputDetailJson = EditorPrefs.GetBool(kBuildOptionOutputDetailJsonEditorPrefsKey, false)? 1: 0;
			}
			return s_BuildOptionOutputDetailJson != 0;
		} set{
			var intValue = value? 1: 0;
			if (s_BuildOptionOutputDetailJson != intValue) {
				s_BuildOptionOutputDetailJson = intValue;
				EditorPrefs.SetBool(kBuildOptionOutputDetailJsonEditorPrefsKey, value);
			}
		}}

		/// <summary>
		/// SkipFileDeploymentOfDeliveryStreamingAssetsビルドオプション
		/// </summary>
		public static bool buildOptionSkipFileDeploymentOfDeliveryStreamingAssets {get{
			if (s_BuildOptionSkipFileDeploymentOfDeliveryStreamingAssets == -1) {
				s_BuildOptionSkipFileDeploymentOfDeliveryStreamingAssets = EditorPrefs.GetBool(kBuildOptionSkipFileDeploymentOfDeliveryStreamingAssetsEditorPrefsKey, false)? 1: 0;
			}
			return s_BuildOptionSkipFileDeploymentOfDeliveryStreamingAssets != 0;
		} set{
			var intValue = value? 1: 0;
			if (s_BuildOptionSkipFileDeploymentOfDeliveryStreamingAssets != intValue) {
				s_BuildOptionSkipFileDeploymentOfDeliveryStreamingAssets = intValue;
				EditorPrefs.SetBool(kBuildOptionSkipFileDeploymentOfDeliveryStreamingAssetsEditorPrefsKey, value);
			}
		}}

		/// <summary>
		/// ForceCryptoビルドオプション
		/// </summary>
		public static bool buildOptionForceCrypto {get{
			if (s_BuildOptionForceCrypto == -1) {
				s_BuildOptionForceCrypto = EditorPrefs.GetBool(kBuildOptionForceCryptoEditorPrefsKey, false)? 1: 0;
			}
			return s_BuildOptionForceCrypto != 0;
		} set{
			var intValue = value? 1: 0;
			if (s_BuildOptionForceCrypto != intValue) {
				s_BuildOptionForceCrypto = intValue;
				EditorPrefs.SetBool(kBuildOptionForceCryptoEditorPrefsKey, value);
			}
		}}

		/// <summary>
		/// NonDeterministicCryptoビルドオプション
		/// </summary>
		public static bool buildOptionNonDeterministicCrypto {get{
			if (s_BuildOptionNonDeterministicCrypto == -1) {
				s_BuildOptionNonDeterministicCrypto = EditorPrefs.GetBool(kBuildOptionNonDeterministicCryptoEditorPrefsKey, false)? 1: 0;
			}
			return s_BuildOptionNonDeterministicCrypto != 0;
		} set{
			var intValue = value? 1: 0;
			if (s_BuildOptionNonDeterministicCrypto != intValue) {
				s_BuildOptionNonDeterministicCrypto = intValue;
				EditorPrefs.SetBool(kBuildOptionNonDeterministicCryptoEditorPrefsKey, value);
			}
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// 現在のビルドターゲットの取得
		/// </summary>
		/// <returns></returns>
		public static BuildTarget GetCurrentBuildTarget() {
#if UNITY_STANDALONE_WIN
			return BuildTarget.StandaloneWindows64;
#elif UNITY_STANDALONE_OSX
			return BuildTarget.StandaloneOSX;
#elif UNITY_STANDALONE_LINUX
			return BuildTarget.StandaloneLinux64;
#elif UNITY_ANDROID
			return BuildTarget.Android;
#elif UNITY_IOS
			return BuildTarget.iOS;
#elif UNITY_WEBGL
			return BuildTarget.WebGL;
#else
			return BuildTarget.NoTarget;
#endif
		}

		/// <summary>
		/// プラットフォーム文字列の取得
		/// </summary>
		/// <param name="targetPlatform">ターゲットプラットフォーム</param>
		/// <returns>プラットフォーム文字列</returns>
		public static string GetPlatformString(BuildTarget targetPlatform) {
			switch (targetPlatform) {
			case BuildTarget.StandaloneWindows64: return "windows";
			case BuildTarget.StandaloneOSX: return "macosx";
			case BuildTarget.StandaloneLinux64: return "linux";
			case BuildTarget.Android: return "android";
			case BuildTarget.iOS: return "ios";
			case BuildTarget.WebGL: return "webgl";
			default: return "unknown";
			}
		}

		/// <summary>
		/// 存在確認
		/// </summary>
		/// <param name="path"></param>
		/// <returns>true:存在する, false:存在しない</returns>
		public static bool Exists(string path) {
			var fullPath = Application.dataPath + "/../" + path;
			var result = Directory.Exists(fullPath) || File.Exists(fullPath);
			return result;
		}

		/// <summary>
		/// メニューのビルドオプション取得
		/// </summary>
		/// <returns>ビルドオプション</returns>
		public static AssetBundleBuilder.BuildFlags GetBuildOptions() {
			var result = AssetBundleBuilder.BuildFlags.Null;
			if (AssetBundleEditorUtility.buildOptionOutputDetailJson) {
				result |= AssetBundleBuilder.BuildFlags.OutputDetailJson;
			}
			if (AssetBundleEditorUtility.buildOptionSkipFileDeploymentOfDeliveryStreamingAssets) {
				result |= AssetBundleBuilder.BuildFlags.SkipFileDeploymentOfDeliveryStreamingAssets;
			}
			if (AssetBundleEditorUtility.buildOptionForceCrypto) {
				result |= AssetBundleBuilder.BuildFlags.ForceCrypto;
			}
			if (AssetBundleEditorUtility.buildOptionNonDeterministicCrypto) {
				result |= AssetBundleBuilder.BuildFlags.NonDeterministicCrypto;
			}
			return result;
		}

		/// <summary>
		/// ダウンロードキューの取得
		/// </summary>
		/// <param name="manager">マネージャー</param>
		/// <returns>ダウンロードキュー</returns>
		public static Queue<AssetBundleBase> GetDownloadQueue(this AssetBundleManager manager) {
			return GetDownloadQueueFunction(manager);
		}

		/// <summary>
		/// ダウンロード中の取得
		/// </summary>
		/// <param name="manager">マネージャー</param>
		/// <returns>ダウンロード中ー</returns>
		public static Queue<AssetBundleBase> GetDownloading(this AssetBundleManager manager) {
			return GetDownloadingFunction(manager);
		}

		/// <summary>
		/// ダウンロード済みの取得
		/// </summary>
		/// <param name="manager">マネージャー</param>
		/// <returns>ダウンロード済み</returns>
		public static Dictionary<string, AssetBundleBase> GetDownloaded(this AssetBundleManager manager) {
			return GetDownloadedFunction(manager);
		}

		/// <summary>
		/// 進捗中の取得
		/// </summary>
		/// <param name="manager">マネージャー</param>
		/// <returns>進捗中</returns>
		public static Dictionary<string, AssetBundleBase> GetProgressing(this AssetBundleManager manager) {
			return GetProgressingFunction(manager);
		}

		/// <summary>
		/// 参照カウンターの取得
		/// </summary>
		/// <param name="manager">マネージャー</param>
		/// <returns>参照カウンター</returns>
		public static int GetReferenceCount(this AssetBundleBase assetBundle) {
			return GetReferenceCountFunction(assetBundle);
		}

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// ダウンロードキュー取得関数
		/// </summary>
		private static System.Func<AssetBundleManager, Queue<AssetBundleBase>> s_GetDownloadQueueFunction = null;
		private static System.Func<AssetBundleManager, Queue<AssetBundleBase>> GetDownloadQueueFunction {get{
			if (s_GetDownloadQueueFunction == null) {
				var kBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
				var fieldInfo = typeof(AssetBundleManager).GetField("m_DownloadQueue", kBindingFlags);
				s_GetDownloadQueueFunction = x=>(Queue<AssetBundleBase>)fieldInfo.GetValue(x);
			}
			return s_GetDownloadQueueFunction;
		}}

		/// <summary>
		/// ダウンロード中取得関数
		/// </summary>
		private static System.Func<AssetBundleManager, Queue<AssetBundleBase>> s_GetDownloadingFunction = null;
		private static System.Func<AssetBundleManager, Queue<AssetBundleBase>> GetDownloadingFunction {get{
			if (s_GetDownloadingFunction == null) {
				var kBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
				var fieldInfo = typeof(AssetBundleManager).GetField("m_Downloading", kBindingFlags);
				s_GetDownloadingFunction = x=>(Queue<AssetBundleBase>)fieldInfo.GetValue(x);
			}
			return s_GetDownloadingFunction;
		}}

		/// <summary>
		/// ダウンロード済み取得関数
		/// </summary>
		private static System.Func<AssetBundleManager, Dictionary<string, AssetBundleBase>> s_GetDownloadedFunction = null;
		private static System.Func<AssetBundleManager, Dictionary<string, AssetBundleBase>> GetDownloadedFunction {get{
			if (s_GetDownloadedFunction == null) {
				var kBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
				var fieldInfo = typeof(AssetBundleManager).GetField("m_Downloaded", kBindingFlags);
				s_GetDownloadedFunction = x=>(Dictionary<string, AssetBundleBase>)fieldInfo.GetValue(x);
			}
			return s_GetDownloadedFunction;
		}}

		/// <summary>
		/// 進捗中取得関数
		/// </summary>
		private static System.Func<AssetBundleManager, Dictionary<string, AssetBundleBase>> s_GetProgressingFunction = null;
		private static System.Func<AssetBundleManager, Dictionary<string, AssetBundleBase>> GetProgressingFunction {get{
			if (s_GetProgressingFunction == null) {
				var kBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
				var fieldInfo = typeof(AssetBundleManager).GetField("m_Progressing", kBindingFlags);
				s_GetProgressingFunction = x=>(Dictionary<string, AssetBundleBase>)fieldInfo.GetValue(x);
			}
			return s_GetProgressingFunction;
		}}

		/// <summary>
		/// 進捗中取得関数
		/// </summary>
		private static System.Func<AssetBundleBase, int> s_GetReferenceCountFunction = null;
		private static System.Func<AssetBundleBase, int> GetReferenceCountFunction {get{
			if (s_GetReferenceCountFunction == null) {
				var kBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
				var fieldInfo = typeof(AssetBundleBase).GetField("m_ReferenceCount", kBindingFlags);
				s_GetReferenceCountFunction = x=>(int)fieldInfo.GetValue(x);
			}
			return s_GetReferenceCountFunction;
		}}

		/// <summary>
		/// OutputDetailJsonビルドオプション
		/// </summary>
		private static int s_BuildOptionOutputDetailJson = -1;

		/// <summary>
		/// SkipFileDeploymentOfDeliveryStreamingAssetsビルドオプション
		/// </summary>
		private static int s_BuildOptionSkipFileDeploymentOfDeliveryStreamingAssets = -1;

		/// <summary>
		/// ForceCryptoビルドオプション
		/// </summary>
		private static int s_BuildOptionForceCrypto = -1;

		/// <summary>
		/// NonDeterministicCryptoビルドオプション
		/// </summary>
		private static int s_BuildOptionNonDeterministicCrypto = -1;

		#endregion
		#region Private methods
		#endregion
	}
}
