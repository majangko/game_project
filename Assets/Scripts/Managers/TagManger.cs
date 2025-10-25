using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TagManager : MonoBehaviour
{
    public static TagManager Instance;

    [Header("Tag Settings")]
    public float tagCooldown = 3f;
    public float invincibleDuration = 0.5f;
    private float tagTimer = 0f;

    [Header("Characters")]
    public List<SpumPlatformerController> characters = new List<SpumPlatformerController>();
    private int currentIndex = 0;
    private bool _isLinkedToParty = false;

    [Header("VFX / UI")]
    public GameObject tagEffectPrefab;
    public UnityEvent<float> OnTagCooldownUpdate;
    public UnityEvent<int> OnCharacterSwitched;

    private HUDController hud;

    void Awake()
    {
        Instance = this;
        Debug.Log("<color=yellow>[TagManager] Awake</color>");
    }

    void Start()
    {
        hud = FindObjectOfType<HUDController>();

        // 기본 캐릭터 등록
        var player = FindObjectOfType<SpumPlatformerController>();
        if (player != null && characters.Count == 0)
        {
            characters.Add(player);
            currentIndex = 0;
            Debug.Log($"[TagManager] 기본 캐릭터 '{player.name}' 등록됨.");
        }

        TryConnectPartyManager();
        RefreshHUD();

        // ✅ 시작 시 카메라가 첫 캐릭터를 바라보게 설정
        var cam = FindObjectOfType<CameraFollow>();
        if (cam != null && characters.Count > 0)
            cam.target = characters[currentIndex].transform;
    }

    void Update()
    {
        if (!_isLinkedToParty)
            TryConnectPartyManager();

        // 쿨타임 감소
        if (tagTimer > 0)
        {
            tagTimer -= Time.deltaTime;
            OnTagCooldownUpdate?.Invoke(Mathf.Clamp01(tagTimer / tagCooldown));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int nextIndex = (currentIndex + 1) % characters.Count;
            TryTag(nextIndex);
        }
    }

    // -------------------- PartyManager 연동 --------------------
    void TryConnectPartyManager()
    {
        if (PartyManager.Instance == null) return;
        if (PartyManager.Instance.GetCount() == 0) return;

        Debug.Log("<color=orange>[TagManager] PartyManager → TagManager 연결 시도...</color>");
        PartyManager.Instance.AssignToTagManager(this, keepExisting: true);
        _isLinkedToParty = true;

        for (int i = 0; i < characters.Count; i++)
            if (characters[i] != null)
                characters[i].gameObject.SetActive(i == 0);

        currentIndex = 0;
        RefreshHUD();

        // ✅ 카메라 대상 초기 설정
        var cam = FindObjectOfType<CameraFollow>();
        if (cam != null && characters.Count > 0)
            cam.target = characters[currentIndex].transform;

        var tagUI = FindObjectOfType<TagPanelUI>();
        if (tagUI != null)
            tagUI.LoadCharacterPortraits();

        // 🟢 플레이어 생성 후 EnemyAI 등록 (지연 호출)
        StartCoroutine(DelayedEnemyRegister());

        Debug.Log("<color=green>[TagManager] PartyManager 연동 완료 ✅</color>");
    }

    // -------------------- Enemy 등록 (딜레이 적용) --------------------
    public IEnumerator DelayedEnemyRegister()
    {
        yield return new WaitForSeconds(0.5f); // 몬스터 스폰 완료 대기 (시간 약간 늘림)
        RegisterEnemiesToPlayer();
    }

    private void RegisterEnemiesToPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("[TagManager] RegisterEnemiesToPlayer() - Player를 찾지 못함 ❌");
            return;
        }

        var playerTransform = playerObj.transform;
        int count = 0;

        // 비활성화된 적도 포함해서 모두 탐색
        var enemies = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            var typeName = enemy.GetType().Name;

            if (typeName.StartsWith("Enemy"))
            {
                enemy.SendMessage("SetPlayer", playerTransform, SendMessageOptions.DontRequireReceiver);
                count++;
            }
        }

        Debug.Log($"[TagManager] Player 등록 완료 → {count}개의 EnemyAI에 연결됨 ✅");
    }

    // -------------------- 캐릭터 전환 --------------------
    public void TryTag(int targetIndex)
    {
        if (characters.Count == 0) return;
        if (targetIndex < 0 || targetIndex >= characters.Count) return;
        if (tagTimer > 0) return;
        if (targetIndex == currentIndex) return;

        SwitchCharacter(targetIndex);
        tagTimer = tagCooldown;
    }

    void SwitchCharacter(int newIndex)
    {
        if (characters.Count == 0) return;

        var current = characters[currentIndex];
        var next = characters[newIndex];
        if (current == null || next == null)
        {
            Debug.LogWarning("[TagManager] 캐릭터가 null입니다. 전환 실패.");
            return;
        }

        // 위치 유지
        Vector3 pos = current.transform.position;
        next.transform.position = pos;

        // 이펙트 출력
        if (tagEffectPrefab)
            Instantiate(tagEffectPrefab, pos, Quaternion.identity);

        // 활성/비활성 전환
        current.gameObject.SetActive(false);
        next.gameObject.SetActive(true);

        // ✅ 카메라 대상 갱신
        var cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
            cam.target = next.transform;

        // 무적 처리
        var dmg = next.GetComponent<Damageable>();
        if (dmg != null)
            StartCoroutine(InvincibleCoroutine(dmg));

        // 인덱스 및 순서 정렬
        currentIndex = newIndex;
        RefreshCharacterOrder();

        // ✅ HUD 완전 갱신
        RefreshHUD();

        // 이벤트 호출 (TagPanelUI용)
        OnCharacterSwitched?.Invoke(currentIndex);
        Debug.Log($"[TagManager] 캐릭터 전환 완료 → {next.name}");

        // 태그 UI 갱신
        var tagUI = FindObjectOfType<TagPanelUI>();
        if (tagUI != null)
        {
            tagUI.LoadCharacterPortraits();
            tagUI.UpdateHighlight(0);
        }

        // 🟢 캐릭터 전환 후에도 적이 새 플레이어를 인식하도록 재등록
        StartCoroutine(DelayedEnemyRegister());
    }

    // -------------------- 순서 정렬 --------------------
    public void RefreshCharacterOrder()
    {
        if (characters == null || characters.Count == 0) return;

        var current = GetCurrentCharacter();
        if (current == null) return;

        if (characters.Contains(current))
        {
            characters.Remove(current);
            characters.Insert(0, current);
        }

        currentIndex = 0; // 활성 캐릭터는 항상 0번
        Debug.Log($"[TagManager] RefreshCharacterOrder() 실행 → {string.Join(", ", characters.Select(c => c.name))}");
    }

    IEnumerator InvincibleCoroutine(Damageable dmg)
    {
        dmg.SetInvincible(true);
        yield return new WaitForSeconds(invincibleDuration);
        dmg.SetInvincible(false);
    }

    // -------------------- HUD 자동 갱신 --------------------
    void RefreshHUD()
    {
        if (hud == null)
            hud = FindObjectOfType<HUDController>();
        if (hud == null) return;
        if (characters.Count == 0) return;

        var activeChar = characters[currentIndex];
        if (activeChar == null) return;

        var stats = activeChar.GetComponent<PlayerStats>();
        if (stats != null)
        {
            hud.BindToPlayer(stats);
            Debug.Log($"[TagManager] HUD 갱신 완료 → {stats.name}");
        }
    }

    // -------------------- 접근자 --------------------
    public int GetCurrentIndex() => currentIndex;
    public SpumPlatformerController GetCurrentCharacter() =>
        (characters != null && characters.Count > 0) ? characters[currentIndex] : null;
}
