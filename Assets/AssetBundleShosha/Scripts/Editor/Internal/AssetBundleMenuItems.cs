// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using UnityEngine;
	using UnityEditor;
	using AssetBundleShosha.Internal;

	public static class AssetBundleMenuItems {
		#region Public methods

		/// <summary>
		/// 現在のプラットフォーム向けアセットバンドル作成
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Current Platform", false, 30010)]
		public static void BuildAssetBundlesForCurrentPlatform() {
			var targetPlatform = AssetBundleEditorUtility.GetCurrentBuildTarget();
			if (targetPlatform != BuildTarget.NoTarget) {
				var options = AssetBundleEditorUtility.GetBuildOptions();
				AssetBundleBuilder.Build(targetPlatform, options);
			} else {
				Debug.LogError("Current platform has not supported.");
			}
		}

		/// <summary>
		/// Windows向けアセットバンドル作成
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Windows", false, 31010)]
		public static void BuildAssetBundlesForWindows() {
			var options = AssetBundleEditorUtility.GetBuildOptions();
			AssetBundleBuilder.Build(BuildTarget.StandaloneWindows64, options);
		}

		/// <summary>
		/// Android向けアセットバンドル作成
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Android", false, 31020)]
		public static void BuildAssetBundlesForAndroid() {
			var options = AssetBundleEditorUtility.GetBuildOptions();
			AssetBundleBuilder.Build(BuildTarget.Android, options);
		}

		/// <summary>
		/// iOS向けアセットバンドル作成
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/iOS", false, 31030)]
		public static void BuildAssetBundlesForIos() {
			var options = AssetBundleEditorUtility.GetBuildOptions();
			AssetBundleBuilder.Build(BuildTarget.iOS, options);
		}

		/// <summary>
		/// WebGL向けアセットバンドル作成
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/WebGL", false, 31040)]
		public static void BuildAssetBundlesForWebGL() {
			var options = AssetBundleEditorUtility.GetBuildOptions();
			AssetBundleBuilder.Build(BuildTarget.WebGL, options);
		}

		/// <summary>
		/// アセットバンドル作成オプション・詳細JSONを出力する
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Output Detail Json", false, 32010)]
		public static void BuildOptionsOutputDetailJson() {
			AssetBundleEditorUtility.buildOptionOutputDetailJson = !AssetBundleEditorUtility.buildOptionOutputDetailJson;
		}
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Output Detail Json", true)]
		public static bool BuildOptionsOutputDetailJsonValidate() {
			var isChecked = AssetBundleEditorUtility.buildOptionOutputDetailJson;
			Menu.SetChecked("Assets/AssetBundles/Build AssetBundles/Options/Output Detail Json", isChecked);
			return true;
		}

		/// <summary>
		/// アセットバンドル作成オプション・強制再ビルド
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Force Rebuild", false, 32015)]
		public static void BuildOptionsForceRebuild() {
			AssetBundleEditorUtility.buildOptionForceRebuild = !AssetBundleEditorUtility.buildOptionForceRebuild;
		}
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Force Rebuild", true)]
		public static bool BuildOptionsForceRebuildValidate() {
			var isChecked = AssetBundleEditorUtility.buildOptionForceRebuild;
			Menu.SetChecked("Assets/AssetBundles/Build AssetBundles/Options/Force Rebuild", isChecked);
			return true;
		}

		/// <summary>
		/// アセットバンドル作成オプション・配信ストリーミングアセットのファイルデプロイを省略する
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Skip file deployment of Delivery Streaming Assets", false, 32020)]
		public static void BuildOptionsSkipFileDeploymentOfDeliveryStreamingAssets() {
			AssetBundleEditorUtility.buildOptionSkipFileDeploymentOfDeliveryStreamingAssets = !AssetBundleEditorUtility.buildOptionSkipFileDeploymentOfDeliveryStreamingAssets;
		}
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Skip file deployment of Delivery Streaming Assets", true)]
		public static bool BuildOptionsSkipFileDeploymentOfDeliveryStreamingAssetsValidate() {
			var isChecked = AssetBundleEditorUtility.buildOptionSkipFileDeploymentOfDeliveryStreamingAssets;
			Menu.SetChecked("Assets/AssetBundles/Build AssetBundles/Options/Skip file deployment of Delivery Streaming Assets", isChecked);
			return true;
		}

		/// <summary>
		/// アセットバンドル作成オプション・強制暗号化
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Force Crypto", false, 32030)]
		public static void BuildOptionsForceCrypto() {
			AssetBundleEditorUtility.buildOptionForceCrypto = !AssetBundleEditorUtility.buildOptionForceCrypto;
		}
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Force Crypto", true)]
		public static bool BuildOptionsForceCryptoValidate() {
			var isChecked = AssetBundleEditorUtility.buildOptionForceCrypto;
			Menu.SetChecked("Assets/AssetBundles/Build AssetBundles/Options/Force Crypto", isChecked);
			return true;
		}

		/// <summary>
		/// アセットバンドル作成オプション・非決定性暗号化
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Non-Deterministic Crypto", false, 32040)]
		public static void BuildOptionsNonDeterministicCrypto() {
			AssetBundleEditorUtility.buildOptionNonDeterministicCrypto = !AssetBundleEditorUtility.buildOptionNonDeterministicCrypto;
		}
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Non-Deterministic Crypto", true)]
		public static bool BuildOptionsNonDeterministicCryptoValidate() {
			var isChecked = AssetBundleEditorUtility.buildOptionNonDeterministicCrypto;
			Menu.SetChecked("Assets/AssetBundles/Build AssetBundles/Options/Non-Deterministic Crypto", isChecked);
			return true;
		}

		/// <summary>
		/// アセットバンドル作成オプション・梱包アセットを詳細JSONに出力しない
		/// </summary>
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Skip listup included assets to Detail Json", false, 32040)]
		public static void BuildOptionsSkipListupIncludedAssetsToDetailJson() {
			AssetBundleEditorUtility.buildOptionSkipListupIncludedAssetsToDetailJson = !AssetBundleEditorUtility.buildOptionSkipListupIncludedAssetsToDetailJson;
		}
		[MenuItem("Assets/AssetBundles/Build AssetBundles/Options/Skip listup included assets to Detail Json", true)]
		public static bool BuildOptionsSkipListupIncludedAssetsToDetailJsonValidate() {
			var isChecked = AssetBundleEditorUtility.buildOptionSkipListupIncludedAssetsToDetailJson;
			Menu.SetChecked("Assets/AssetBundles/Build AssetBundles/Options/Skip listup included assets to Detail Json", isChecked);
			return true;
		}

		/// <summary>
		/// サーバーエミュレーション・エミュレーションなし
		/// </summary>
		[MenuItem("Assets/AssetBundles/Server Emulation/None", false, 30020)]
		public static void ServerEmulationNone() {
			AssetBundleUtility.serverEmulation = AssetBundleUtility.ServerEmulation.None;
		}
		[MenuItem("Assets/AssetBundles/Server Emulation/None", true)]
		public static bool ServerEmulationNoneValidate() {
			var isChecked = AssetBundleUtility.serverEmulation == AssetBundleUtility.ServerEmulation.None;
			Menu.SetChecked("Assets/AssetBundles/Server Emulation/None", isChecked);
			return true;
		}

		/// <summary>
		/// サーバーエミュレーション・直接読み込み
		/// </summary>
		[MenuItem("Assets/AssetBundles/Server Emulation/Load Assets Direct", false, 31010)]
		public static void ServerEmulationDirectAssetsLoad() {
			AssetBundleUtility.serverEmulation = AssetBundleUtility.ServerEmulation.LoadAssetsDirect;
		}
		[MenuItem("Assets/AssetBundles/Server Emulation/Load Assets Direct", true)]
		public static bool ServerEmulationDirectAssetsLoadValidate() {
			var isChecked = AssetBundleUtility.serverEmulation == AssetBundleUtility.ServerEmulation.LoadAssetsDirect;
			Menu.SetChecked("Assets/AssetBundles/Server Emulation/Load Assets Direct", isChecked);
			return true;
		}

		/// <summary>
		/// ビューアー・マネージャー
		/// </summary>
		[MenuItem("Assets/AssetBundles/Viewer/Shosha Viewer", false, 30030)]
		public static void ShowManagerViewer() {
			var inspectorWindowType = System.Reflection.Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");
			EditorWindow.GetWindow<AssetBundleManagerViewer>(inspectorWindowType);
		}

		/// <summary>
		/// ビューアー・HTTPサーバー
		/// </summary>
		[MenuItem("Assets/AssetBundles/Viewer/Shosha HTTP Server Viewer", false, 30040)]
		public static void ShowHttpServerViewer() {
			var inspectorWindowType = System.Reflection.Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");
			EditorWindow.GetWindow<HttpServerViewer>(inspectorWindowType);
		}
		[MenuItem("Assets/AssetBundles/Viewer/Shosha HTTP Server Viewer", true)]
		public static bool ShowHttpServerViewerValidate() {
			var isChecked = HttpServerViewer.enable;
			Menu.SetChecked("Assets/AssetBundles/Viewer/Shosha HTTP Server Viewer", isChecked);
			return true;
		}

		/// <summary>
		/// アセットバンドル・配信ストリーミングアセットのキャッシュ削除
		/// </summary>
		[MenuItem("Assets/AssetBundles/Caches/Clear Cache", false, 30040)]
		public static void ClearCache() {
			var resultOfClearDeliveryStreamingAssetsCache = false;
			var clearDeliveryStreamingAssetsCache = AssetBundleUtility.ClearDeliveryStreamingAssetsCacheThread(x=>resultOfClearDeliveryStreamingAssetsCache = x);
			clearDeliveryStreamingAssetsCache.Start();
			var resultOfClearAssetBundlesCache = Caching.ClearCache();
			clearDeliveryStreamingAssetsCache.Join();
			
			if (!resultOfClearAssetBundlesCache || !resultOfClearDeliveryStreamingAssetsCache) {
				Debug.LogError("Cache clear failed");
				Debug.LogError("resultOfClearAssetBundlesCache:" + resultOfClearAssetBundlesCache + ", resultOfClearDeliveryStreamingAssetsCache:" + resultOfClearDeliveryStreamingAssetsCache);
			}
		}

		/// <summary>
		/// アセットバンドルのキャッシュディレクトリを開く
		/// </summary>
		[MenuItem("Assets/AssetBundles/Caches/Open AssetBundles Cache Directory", false, 31010)]
		public static void OpenAssetBundlesCacheDirectory() {
			var cachePath = Caching.defaultCache.path;
			Application.OpenURL(cachePath);
		}

		/// <summary>
		/// 配信ストリーミングアセットのキャッシュディレクトリを開く
		/// </summary>
		[MenuItem("Assets/AssetBundles/Caches/Open DeliveryStreamingAssets Cache Directory", false, 31020)]
		public static void OpenDeliveryStreamingAssetsCacheDirectory() {
			var cachePath = AssetBundleUtility.temporaryCacheBasePath + AssetBundleUtility.kDeliveryStreamingAssetsCacheMiddlePath;
			if (!System.IO.Directory.Exists(cachePath)) {
				var length = cachePath.Length;
				if (cachePath.EndsWith("/")) {
					length = cachePath.LastIndexOf('/', length - 2);
					if (0 <= length) {
						++length;
					}
				} else {
					length = cachePath.LastIndexOf('/');
				}
				if (0 <= length) {
					cachePath = cachePath.Substring(0, length);
				}
			}
			Application.OpenURL(cachePath);
		}

		/// <summary>
		/// アセットバンドル・配信ストリーミングアセットのキャッシュ削除
		/// </summary>
		[MenuItem("Assets/AssetBundles/Crypto/Create Crypto Key", false, 30050)]
		public static void CreateCryptoKey() {
			var path = AssetBundleCryptoEditor.CreateCryptoKey();
			var asset = AssetDatabase.LoadMainAssetAtPath(path);
			EditorGUIUtility.PingObject(asset);
		}

		#endregion
		#region Private methods
		#endregion
	}
}
