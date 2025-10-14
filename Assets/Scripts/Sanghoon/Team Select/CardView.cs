// FILE: CardView.cs
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("앞면")]
    public Image portrait;          // 캐릭터 초상화
    public TMP_Text nameText;       // 캐릭터 이름
    public Image frame;             // 카드 테두리

    [Header("버튼")]
    public Button selectButton;
    public Button rerollButton;
    public Button infoButton;       // 앞면 i 버튼
    public Button infoButtonBack;   // 뒷면 i 버튼

    [Header("뒷면(선택 사항)")]
    public GameObject backRoot;
    public TMP_Text descText;
    public Image skillIconA;
    public Image skillIconB;

    [Header("컴포넌트")]
    public FlipCard flip;

    // 데이터 바인딩 대상
    [HideInInspector] public TeamMemberSO bound;

    // 외부 이벤트
    public Action<CardView> OnRerollClicked;
    public Action<CardView> OnSelectClicked;
    public Action<CardView> OnInfoClicked;

    void Awake()
    {
        // 버튼 연결
        if (rerollButton)
            rerollButton.onClick.AddListener(() => OnRerollClicked?.Invoke(this));

        if (selectButton)
            selectButton.onClick.AddListener(() => OnSelectClicked?.Invoke(this));

        if (infoButton)
            infoButton.onClick.AddListener(() =>
            {
                OnInfoClicked?.Invoke(this);
                if (flip != null) flip.Toggle();
            });

        if (infoButtonBack)
            infoButtonBack.onClick.AddListener(() =>
            {
                OnInfoClicked?.Invoke(this);
                if (flip != null) flip.Toggle();
            });

        // 기본은 앞면으로 시작
        if (flip != null)
            flip.ShowFront();
    }

    // ----------------------------------------------------------
    // 캐릭터 데이터 바인딩
    // ----------------------------------------------------------
    public void Bind(TeamMemberSO m)
    {
        bound = m;
        if (m == null)
        {
            Debug.LogWarning("[CardView] Bind called with null member");
            return;
        }

        StartCoroutine(ApplyLate(m)); // UI 초기화 후 한 프레임 뒤에 적용
    }

    private IEnumerator ApplyLate(TeamMemberSO m)
    {
        // 한 프레임 대기 (TMP 초기화 대기)
        yield return null;

        Debug.Log($"[CardView] Binding {m.id}, name={m.displayName}");

        // ------------------------------
        // 이름 표시
        // ------------------------------
        if (nameText)
        {
            nameText.text = !string.IsNullOrEmpty(m.displayName)
                ? m.displayName
                : "(이름 없음)";

            nameText.color = Color.black; // 배경 대비용
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontSize = 36;
            nameText.ForceMeshUpdate(true); // ✅ 즉시 메쉬 갱신
        }

        // ------------------------------
        // 초상화 이미지
        // ------------------------------
        if (portrait)
        {
            portrait.sprite = m.portrait;
            portrait.preserveAspect = true;
            portrait.color = m.portrait ? Color.white : new Color(1, 1, 1, 0.2f);
        }

        // ------------------------------
        // 희귀도 프레임 색상
        // ------------------------------
        if (frame)
        {
            switch (m.rarity)
            {
                case Rarity.Common:
                    frame.color = new Color(0.8f, 0.8f, 0.8f);
                    break;
                case Rarity.Epic:
                    frame.color = new Color(0.6f, 0.3f, 0.9f);
                    break;
                case Rarity.Legendary:
                    frame.color = new Color(0.95f, 0.8f, 0.2f);
                    break;
                default:
                    frame.color = Color.white;
                    break;
            }
        }

        // ------------------------------
        // 설명 텍스트
        // ------------------------------
        if (descText)
        {
            descText.text = !string.IsNullOrEmpty(m.description)
                ? m.description
                : "설명이 없습니다.";
        }

        // ------------------------------
        // 스킬 아이콘
        // ------------------------------
        if (skillIconA)
            skillIconA.sprite = m.skillIconA;

        if (skillIconB)
            skillIconB.sprite = m.skillIconB;

        // ------------------------------
        // 앞면으로 강제 노출
        // ------------------------------
        if (flip != null)
            flip.ShowFront();
    }
}
