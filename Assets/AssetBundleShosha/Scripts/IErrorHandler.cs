// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha {

	public interface IErrorHandler {
		#region Public methods

		/// <summary>
		/// エラー発生
		/// </summary>
		/// <param name="handle">エラーハンドル</param>
		/// <remarks>
		/// 	リトライするなら「handle.Retry()」、エラーを無視するなら「handle.Ignore()」を呼ぶ
		/// 	翌フレームに持ち越してもOK
		/// </remarks>
		void Error(IErrorHandle handle);

		#endregion
	}

	public interface IErrorHandle {
		#region Public fields and properties

		/// <summary>
		/// アセットバンドル名
		/// </summary>
		string assetBundleName {get;}

		/// <summary>
		/// バリアント付きアセットバンドル名
		/// </summary>
		string assetBundleNameWithVariant {get;}

		/// <summary>
		/// URL
		/// </summary>
		string url {get;}

		/// <summary>
		/// ファイル名
		/// </summary>
		string fileName {get;}

		/// <summary>
		/// ファイルサイズ
		/// </summary>
		uint fileSize {get;}

		/// <summary>
		/// エラーコード
		/// </summary>
		AssetBundleErrorCode errorCode {get;}

		/// <summary>
		/// 配信ストリーミングアセットならば、true を返します。
		/// </summary>
		bool isDeliveryStreamingAsset {get;}

		/// <summary>
		/// 初期化ならば、true を返します。
		/// </summary>
		bool isInitialize {get;}

		#endregion
		#region Public methods

		/// <summary>
		/// リトライ
		/// </summary>
		void Retry();

		/// <summary>
		/// 無視
		/// </summary>
		void Ignore();

		#endregion
	}
}
