// FILE: ConfirmPanel.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    public GameObject window;   // 패널 루트
    public TMP_Text text;       // 메시지
    public Button yesButton;
    public Button noButton;

    Action _onYes, _onNo;

    void Awake()
    {
        if (window) window.SetActive(false);
        if (yesButton) yesButton.onClick.AddListener(() => { _onYes?.Invoke(); Hide(); });
        if (noButton) noButton.onClick.AddListener(() => { _onNo?.Invoke(); Hide(); });
    }

    public void Show(string message, Action onYes = null, Action onNo = null)
    {
        _onYes = onYes;
        _onNo = onNo;
        if (text) text.text = message;
        if (window) window.SetActive(true);
    }

    public void Hide()
    {
        if (window) window.SetActive(false);
        _onYes = _onNo = null;
    }
}
