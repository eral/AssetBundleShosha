// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.IO;
	using System.Threading;
	using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
#endif

	public static class AssetBundleUtility {
		#region Public types

#if UNITY_EDITOR
		/// <summary>
		/// サーバーエミュレーション
		/// </summary>
		public enum ServerEmulation {
			None,				//エミュレーションなし
			Reserve1,			//予約 (CRCチェックなし読み込み)
			Reserve2,			//予約 (サーバー自作ローカル読み込み)
			Reserve3,			//予約 (fileプロトコルローカル読み込み)
			Reserve4,			//予約 (キャッシュ無効ローカル読み込み)
			LoadAssetsDirect,	//直接読み込み
		}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// 配信ストリーミングアセット情報
		/// </summary>
		[System.Serializable]
		public struct DeliveryStreamingAssetInfo {
			/// <summary>
			/// アセットバンドル名
			/// </summary>
			public string deliveryStreamingAssetName;

			/// <summary>
			/// バリアント付きアセットバンドル名
			/// </summary>
			public string deliveryStreamingAssetNameWithVariant;

			/// <summary>
			/// アセットバンドル名
			/// </summary>
			public string variant;

			/// <summary>
			/// ファイルパス
			/// </summary>
			public string path;
		}
#endif

		#endregion
		#region Public const fields

		/// <summary>
		/// 配信ストリーミングアセット用アセットバンドル名のプレフィックス
		/// </summary>
		public const string kDeliveryStreamingAssetsPrefix = "deliverystreamingassets:";

		/// <summary>
		/// 配信ストリーミングアセットキャッシュ中間パス
		/// </summary>
		public const string kDeliveryStreamingAssetsCacheMiddlePath = "/deliverystreamingassets/";

#if UNITY_EDITOR
		/// <summary>
		/// ServerEmulationのEditorPrefsキー
		/// </summary>
		private const string kServerEmulationEditorPrefsKey = "AssetBundleShosha/ServerEmulation";
#endif

		#endregion
		#region Public fields and properties

		/// <summary>
		/// 一時キャッシュパス
		/// </summary>
		public static string temporaryCacheBasePath {get{
#if UNITY_EDITOR
			return Application.temporaryCachePath;
#elif UNITY_IOS
			return Application.temporaryCachePath + "/../ShoshaCache";
#else
			return Application.persistentDataPath;
#endif
		}}

#if UNITY_EDITOR
		/// <summary>
		/// サーバーエミュレーション
		/// </summary>
		public static ServerEmulation serverEmulation {get{
			if (s_ServerEmulation == (ServerEmulation)(-1)) {
				s_ServerEmulation = (ServerEmulation)EditorPrefs.GetInt(kServerEmulationEditorPrefsKey, (int)ServerEmulation.None);
			}
			return s_ServerEmulation;
		} set{
			if (s_ServerEmulation != value) {
				s_ServerEmulation = value;
				EditorPrefs.SetInt(kServerEmulationEditorPrefsKey, (int)s_ServerEmulation);
			}
		}}
#endif

		#endregion
		#region Public methods

		/// <summary>
		/// プラットフォーム文字列の取得
		/// </summary>
		/// <returns>プラットフォーム文字列</returns>
		public static string GetPlatformString() {
#if UNITY_STANDALONE_WIN
			return "windows";
#elif UNITY_STANDALONE_OSX
			return "macosx";
#elif UNITY_STANDALONE_LINUX
			return "linux";
#elif UNITY_ANDROID
			return "android";
#elif UNITY_IOS
			return "ios";
#elif UNITY_WEBGL
			return "webgl";
#else
			return "unknown";
#endif
		}

		/// <summary>
		/// バリアント除去
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>アセットバンドル名</returns>
		public static string DeleteVariant(string assetBundleNameWithVariant) {
			var variantStartIndex = assetBundleNameWithVariant.IndexOf('.');
			if (variantStartIndex < 0) {
				//バリアントが無い
				return assetBundleNameWithVariant;
			}
			//バリアント付き
			var assetBundleName = assetBundleNameWithVariant.Substring(0, variantStartIndex);
			return assetBundleName;
		}

		/// <summary>
		/// 配信ストリーミングアセット確認
		/// </summary>
		/// <param name="assetBundleName">アセットバンドルラベル</param>
		/// <returns>true:配信ストリーミングアセット、false:配信ストリーミングアセットではない</returns>
		public static bool IsDeliveryStreamingAsset(string assetBundleName) {
			return assetBundleName.StartsWith(kDeliveryStreamingAssetsPrefix, System.StringComparison.InvariantCultureIgnoreCase);
		}

#if UNITY_EDITOR
		/// <summary>
		/// パスから配信ストリーミングアセット情報の取得
		/// </summary>
		/// <param name="path">パス</param>
		/// <returns>配信ストリーミングアセット情報</returns>
		public static List<DeliveryStreamingAssetInfo> GetAllDeliveryStreamingAssetInfos() {
			var deliveryStreamingAssetDirectorys = AssetDatabase.FindAssets("t:Folder,DeliveryStreamingAssets")
															.Select(x=>AssetDatabase.GUIDToAssetPath(x))
															.ToArray();
			var deliveryStreamingAssetObjects = AssetDatabase.FindAssets("t:Object", deliveryStreamingAssetDirectorys)
															.Select(x=>AssetDatabase.GUIDToAssetPath(x))
															.Where(x=>!AssetDatabase.IsValidFolder(x))
															.ToArray();
			var deliveryStreamingAssetInfos = deliveryStreamingAssetObjects.Select(x=>GetDeliveryStreamingAssetInfoFromPath(x))
														.ToList();
			return deliveryStreamingAssetInfos;
		}
#endif

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
		/// Queue版RemoveAll
		/// </summary>
		/// <typeparam name="T">要素型</typeparam>
		/// <param name="queue">対象</param>
		/// <param name="match">条件式</param>
		public static void QueueRemoveAll<T>(Queue<T> queue, System.Predicate<T> match) {
			for (int i = 0, iMax = queue.Count; i < iMax; ++i) {
				var item = queue.Dequeue();
				if (!match(item)) {
					queue.Enqueue(item);
				}
			}
		}

		/// <summary>
		/// 配信ストリーミングアセットのキャッシュを削除
		/// </summary>
		/// <returns>成功確認</returns>
		public static bool ClearDeliveryStreamingAssetsCache() {
			var result = true;
			var cachePath = temporaryCacheBasePath + kDeliveryStreamingAssetsCacheMiddlePath;
			if (System.IO.Directory.Exists(cachePath)) {
				try {
					System.IO.Directory.Delete(cachePath, true);
				} catch {
					result = false;
				}
			}
			return result;
		}

		/// <summary>
		/// 配信ストリーミングアセットのキャッシュを削除(スレッド版)
		/// </summary>
		/// <param name="onFinished">終了時イベント(別スレッドで呼ばれるので注意)</param>
		/// <returns>削除用スレッド(Start呼び出し前)</returns>
		public static Thread ClearDeliveryStreamingAssetsCacheThread(System.Action<bool> onFinished) {
			var cachePath = temporaryCacheBasePath + kDeliveryStreamingAssetsCacheMiddlePath;
			var result = new Thread(()=>{
				var r = true;
				if (System.IO.Directory.Exists(cachePath)) {
					try {
						System.IO.Directory.Delete(cachePath, true);
					} catch {
						r = false;
					}
				}
				if (onFinished != null) onFinished(r);
			});
			return result;
		}

#if UNITY_EDITOR
		/// <summary>
		/// エディタ用マネージャーの取得
		/// </summary>
		/// <param name="manager">アセットバンドルマネージャー</param>
		/// <returns>エディタ用マネージャー</returns>
		public static AssetBundleManagerEditor GetAssetBundleManagerEditor(AssetBundleManager manager) {
			return manager.editor;
		}
#endif

		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

#if UNITY_EDITOR
		/// <summary>
		/// サーバーエミュレーション
		/// </summary>
		private static ServerEmulation s_ServerEmulation = (ServerEmulation)(-1);
#endif

		#endregion
		#region Private methods

#if UNITY_EDITOR
		/// <summary>
		/// パスから配信ストリーミングアセット情報の取得
		/// </summary>
		/// <param name="path">パス</param>
		/// <returns>配信ストリーミングアセット情報</returns>
		private static DeliveryStreamingAssetInfo GetDeliveryStreamingAssetInfoFromPath(string path) {
			var result = new DeliveryStreamingAssetInfo();
			var pathLower = path.ToLower();
			var match = Regex.Match(pathLower, @"deliverystreamingassets/((?:\w+/)*)(?:@(\w+)/)?((?:\w+/)*\w+)(?:\.\w+)?");
			if (match.Success) {
				result.path = path;
				var sb = new StringBuilder(path.Length);
				sb.Append(kDeliveryStreamingAssetsPrefix);
				sb.Append(match.Groups[1].Value);
				sb.Append(match.Groups[3].Value);
				result.deliveryStreamingAssetName = sb.ToString();
				if (match.Groups[2].Success) {
					sb.Append('.');
					sb.Append(match.Groups[2].Value);
					result.variant = match.Groups[2].Value;
				}
				result.deliveryStreamingAssetNameWithVariant = sb.ToString();
			}
			return result;
		}
#endif

		#endregion
	}
}
