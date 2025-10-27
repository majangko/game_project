using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("UI References")]
    public GameObject gameOverPanel; // 게임오버 UI 패널
    public Button retryButton;
    public Button quitButton;

    private bool _isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬 로드 감시 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameFlowManager] Scene Loaded: {scene.name}");
        _isGameOver = false;

        // 플레이어 데이터 불러오기
        PlayerData.LoadPlayerData();

        // HUD 및 카메라 복원
        ReconnectCamera();
        InitializeHUD();

        // 새 씬마다 GameOver UI 재연결
        FindGameOverUI();
    }

    private void FindGameOverUI()
    {
        // 씬 전환 시 UI 오브젝트 새로 연결
        gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel != null)
        {
            retryButton = gameOverPanel.transform.Find("RetryButton")?.GetComponent<Button>();
            quitButton = gameOverPanel.transform.Find("QuitButton")?.GetComponent<Button>();

            if (retryButton != null)
                retryButton.onClick.AddListener(Retry);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitToMenu);

            gameOverPanel.SetActive(false);
            Debug.Log("[GameFlowManager] GameOver UI 연결 완료 ✅");
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] GameOverPanel을 찾지 못했습니다 (씬에 추가되어야 함).");
        }
    }

    // ============================================================
    // 🔴 게임오버 처리
    // ============================================================
    public void OnGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        Debug.Log("<color=red>[GameFlowManager] 게임오버 발생!</color>");

        // 시간 멈춤
        Time.timeScale = 0f;

        // ✅ 게임오버 씬으로 전환
        if (SceneLoader.Instance != null)
        {
            Time.timeScale = 1f; // 씬 전환 전에 다시 시간 복구 (중요!)
            SceneLoader.Instance.LoadGameOver();
        }
        else
        {
            // SceneLoader가 없을 경우 대비용 (Fallback)
            Debug.LogWarning("[GameFlowManager] SceneLoader.Instance가 없어 직접 로드 수행");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }


    // ============================================================
    // 🔁 Retry / Quit 버튼 로직
    // ============================================================
    public void Retry()
    {
        Debug.Log("[GameFlowManager] Retry 버튼 클릭됨");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Debug.Log("[GameFlowManager] Quit 버튼 클릭됨");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // 메인 메뉴 씬 이름에 맞게 변경
    }

    // ============================================================
    // 기타 기능
    // ============================================================
    private void ReconnectCamera()
    {
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (cam && player)
        {
            var follow = cam.GetComponent<CameraFollow>();
            if (follow != null)
            {
                follow.target = player.transform;
                Debug.Log("[GameFlowManager] Camera reconnected to player.");
            }
        }
    }

    private void InitializeHUD()
    {
        GameObject hudRoot = GameObject.Find("HUDRoot");
        if (hudRoot != null)
        {
            Debug.Log("[GameFlowManager] HUD initialized.");
        }
    }
}
