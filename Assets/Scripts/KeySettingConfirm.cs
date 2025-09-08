using UnityEngine;
using UnityEngine.UI;

public class KeySettingConfirm : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject keySettingPanel;   // 전체 설정 패널 (닫을 때 숨김)
    [SerializeField] private GameObject confirmPanel;      // 확인창 패널 (모달)

    [Header("Buttons")]
    [SerializeField] private Button saveButton;            // 상단 Save 버튼
    [SerializeField] private Button yesButton;             // 확인창 Yes
    [SerializeField] private Button noButton;              // 확인창 No

    private void Awake()
    {
        // 시작 시 확인창은 숨김
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // 버튼 이벤트 연결
        if (saveButton != null) saveButton.onClick.AddListener(OpenConfirm);
        if (yesButton != null) yesButton.onClick.AddListener(ConfirmYes);
        if (noButton != null) noButton.onClick.AddListener(ConfirmNo);
    }

    // Save 클릭 → 확인창 열기
    private void OpenConfirm()
    {
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    // 예(Yes) → 저장 로직 실행 후 확인창 닫고 설정창도 닫기
    private void ConfirmYes()
    {
        // TODO: 실제 키바인딩 저장 로직을 여기서 호출
        // 예시) PlayerPrefs.Save();  또는 KeyBindingManager.Instance.Save();
        Debug.Log("[KeySetting] 저장 완료");

        if (confirmPanel != null) confirmPanel.SetActive(false);

        // 설정 UI까지 닫고 게임 화면으로 복귀
        if (keySettingPanel != null) keySettingPanel.SetActive(false);
        // 필요하면 다른 Canvas/게임 HUD를 SetActive(true) 해도 됨
    }

    // 아니오(No) → 확인창만 닫기
    private void ConfirmNo()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }
}
