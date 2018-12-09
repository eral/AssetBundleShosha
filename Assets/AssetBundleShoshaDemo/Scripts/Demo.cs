// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaDemo {
	using System.Text;
	using UnityEngine;
	using UnityEngine.UI;
	using AssetBundleShosha;

	public class Demo : MonoBehaviour, IProgressReceiver, IErrorHandler {
		#region Public methods

		/// <summary>
		/// マネージャー初期化
		/// </summary>
		public void AssetBundleManagerInitialize() {
//			var baseURL = "file:///" + Application.dataPath.Replace('\\', '/') + "/../AssetBundles/";
			var baseURL = "http://localhost:3080/";
			AssetBundleManager.Instance.Initialize(baseURL);
		}

		/// <summary>
		/// アセットバンドル全クリア
		/// </summary>
		public void AssetBundleManagerClearAllAssetBundle() {
			AssetBundleManager.Instance.ClearAllAssetBundle();
		}

		/// <summary>
		/// アセットバンドルキャッシュクリア
		/// </summary>
		public void AssetBundleManagerClearCache() {
			AssetBundleManager.Instance.ClearCache();
		}

		/// <summary>
		/// ソロアセットバンドル読み込み
		/// </summary>
		public void LoadSoloAssetBundle() {
			m_SoloAssetBundle = AssetBundleManager.Instance.LoadAssetBundle("AssetBundleShoshaDemo/Textures1");
		}

		/// <summary>
		/// ソロアセットバンドル破棄
		/// </summary>
		public void DisposeSoloAssetBundle() {
			if (m_SoloAssetBundle != null) {
				m_SoloAssetBundle.Dispose();
				m_SoloAssetBundle = null;
			}
		}

		/// <summary>
		/// ソロアセットバンドルnull化
		/// </summary>
		public void NullifySoloAssetBundle() {
			m_SoloAssetBundle = null;
		}

		/// <summary>
		/// ソロアセットバンドルからテクスチャ読み込み
		/// </summary>
		public void LoadTextureFromSoloAssetBundle() {
			if (m_SoloAssetBundle != null) {
				m_SoloAssetBundle.LoadAssetAsync<Texture>("Noise0").completed += x=>{
					var loadRequest = (IAssetBundleRequest)x;
					m_Texture = (Texture)loadRequest.asset;
				};
			}
		}

		/// <summary>
		/// テクスチャnull化
		/// </summary>
		public void NullifyTexture() {
			m_Texture = null;
		}

		/// <summary>
		/// ソロアセットバンドルからスプライト読み込み
		/// </summary>
		public void LoadSpriteFromSoloAssetBundle() {
			if (m_SoloAssetBundle != null) {
				m_SoloAssetBundle.LoadAssetAsync<Sprite>("Noise0").completed += x=>{
					var loadRequest = (IAssetBundleRequest)x;
					m_Sprite = (Sprite)loadRequest.asset;
				};
			}
		}

		/// <summary>
		/// スプライトnull化
		/// </summary>
		public void NullifySprite() {
			m_Sprite = null;
		}

		/// <summary>
		/// 依存有りアセットバンドル読み込み
		/// </summary>
		public void LoadDependenciesAssetBundle() {
			AssetBundleManager.Instance.LoadAssetBundle("AssetBundleShoshaDemo/Materials", x=>{
				m_DependenciesAssetBundle = x;
				UpdateStatus();
			});
		}

		/// <summary>
		/// 依存有りアセットバンドル破棄
		/// </summary>
		public void DisposeDependenciesAssetBundle() {
			if (m_DependenciesAssetBundle != null) {
				m_DependenciesAssetBundle.Dispose();
				m_DependenciesAssetBundle = null;
			}
		}

		/// <summary>
		/// 依存有りアセットバンドルnull化
		/// </summary>
		public void NullifyDependenciesAssetBundle() {
			m_DependenciesAssetBundle = null;
		}

		/// <summary>
		/// 依存有りアセットバンドルからサークル読み込み
		/// </summary>
		public void LoadCircleFromDependenciesAssetBundle() {
			if (m_DependenciesAssetBundle != null) {
				m_DependenciesAssetBundle.LoadAssetAsync<Material>("Circle").completed += x=>{
					m_Circle = (Material)x;
				};
			}
		}

		/// <summary>
		/// サークルnull化
		/// </summary>
		public void NullifyCircle() {
			m_Circle = null;
		}

		/// <summary>
		/// 存在しないアセットバンドル読み込み
		/// </summary>
		public void LoadMissingAssetBundle() {
			m_MissingAssetBundle = AssetBundleManager.Instance.LoadAssetBundle("AssetBundleShoshaDemo/Missing");
		}

		/// <summary>
		/// 存在しないアセットバンドル破棄
		/// </summary>
		public void DisposeMissingAssetBundle() {
			if (m_MissingAssetBundle != null) {
				m_MissingAssetBundle.Dispose();
				m_MissingAssetBundle = null;
			}
		}

		/// <summary>
		/// 存在しないアセットバンドルnull化
		/// </summary>
		public void NullifyMissingAssetBundle() {
			m_MissingAssetBundle = null;
		}

		/// <summary>
		/// 存在しないアセットバンドル読み込み(特殊エラー処理対応)
		/// </summary>
		public void LoadMissingAssetBundleWithSpecialErrorAction() {
			m_MissingAssetBundle = AssetBundleManager.Instance.LoadAssetBundle("AssetBundleShoshaDemo/Missing");
			m_MissingAssetBundle.errorHandler = new ErrorAction(y=>{
				var message = string.Format(kDownloadErrorMessageFormat, y.errorCode);
				m_DemoDialog.Create(message, new[]{kDownloadErrorButtonTexts[0]}, onFinished:x=>{
					switch (x) {
					case 0: //Ignore
					default:
						y.Ignore();
						break;
					}
				});
			});
		}

		/// <summary>
		/// 進捗開始
		/// </summary>
		/// <remarks>
		///		この関数1回に付きProgressFinished()が1回呼ばれる
		///		この関数呼び出し後、同一フレームで最低1回ProgressUpdate()が呼び出される
		/// </remarks>
		public void ProgressStart() {
			m_ProgressBar.color = Color.cyan;
		}

		/// <summary>
		/// 進捗更新
		/// </summary>
		/// <remarks>ProgressStart()とProgressFinished()の間でのみ呼ばれる</remarks>
		public void ProgressUpdate(float progress) {
			m_ProgressText.text = progress.ToString("##0.0%");
			m_ProgressBar.rectTransform.localScale = new Vector3(progress, 1.0f, 1.0f);
		}

		/// <summary>
		/// 進捗終了
		/// </summary>
		/// <remarks>この関数が呼ばれる前には必ずProgressStart()が呼ばれている</remarks>
		public void ProgressFinished() {
			m_ProgressBar.color = Color.gray;
		}

		/// <summary>
		/// エラー開始
		/// </summary>
		/// <remarks>
		///		ProgressStart()とProgressFinished()の間でのみ呼ばれる
		///		この関数1回に付きProgressErrorFinished()が1回呼ばれる
		/// </remarks>
		public void ProgressErrorStart() {
			m_ProgressBar.color = Color.red;
		}

		/// <summary>
		/// エラー終了
		/// </summary>
		/// <remarks>
		///		ProgressStart()とProgressFinished()の間でのみ呼ばれる
		///		この関数が呼ばれる前には必ずProgressErrorStart()が呼ばれている
		///	</remarks>
		public void ProgressErrorFinished() {
			m_ProgressBar.color = Color.cyan;
		}

		/// <summary>
		/// エラー発生
		/// </summary>
		/// <param name="handle">エラーハンドル</param>
		public void Error(IErrorHandle handle) {
			var message = string.Format(kDownloadErrorMessageFormat, handle.errorCode);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			message += "\n\n[DebugInfo]\n" + handle.assetBundleNameWithVariant;
#endif
			m_DemoDialog.Create(message, kDownloadErrorButtonTexts, onFinished:x=>{
				switch (x) {
				case 1: //Retry
					handle.Retry();
					break;
				case 0: //Ignore
				default:
					handle.Ignore();
					break;
				}
			});
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// 初回更新前
		/// </summary>
		protected virtual void Start() {
			StartStatus();
		}

		/// <summary>
		/// 更新
		/// </summary>
		protected virtual void Update() {
			UpdateStatus();
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// ダウンロードエラーメッセージテキスト
		/// </summary>
		private const string kDownloadErrorMessageFormat = "A download error occurred.\n(ErrorCode:{0})";

		/// <summary>
		/// ダウンロードエラーボタンテキスト群
		/// </summary>
		private readonly string[] kDownloadErrorButtonTexts = new[]{"Ignore", "Retry"};

		#endregion
		#region Private fields and properties

		/// <summary>
		/// ステータス
		/// </summary>
		[SerializeField]
		private Text m_Status;

		/// <summary>
		/// ソロアセットバンドル
		/// </summary>
		[SerializeField][HideInInspector]
		private IAssetBundle m_SoloAssetBundle;

		/// <summary>
		/// テクスチャ
		/// </summary>
		[SerializeField][HideInInspector]
		private Texture m_Texture;

		/// <summary>
		/// テクスチャ
		/// </summary>
		[SerializeField][HideInInspector]
		private Sprite m_Sprite;

		/// <summary>
		/// 依存有りアセットバンドル
		/// </summary>
		[SerializeField][HideInInspector]
		private IAssetBundle m_DependenciesAssetBundle;

		/// <summary>
		/// Circle
		/// </summary>
		[SerializeField][HideInInspector]
		private Material m_Circle;

		/// <summary>
		/// 存在しないアセットバンドル
		/// </summary>
		[SerializeField][HideInInspector]
		private IAssetBundle m_MissingAssetBundle;

		/// <summary>
		/// ステータス文字列
		/// </summary>
		private StringBuilder m_StatusStringBuilder;

		/// <summary>
		/// プログレステキスト
		/// </summary>
		[SerializeField]
		private Text m_ProgressText;

		/// <summary>
		/// プログレスバー
		/// </summary>
		[SerializeField]
		private Image m_ProgressBar;

		/// <summary>
		/// エラーダイアログ
		/// </summary>
		[SerializeField]
		private DemoDialog m_DemoDialog;

		#endregion
		#region Private methods

		/// <summary>
		/// ステータス初回更新前
		/// </summary>
		private void StartStatus() {
			UpdateStatus();
		}

		/// <summary>
		/// ステータス更新
		/// </summary>
		private void UpdateStatus() {
			if (m_StatusStringBuilder != null) {
				m_StatusStringBuilder.Length = 0;
			} else {
				m_StatusStringBuilder = new StringBuilder();
			}

			var assetBundleManager = AssetBundleManager.Instance;
			m_StatusStringBuilder.Append("AssetBundleManager: ");
			if (!string.IsNullOrEmpty(assetBundleManager.baseURL)) {
				m_StatusStringBuilder.Append("Ready");
			} else {
				m_StatusStringBuilder.Append("No Ready");
			}
			m_StatusStringBuilder.Append('\n');
			m_StatusStringBuilder.Append("SoloAssetBundle: ");
			if (m_SoloAssetBundle == null) {
				m_StatusStringBuilder.Append("null");
			} else if (!m_SoloAssetBundle.isDone) {
				m_StatusStringBuilder.Append("downloading");
			} else {
				m_StatusStringBuilder.Append("valid");
			}
			m_StatusStringBuilder.Append('\n');
			m_StatusStringBuilder.Append("Texture: ");
			if (m_Texture != null) {
				m_StatusStringBuilder.Append("valid");
			} else {
				m_StatusStringBuilder.Append("null");
			}
			m_StatusStringBuilder.Append('\n');
			m_StatusStringBuilder.Append("Sprite: ");
			if (m_Sprite != null) {
				m_StatusStringBuilder.Append("valid");
			} else {
				m_StatusStringBuilder.Append("null");
			}
			m_StatusStringBuilder.Append('\n');
			m_StatusStringBuilder.Append("DependenciesAssetBundle: ");
			if (m_DependenciesAssetBundle != null) {
				m_StatusStringBuilder.Append("valid");
			} else {
				m_StatusStringBuilder.Append("null");
			}
			m_StatusStringBuilder.Append('\n');
			m_StatusStringBuilder.Append("Circle: ");
			if (m_Circle != null) {
				m_StatusStringBuilder.Append("valid");
			} else {
				m_StatusStringBuilder.Append("null");
			}
			m_StatusStringBuilder.Append('\n');

			--m_StatusStringBuilder.Length; //最後の改行削除
			m_Status.text = m_StatusStringBuilder.ToString();

		}

		#endregion
	}

}
