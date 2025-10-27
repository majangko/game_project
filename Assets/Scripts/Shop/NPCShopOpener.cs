// Assets/Scripts/Shop/NPCShopOpener_Debug.cs
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class NPCShopOpener_Debug : MonoBehaviour
{
    public string shopSceneName = "Shop";
    public PromptUIController prompt;                 // 드래그 연결
    [TextArea] public string promptText = "E키를 누르세요.";

    public Transform player;                          // Player Transform (없으면 태그로 찾음)
    public float requiredDistance = 1.8f;             // 거리 체크(트리거가 안먹을 때도 대안)

    bool _playerIn;
    string _last;                                     // 상태 로그 누적

    void Start()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (prompt != null) prompt.Hide();

        Log($"Start | scene='{shopSceneName}', prompt={(prompt ? "OK" : "NULL")}, player={(player ? player.name : "NULL")}");
    }

    void Update()
    {
        // 거리 기반 보조 체크(트리거 미작동시 대비)
        if (player != null)
        {
            float d = Vector2.Distance(player.position, transform.position);
            bool near = d <= requiredDistance;

            if (near && !_playerIn)
            {
                _playerIn = true;
                if (prompt != null) prompt.Show(promptText);
                Log($"[DIST] Enter (d={d:0.00})");
            }
            else if (!near && _playerIn)
            {
                _playerIn = false;
                if (prompt != null) prompt.Hide();
                Log($"[DIST] Exit (d={d:0.00})");
            }
        }

        if (_playerIn && Input.GetKeyDown(KeyCode.E))
        {
            Log("E pressed → Load Shop");
            var op = SceneManager.LoadSceneAsync("shop", LoadSceneMode.Additive);
            if (op == null) Debug.LogError("[NPCShopOpener_Debug] LoadSceneAsync returned null. Build Settings or scene name wrong.");
            if (prompt != null) prompt.Hide();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Log($"OnTriggerEnter2D: from={other.name}, tag={other.tag}");
        if (other.CompareTag("Player"))
        {
            _playerIn = true;
            if (prompt != null) prompt.Show(promptText);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        Log($"OnTriggerExit2D: from={other.name}, tag={other.tag}");
        if (other.CompareTag("Player"))
        {
            _playerIn = false;
            if (prompt != null) prompt.Hide();
        }
    }

    void OnGUI() // 화면 왼쪽 위 상태표시
    {
        GUI.Label(new Rect(10, 10, 800, 22), $"NPCShopOpener: in={_playerIn}  scene='{shopSceneName}'");
        if (!string.IsNullOrEmpty(_last))
            GUI.Label(new Rect(10, 30, 1200, 22), _last);
    }

    void Log(string s)
    {
        _last = s;
        Debug.Log("[NPCShopOpener_Debug] " + s);
    }
}
