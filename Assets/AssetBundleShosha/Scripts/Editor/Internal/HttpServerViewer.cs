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
	using AssetBundleShosha.Internal;

	public class HttpServerViewer : EditorWindow {
		#region Public types
		#endregion
		#region Public const fields

		/// <summary>
		/// HTTPサーバー有効確認
		/// </summary>
		public static bool enable {get{
			var result = HttpServer.Instance != null;
			return result;
		} set{
			var httpServers = Resources.FindObjectsOfTypeAll<HttpServer>();
			if (value) {
				//起動
				if ((httpServers == null) || (httpServers.Length == 0)) {
					EditorUtility.CreateGameObjectWithHideFlags(typeof(HttpServer).Name, HideFlags.HideAndDontSave, typeof(HttpServer));
				}
			} else {
				//終了
				if (httpServers != null) {
					foreach (var httpServer in httpServers) {
						DestroyImmediate(httpServer.gameObject);
					}
				}
			}
		}}

		#endregion
		#region Public fields and properties
		#endregion
		#region Public methods
		#endregion
		#region Protected methods

		/// <summary>
		/// 構築
		/// </summary>
		protected virtual void OnEnable() {
			titleContent = new GUIContent("Shosha HTTP Server Viewer");

			m_Enable = enable;
			if (m_Enable) {
				AddOnWillFinishListener();
				EditorApplication.update += HttpServerUpdate;
			}
		}

		/// <summary>
		/// 破棄
		/// </summary>
		protected virtual void OnDisable() {
			EditorApplication.update -= HttpServerUpdate;
			RemoveOnWillFinishListener();
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
			OnGUIForBasic();
			OnGUIForConfig();
		}

		#endregion
		#region Internal const fields
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
		/// 有効無効コンテント
		/// </summary>
		private static readonly GUIContent kEnableContent = new GUIContent("Enable");

		/// <summary>
		/// ポートコンテント
		/// </summary>
		private static readonly GUIContent kPortContent = new GUIContent("HTTP Port");

		/// <summary>
		/// ポートリセットコンテント
		/// </summary>
		private static readonly GUIContent kPortResetContent = new GUIContent("Reset");

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
		/// 有効確認
		/// </summary>
		[System.NonSerialized]
		private bool m_Enable = false;

		/// <summary>
		/// ポート
		/// </summary>
		[System.NonSerialized]
		private int m_Port = HttpServer.kHttpServerPortDefault;

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
		private HttpServerConfig? m_HttpServerConfig = null;

		/// <summary>
		/// HTTPサーバーコンフィグ状態
		/// </summary>
		[SerializeField]
		private HttpServerConfigState m_HttpServerConfigState = HttpServerConfigState.Empty;

		#endregion
		#region Private methods

		/// <summary>
		/// 基本描画
		/// </summary>
		private void OnGUIForBasic() {
			var enableValue = m_Enable;

			EditorGUI.BeginChangeCheck();
			enableValue = EditorGUILayout.Toggle(kEnableContent, enableValue);
			if (EditorGUI.EndChangeCheck()) {
				m_Enable = enableValue;
				enable = m_Enable;
				if (m_Enable) {
					//有効化
					AddOnWillFinishListener();
					EditorApplication.update += HttpServerUpdate;
				} else {
					//無効化
					RemoveOnWillFinishListener();
					EditorApplication.update -= HttpServerUpdate;
					url = null;
				}
			}

			if (enableValue) {
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
				using (new EditorGUILayout.HorizontalScope()) {
					var port = m_Port;
					EditorGUI.BeginChangeCheck();
					port = EditorGUILayout.IntField(kPortContent, port);
					if (EditorGUI.EndChangeCheck()) {
						m_Port = port;
						EditorPrefs.SetInt(HttpServer.kHttpServerPortEditorPrefsKey, m_Port);
					}

					if (GUILayout.Button(kPortResetContent, EditorStyles.miniButton)) {
						m_Port = HttpServer.kHttpServerPortDefault;
						EditorPrefs.DeleteKey(HttpServer.kHttpServerPortEditorPrefsKey);
					}
				}
			}
		}


		/// <summary>
		/// コンフィグ描画
		/// </summary>
		private void OnGUIForConfig() {
			var config = new HttpServerConfig();
			var isChange = false;
			if (m_HttpServerConfig.HasValue && (m_HttpServerConfigState != HttpServerConfigState.Invalid)) {
				config = m_HttpServerConfig.Value;
			}

			var GuiEnabledOld = GUI.enabled;
			switch (m_HttpServerConfigState) {
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
				m_HttpServerConfig = config;
				if (m_HttpServerConfigState == HttpServerConfigState.Connecting) {
					m_HttpServerConfigState = HttpServerConfigState.ConnectingAndChange;
				} else {
					m_HttpServerConfigState = HttpServerConfigState.Change;
				}
			}
			GUI.enabled = GuiEnabledOld;
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
		private void HttpServerUpdate() {
			System.Action<AsyncOperation> readConfig = (x)=>{
				var u = (UnityWebRequestAsyncOperation)x;
				HttpServerConfigState state;
				if (!u.webRequest.isNetworkError && !u.webRequest.isHttpError) {
					var text = u.webRequest.downloadHandler.text;
					m_HttpServerConfig = JsonUtility.FromJson<HttpServerConfig>(text);
					state = HttpServerConfigState.Valid;
				} else {
					m_HttpServerConfig = null;
					state = HttpServerConfigState.Invalid;
				}
				if (m_HttpServerConfigState == HttpServerConfigState.ConnectingAndChange) {
					m_HttpServerConfigState = HttpServerConfigState.Change;
				} else {
					m_HttpServerConfigState = state;
				}
				m_DisplayDirty = true;
			};

			if (m_HttpServerConfig == null) {
				//コンフィグが無いなら
				switch (m_HttpServerConfigState) {
				case HttpServerConfigState.Empty:
				case HttpServerConfigState.Valid:
					//コンフィグがなく、空なら
					m_HttpServerConfigState = HttpServerConfigState.Connecting;

					var port = EditorPrefs.GetInt(HttpServer.kHttpServerPortEditorPrefsKey, HttpServer.kHttpServerPortDefault);
					var url = GetUrl(port);
					var unityWebRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
					var request = unityWebRequest.SendWebRequest();
					request.completed += readConfig;
					break;
				default:
					//empty.
					break;
				}
			} else if (m_HttpServerConfigState == HttpServerConfigState.Change) {
				//コンフィグが変更されているなら
				m_HttpServerConfigState = HttpServerConfigState.Connecting;

				var urlSb = new StringBuilder();
				var port = EditorPrefs.GetInt(HttpServer.kHttpServerPortEditorPrefsKey, HttpServer.kHttpServerPortDefault);
				urlSb.Append(GetUrl(port));
				urlSb.Append('?');
				urlSb.Append("MaxBandwidthBps=");
				urlSb.Append(m_HttpServerConfig.Value.MaxBandwidthBps);
				var url = urlSb.ToString();
				var unityWebRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
				var request = unityWebRequest.SendWebRequest();
				request.completed += readConfig;
			}
		}

		/// <summary>
		/// HTTPサーバー終了時イベント登録
		/// </summary>
		private void AddOnWillFinishListener() {
			var httpServers = Resources.FindObjectsOfTypeAll<HttpServer>();
			if (httpServers != null) {
				foreach (var httpServer in httpServers) {
					httpServer.onWillFinish.RemoveListener(OnWillFinishHttpServer); //既に登録済みのHttpServerにも呼ぶ事がある為、一旦外してみる
					httpServer.onWillFinish.AddListener(OnWillFinishHttpServer);
				}
			}
		}

		/// <summary>
		/// HTTPサーバー終了時イベント解除
		/// </summary>
		private void RemoveOnWillFinishListener() {
			var httpServers = Resources.FindObjectsOfTypeAll<HttpServer>();
			if (httpServers != null) {
				foreach (var httpServer in httpServers) {
					httpServer.onWillFinish.RemoveListener(OnWillFinishHttpServer);
				}
			}
		}

		/// <summary>
		/// HTTPサーバー終了時イベント
		/// </summary>
		private void OnWillFinishHttpServer() {
			m_Enable = false;
		}

		#endregion
	}
}
