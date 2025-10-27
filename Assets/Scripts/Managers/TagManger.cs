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
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        // HUDController, PartyManager 초기화 대기
        yield return null;
        yield return null;

        hud = FindObjectOfType<HUDController>();

        var player = FindObjectOfType<SpumPlatformerController>();
        if (player != null && characters.Count == 0)
        {
            characters.Add(player);
            currentIndex = 0;
            Debug.Log($"[TagManager] 기본 캐릭터 '{player.name}' 등록됨.");
        }

        TryConnectPartyManager();

        // HUD는 약간 더 늦게 Refresh (HUDController가 초기화 끝난 후)
        yield return null;
        RefreshHUD();

        var cam = FindObjectOfType<CameraFollow>();
        if (cam != null && characters.Count > 0)
            cam.target = characters[currentIndex].transform;

        // ✅ 사망 이벤트 구독
        SubscribeDeathEvents();
    }

    void Update()
    {
        if (!_isLinkedToParty)
            TryConnectPartyManager();

        if (tagTimer > 0)
        {
            tagTimer -= Time.deltaTime;
            OnTagCooldownUpdate?.Invoke(Mathf.Clamp01(tagTimer / tagCooldown));
        }

        // 수동 태그 테스트용 키 (1키 누르면 다음 캐릭터)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int nextIndex = (currentIndex + 1) % characters.Count;
            TryTag(nextIndex);
        }
    }

    // -------------------- PartyManager 연동 --------------------
    public void TryConnectPartyManager()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("[TagManager] PartyManager가 아직 준비되지 않음.");
            return;
        }
        if (PartyManager.Instance.GetCount() == 0)
        {
            Debug.LogWarning("[TagManager] PartyManager 멤버가 없음.");
            return;
        }

        Debug.Log("<color=orange>[TagManager] PartyManager → TagManager 연결 시도...</color>");
        PartyManager.Instance.AssignToTagManager(this, keepExisting: true);
        _isLinkedToParty = true;

        for (int i = 0; i < characters.Count; i++)
            if (characters[i] != null)
                characters[i].gameObject.SetActive(i == 0);

        currentIndex = 0;
        StartCoroutine(DelayedHUDRefresh());
        StartCoroutine(DelayedCameraSet());
        StartCoroutine(DelayedEnemyRegister());
    }

    private IEnumerator DelayedHUDRefresh()
    {
        yield return null;
        yield return null;
        RefreshHUD();
    }

    private IEnumerator DelayedCameraSet()
    {
        yield return null;
        var cam = FindObjectOfType<CameraFollow>();
        if (cam != null && characters.Count > 0)
            cam.target = characters[currentIndex].transform;
    }

    // -------------------- Enemy 등록 --------------------
    public IEnumerator DelayedEnemyRegister()
    {
        yield return new WaitForSeconds(0.5f);
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

        Vector3 pos = current.transform.position;
        next.transform.position = pos;

        if (tagEffectPrefab)
            Instantiate(tagEffectPrefab, pos, Quaternion.identity);

        current.gameObject.SetActive(false);
        next.gameObject.SetActive(true);

        var cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
            cam.target = next.transform;

        var dmg = next.GetComponent<Damageable>();
        if (dmg != null)
            StartCoroutine(InvincibleCoroutine(dmg));

        currentIndex = newIndex;
        RefreshCharacterOrder();
        RefreshHUD();

        OnCharacterSwitched?.Invoke(currentIndex);
        Debug.Log($"[TagManager] 캐릭터 전환 완료 → {next.name}");

        var tagUI = FindObjectOfType<TagPanelUI>();
        if (tagUI != null)
        {
            tagUI.LoadCharacterPortraits();
            tagUI.UpdateHighlight(currentIndex);
        }

        StartCoroutine(DelayedEnemyRegister());
    }

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

        currentIndex = 0;
        Debug.Log($"[TagManager] RefreshCharacterOrder() 실행 → {string.Join(", ", characters.Select(c => c.name))}");
    }

    IEnumerator InvincibleCoroutine(Damageable dmg)
    {
        dmg.SetInvincible(true);
        yield return new WaitForSeconds(invincibleDuration);
        dmg.SetInvincible(false);
    }

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
    private void OnEnable()
    {
        // 각 캐릭터의 PlayerStats.OnDied 이벤트 구독
        StartCoroutine(SubscribeDeathEventsDelayed());
    }

    private IEnumerator SubscribeDeathEventsDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var ch in characters)
        {
            if (ch == null) continue;
            var stats = ch.GetComponent<PlayerStats>();
            if (stats != null)
                stats.OnDied += HandleCharacterDeath;
        }
    }

    private void HandleCharacterDeath()
    {
        // 모든 캐릭터가 죽었는지 확인
        bool allDead = true;
        foreach (var ch in characters)
        {
            if (ch == null) continue;
            var stats = ch.GetComponent<PlayerStats>();
            if (stats != null && stats.HP > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            Debug.Log("<color=red>[TagManager] 모든 캐릭터 사망 감지 → 게임오버</color>");
            GameManager.Instance?.OnPlayerDeath("all_dead");
        }
    }


    // -------------------- 사망 처리 --------------------
    private void SubscribeDeathEvents()
    {
        foreach (var ch in characters)
        {
            if (ch == null) continue;
            var stats = ch.GetComponent<PlayerStats>();
            if (stats == null) continue;

            // 중복 방지
            stats.OnDied -= () => OnCharacterDied(ch);
            stats.OnDied += () => OnCharacterDied(ch);
        }

        Debug.Log($"[TagManager] {characters.Count}명의 캐릭터 사망 이벤트 구독 완료 ✅");
    }

    public void OnCharacterDied(SpumPlatformerController deadChar)
    {
        Debug.Log($"[TagManager] 캐릭터 사망 감지 → {deadChar.name}");

        if (!characters.Contains(deadChar)) return;

        // 🔹 캐릭터를 태그 리스트에서 제거
        characters.Remove(deadChar);

        // 🔹 즉시 비활성화 (물리 낙하, 애니메이션, 사운드 정지)
        deadChar.gameObject.SetActive(false);

        Debug.Log($"[TagManager] {deadChar.name} 비활성화 완료. 남은 캐릭터: {characters.Count}");

        // 🔹 HUD 갱신
        HUDController hud = FindObjectOfType<HUDController>();
        if (hud != null)
            hud.UpdatePartyPortraits();

        // 🔹 다음 생존 캐릭터로 자동 전환
        if (characters.Count > 0)
        {
            int nextIndex = Mathf.Clamp(currentIndex, 0, characters.Count - 1);
            currentIndex = nextIndex;
            SwitchCharacter(nextIndex);
        }
        else
        {
            Debug.Log("<color=red>[TagManager] 모든 캐릭터 사망 → 게임오버</color>");
            GameManager.Instance?.OnPlayerDeath("all_dead");
            FindObjectOfType<GameFlowManager>()?.OnGameOver();
        }
    }


    private int FindNextAliveIndex()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            var stats = characters[i]?.GetComponent<PlayerStats>();
            if (stats != null && stats.HP > 0)
                return i;
        }
        return -1;
    }

    // -------------------- 접근자 --------------------
    public int GetCurrentIndex() => currentIndex;
    public SpumPlatformerController GetCurrentCharacter() =>
        (characters != null && characters.Count > 0) ? characters[currentIndex] : null;
}
