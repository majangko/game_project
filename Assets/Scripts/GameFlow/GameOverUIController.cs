using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using System.Collections;

public class GameOverUIController : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text haveValue;   // 보유 골드
    public TMP_Text costValue;   // 소모 골드
    public Button continueButton;
    public Button quitButton;

    [Header("Colors")]
    public Color valueNormal = new Color32(0x33, 0x33, 0x33, 0xFF);
    public Color valueBad = new Color32(0xD9, 0x3C, 0x2E, 0xFF);

    [Header("Settings")]
    public int reviveCost = 300; // 부활 소모 골드

    private GoldManager goldMgr;
    private GameManager gameMgr;

    // ==========================================================
    // 초기화
    // ==========================================================
    void Start()
    {
        StartCoroutine(InitAfterDelay());
    }

    private IEnumerator InitAfterDelay()
    {
        yield return null; // 씬 전환 안정화 대기

        goldMgr = GoldManager.Instance;
        gameMgr = GameManager.Instance;

        if (goldMgr == null)
            Debug.LogWarning("[GameOverUI] GoldManager.Instance가 null입니다 ❌");
        if (gameMgr == null)
            Debug.LogWarning("[GameOverUI] GameManager.Instance가 null입니다 ❌");

        Refresh();

        if (continueButton) continueButton.onClick.AddListener(OnContinue);
        if (quitButton) quitButton.onClick.AddListener(OnQuit);
    }

    // ==========================================================
    // UI 갱신
    // ==========================================================
    void Refresh()
    {
        int have = goldMgr != null ? goldMgr.CurrentGold : 0;
        int cost = reviveCost;

        if (haveValue) haveValue.text = have.ToString("N0");
        if (costValue) costValue.text = "-" + cost.ToString("N0");

        bool afford = have >= cost;
        if (costValue) costValue.color = afford ? valueNormal : valueBad;
        if (continueButton) continueButton.interactable = afford;
    }

    // ==========================================================
    // 버튼 동작
    // ==========================================================
    void OnContinue()
    {
        if (goldMgr == null || gameMgr == null)
        {
            Debug.LogError("[GameOverUI] GoldManager 또는 GameManager가 null이라 부활 불가 ❌");
            return;
        }

        // 부활 비용 지불 시도
        if (!goldMgr.SpendGold(reviveCost))
        {
            Refresh();
            return;
        }

        Debug.Log($"[GameOverUI] {reviveCost} 골드 사용 → 현재 {goldMgr.CurrentGold} 골드 남음 ✅");

        // 다시 현재 스테이지로 복귀
        int stage = Mathf.Clamp(gameMgr.currentStage, 1, 5);
        string scene = $"Stage0{stage}";

        Time.timeScale = 1f;

        if (FadeTransition.Instance != null)
            FadeTransition.Instance.WipeToScene(scene, 0.35f, 0.1f, 0.35f);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    void OnQuit()
    {
        Debug.Log("[GameOverUI] 그만하기 → 시작섬 복귀");
        Time.timeScale = 1f;

        if (FadeTransition.Instance != null)
            FadeTransition.Instance.FadeToScene("StartIsland-1");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartIsland-1");
    }
}
