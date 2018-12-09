// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Internal {
	using UnityEngine.Networking;
	using AssetBundleShosha;

	/// <summary>
	/// エラーコード拡張メソッド
	/// </summary>
	public static class AssetBundleErrorCodeUtility {
		/// <summary>
		/// エラーコード解析
		/// </summary>
		/// <param name="request">解析対象のWebリクエスト</param>
		/// <param name="errorCode">エラーコード</param>
		/// <returns>true:正常、false:エラー</returns>
		public static bool TryParse(UnityWebRequest request, out AssetBundleErrorCode errorCode) {
			var result = false;
			if (!request.isDone) {
				errorCode = AssetBundleErrorCode.Timeout;
				result = true;
			} else if (request.isNetworkError) {
				errorCode = AssetBundleErrorCode.NetworkError;
				result = true;
			} else if (request.isHttpError) {
				errorCode = (AssetBundleErrorCode)(request.responseCode + (int)AssetBundleErrorCode.HttpStatusCodeBegin);
				result = true;
			} else {
				errorCode = AssetBundleErrorCode.Null;
			}
			return result;
		}

		/// <summary>
		/// 初期化用エラーコード解析
		/// </summary>
		/// <param name="errorCode">エラーコード</param>
		/// <param name="request">解析対象のWebリクエスト</param>
		/// <returns>true:正常、false:エラー</returns>
		public static bool TryParseForInitialize(UnityWebRequest request, out AssetBundleErrorCode errorCode) {
			var result = false;
			AssetBundleErrorCode errorCodeNoInitialize;
			if (TryParse(request, out errorCodeNoInitialize)) {
				if ((AssetBundleErrorCode.HttpStatusCodeBegin <= errorCodeNoInitialize) && (errorCodeNoInitialize < AssetBundleErrorCode.HttpStatusCodeEnd)) {
					errorCode = errorCodeNoInitialize - AssetBundleErrorCode.HttpStatusCodeBegin + AssetBundleErrorCode.InitializeHttpStatusCodeBegin;
					result = true;
				} else if (errorCodeNoInitialize == AssetBundleErrorCode.Timeout) {
					errorCode = AssetBundleErrorCode.InitializeTimeout;
					result = true;
				} else if (errorCodeNoInitialize == AssetBundleErrorCode.NetworkError) {
					errorCode = AssetBundleErrorCode.InitializeNetworkError;
					result = true;
				} else {
					errorCode = AssetBundleErrorCode.Null;
				}
			} else {
				errorCode = AssetBundleErrorCode.Null;
			}
			return result;
		}
	}
}
