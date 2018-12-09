// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {

	/// <summary>
	/// エラーコード
	/// </summary>
	public enum AssetBundleErrorCode : int {
		/// <summary>
		/// 初期値
		/// </summary>
		Null = 0,

		///100xxxは通信エラー用、下位3桁の値はHTTPステータスコード
		///HTTPステータスコードが200でもダウンロード成功とは限らない(途中で回線が途切れる等するとエラーとなる)ので注意
		HttpStatusCodeBegin = 100000,
		HttpStatusCodeEnd = 101000,

		/// <summary>
		/// タイムアウト
		/// </summary>
		Timeout = 101001,

		/// <summary>
		/// ネットワーク不良
		/// </summary>
		NetworkError = 101999,

		///200xxxは初期化時エラー用、下位3桁の値はHTTPステータスコード
		InitializeHttpStatusCodeBegin = 200000,
		InitializeHttpStatusCodeEnd = 201000,

		/// <summary>
		/// 初期化タイムアウト
		/// </summary>
		InitializeTimeout = 201001,

		/// <summary>
		/// 初期化ネットワーク不良
		/// </summary>
		InitializeNetworkError = 201999,

		/// <summary>
		/// 初期化用アセットバンドル発見出来ず
		/// </summary>
		InitializeNotFoundAssetBundle = 202001,

		/// <summary>
		/// 初期化用アセット発見出来ず
		/// </summary>
		InitializeNotFoundAsset = 202002,

		/// <summary>
		/// 暗号化解除ファイル無し
		/// </summary>
		DecryptDataNotFound = 500001,

		/// <summary>
		/// 暗号化解除失敗
		/// </summary>
		DecryptFailed = 500002,
	}

	/// <summary>
	/// エラーコード拡張メソッド
	/// </summary>
	public static class AssetBundleErrorCodeExtensionMethods {
		/// <summary>
		/// エラー確認
		/// </summary>
		/// <param name="errorCode">エラーコード</param>
		/// <returns>true:エラー、false:正常</returns>
		public static bool IsError(this AssetBundleErrorCode errorCode) {
			return errorCode != AssetBundleErrorCode.Null;
		}

		/// <summary>
		/// HTTPステータスコード確認
		/// </summary>
		/// <param name="errorCode">エラーコード</param>
		/// <returns>true:HTTPステータスコード、false:HTTPステータスコードではない</returns>
		public static bool IsHttpStatusCode(this AssetBundleErrorCode errorCode) {
			var result = (AssetBundleErrorCode.HttpStatusCodeBegin <= errorCode) && (errorCode < AssetBundleErrorCode.HttpStatusCodeEnd);
			result = result || ((AssetBundleErrorCode.InitializeHttpStatusCodeBegin <= errorCode) && (errorCode < AssetBundleErrorCode.InitializeHttpStatusCodeEnd));
			return result;
		}

		/// <summary>
		/// HTTPステータスコード取得
		/// </summary>
		/// <param name="errorCode">エラーコード</param>
		/// <returns>HTTPステータスコード</returns>
		public static int? GetHttpStatusCode(this AssetBundleErrorCode errorCode) {
			return ((errorCode.IsHttpStatusCode())? (int?)((int)errorCode % 1000): null);
		}
	}
}
