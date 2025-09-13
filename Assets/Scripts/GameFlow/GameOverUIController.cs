using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

public class GameOverUIController : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text haveLabel;   // "보유 골드 :"
    public TMP_Text haveValue;   // 00000
    public TMP_Text costLabel;   // "소모 골드 :"
    public TMP_Text costValue;   // -000
    public Image  coinHave;
    public Image  coinCost;
    public Button continueButton;
    public Button quitButton;

    [Header("Colors")]
    public Color valueNormal = new Color32(0x33,0x33,0x33,0xFF);
    public Color valueBad    = new Color32(0xD9,0x3C,0x2E,0xFF); // 부족 시 빨강

    void Start()
    {
        Refresh();

        if (continueButton) continueButton.onClick.AddListener(OnContinue);
        if (quitButton)     quitButton.onClick.AddListener(OnQuit);
    }

    void Refresh()
    {
        var gm = GameManager.Instance;
        int have = gm.gold;
        int cost = gm.GetReviveCost();

        if (haveValue) haveValue.text = have.ToString("N0");
        if (costValue) costValue.text = "-" + cost.ToString("N0");

        bool afford = have >= cost;
        if (costValue)  costValue.color  = afford ? valueNormal : valueBad;
        if (continueButton) continueButton.interactable = afford;
    }

    void OnContinue()
    {
        var gm = GameManager.Instance;
        int cost = gm.GetReviveCost();
        if (!gm.TrySpendGold(cost)) { Refresh(); return; }

        // 같은 스테이지로 복귀 (정책: 중간 저장은 없지만 '부활'로 현재 스테이지 재도전)
        string scene = $"Stage0{Mathf.Clamp(gm.currentStage, 1, 5)}";
        if (gm.currentStage == 5) scene = "Stage05"; // 필요 시 Boss/LastBoss 분기 로직 넣기

        var fade = FadeTransition.Instance;
        if (fade != null) fade.WipeToScene(scene, 0.35f, 0.1f, 0.35f);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    void OnQuit()
    {
        // 결과 화면으로 이동 → 이후 StartIsland로 복귀(팀 정책)
        var fade = FadeTransition.Instance;
        if (fade != null) fade.FadeToScene("GameOverResult");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverResult");
    }
}
