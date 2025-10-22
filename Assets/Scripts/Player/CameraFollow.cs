using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0, 1, -10);
    public float smoothSpeed = 5f; // 부드럽게 이동 속도

    void Awake()
    {
        // 씬이 로드될 때마다 자동으로 FindPlayer 실행
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // 초기에도 target이 없으면 플레이어 찾기
        if (target == null)
            FindPlayer();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀌면 자동으로 플레이어 다시 찾기
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log($"[CameraFollow] Player found: {player.name}");
        }
        else
        {
            Debug.LogWarning("[CameraFollow] Player not found in scene!");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
