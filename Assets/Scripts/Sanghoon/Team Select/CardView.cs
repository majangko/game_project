// FILE: CardView.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("앞면")]
    public Image portrait;          // 캐릭터 전체 이미지
    public TMP_Text nameText;       // 이름(숫자 목업)
    public Image frame;             // 테두리(희귀도 색)

    [Header("버튼")]
    public Button selectButton;
    public Button rerollButton;
    public Button infoButton;       // 앞면 i 버튼
    public Button infoButtonBack;   // 뒷면 i 버튼(NEW)

    [Header("뒷면(선택 사항)")]
    public GameObject backRoot;     // 뒷면 루트
    public TMP_Text descText;
    public Image skillIconA;
    public Image skillIconB;

    [Header("컴포넌트")]
    public FlipCard flip;

    // 바인딩 데이터
    [HideInInspector] public TeamMemberSO bound;

    // 외부에 알리는 이벤트
    public Action<CardView> OnRerollClicked;
    public Action<CardView> OnSelectClicked;
    public Action<CardView> OnInfoClicked;

    void Awake()
    {
        if (rerollButton) rerollButton.onClick.AddListener(() => OnRerollClicked?.Invoke(this));
        if (selectButton) selectButton.onClick.AddListener(() => OnSelectClicked?.Invoke(this));

        if (infoButton) infoButton.onClick.AddListener(() =>
        {
            OnInfoClicked?.Invoke(this);
            if (flip != null) flip.Toggle();
        });

        if (infoButtonBack) infoButtonBack.onClick.AddListener(() =>
        {
            OnInfoClicked?.Invoke(this);
            if (flip != null) flip.Toggle();
        });

        // 시작 기본은 앞면
        if (flip != null) flip.ShowFront();
    }

    public void Bind(TeamMemberSO m)
    {
        bound = m;
        if (!m) return;

        if (nameText) nameText.text = m.displayName;
        if (portrait) portrait.sprite = m.portrait;

        // 희귀도 색상
        if (frame)
        {
            var c = Color.white;
            switch (m.rarity)
            {
                case Rarity.Common: c = Color.black; break;
                case Rarity.Epic: c = new Color(0.6f, 0.3f, 0.9f); break;
                case Rarity.Legendary: c = new Color(0.95f, 0.8f, 0.2f); break;
            }
            frame.color = c;
        }

        // 뒷면
        if (descText) descText.text = m.description;
        if (skillIconA) skillIconA.sprite = m.skillIconA;
        if (skillIconB) skillIconB.sprite = m.skillIconB;

        if (flip != null) flip.ShowFront();
    }
}
