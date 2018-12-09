// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using UnityEngine;
	using UnityEngine.Networking;
	using UnityEditor;

	public class HttpServerViewer : EditorWindow {
		#region Public types
		#endregion
		#region Public const fields

		/// <summary>
		/// HTTPサーバー有効確認
		/// </summary>
		public static bool enable {get{
			var processId = SessionState.GetInt(kHttpServerProcessIdSessionStateKey, 0);
			var result = processId != 0;
			return result;
		}}

		#endregion
		#region Public fields and properties

		/// <summary>
		/// HTTPサーバープロセス起動
		/// </summary>
		/// <param name="port">ポート</param>
		/// <returns>true:成功、false:失敗</returns>
		public static bool StartHttpServer(int port) {
			var result = false;
			var process = StartHttpServerProcess(port);
			if (process != null) {
				var processId = process.Id;
				SessionState.SetInt(kHttpServerProcessIdSessionStateKey, processId);
				result = true;
			}
			return result;
		}
		public static bool StartHttpServer() {
			var port = EditorPrefs.GetInt(kHttpServerPortEditorPrefsKey, kHttpServerPortDefault);
			return StartHttpServer(port);
		}

		/// <summary>
		/// HTTPサーバー終了
		/// </summary>
		/// <param name="processId">プロセスID</param>
		/// <returns>true:成功、false:失敗</returns>
		private static bool ExitHttpServer() {
			var result = false;
			var processId = SessionState.GetInt(kHttpServerProcessIdSessionStateKey, 0);
			if (processId != 0) {
				var process = FindHttpServerProcess(processId);
				if ((process != null) && !process.HasExited) {
					process.Kill();
					//OnExitedHttpServer() に続く
					result = true;
				}
			}
			return result;
		}

		#endregion
		#region Public methods
		#endregion
		#region Protected methods

		/// <summary>
		/// 構築
		/// </summary>
		protected virtual void OnEnable() {
			titleContent = new GUIContent("Shosha HTTP Server Viewer");

			m_ProcessId = SessionState.GetInt(kHttpServerProcessIdSessionStateKey, 0);
			m_Port = EditorPrefs.GetInt(kHttpServerPortEditorPrefsKey, kHttpServerPortDefault);

			if (m_ProcessId != 0) {
				EditorApplication.update += HttpServerUpdate;
			}
		}

		/// <summary>
		/// 破棄
		/// </summary>
		protected virtual void OnDisable() {
			//empty.
		}

		/// <summary>
		/// 更新
		/// </summary>
		protected virtual void Update() {
			if (m_DisplayDirty) {
				m_DisplayDirty = false;
				Repaint();
			}
		}

		/// <summary>
		/// 描画
		/// </summary>
		protected virtual void OnGUI() {
			EditorGUILayout.HelpBox("experimental", MessageType.Warning);
			OnGUIForBasic();
			OnGUIForConfig();
		}

		#endregion
		#region Private types

		/// <summary>
		/// HTTPサーバーコンフィグ
		/// </summary>
		[System.Serializable]
		private struct HttpServerConfig {
			public int MaxBandwidthBps;
		}

		/// <summary>
		/// HTTPサーバーコンフィグ状態
		/// </summary>
		private enum HttpServerConfigState {
			Empty,
			Valid,
			Invalid,
			Connecting,
			Change,
			ConnectingAndChange,
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// HTTPサーバープロセスIDのSessionStateキー
		/// </summary>
		private const string kHttpServerProcessIdSessionStateKey = "AssetBundleShosha/HttpServer/ProcessId";

		/// <summary>
		/// HTTPサーバーポート初期値
		/// </summary>
		private const int kHttpServerPortDefault = 3080;

		/// <summary>
		/// HTTPサーバーポートのEditorPrefsキー
		/// </summary>
		private const string kHttpServerPortEditorPrefsKey = "AssetBundleShosha/HttpServer/Port";

		/// <summary>
		/// 有効無効コンテント
		/// </summary>
		private static readonly GUIContent kEnableContent = new GUIContent("Enable");

		/// <summary>
		/// ポートコンテント
		/// </summary>
		private static readonly GUIContent kPortContent = new GUIContent("HTTP Port");

		/// <summary>
		/// URLコンテント
		/// </summary>
		private static readonly GUIContent kUrlContent = new GUIContent("URL");

		/// <summary>
		/// ログコピーコンテント
		/// </summary>
		private static readonly GUIContent kUrlCopyContent = new GUIContent("Copy");

		/// <summary>
		/// 最大帯域コンテント
		/// </summary>
		private static readonly GUIContent kMaxBandwidthKindContent = new GUIContent("Max Bandwidth");

		/// <summary>
		/// 帯域種類コンテント
		/// </summary>
		private static readonly int[] kBandwidthBps = {0 //None
													, 1 * 1000 * 1000 * 1000 / 100 * 80 //FTTH (1Gbps * 80%)
													, 300 * 1000 * 1000 / 100 * 80 //4G (300Mbps * 80%)
													, 54 * 1000 * 1000 / 100 * 80 //Wi-Fi 11a/g (54Mbps * 80%)
													, 40 * 1000 * 1000 / 100 * 80 //3.9G (40Mbps * 80%)
													, 11 * 1000 * 1000 / 100 * 80 //Wi-Fi 11b (11Mbps * 80%)
													, 1500 * 1000 / 100 * 80 //DSL (1.5Mbps * 80%)
													, 384 * 1000 / 100 * 80 //3G (384Kbps * 80%)
													, 200 * 1000 / 100 * 80 //Bandwidth Control Mobile Network (200Kbps * 80%)
													, 128 * 1000 / 100 * 80 //ISDN (128Kbps * 80%)
													, 56 * 1000 / 100 * 80 //DialUp (56Kbps * 80%)
													};

		/// <summary>
		/// 帯域種類コンテント
		/// </summary>
		private static readonly GUIContent[] kBandwidthKindContent = {new GUIContent("None")
																	, new GUIContent("FTTH (1Gbps * 80%)")
																	, new GUIContent("4G (300Mbps * 80%)")
																	, new GUIContent("Wi-Fi 11a|g (54Mbps * 80%)")
																	, new GUIContent("3.9G (40Mbps * 80%)")
																	, new GUIContent("Wi-Fi 11b (11Mbps * 80%)")
																	, new GUIContent("DSL (1.5Mbps * 80%)")
																	, new GUIContent("3G (384Kbps * 80%)")
																	, new GUIContent("Bandwidth Control Mobile Network (200Kbps * 80%)")
																	, new GUIContent("ISDN (128Kbps * 80%)")
																	, new GUIContent("DialUp (56Kbps * 80%)")
																	, new GUIContent("Custom")
																	};

		/// <summary>
		/// 最大帯域(bps)コンテント
		/// </summary>
		private static readonly GUIContent kMaxBandwidthBpsContent = new GUIContent("Max Bandwidth(bps)");

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 起動済みHTTPサーバーのポート
		/// </summary>
		[System.NonSerialized]
		private int m_ProcessId = 0;

		/// <summary>
		/// ポート
		/// </summary>
		[System.NonSerialized]
		private int m_Port = kHttpServerPortDefault;

		/// <summary>
		/// URL
		/// </summary>
		private string url {get{
			if (string.IsNullOrEmpty(m_Url)) {
				m_Url = GetUrl(m_Port);
			}
			return m_Url;
		} set{
			m_Url = value;
		}}
		[System.NonSerialized]
		private string m_Url = null;

		/// <summary>
		/// ダーティ
		/// </summary>
		[SerializeField]
		private bool m_DisplayDirty = false;

		/// <summary>
		/// 帯域種類カスタム
		/// </summary>
		[SerializeField]
		private bool m_IsCustomBandwidthKind = false;

		/// <summary>
		/// HTTPサーバーコンフィグ
		/// </summary>
		[SerializeField]
		private static HttpServerConfig? s_HttpServerConfig = null;

		/// <summary>
		/// HTTPサーバーコンフィグ状態
		/// </summary>
		[SerializeField]
		private static HttpServerConfigState s_HttpServerConfigState = HttpServerConfigState.Empty;

		#endregion
		#region Private methods

		/// <summary>
		/// 基本描画
		/// </summary>
		private void OnGUIForBasic() {
			var enable = m_ProcessId != 0;

			EditorGUI.BeginChangeCheck();
			enable = EditorGUILayout.Toggle(kEnableContent, enable);
			if (EditorGUI.EndChangeCheck()) {
				if (enable) {
					//有効化
					var process = StartHttpServerProcess(m_Port);
					if (process != null) {
						var processId = process.Id;
						m_ProcessId = processId;
						SessionState.SetInt(kHttpServerProcessIdSessionStateKey, processId);
					}
				} else {
					//無効化
					if (!ExitHttpServerProcess(m_ProcessId)) {
						//終了に失敗したならプロセス情報を即時削除
						m_ProcessId = 0;
						SessionState.EraseInt(kHttpServerProcessIdSessionStateKey);
					}
					url = null;
				}
			}

			if (enable) {
				//実行中
				using (new EditorGUILayout.HorizontalScope()) {
					var editingTextFieldOld = EditorGUIUtility.editingTextField;
					EditorGUIUtility.editingTextField = false;
					EditorGUILayout.TextField(kUrlContent, url);
					EditorGUIUtility.editingTextField = editingTextFieldOld;

					if (GUILayout.Button(kUrlCopyContent, EditorStyles.miniButton)) {
						GUIUtility.systemCopyBuffer = url;
					}
				}
			} else {
				//停止中
				var port = m_Port;
				EditorGUI.BeginChangeCheck();
				port = EditorGUILayout.IntField(kPortContent, port);
				if (EditorGUI.EndChangeCheck()) {
					m_Port = port;
					EditorPrefs.SetInt(kHttpServerPortEditorPrefsKey, m_Port);
				}
			}
		}


		/// <summary>
		/// コンフィグ描画
		/// </summary>
		private void OnGUIForConfig() {
			HttpServerConfig config = new HttpServerConfig();
			var isChange = false;
			if (s_HttpServerConfig.HasValue && (s_HttpServerConfigState != HttpServerConfigState.Invalid)) {
				config = s_HttpServerConfig.Value;
			}

			var GuiEnabledOld = GUI.enabled;
			switch (s_HttpServerConfigState) {
			case HttpServerConfigState.Valid:
			case HttpServerConfigState.Connecting:
			case HttpServerConfigState.ConnectingAndChange:
				GUI.enabled = true;
				break;
			default:
				GUI.enabled = false;
				break;
			}

			var maxBandwidthKind = System.Array.IndexOf(kBandwidthBps, config.MaxBandwidthBps);
			if (maxBandwidthKind < 0) {
				maxBandwidthKind = kBandwidthKindContent.Length - 1;
				m_IsCustomBandwidthKind = true;
			}
			EditorGUI.BeginChangeCheck();
			maxBandwidthKind = EditorGUILayout.Popup(kMaxBandwidthKindContent, maxBandwidthKind, kBandwidthKindContent);
			if (EditorGUI.EndChangeCheck()) {
				if (maxBandwidthKind < kBandwidthBps.Length) {
					config.MaxBandwidthBps = kBandwidthBps[maxBandwidthKind];
					m_IsCustomBandwidthKind = false;
					isChange = true;
				} else {
					m_IsCustomBandwidthKind = true;
				}
			}

			var GuiEnabledOld2 = GUI.enabled;
			if (GUI.enabled) {
				GUI.enabled = m_IsCustomBandwidthKind;
			}
			EditorGUI.BeginChangeCheck();
			var maxBandwidthBps = EditorGUILayout.IntField(kMaxBandwidthBpsContent, config.MaxBandwidthBps);
			if (EditorGUI.EndChangeCheck()) {
				config.MaxBandwidthBps = maxBandwidthBps;
				isChange = true;
			}
			GUI.enabled = GuiEnabledOld2;

			if (isChange) {
				s_HttpServerConfig = config;
				if (s_HttpServerConfigState == HttpServerConfigState.Connecting) {
					s_HttpServerConfigState = HttpServerConfigState.ConnectingAndChange;
				} else {
					s_HttpServerConfigState = HttpServerConfigState.Change;
				}
			}
			GUI.enabled = GuiEnabledOld;
		}

		/// <summary>
		/// HTTPサーバープロセス起動
		/// </summary>
		/// <param name="port">ポート</param>
		/// <returns>プロセス</returns>
		private static Process StartHttpServerProcess(int port) {
			var nodeJsPath = EditorApplication.applicationContentsPath + "/Tools/nodejs/node";
			var httpServerNodeJsPath = Application.dataPath + "/AssetBundleShosha/Scripts/Editor/Internal/HttpServer.nodejs";
			var httpServerAssetBundlesDirectoryPath = Application.dataPath + "/../AssetBundles";
			var httpServerWorkingDirectoryPath = Application.dataPath + "/..";

			var process = new Process();
			process.StartInfo.FileName = nodeJsPath;
			var processArgs = "\"" + httpServerNodeJsPath + "\" --port " + port + " --directory \"" + httpServerAssetBundlesDirectoryPath + "\"";
			process.StartInfo.Arguments = processArgs;
			process.StartInfo.WorkingDirectory = httpServerWorkingDirectoryPath;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			process.Exited += OnExitedHttpServer;
			Process result = null;
			if (process.Start()) {
				if (!process.HasExited) {
					s_HttpServerConfig = null;
					s_HttpServerConfigState = HttpServerConfigState.Empty;
					EditorApplication.update += HttpServerUpdate;
					result = process;
				}
			}
			//UnityEngine.Debug.Log("\"" + nodeJsPath + "\" " + processArgs);
			return result;
		}

		/// <summary>
		/// HTTPサーバープロセス終了
		/// </summary>
		/// <param name="processId">プロセスID</param>
		/// <returns>true:成功, false:失敗</returns>
		private static bool ExitHttpServerProcess(int processId) {
			var result = false;
			if (processId != 0) {
				var process = FindHttpServerProcess(processId);
				if ((process != null) && !process.HasExited) {
					process.Kill();
					//OnExitedHttpServer() に続く
					result = true;
				}
			}
			return result;
		}

		/// <summary>
		/// HTTPサーバープロセス検索
		/// </summary>
		/// <param name="processId">プロセスID</param>
		/// <returns>プロセス</returns>
		private static Process FindHttpServerProcess(int processId) {
			Process result = null;
			try {
				result = Process.GetProcessById(processId);
			} catch (System.ArgumentException) {
				//empty.
			}
			return result;
		}

		/// <summary>
		/// URL取得
		/// </summary>
		/// <param name="port">ポート</param>
		/// <returns>URL</returns>
		private static string GetUrl(int port) {
			const int kUrlMaxlength = 55; //最長IPv6表記(39文字)と最長ポート表記(5文字)それにプロトコルや区切り文字が全て入る長さ
			var sb = new StringBuilder(kUrlMaxlength);
			sb.Append("http://");

			var hostname = Dns.GetHostName();
			var hostAddresses = Dns.GetHostAddresses(hostname);
			if (0 < hostAddresses.Length) {
				var hostAddress = hostAddresses.FirstOrDefault(x=>x.AddressFamily == AddressFamily.InterNetwork);
				if (hostAddress == null) {
					hostAddress = hostAddresses[0];
				}
				var address = hostAddress.ToString();
				if (0 <= address.IndexOf('.')) {
					//IPv4
					sb.Append(address);
				} else {
					//IPv6
					sb.Append('[');
					sb.Append(address);
					sb.Append(']');
				}
			} else {
				sb.Append("localhost");
			}
			sb.Append(':');
			sb.Append(port);
			sb.Append('/');

			var result = sb.ToString();
			return result;
		}

		/// <summary>
		/// HTTPサーバー更新
		/// </summary>
		private static void HttpServerUpdate() {
			System.Action<AsyncOperation> readConfig = (x)=>{
				var u = (UnityWebRequestAsyncOperation)x;
				HttpServerConfigState state;
				if (!u.webRequest.isNetworkError && !u.webRequest.isHttpError) {
					var text = u.webRequest.downloadHandler.text;
					s_HttpServerConfig = JsonUtility.FromJson<HttpServerConfig>(text);
					state = HttpServerConfigState.Valid;
				} else {
					s_HttpServerConfig = null;
					state = HttpServerConfigState.Invalid;
				}
				if (s_HttpServerConfigState == HttpServerConfigState.ConnectingAndChange) {
					s_HttpServerConfigState = HttpServerConfigState.Change;
				} else {
					s_HttpServerConfigState = state;
				}
			};

			if (s_HttpServerConfig == null) {
				//コンフィグが無いなら
				if (s_HttpServerConfigState == HttpServerConfigState.Empty) {
					//コンフィグがなく、空なら
					s_HttpServerConfigState = HttpServerConfigState.Connecting;

					var port = EditorPrefs.GetInt(kHttpServerPortEditorPrefsKey, kHttpServerPortDefault);
					var url = GetUrl(port);
					var unityWebRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
					var request = unityWebRequest.SendWebRequest();
					request.completed += readConfig;
				}
			} else if (s_HttpServerConfigState == HttpServerConfigState.Change) {
				//コンフィグが変更されているなら
				s_HttpServerConfigState = HttpServerConfigState.Connecting;

				var urlSb = new StringBuilder();
				var port = EditorPrefs.GetInt(kHttpServerPortEditorPrefsKey, kHttpServerPortDefault);
				urlSb.Append(GetUrl(port));
				urlSb.Append('?');
				urlSb.Append("MaxBandwidthBps=");
				urlSb.Append(s_HttpServerConfig.Value.MaxBandwidthBps);
				var url = urlSb.ToString();
				var unityWebRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
				var request = unityWebRequest.SendWebRequest();
				request.completed += readConfig;
			}
		}

		/// <summary>
		/// HTTPサーバー終了イベント
		/// </summary>
		/// <param name="sender">送信者</param>
		/// <param name="e">イベントデータ</param>
		private static void OnExitedHttpServer(object sender, System.EventArgs e) {
			EditorApplication.delayCall = ()=>{
				SessionState.EraseInt(kHttpServerProcessIdSessionStateKey);
				EditorApplication.update -= HttpServerUpdate;
				s_HttpServerConfig = null;
				s_HttpServerConfigState = HttpServerConfigState.Empty;
				var httpServerViewers = Resources.FindObjectsOfTypeAll<HttpServerViewer>();
				if (httpServerViewers != null) {
					foreach (var httpServerViewer in httpServerViewers) {
						httpServerViewer.OnExitedHttpServer();
					}
				}
			};
		}

		/// <summary>
		/// HTTPサーバー終了イベント
		/// </summary>
		private void OnExitedHttpServer() {
			m_ProcessId = 0;
			m_DisplayDirty = true;
		}

		#endregion
	}
}
