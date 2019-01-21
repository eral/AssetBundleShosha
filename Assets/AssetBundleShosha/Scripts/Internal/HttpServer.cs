// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

#if UNITY_EDITOR
namespace AssetBundleShosha.Internal {
	using System.Collections.Generic;
	using Process = System.Diagnostics.Process;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEditor;
	using AssetBundleShosha.Utility;

	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class HttpServer : SingletonMonoBehaviour<HttpServer> {
		#region Public types
		#endregion
		#region Public const fields

		/// <summary>
		/// HTTPサーバーポート初期値
		/// </summary>
		public const int kHttpServerPortDefault = 5080;

		/// <summary>
		/// HTTPサーバーポートのEditorPrefsキー
		/// </summary>
		public const string kHttpServerPortEditorPrefsKey = "AssetBundleShosha/HttpServer/Port";

		#endregion
		#region Public fields and properties

		/// <summary>
		/// ポート
		/// </summary>
		public int port {get{
			return m_Port;
		} set{
			if (m_Port != value) {
				m_Port = value;
				EditorPrefs.SetInt(kHttpServerPortEditorPrefsKey, m_Port);
				if (m_Process != null) {
					StartCoroutine(RestartHttpServer());
				}
			}
		}}

		/// <summary>
		/// 終了時イベント
		/// </summary>
		public UnityEvent onWillFinish {get{
			if (m_OnWillFinish == null) {
				m_OnWillFinish = new UnityEvent();
			}
			return m_OnWillFinish;
		}}

		#endregion
		#region Public methods
		#endregion
		#region Protected types
		#endregion
		#region Protected const fields
		#endregion
		#region Protected fields and properties

		/// <summary>
		/// 構築
		/// </summary>
		protected override void Awake() {
			base.Awake();
			m_Port = EditorPrefs.GetInt(kHttpServerPortEditorPrefsKey, kHttpServerPortDefault);
		}

		/// <summary>
		/// 有効化
		/// </summary>
		protected virtual void OnEnable() {
			var success = StartHttpServer();
			if (!success) {
				Destroy(gameObject);
			}
			if (!Application.isPlaying) {
				EditorApplication.update += Update;
			}
		}

		/// <summary>
		/// 無効化
		/// </summary>
		protected virtual void OnDisable() {
			EditorApplication.update -= Update;
			ExitHttpServer();
			if (m_OnWillFinish != null) {
				m_OnWillFinish.Invoke();
				m_OnWillFinish = null;
			}
		}

		/// <summary>
		/// 更新
		/// </summary>
		protected virtual void Update() {
			if ((m_Process == null) || (m_Process.HasExited)) {
				if (Application.isPlaying) {
					Destroy(gameObject);
				} else {
					DestroyImmediate(gameObject);
				}
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// バリデーション
		/// </summary>
		protected virtual void OnValidate() {
			var port = EditorPrefs.GetInt(kHttpServerPortEditorPrefsKey, kHttpServerPortDefault);
			if (m_Port != port) {
				m_Port = port;
				EditorPrefs.SetInt(kHttpServerPortEditorPrefsKey, m_Port);
				if (m_Process != null) {
					StartCoroutine(RestartHttpServer());
				}
			}
		}
#endif

		#endregion
		#region Protected methods
		#endregion
		#region Internal types
		#endregion
		#region Internal const fields
		#endregion
		#region Internal fields and properties
		#endregion
		#region Internal methods
		#endregion
		#region Private types
		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// 起動済みHTTPサーバーのプロセス
		/// </summary>
		[System.NonSerialized]
		private Process m_Process = null;

		/// <summary>
		/// ポート
		/// </summary>
		[System.NonSerialized]
		private int m_Port = kHttpServerPortDefault;

		/// <summary>
		/// 終了時イベント
		/// </summary>
		[System.NonSerialized]
		private UnityEvent m_OnWillFinish = null;

		#endregion
		#region Private methods

		/// <summary>
		/// HTTPサーバープロセス起動
		/// </summary>
		/// <returns>true:成功、false:失敗</returns>
		private bool StartHttpServer() {
			if (m_Process != null) {
				throw new System.InvalidOperationException("Already started");
			}

#if UNITY_EDITOR_WIN
			var nodeJsPath = EditorApplication.applicationContentsPath + "/Tools/nodejs/node";
#else
			var nodeJsPath = EditorApplication.applicationContentsPath + "/Tools/nodejs/bin/node";
#endif
			var httpServerNodeJsPath = Application.dataPath + "/AssetBundleShosha/Scripts/Editor/Internal/HttpServer.nodejs";
			var httpServerAssetBundlesDirectoryPath = Application.dataPath + "/../AssetBundles";
			var httpServerWorkingDirectoryPath = Application.dataPath + "/..";

			var process = new Process();
			process.StartInfo.FileName = nodeJsPath;
			var processArgs = "\"" + httpServerNodeJsPath + "\" --port " + m_Port + " --directory \"" + httpServerAssetBundlesDirectoryPath + "\"";
			process.StartInfo.Arguments = processArgs;
			process.StartInfo.WorkingDirectory = httpServerWorkingDirectoryPath;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			process.Exited += OnExitedHttpServer;
			var result = false;
			if (process.Start()) {
				if (!process.HasExited) {
					m_Process = process;
					result = true;
				}
			}
			//UnityEngine.Debug.Log("\"" + nodeJsPath + "\" " + processArgs);
			return result;
		}

		/// <summary>
		/// HTTPサーバー終了
		/// </summary>
		/// <returns>true:成功、false:失敗</returns>
		private bool ExitHttpServer() {
			var result = false;
			if ((m_Process != null) && !m_Process.HasExited) {
				m_Process.Kill();
				//OnExitedHttpServer() に続く
				result = true;
			}
			return result;
		}

		/// <summary>
		/// HTTPサーバー終了イベント
		/// </summary>
		/// <param name="sender">送信者</param>
		/// <param name="e">イベントデータ</param>
		private void OnExitedHttpServer(object sender, System.EventArgs e) {
			EditorApplication.delayCall = ()=>{
				OnExitedHttpServer();
			};
		}

		/// <summary>
		/// HTTPサーバー終了イベント
		/// </summary>
		private void OnExitedHttpServer() {
			m_Process = null;
		}

		/// <summary>
		/// HTTPサーバー再起動
		/// </summary>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> RestartHttpServer() {
			ExitHttpServer();
			while (m_Process != null) {
				yield return null;
			}
			StartHttpServer();
		}

		#endregion
	}
}
#endif
