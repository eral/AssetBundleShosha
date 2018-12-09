// Created by ERAL
// This is free and unencumbered software released into the public domain.

namespace AssetBundleShoshaDemo {
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;

	public class DemoDialog : MonoBehaviour {
		#region Public fields and properties

		/// <summary>
		/// 選択
		/// </summary>
		public int selectIndex {get{return m_SelectIndex;}}

		#endregion
		#region Public methods

		/// <summary>
		/// 作成
		/// </summary>
		/// <param name="messageText">メッセージテキスト</param>
		/// <param name="buttonTexts">ボタンテキスト群</param>
		/// <param name="onSelected">選択時イベント</param>
		/// <param name="onFinished">終了時イベント</param>
		public void Create(string messageText, string[] buttonTexts, UnityAction<int> onSelected = null, UnityAction<int> onFinished = null) {
			m_OnSelected = onSelected;
			m_OnFinished = onFinished;
			m_SelectIndex = -1;
			SetMessageText(messageText);
			SetButtonTexts(buttonTexts);
			gameObject.SetActive(true);
		}

		/// <summary>
		/// ボタン押下
		/// </summary>
		/// <param name="self">ボタン自身</param>
		public void OnButton(Button self) {
			var selfGo = self.gameObject;
			m_SelectIndex = m_Buttons.FindIndex(x=>x == selfGo);
			if (m_OnSelected != null) {
				m_OnSelected(m_SelectIndex);
				m_OnSelected = null;
			}
			StartCloseAnimation();
		}

		#endregion
		#region Protected methods

		/// <summary>
		/// 有効化
		/// </summary>
		protected virtual void OnEnable() {
			StartOpenAnimation();
		}

		#endregion
		#region Private const fields

		/// <summary>
		/// アニメーター用OpenパラメータのID
		/// </summary>
		private readonly int m_AnimatorOpenId = Animator.StringToHash("Open");

		#endregion
		#region Private fields and properties

		/// <summary>
		/// 選択
		/// </summary>
		[SerializeField]
		private int m_SelectIndex;

		/// <summary>
		/// 選択時イベント
		/// </summary>
		UnityAction<int> m_OnSelected;

		/// <summary>
		/// 終了時イベント
		/// </summary>
		UnityAction<int> m_OnFinished;

		/// <summary>
		/// アニメーター
		/// </summary>
		[SerializeField]
		private Animator m_Animator;

		/// <summary>
		/// キャンバスグループ
		/// </summary>
		[SerializeField]
		private CanvasGroup m_CanvasGroup;

		/// <summary>
		/// アニメーション終了イベント発行コルーチン
		/// </summary>
		private Coroutine m_AnimationExitEventRaiser = null;

		/// <summary>
		/// メッセージ
		/// </summary>
		[SerializeField]
		private Text m_MessageText;

		/// <summary>
		/// ボタン群
		/// </summary>
		[SerializeField]
		private List<GameObject> m_Buttons;

		/// <summary>
		/// ボタンテキスト
		/// </summary>
		[SerializeField]
		private List<Text> m_ButtonTexts;

		#endregion
		#region Private methods

		/// <summary>
		/// メッセージテキスト設定
		/// </summary>
		/// <param name="text">メッセージテキスト</param>
		private void SetMessageText(string text) {
			m_MessageText.text = text;
		}

		/// <summary>
		/// ボタンテキスト設定
		/// </summary>
		/// <param name="texts">ボタンテキスト群</param>
		private void SetButtonTexts(string[] texts) {
			if (m_Buttons.Count < texts.Length) {
				//ボタンが不足しているなら
				//追加
				if (m_Buttons.Capacity < texts.Length) {
					m_Buttons.Capacity = texts.Length;
				}
				if (m_ButtonTexts.Capacity < texts.Length) {
					m_ButtonTexts.Capacity = texts.Length;
				}
				var sourceGo = m_Buttons[0];
				var parentTransform = sourceGo.transform.parent;
				while (m_Buttons.Count < texts.Length) {
					var buttonIndex = m_Buttons.Count;
					var go = Instantiate<GameObject>(sourceGo);
					m_Buttons.Add(go);
					var text = go.GetComponentInChildren<Text>();
					m_ButtonTexts.Add(text);
					go.transform.SetParent(parentTransform, false);
				}
			}
			var index = 0;
			while (index < texts.Length) {
				m_ButtonTexts[index].text = texts[index];
				m_Buttons[index].SetActive(true);
				++index;
			}
			while (index < m_Buttons.Count) {
				m_Buttons[index].SetActive(false);
				++index;
			}
		}

		/// <summary>
		/// 開くアニメーション開始
		/// </summary>
		private void StartOpenAnimation() {
			if (m_AnimationExitEventRaiser != null) {
				StopCoroutine(m_AnimationExitEventRaiser);
			}
			m_CanvasGroup.interactable = false;
			m_CanvasGroup.blocksRaycasts = true;
			m_Animator.SetBool(m_AnimatorOpenId, true);
			m_AnimationExitEventRaiser = StartCoroutine(AnimationExitEventRaiser());
		}

		/// <summary>
		/// 閉じるアニメーション開始
		/// </summary>
		private void StartCloseAnimation() {
			if (m_AnimationExitEventRaiser != null) {
				StopCoroutine(m_AnimationExitEventRaiser);
			}
			m_Animator.SetBool(m_AnimatorOpenId, false);
			m_AnimationExitEventRaiser = StartCoroutine(AnimationExitEventRaiser());
		}

		/// <summary>
		/// アニメーション終了イベント発行
		/// </summary>
		/// <returns>コルーチン</returns>
		private IEnumerator<object> AnimationExitEventRaiser() {
			while (1.0f <= m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime) {
				//開始時点で既に終了しているなら
				//新規に開始する迄待つ
				yield return null;
			}
			while (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) {
				yield return null;
			}
			m_AnimationExitEventRaiser = null;
			if (m_Animator.GetBool(m_AnimatorOpenId)) {
				OnOpened();
			} else {
				OnClosed();
			}
		}

		/// <summary>
		/// 開いた
		/// </summary>
		private void OnOpened() {
			m_CanvasGroup.interactable = true;
		}

		/// <summary>
		/// 閉じた
		/// </summary>
		private void OnClosed() {
			m_CanvasGroup.interactable = false;
			m_CanvasGroup.blocksRaycasts = false;
			if (m_OnFinished != null) {
				m_OnFinished(m_SelectIndex);
				m_OnFinished = null;
			}
			gameObject.SetActive(false);
		}

		#endregion
	}

}
