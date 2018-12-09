// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaTest.Editor {
	using UnityEngine;
	using UnityEditor;
	using UnityEngine.Events;
	using UnityEngine.TestTools;
	using NUnit.Framework;
	using System.Collections.Generic;
	using System.IO;
	using AssetBundleShosha.Internal;
	using AssetBundleShosha.Editor;
	using AssetBundleShosha.Editor.Internal;

	public class AssetBundleBuildTest {
		#region Internal fields and properties

		/// <summary>
		/// テストビルド有効
		/// </summary>
		internal static bool testBuildEnable {get{return s_TestBuildEnable;} set{s_TestBuildEnable = value;}}

		#endregion
		#region Public methods

		/// <summary>
		/// ワンタイム準備
		/// </summary>
		[OneTimeSetUp]
		public void OneTimeSetUp() {
			s_TestBuildEnable = true;
			BackupWorkDirectory();

			s_FunctionCallOrders = new List<int>();
			s_PackerInfoList = new Dictionary<string, AssetBundlePackerArg.AssetBundleFlags>();
			Assert.DoesNotThrow(()=>{
				AssetBundleMenuItems.BuildAssetBundlesForCurrentPlatform();
			});
		}

		/// <summary>
		/// ワンタイム破棄
		/// </summary>
		[OneTimeTearDown]
		public void OneTimeTearDown() {
			RecoveryWorkDirectory();
			s_TestBuildEnable = false;
			s_FunctionCallOrders = null;
			s_PackerInfoList = null;
		}

		/// <summary>
		/// 割り込み系検証
		/// </summary>
		[Test]
		public void Interpreter() {
			Assert.GreaterOrEqual(s_FunctionCallOrders.Count, 16);
			Assert.Contains(kPackerFunctionCallOrder, s_FunctionCallOrders);
			Assert.Contains(kPreprocessorFunctionCallOrder + 1, s_FunctionCallOrders);
			Assert.Contains(kPreprocessorFunctionCallOrder + 2, s_FunctionCallOrders);
			Assert.Contains(kPostprocessorFunctionCallOrder + 1, s_FunctionCallOrders);
			Assert.Contains(kPostprocessorFunctionCallOrder + 2, s_FunctionCallOrders);
			for (int i = 0, iMax = s_FunctionCallOrders.Count - 1; i < iMax; ++i) {
				Assert.LessOrEqual(s_FunctionCallOrders[i], s_FunctionCallOrders[i+1]);
			}
		}

		/// <summary>
		/// Excludeアセットバンドルフラグ検証
		/// </summary>
		[Test]
		public void PackerExcludeFlag() {
			var packerNames = s_PackerInfoList.Keys;
			Assert.Contains("assetbundleshoshatest/primitive", packerNames);
			Assert.Contains("assetbundleshoshatest/colormaterialscommon", packerNames);
			Assert.Contains("assetbundleshoshatest/colormaterials.red", packerNames);
			Assert.Contains("assetbundleshoshatest/colormaterials.green", packerNames);
			Assert.Contains("assetbundleshoshatest/colormaterials.blue", packerNames);
			Assert.Contains("assetbundleshoshatest/excludematerials", packerNames);
			Assert.Contains("assetbundleshoshatest/excludeassetbundle", packerNames);
			Assert.Contains("assetbundleshoshatest/cryptoassetbundle", packerNames);
			Assert.Contains("deliverystreamingassets:assetbundleshoshatest/binary", packerNames);
			Assert.Contains("deliverystreamingassets:assetbundleshoshatest/deliverystreamingassets", packerNames);
			Assert.Contains("deliverystreamingassets:assetbundleshoshatest/variantstream.variant1", packerNames);
			Assert.Contains("deliverystreamingassets:assetbundleshoshatest/variantstream.variant2", packerNames);
			Assert.Contains("deliverystreamingassets:assetbundleshoshatest/zerobytedeliverystreamingassets", packerNames);
			Assert.Contains("deliverystreamingassets:assetbundleshoshatest/excludedeliverystreamingassets", packerNames);

			Assert.Zero((int)(s_PackerInfoList["assetbundleshoshatest/primitive"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["assetbundleshoshatest/colormaterialscommon"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["assetbundleshoshatest/colormaterials.red"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["assetbundleshoshatest/colormaterials.green"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["assetbundleshoshatest/colormaterials.blue"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["assetbundleshoshatest/excludematerials"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.NotZero((int)(s_PackerInfoList["assetbundleshoshatest/excludeassetbundle"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["assetbundleshoshatest/cryptoassetbundle"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["deliverystreamingassets:assetbundleshoshatest/binary"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["deliverystreamingassets:assetbundleshoshatest/deliverystreamingassets"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["deliverystreamingassets:assetbundleshoshatest/variantstream.variant1"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["deliverystreamingassets:assetbundleshoshatest/variantstream.variant2"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.Zero((int)(s_PackerInfoList["deliverystreamingassets:assetbundleshoshatest/zerobytedeliverystreamingassets"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
			Assert.NotZero((int)(s_PackerInfoList["deliverystreamingassets:assetbundleshoshatest/excludedeliverystreamingassets"] & AssetBundlePackerArg.AssetBundleFlags.Exclude));
		}

		/// <summary>
		/// 出力ファイル検証
		/// </summary>
		[Test]
		public void OutputFiles() {
			Assert.IsTrue(AssetBundleEditorUtility.Exists(AssetBundleBuilder.kOutputPath));

			var platformString = AssetBundleUtility.GetPlatformString();
			Assert.IsTrue(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/primitive"));
			Assert.IsTrue(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/colormaterialscommon"));
			Assert.IsTrue(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/colormaterials.red"));
			Assert.IsTrue(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/colormaterials.green"));
			Assert.IsTrue(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/colormaterials.blue"));
			Assert.IsTrue(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/excludematerials"));
			Assert.IsFalse(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/excludeassetbundle"));
			Assert.IsTrue(ExistsAssetBundleFile(platformString, "assetbundleshoshatest/cryptoassetbundle"));
			Assert.IsTrue(ExistsAssetBundleFile(null, "deliverystreamingassets:assetbundleshoshatest/binary"));
			Assert.IsTrue(ExistsAssetBundleFile(null, "deliverystreamingassets:assetbundleshoshatest/deliverystreamingassets"));
			Assert.IsTrue(ExistsAssetBundleFile(null, "deliverystreamingassets:assetbundleshoshatest/variantstream.variant1"));
			Assert.IsTrue(ExistsAssetBundleFile(null, "deliverystreamingassets:assetbundleshoshatest/variantstream.variant2"));
			Assert.IsTrue(ExistsAssetBundleFile(null, "deliverystreamingassets:assetbundleshoshatest/zerobytedeliverystreamingassets"));
			Assert.IsFalse(ExistsAssetBundleFile(null, "deliverystreamingassets:assetbundleshoshatest/excludedeliverystreamingassets"));
		}

		/// <summary>
		/// アセットバンドル内検証
		/// </summary>
		[Test]
		public void IncludeAssetsInAssetBundle() {
			var platformString = AssetBundleUtility.GetPlatformString();
			TestInAssetBundle(platformString, "assetbundleshoshatest/primitive", ab=>{
				Assert.IsTrue(ab != null);
				Assert.IsTrue(ab.LoadAsset<Texture>("Primitive") != null);
				Assert.AreEqual(4, ab.LoadAllAssets<Sprite>().Length);
			});
			TestInAssetBundle(platformString, "assetbundleshoshatest/colormaterialscommon", ab=>{
				Assert.IsTrue(ab != null);
				Assert.AreEqual(4, ab.LoadAllAssets<Texture>().Length);
				Assert.IsTrue(ab.LoadAsset("Exclude") == null);
			});
			TestInAssetBundle(platformString, "assetbundleshoshatest/colormaterials.red", ab=>{
				Assert.IsTrue(ab != null);
				var materials = ab.LoadAllAssets<Material>();
				Assert.AreEqual(4, materials.Length);
				Assert.AreEqual(new Color(255.0f/255.0f, 32.0f/255.0f, 32.0f/255.0f, 255.0f/255.0f), materials[0].color);
				Assert.IsTrue(ab.LoadAsset<Material>("Square").mainTexture == null); //依存先ABを読み込んでいないのでMissing
			});
			TestInAssetBundle(platformString, "assetbundleshoshatest/colormaterials.green", ab=>{
				Assert.IsTrue(ab != null);
				var materials = ab.LoadAllAssets<Material>();
				Assert.AreEqual(4, materials.Length);
				Assert.AreEqual(new Color(16.0f/255.0f, 255.0f/255.0f, 16.0f/255.0f, 255.0f/255.0f), materials[0].color);
				Assert.IsTrue(ab.LoadAsset<Material>("Square").mainTexture == null); //依存先ABを読み込んでいないのでMissing
			});
			TestInAssetBundle(platformString, "assetbundleshoshatest/colormaterialscommon", ab=>{
				Assert.IsTrue(ab != null);
				TestInAssetBundle(platformString, "assetbundleshoshatest/colormaterials.red", ab2=>{
					Assert.IsTrue(ab2 != null);
					var tex = ab2.LoadAsset<Material>("Square").mainTexture;
					Assert.IsTrue(tex != null); //依存先ABを読み込んでいるので有効値
					Assert.AreEqual("Square", tex.name);
				});
			});
		}

		/// <summary>
		/// アセットバンドル内除外アセット検証
		/// </summary>
		[Test]
		public void ExcludeAssetsInAssetBundle() {
			var platformString = AssetBundleUtility.GetPlatformString();
			TestInAssetBundle(platformString, "assetbundleshoshatest/excludematerials", ab=>{
				Assert.IsTrue(ab != null);
				var assets = ab.LoadAllAssets();
				Assert.AreEqual(1, assets.Length);
				Assert.AreEqual(typeof(DummyWeight), assets[0].GetType());
			});
		}

		/// <summary>
		/// 暗号化アセットバンドル内検証
		/// </summary>
		[Test]
		public void IncludeAssetsInCryptoAssetBundle() {
			var platformString = AssetBundleUtility.GetPlatformString();
			TestInAssetBundle(platformString, "assetbundleshoshatest/cryptoassetbundle", ab=>{
				Assert.IsTrue(ab != null);
				var assets = ab.LoadAllAssets();
				Assert.AreEqual(1, assets.Length);
				var textAsset = (TextAsset)assets[0];
				Assert.IsTrue(textAsset.bytes != null);
			});
		}

		/// <summary>
		/// 配信ストリーミングアセット内検証
		/// </summary>
		[Test]
		public void IncludeAssetsInDeliveryStreamingAssets() {
			TestInDeliveryStreamingAssets(null, "deliverystreamingassets:assetbundleshoshatest/binary", dsa=>{
				Assert.IsTrue(dsa != null);
				Assert.AreEqual(256, dsa.Length);
				for (var i = 0; i < dsa.Length; ++i) {
					Assert.AreEqual(i, dsa[i]);
				}
			});
		}

		/// <summary>
		/// 0バイト配信ストリーミングアセット検証
		/// </summary>
		[Test]
		public void ZeroByteDeliveryStreamingAssets() {
			TestInDeliveryStreamingAssets(null, "deliverystreamingassets:assetbundleshoshatest/zerobytedeliverystreamingassets", dsa=>{
				Assert.IsTrue(dsa != null);
				Assert.AreEqual(0, dsa.Length);
			});
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// ビルダー作業パス
		/// </summary>
		private const string kBuilderWorkPath = "AssetBundleShoshaWork";

		/// <summary>
		/// アセット内ビルダー作業パス
		/// </summary>
		private const string kBuilderInAssetsWorkPath = "Assets/AssetBundleShoshaWork";

		/// <summary>
		/// バックアップ基本パス
		/// </summary>
		private const string kBackupBasePath = "AssetBundleShoshaTest";

		/// <summary>
		/// アセットバンドル名固有接頭辞
		/// </summary>
		private const string kMyAssetBundleNamePrefix = "assetbundleshoshatest/";

		/// <summary>
		/// 配信ストリーミングアセット固有接頭辞
		/// </summary>
		private const string kMyDeliveryStreamingAssetNamePrefix = "deliverystreamingassets:assetbundleshoshatest/";

		/// <summary>
		/// パッカー関数呼び出し順
		/// </summary>
		private const int kPackerFunctionCallOrder = 2000;

		/// <summary>
		/// プリプロセス関数呼び出し順
		/// </summary>
		private const int kPreprocessorFunctionCallOrder = 1000;

		/// <summary>
		/// ポストプロセス関数呼び出し順
		/// </summary>
		private const int kPostprocessorFunctionCallOrder = 3000;

		#endregion
		#region Private fields and properties

		/// <summary>
		/// テストビルド有効
		/// </summary>
		private static bool s_TestBuildEnable = false;

		/// <summary>
		/// 関数呼び出し順リスト
		/// </summary>
		private static List<int> s_FunctionCallOrders = null;

		/// <summary>
		/// パッカー情報リスト
		/// </summary>
		private static Dictionary<string, AssetBundlePackerArg.AssetBundleFlags> s_PackerInfoList = null;

		/// <summary>
		/// 出力ディレクトリバックアップパス
		/// </summary>
		private string m_BackupOutputPath = null;

		/// <summary>
		/// 作業ディレクトリバックアップパス
		/// </summary>
		private string m_BackupWorkPath = null;

		/// <summary>
		/// アセット内作業ディレクトリバックアップパス
		/// </summary>
		private string m_BackupInAssetsWorkPath = null;

		/// <summary>
		/// 作業ディレクトリバックアップパス
		/// </summary>
		private HashAlgorithm m_HashAlgorithm = new HashAlgorithm();

		#endregion
		#region Private methods

		/// <summary>
		/// 作業ディレクトリのバックアップ
		/// </summary>
		private void BackupWorkDirectory() {
			if (!AssetBundleEditorUtility.Exists(kBackupBasePath)) {
				Directory.CreateDirectory(Application.dataPath + "/../" + kBackupBasePath);
			}
			if (AssetBundleEditorUtility.Exists(AssetBundleBuilder.kOutputPath)) {
				m_BackupOutputPath = GetBackupDirectoryPath();
				FileUtil.MoveFileOrDirectory(AssetBundleBuilder.kOutputPath, m_BackupOutputPath);
			} else {
				m_BackupOutputPath = null;
			}
			if (AssetBundleEditorUtility.Exists(kBuilderWorkPath)) {
				m_BackupWorkPath = GetBackupDirectoryPath();
				FileUtil.MoveFileOrDirectory(kBuilderWorkPath, m_BackupWorkPath);
			} else {
				m_BackupWorkPath = null;
			}
			if (AssetBundleEditorUtility.Exists(kBuilderInAssetsWorkPath)) {
				m_BackupInAssetsWorkPath = GetBackupDirectoryPath();
				FileUtil.MoveFileOrDirectory(kBuilderInAssetsWorkPath, m_BackupInAssetsWorkPath);
				if (AssetBundleEditorUtility.Exists(kBuilderInAssetsWorkPath + ".meta")) {
					FileUtil.MoveFileOrDirectory(kBuilderInAssetsWorkPath + ".meta", m_BackupInAssetsWorkPath + ".meta");
				}
			} else {
				m_BackupWorkPath = null;
			}
		}

		/// <summary>
		/// 作業ディレクトリの復元
		/// </summary>
		private void RecoveryWorkDirectory() {
			FileUtil.DeleteFileOrDirectory(kBuilderInAssetsWorkPath + ".meta");
			FileUtil.DeleteFileOrDirectory(kBuilderInAssetsWorkPath);
			if (!string.IsNullOrEmpty(m_BackupInAssetsWorkPath)) {
				FileUtil.MoveFileOrDirectory(m_BackupInAssetsWorkPath + ".meta", kBuilderInAssetsWorkPath + ".meta");
				FileUtil.MoveFileOrDirectory(m_BackupInAssetsWorkPath, kBuilderInAssetsWorkPath);
				FileUtil.DeleteFileOrDirectory(m_BackupInAssetsWorkPath + ".meta");
				FileUtil.DeleteFileOrDirectory(m_BackupInAssetsWorkPath);
				m_BackupInAssetsWorkPath = null;
			}

			FileUtil.DeleteFileOrDirectory(kBuilderWorkPath);
			if (!string.IsNullOrEmpty(m_BackupWorkPath)) {
				FileUtil.MoveFileOrDirectory(m_BackupWorkPath, kBuilderWorkPath);
				FileUtil.DeleteFileOrDirectory(m_BackupWorkPath);
				m_BackupWorkPath = null;
			}
			FileUtil.DeleteFileOrDirectory(AssetBundleBuilder.kOutputPath);
			if (!string.IsNullOrEmpty(m_BackupOutputPath)) {
				FileUtil.MoveFileOrDirectory(m_BackupOutputPath, AssetBundleBuilder.kOutputPath);
				FileUtil.DeleteFileOrDirectory(m_BackupOutputPath);
				m_BackupOutputPath = null;
			}
			FileUtil.DeleteFileOrDirectory(kBackupBasePath);
		}

		/// <summary>
		/// バックアップディレクトリパスの取得
		/// </summary>
		/// <returns>バックアップディレクトリパス</returns>
		private static string GetBackupDirectoryPath() {
			var index = 0;
			while (AssetBundleEditorUtility.Exists(kBackupBasePath + "/" + index.ToString())) {
				++index;
			}
			var result = kBackupBasePath + "/" + index.ToString();
			return result;
		}

		/// <summary>
		/// アセットバンドルファイル存在確認
		/// </summary>
		/// <param name="platform">プラットフォーム</param>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:存在する, false:無い</returns>
		private bool ExistsAssetBundleFile(string platform, string assetBundleNameWithVariant) {
			var fileName = m_HashAlgorithm.GetAssetBundleFileName(platform, assetBundleNameWithVariant);
			var filePath = AssetBundleBuilder.kOutputPath + "/" + fileName;
			var result = AssetBundleEditorUtility.Exists(filePath);
			return result;
		}

		/// <summary>
		/// アセットバンドル内テスト
		/// </summary>
		/// <param name="platform">プラットフォーム</param>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <param name="onDownloaded">ダウンロード後アクション</param>
		private void TestInAssetBundle(string platform, string assetBundleNameWithVariant, UnityAction<AssetBundle> onDownloaded) {
			var fileName = m_HashAlgorithm.GetAssetBundleFileName(platform, assetBundleNameWithVariant);
			var filePath = AssetBundleBuilder.kOutputPath + "/" + fileName;
			var fileFullPath = Application.dataPath + "/../" + filePath;
			var ab = AssetBundle.LoadFromFile(fileFullPath);
			onDownloaded.Invoke(ab);
			if (ab != null) {
				ab.Unload(true);
			}
		}

		/// <summary>
		/// 配信ストリーミングアセット内テスト
		/// </summary>
		/// <param name="platform">プラットフォーム</param>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <param name="onDownloaded">ダウンロード後アクション</param>
		private void TestInDeliveryStreamingAssets(string platform, string assetBundleNameWithVariant, UnityAction<byte[]> onDownloaded) {
			var fileName = m_HashAlgorithm.GetAssetBundleFileName(platform, assetBundleNameWithVariant);
			var filePath = AssetBundleBuilder.kOutputPath + "/" + fileName;
			var fileFullPath = Application.dataPath + "/../" + filePath;
			var dsa = File.ReadAllBytes(fileFullPath);
			onDownloaded.Invoke(dsa);
		}

		/// <summary>
		/// テスト用アセットバンドルのビルド範囲限定パッカー
		///	テスト中は所属以外のアセットバンドルを作成しないようにし、テスト外では所属アセットバンドルを作成しない様にする
		/// </summary>
		/// <param name="arg">パッカー引数</param>
		[AssetBundlePacker(100000000)]
		private static void BuildRangeLimitedPackerForTest(AssetBundlePackerArg arg) {
			var isMyAssetBundles = IsMyAssetBundles(arg.assetBundleNameWithVariant);
			if (s_TestBuildEnable) {
				//テスト用ビルド
				if (!isMyAssetBundles) {
					//テスト系リソース以外は除外
					arg.options |= AssetBundlePackerArg.AssetBundleFlags.Exclude;
				}
				if (isMyAssetBundles) {
					if (s_FunctionCallOrders != null) {
						s_FunctionCallOrders.Add(kPackerFunctionCallOrder);
					}
					if (s_PackerInfoList != null) {
						s_PackerInfoList.Add(arg.assetBundleNameWithVariant, arg.options);
					}
				}
			} else {
				//非テスト時ビルド
				if (isMyAssetBundles) {
					//テスト系リソースは除外
					arg.options |= AssetBundlePackerArg.AssetBundleFlags.Exclude;
				}
			}
		}

		/// <summary>
		/// 所属アセットバンドル確認
		/// </summary>
		/// <param name="assetBundleNameWithVariant">バリアント付きアセットバンドル名</param>
		/// <returns>true:所属アセットバンドル、false:部外所属アセットバンドル</returns>
		private static bool IsMyAssetBundles(string assetBundleNameWithVariant) {
			var result = false;
			if (assetBundleNameWithVariant.StartsWith(kMyDeliveryStreamingAssetNamePrefix)) {
				//所属配信ストリーミングアセット
				result = true;
			} else if (assetBundleNameWithVariant.StartsWith(kMyAssetBundleNamePrefix)) {
				//所属アセットバンドル
				result = true;
			}
			return result;
		}

		[AssetBundlePreprocessor(1)]
		private static void PreprocessorBuild1ForTest(AssetBundlePreprocessorArg arg) {
			if (s_FunctionCallOrders != null) {
				s_FunctionCallOrders.Add(kPreprocessorFunctionCallOrder + 1);
			}
		}

		[AssetBundlePreprocessor(2)]
		private static void PreprocessorBuild2ForTest(AssetBundlePreprocessorArg arg) {
			if (s_FunctionCallOrders != null) {
				s_FunctionCallOrders.Add(kPreprocessorFunctionCallOrder + 2);
			}
		}

		[AssetBundlePostprocessor(1)]
		private static void PostprocessorBuild1ForTest(AssetBundlePostprocessorArg arg) {
			if (s_FunctionCallOrders != null) {
				s_FunctionCallOrders.Add(kPostprocessorFunctionCallOrder + 1);
			}
		}

		[AssetBundlePostprocessor(2)]
		private static void PostprocessorBuild2ForTest(AssetBundlePostprocessorArg arg) {
			if (s_FunctionCallOrders != null) {
				s_FunctionCallOrders.Add(kPostprocessorFunctionCallOrder + 2);
			}
		}

		#endregion
	}
}
