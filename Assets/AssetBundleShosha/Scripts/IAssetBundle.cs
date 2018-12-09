// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {
	using System.Collections.Generic;
	using UnityEngine;

	public interface IAssetBundle : IEnumerator<object> {
		#region Public fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		string name {get;}

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		string nameWithVariant {get;}

		/// <summary>
		/// 終了確認
		/// </summary>
		bool isDone {get;}

		/// <summary>
		/// エラーコード
		/// </summary>
		AssetBundleErrorCode errorCode {get;}

		/// <summary>
		/// エラーハンドラー
		/// </summary>
		IErrorHandler errorHandler {get; set;}

		/// <summary>
		/// アセットバンドルをビルドするときに、必ず使うアセットを設定します（読み取り専用）
		/// </summary>
		Object mainAsset {get;}

		/// <summary>
		/// アセットバンドルがストリーミングされたシーンのアセットバンドルならば、true を返します。
		/// </summary>
		bool isStreamedSceneAssetBundle {get;}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		bool isDeliveryStreamingAsset {get;}

		/// <summary>
		/// 配信ストリーミングアセットのパスを返します。
		/// </summary>
		string deliveryStreamingAssetPath {get;}

		#endregion
		#region Public methods

		/// <summary>
		/// 特定のオブジェクトがアセットバンドルに含まれているか確認します。
		/// </summary>
		/// <param name="name">アセット名</param>
		/// <returns>true:含まれる、false:含まれない</returns>
		bool Contains(string name);

		/// <summary>
		/// アセットバンドルにあるすべてのアセット名を返します。
		/// </summary>
		/// <returns>すべてのアセット名</returns>
		string[] GetAllAssetNames();

		/// <summary>
		/// アセットバンドルにあるすべてのシーンアセットのパス( *.unity アセットへのパス)を返します。
		/// </summary>
		/// <returns>すべてのシーンアセットのパス</returns>
		string[] GetAllScenePaths();

		/// <summary>
		/// 型から継承したアセットバンドルに含まれるすべてのアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するすべてのアセット</returns>
		T[] LoadAllAssets<T>() where T : Object;
		Object[] LoadAllAssets(System.Type type);
		Object[] LoadAllAssets();

		/// <summary>
		/// アセットバンドルに含まれるすべてのアセットを非同期で読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		IAssetBundleRequest LoadAllAssetsAsync<T>();
		IAssetBundleRequest LoadAllAssetsAsync(System.Type type);
		IAssetBundleRequest LoadAllAssetsAsync();

		/// <summary>
		/// アセットバンドルから指定する name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>該当するアセット</returns>
		T LoadAsset<T>(string name) where T : Object;
		Object LoadAsset(string name, System.Type type);
		Object LoadAsset(string name);

		/// <summary>
		/// 非同期でアセットバンドルから name のアセットを読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		IAssetBundleRequest LoadAssetAsync<T>(string name);
		IAssetBundleRequest LoadAssetAsync(string name, System.Type type);
		IAssetBundleRequest LoadAssetAsync(string name);

		/// <summary>
		/// name のアセットとサブアセットをアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <param name="type"></param>
		/// <returns>該当するアセット</returns>
		T[] LoadAssetWithSubAssets<T>(string name) where T : Object;
		Object[] LoadAssetWithSubAssets(string name, System.Type type);
		Object[] LoadAssetWithSubAssets(string name);

		/// <summary>
		/// name のアセットとサブアセットを非同期でアセットバンドルから読み込みます。
		/// </summary>
		/// <typeparam name="T">読み込む型</typeparam>
		/// <param name="name">アセット名</param>
		/// <param name="type">読み込む型</param>
		/// <returns>アセットバンドルリクエスト</returns>
		IAssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name);
		IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name, System.Type type);
		IAssetBundleRequest LoadAssetWithSubAssetsAsync(string name);

		#endregion
	}
}
