using UnityEngine;
using System.Collections;

public class Boss03AI : MonoBehaviour,IEnemyAIEvents
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private AttackHitbox attackHitbox;
    private Transform player;

    [Header("Detect / Attack Settings")]
    [SerializeField] private float detectRadius = 8f;
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float chargeTime = 0.6f;
    [SerializeField] private float swingDelay = 0.25f;
    private float lastAttackTime;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Animator Parameters")]
    [SerializeField] private string moveBool = "1_Move";
    [SerializeField] private string attackTrig = "2_Attack";
    [SerializeField] private string hitTrig = "3_Damage";
    [SerializeField] private string dieTrig = "4_Death";

    [Header("Aura Pattern")]
    public GameObject auraBlue;
    public GameObject auraYellow;
    public float auraInterval = 5f;
    public float auraDuration = 2f;

    [Header("Skill Prefabs")]
    public GameObject lightningFieldPrefab;          // 파랑 오오라 스킬: 감전 장판
    public GameObject yellowLightningBoltPrefab;     // 노랑 오오라 스킬: 번개볼트
    public Transform boltSpawnPoint;                 // 번개볼트 발사 위치
    public Transform lightningFieldSpawnPoint;       // ⚡ 새로 추가: 감전장판 생성 위치

    private bool isDead;
    private Coroutine auraRoutine;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = transform;
    }

    private void Start()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer) player = foundPlayer.transform;

        auraRoutine = StartCoroutine(AuraLoop());
    }

    private void Update()
    {
        if (isDead || !player) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            StopMoving();
            TryAttack();
        }
        else if (distance <= detectRadius)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }

        FlipToPlayer();
    }

    // -------------------- 이동 / 공격 --------------------
    private void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        animator.SetBool(moveBool, true);
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool(moveBool, false);
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        animator.SetBool(moveBool, false);
        animator.SetTrigger(attackTrig);
        yield return new WaitForSeconds(chargeTime);
        yield return new WaitForSeconds(swingDelay);

        if (attackHitbox != null)
        {
            Debug.Log("[Boss03AI] 기본 공격 발동");
            attackHitbox.DoAttack();
        }
    }

    private void FlipToPlayer()
    {
        if (!player) return;

        Vector3 scale = visualRoot.localScale;
        if (player.position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        visualRoot.localScale = scale;

        if (attackHitbox)
        {
            Vector3 hbScale = attackHitbox.transform.localScale;
            hbScale.x = Mathf.Sign(scale.x) * Mathf.Abs(hbScale.x);
            attackHitbox.transform.localScale = hbScale;
        }
    }

    // -------------------- 오오라 루프 --------------------
    private IEnumerator AuraLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(auraInterval);
            yield return StartCoroutine(ActivateRandomAura());
        }
    }

    private IEnumerator ActivateRandomAura()
    {
        DisableAllAuras();

        int rand = Random.Range(0, 2);
        GameObject selectedAura = (rand == 0) ? auraBlue : auraYellow;

        if (selectedAura == null)
        {
            Debug.LogWarning("[Boss03AI] 오오라 프리팹이 연결되지 않았습니다!");
            yield break;
        }

        selectedAura.SetActive(true);
        Debug.Log($"[Boss03AI] {selectedAura.name} 오오라 발동!");
        yield return new WaitForSeconds(auraDuration);
        selectedAura.SetActive(false);
        Debug.Log($"[Boss03AI] {selectedAura.name} 오오라 종료.");

        TriggerSkillBasedOnAura(selectedAura);
    }

    private void DisableAllAuras()
    {
        if (auraBlue) auraBlue.SetActive(false);
        if (auraYellow) auraYellow.SetActive(false);
    }

    private void TriggerSkillBasedOnAura(GameObject aura)
    {
        if (aura == auraBlue)
        {
            Debug.Log("⚡ 파랑 오오라 스킬 발동!");
            CastBlueLightningSkill();
        }
        else if (aura == auraYellow)
        {
            Debug.Log("🟡 노랑 오오라 스킬 발동!");
            StartCoroutine(CastYellowLightningSkill());
        }
    }

    // -------------------- 파랑 오오라 스킬 --------------------
    private void CastBlueLightningSkill()
    {
        if (lightningFieldPrefab == null)
        {
            Debug.LogWarning("[Boss03AI] lightningFieldPrefab이 연결되지 않았습니다!");
            return;
        }

        // ⚡ Lightning Field Spawn Point 기준으로 생성
        Vector3 spawnPos = lightningFieldSpawnPoint != null
            ? lightningFieldSpawnPoint.position
            : new Vector3(transform.position.x, transform.position.y - 0.5f, 0f);

        Instantiate(lightningFieldPrefab, spawnPos, Quaternion.identity);
        Debug.Log("⚡ 번개 장판 생성 완료!");
    }

    // -------------------- 노랑 오오라 스킬 --------------------
    private IEnumerator CastYellowLightningSkill()
    {
        if (yellowLightningBoltPrefab == null)
        {
            Debug.LogWarning("[Boss03AI] yellowLightningBoltPrefab이 연결되지 않았습니다!");
            yield break;
        }

        Debug.Log("⚡ 노랑 오오라 스킬 시작!");
        for (int i = 0; i < 3; i++)
        {
            GameObject boltObj = Instantiate(yellowLightningBoltPrefab, boltSpawnPoint.position, Quaternion.identity);
            YellowLightningBolt bolt = boltObj.GetComponent<YellowLightningBolt>();
            if (bolt != null && player != null)
            {
                Vector2 dir = (player.position - boltSpawnPoint.position).normalized;
                bolt.SetDirection(dir);
            }

            yield return new WaitForSeconds(0.6f); // 3회 발사 간격
        }

        Debug.Log("⚡ 노랑 오오라 스킬 종료 (3회 발사 완료)");
    }

    // -------------------- 데미지 / 사망 처리 --------------------
    public void OnHurt()
    {
        if (!isDead && animator)
            animator.SetTrigger(hitTrig);
    }

    public void OnDie()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[Boss03AI] {name} OnDie() 호출됨 ✅");

        animator.SetTrigger(dieTrig);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;
        StopAllCoroutines();
        DisableAllAuras();
        this.enabled = false;
    }

}
