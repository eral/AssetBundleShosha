// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using AssetBundleShosha.Internal;

	public class AssetBundleManagerViewer : EditorWindow, IHasCustomMenu {
		#region Public types
		#endregion
		#region Public const fields
		#endregion
		#region Public fields and properties

		public AssetBundleManager target {get{
			return m_Target;
		} set{
			if ((m_Target != value) || ((m_Target != null) ^ (value != null))) {
				m_Target = value;
				ClearCache();
				RebuildCache();
			}
		}}

		#endregion
		#region Public methods

		/// <summary>
		/// 追加メニュー
		/// </summary>
		/// <param name="menu">メニューコントローラ</param>
		public void AddItemsToMenu(GenericMenu menu) {
			if (m_Target != null) {
				var managers = FindObjectsOfType<AssetBundleManager>();
				if (2 <= managers.Length) {
					System.Array.Sort(managers, (x,y)=>{
						var r = string.Compare(x.name, y.name, false, System.Globalization.CultureInfo.CurrentUICulture);
						return ((r != 0)? r:  x.GetInstanceID() - y.GetInstanceID());
					});

					var overlapping = new Dictionary<string, int>(managers.Length);
					foreach (var manager in managers) {
						int overlapCount;
						overlapping.TryGetValue(manager.name, out overlapCount);
						overlapping[manager.name] = ++overlapCount;
					}
					foreach (var manager in managers) {
						GUIContent content;
						if (1 == overlapping[manager.name]) {
							content = new GUIContent(manager.name);
						} else {
							content = new GUIContent(manager.name + " (" + manager.GetInstanceID() + ')');
						}
						menu.AddItem(content, manager == m_Target, ()=>{
							target = manager;
						});
					}
				}
			}
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// 構築
		/// </summary>
		protected virtual void OnEnable() {
			titleContent = new GUIContent("Shosha Viewer");

			target = AssetBundleManager.Instance;
		}

		/// <summary>
		/// 破棄
		/// </summary>
		protected virtual void OnDisable() {
		}

		/// <summary>
		/// 更新
		/// </summary>
		protected virtual void Update() {
			if (EditorApplication.isPlaying) {
				Repaint();
			}
			if (target != AssetBundleManager.Instance || ((target != null) ^ (AssetBundleManager.Instance != null))) {
				//MEMO: (target != null) ^ (AssetBundleManager.Instance != null)
				//      ビューアーを表示したままプレイを開始すると、
				//      targetはエディト時に生成されたインスタンス
				//      AssetBundleManager.Instanceはプレイ時に生成されたインスタンスとなる
				//      この時インスタンスIDは同じになるがtarget側は寿命が切れている為nullとなるが
				//      AssetBundleManager.Instanceは寿命が切れていないのでnullにはならない
				//      その時target != AssetBundleManager.Instanceはtrueを期待するが、
				//      インスタンスIDが同じの為falseを返す事になる
				//      その挙動を是正する為の比較がXORを用いた上記構文となる
				target = AssetBundleManager.Instance;
				Repaint();
			}
		}

		/// <summary>
		/// 描画
		/// </summary>
		protected virtual void OnGUI() {
			if (m_Target == null) {
				EditorGUILayout.HelpBox("The manager object is not found", MessageType.Warning);
				return;
			}
		
			RebuildCache();

			OnGUIForToolbar();
			switch (m_ToolsIndex) {
			case ToolsIndex.Manager: OnGUIForBasic(); break;
			case ToolsIndex.Catalog: OnGUIForCatalog(); break;
			case ToolsIndex.Editor: OnGUIForEditor(); break;
			case ToolsIndex.Work: OnGUIForWork(); break;
			}
		}

		#endregion
		#region Private types

		/// <summary>
		/// ツールインデックス
		/// </summary>
		private enum ToolsIndex {
			Manager,
			Catalog,
			Editor,
			Work,
		}

		#endregion
		#region Private const fields
		#endregion
		#region Private fields and properties

		/// <summary>
		/// ツールインデックス
		/// </summary>
		[SerializeField]
		private ToolsIndex m_ToolsIndex = ToolsIndex.Manager;

		/// <summary>
		/// ターゲット
		/// </summary>
		[SerializeField]
		private AssetBundleManager m_Target;

		/// <summary>
		/// ターゲットシリアライズオブジェクト
		/// </summary>
		[System.NonSerialized]
		private SerializedObject m_TargetSerialized;

		/// <summary>
		/// 進捗コンテント
		/// </summary>
		[SerializeField]
		private GUIContent m_ProgressContent = new GUIContent("Progress");

		/// <summary>
		/// カタログシリアライズオブジェクト
		/// </summary>
		[System.NonSerialized]
		private SerializedObject m_CatalogSerialized;

		/// <summary>
		/// カタログコンテンツハッシュコンテント
		/// </summary>
		[SerializeField]
		private GUIContent m_CatalogContentHashContent = new GUIContent("ContentHash");

		/// <summary>
		/// エディタシリアライズオブジェクト
		/// </summary>
		[System.NonSerialized]
		private SerializedObject m_EditorSerialized;

		/// <summary>
		/// ダウンロードキュー
		/// </summary>
		[System.NonSerialized]
		private Queue<AssetBundleBase> m_DownloadQueue;

		/// <summary>
		/// ダウンロードキューコンテント
		/// </summary>
		[SerializeField]
		private GUIContent m_DownloadQueueContent = new GUIContent();

		/// <summary>
		/// ダウンロードキュー折り込み
		/// </summary>
		[SerializeField]
		private bool m_DownloadQueueFoldout;

		/// <summary>
		/// ダウンロード中
		/// </summary>
		[System.NonSerialized]
		private Queue<AssetBundleBase> m_Downloading;

		/// <summary>
		/// ダウンロード中コンテント
		/// </summary>
		[SerializeField]
		private GUIContent m_DownloadingContent = new GUIContent();

		/// <summary>
		/// ダウンロード中折り込み
		/// </summary>
		[SerializeField]
		private bool m_DownloadingFoldout;

		/// <summary>
		/// ダウンロード済み
		/// </summary>
		[System.NonSerialized]
		private Dictionary<string, AssetBundleBase> m_Downloaded;

		/// <summary>
		/// ダウンロード済みコンテント
		/// </summary>
		[SerializeField]
		private GUIContent m_DownloadedContent = new GUIContent();

		/// <summary>
		/// ダウンロード済み折り込み
		/// </summary>
		[SerializeField]
		private bool m_DownloadedFoldout;

		/// <summary>
		/// 進捗中
		/// </summary>
		[System.NonSerialized]
		private Dictionary<string, AssetBundleBase> m_Progressing;

		/// <summary>
		/// 進捗中コンテント
		/// </summary>
		private GUIContent m_ProgressingContent = new GUIContent();

		/// <summary>
		/// システム読み込み済みコンテント
		/// </summary>
		private GUIContent m_SystemLoadedContent = new GUIContent();

		/// <summary>
		/// 進捗中折り込み
		/// </summary>
		[SerializeField]
		private bool m_ProgressingFoldout;

		/// <summary>
		/// システム読み込み済み折り込み
		/// </summary>
		[SerializeField]
		private bool m_SystemLoadedFoldout;

		#endregion
		#region Private methods

		/// <summary>
		/// キャッシュ削除
		/// </summary>
		private void ClearCache() {
			m_TargetSerialized = null;
			m_CatalogSerialized = null;
			m_EditorSerialized = null;
			m_DownloadQueue = null;
			m_Downloading = null;
			m_Downloaded = null;
			m_Progressing = null;
		}

		/// <summary>
		/// キャッシュ再構築
		/// </summary>
		private void RebuildCache() {
			if (m_Target != null) {
				if (m_TargetSerialized == null) {
					m_TargetSerialized = new SerializedObject(m_Target);
				}

				if ((m_CatalogSerialized == null) || (m_CatalogSerialized.targetObject == null)) {
					if (m_Target.catalog != null) {
						m_CatalogSerialized = new SerializedObject(m_Target.catalog);
					}
				} else if (m_CatalogSerialized.targetObject != m_Target.catalog) {
					if (m_Target.catalog != null) {
						m_CatalogSerialized = new SerializedObject(m_Target.catalog);
					} else {
						m_CatalogSerialized = null;
					}
				}

				var editor = AssetBundleUtility.GetAssetBundleManagerEditor(m_Target);
				if ((m_EditorSerialized == null) || (m_EditorSerialized.targetObject == null)) {
					if (editor != null) {
						m_EditorSerialized = new SerializedObject(editor);
					}
				} else if (m_EditorSerialized.targetObject != editor) {
					m_EditorSerialized = new SerializedObject(editor);
				}

				if (m_DownloadQueue == null) {
					m_DownloadQueue = m_Target.GetDownloadQueue();
				}
				if (m_Downloading == null) {
					m_Downloading = m_Target.GetDownloading();
				}
				if (m_Downloaded == null) {
					m_Downloaded = m_Target.GetDownloaded();
				}
				if (m_Progressing == null) {
					m_Progressing = m_Target.GetProgressing();
				}
			}
		}

		/// <summary>
		/// ツールバー描画
		/// </summary>
		private void OnGUIForToolbar() {
			EditorGUI.BeginChangeCheck();
			var toolsIndex = (int)m_ToolsIndex;
			toolsIndex = GUILayout.Toolbar(toolsIndex, System.Enum.GetNames(typeof(ToolsIndex)));
			if (EditorGUI.EndChangeCheck()) {
				m_ToolsIndex = (ToolsIndex)toolsIndex;
			}
		}

		/// <summary>
		/// 基本描画
		/// </summary>
		private void OnGUIForBasic() {
			m_TargetSerialized.Update();
			EditorGUILayoutSerializedObjectField(m_TargetSerialized, true);
			m_TargetSerialized.ApplyModifiedProperties();
			{
				var contentHash = 0L;
				if (Event.current.type == EventType.Repaint) {
					contentHash = ((AssetBundleManager)m_TargetSerialized.targetObject).contentHash;
				}
				EditorGUILayout.LongField(m_CatalogContentHashContent, contentHash);
			}
			{
				var progress = m_Target.progress;
				var position = GUILayoutUtility.GetRect(m_ProgressContent, GUI.skin.GetStyle("label"));
				position = EditorGUI.PrefixLabel(position, m_ProgressContent);
				EditorGUI.ProgressBar(position, progress, progress.ToString("0.00"));
			}
		}

		/// <summary>
		/// カタログ描画
		/// </summary>
		private void OnGUIForCatalog() {
			if ((m_CatalogSerialized == null) || (m_CatalogSerialized.targetObject == null)) {
				EditorGUILayout.HelpBox("catalog is null", MessageType.Info);
				return;
			}

			{
				var contentHash = 0L;
				if (Event.current.type == EventType.Repaint) {
					contentHash = ((AssetBundleCatalog)m_CatalogSerialized.targetObject).GetContentHash();
				}
				EditorGUILayout.LongField(m_CatalogContentHashContent, contentHash);
			}

			m_CatalogSerialized.Update();
			EditorGUILayoutSerializedObjectField(m_CatalogSerialized, true);
			m_CatalogSerialized.ApplyModifiedProperties();
		}

		/// <summary>
		/// エディタ描画
		/// </summary>
		private void OnGUIForEditor() {
			if ((m_EditorSerialized == null) || (m_EditorSerialized.targetObject == null)) {
				EditorGUILayout.HelpBox("The dditor object is not found", MessageType.Info);
				return;
			}
		
			m_EditorSerialized.Update();
			EditorGUILayoutSerializedObjectField(m_EditorSerialized, true);
			m_EditorSerialized.ApplyModifiedProperties();
		}

		/// <summary>
		/// ワーク描画
		/// </summary>
		private void OnGUIForWork() {
			var allLoadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles();
			if (Event.current.type == EventType.Repaint) {
				m_DownloadQueueContent.text = "DownloadQueue (" + m_DownloadQueue.Count.ToString() + ")";
				m_DownloadingContent.text = "Downloading (" + m_Downloading.Count.ToString() + ")";
				m_DownloadedContent.text = "Downloaded (" + m_Downloaded.Count.ToString() + ")";
				m_ProgressingContent.text = "Progressing (" + m_Progressing.Count.ToString() + ")";
				m_SystemLoadedContent.text = "SystemLoaded (" + allLoadedAssetBundles.Count().ToString() + ")";
			}
			m_DownloadQueueFoldout = EditorGUILayout.Foldout(m_DownloadQueueFoldout, m_DownloadQueueContent);
			if (m_DownloadQueueFoldout) {
				++EditorGUI.indentLevel;
				OnGUIForQueueAssetBundleBase(m_DownloadQueue);
				--EditorGUI.indentLevel;
			}
			m_DownloadingFoldout = EditorGUILayout.Foldout(m_DownloadingFoldout, m_DownloadingContent);
			if (m_DownloadingFoldout) {
				++EditorGUI.indentLevel;
				OnGUIForQueueAssetBundleBase(m_Downloading);
				--EditorGUI.indentLevel;
			}
			m_DownloadedFoldout = EditorGUILayout.Foldout(m_DownloadedFoldout, m_DownloadedContent);
			if (m_DownloadedFoldout) {
				++EditorGUI.indentLevel;
				OnGUIForDictionaryAssetBundleBase(m_Downloaded);
				--EditorGUI.indentLevel;
			}
			m_ProgressingFoldout = EditorGUILayout.Foldout(m_ProgressingFoldout, m_ProgressingContent);
			if (m_ProgressingFoldout) {
				++EditorGUI.indentLevel;
				OnGUIForDictionaryAssetBundleBase(m_Progressing);
				--EditorGUI.indentLevel;
			}
			m_SystemLoadedFoldout = EditorGUILayout.Foldout(m_SystemLoadedFoldout, m_SystemLoadedContent);
			if (m_SystemLoadedFoldout) {
				++EditorGUI.indentLevel;
				foreach (var assetBundle in allLoadedAssetBundles) {
					EditorGUILayout.LabelField(assetBundle.name);
				}
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// アセットバンドルキュー描画
		/// </summary>
		private static void OnGUIForQueueAssetBundleBase(Queue<AssetBundleBase> queue) {
			foreach (var element in queue) {
				var label = element.nameWithVariant
							+ " ("
							+ element.GetReferenceCount().ToString()
							+ ")";
				EditorGUILayout.LabelField(label);
			}
		}

		/// <summary>
		/// アセットバンドル辞書描画
		/// </summary>
		private static void OnGUIForDictionaryAssetBundleBase(Dictionary<string, AssetBundleBase> dictionary) {
			foreach (var key in dictionary.Keys.OrderBy(x=>x)) {
				var element = dictionary[key];
				var label = element.nameWithVariant
							+ " ("
							+ element.GetReferenceCount().ToString()
							+ ")";
				EditorGUILayout.LabelField(label);
			}
		}

		/// <summary>
		/// シリアライズオブジェクトフィールド
		/// </summary>
		/// <param name="serializedObject">シリアライズオブジェクト</param>
		/// <param name="selfInvisible">self欄の非表示</param>
		private static void EditorGUILayoutSerializedObjectField(SerializedObject serializedObject, bool selfInvisible = false) {
			var iterator = serializedObject.GetIterator();
			if (selfInvisible) {
				iterator.NextVisible(true);
			} else {
				iterator.Next(true);
			}
			while (iterator.NextVisible(false)) {
				EditorGUILayout.PropertyField(iterator, true);
			}
		}

		#endregion
	}
}
