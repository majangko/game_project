using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [Header("Player Gold")]
    [SerializeField] private int currentGold = 0;
    private const string GOLD_KEY = "PlayerGold"; // 🔑 PlayerPrefs 저장 키

    public int CurrentGold => currentGold;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGold(); // ✅ 시작 시 자동 로드
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------------------------------------------
    // 🪙 골드 증가 / 감소 / 초기화
    // ---------------------------------------------------------------

    /// <summary>골드 추가 (저장 및 UI 갱신 포함)</summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
        SaveGold();
        Debug.Log($"[GoldManager] +{amount} Gold → 총 {currentGold}");

        var goldUI = FindObjectOfType<GoldUI>();
        if (goldUI != null)
        {
            goldUI.Refresh();
            goldUI.ShowGainEffect(amount);
        }
    }

    /// <summary>골드 사용 (0 이하로는 떨어지지 않음)</summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        if (currentGold < amount)
        {
            Debug.LogWarning("[GoldManager] 골드가 부족합니다 ❌");
            return false;
        }

        currentGold -= amount;
        SaveGold();
        Debug.Log($"[GoldManager] -{amount} Gold → 총 {currentGold}");
        UpdateUI();
        return true;
    }

    /// <summary>게임 리셋 시 골드는 유지하지만 수동으로 초기화 가능</summary>
    public void ResetGold(bool force = false)
    {
        if (!force)
        {
            Debug.Log("[GoldManager] 로그라이크 모드이므로 골드 유지됨 ✅");
            return;
        }

        currentGold = 0;
        SaveGold();
        Debug.Log("[GoldManager] 골드 완전 초기화 완료 ❌");
        UpdateUI();
    }

    // ---------------------------------------------------------------
    // 💾 저장 / 로드
    // ---------------------------------------------------------------

    private void SaveGold()
    {
        PlayerPrefs.SetInt(GOLD_KEY, currentGold);
        PlayerPrefs.Save();
        Debug.Log($"[GoldManager] 골드 {currentGold} 저장 완료 ✅");
    }

    private void LoadGold()
    {
        currentGold = PlayerPrefs.GetInt(GOLD_KEY, 0);
        Debug.Log($"[GoldManager] 저장된 골드 로드됨 → {currentGold}");
        UpdateUI();
    }

    // ---------------------------------------------------------------
    // 🎨 UI 업데이트
    // ---------------------------------------------------------------

    private void UpdateUI()
    {
        var goldUI = FindObjectOfType<GoldUI>();
        if (goldUI != null)
            goldUI.Refresh();
    }
}
