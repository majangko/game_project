using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬 전환 감시 시작
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

        // 플레이어 데이터 불러오기
        PlayerData.LoadPlayerData();

        // 카메라 자동 재연결 (선택 사항)
        ReconnectCamera();

        // HUD 초기화 (선택 사항)
        InitializeHUD();
    }

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
            // 필요하다면 HUD 초기화 함수 호출 가능
        }
    }
}
