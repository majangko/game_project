using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [Header("UI 요소")]
    public GameObject window;   // 창 루트
    public TMP_Text text;       // 메시지
    public Button yesButton;
    public Button noButton;

    private Action _onYes;
    private Action _onNo;

    void Awake()
    {
        // window 미연결 시 자기 자신으로 대체
        if (window == null)
            window = gameObject;

        if (window.activeSelf)
            window.SetActive(false);

        if (yesButton)
            yesButton.onClick.AddListener(() =>
            {
                Debug.Log("<color=green>[ConfirmPanel] YES 버튼 클릭</color>");
                _onYes?.Invoke();
                Hide();
            });

        if (noButton)
            noButton.onClick.AddListener(() =>
            {
                Debug.Log("<color=yellow>[ConfirmPanel] NO 버튼 클릭</color>");
                _onNo?.Invoke();
                Hide();
            });
    }

    public void Show(string message, Action onYes = null, Action onNo = null)
    {
        _onYes = onYes;
        _onNo = onNo;

        if (window != null)
        {
            window.SetActive(true);   // ✅ 패널 활성화 보장
            Debug.Log("[ConfirmPanel] Window 활성화됨");
        }

        if (text) text.text = message;
    }
    public void Hide()
    {
        Debug.Log("<color=gray>[ConfirmPanel] Hide()</color>");
        if (window)
            window.SetActive(false);
        _onYes = _onNo = null;
    }
}
