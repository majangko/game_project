using UnityEngine;

public class MageBossAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform player;          // 인스펙터 비워두면 자동 탐색
    [SerializeField] private Transform boltSpawnPoint;  // 🔮 볼트 생성 위치
    [SerializeField] private GameObject boltPrefab;     // 🔮 에너지 볼트 프리팹

    [Header("Attack Settings")]
    [SerializeField] private float detectRadius = 10f;   // 플레이어 감지 거리
    [SerializeField] private float attackRange = 8f;     // 사거리
    [SerializeField] private float attackCooldown = 2f;  // 공격 쿨타임
    [SerializeField] private float shootDelay = 0.4f;    // 발사 모션 후 딜레이

    private float lastAttackTime;
    private bool isDead;
    private float nextFindPlayerAt; // 늦게 스폰 대응용

    [Header("Animator Params")]
    [SerializeField] private string attackTrig = "2_Attack";
    [SerializeField] private string hitTrig = "3_Damage";
    [SerializeField] private string dieTrig = "4_Death";

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = transform;
    }

    private void Start()
    {
        TryFindPlayer(); // 1차 탐색
    }

    private void Update()
    {
        if (isDead) return;

        // 플레이어가 늦게 생기는 경우 주기적으로 재탐색
        if (!player && Time.time >= nextFindPlayerAt)
        {
            TryFindPlayer();
            nextFindPlayerAt = Time.time + 0.5f;
        }
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectRadius) return;      // 감지 반경 밖이면 무시

        if (dist <= attackRange)
            TryAttack();

        FlipToPlayer();
    }

    private void TryFindPlayer()
    {
        var found = GameObject.FindGameObjectWithTag("Player");
        if (found) player = found.transform;
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (animator) animator.SetTrigger(attackTrig);
        Invoke(nameof(ShootBolt), shootDelay);

        lastAttackTime = Time.time;
    }

    private void ShootBolt()
    {
        if (!boltPrefab || !boltSpawnPoint || !player) return;

        // 생성
        GameObject bolt = Instantiate(boltPrefab, boltSpawnPoint.position, Quaternion.identity);

        // 방향 계산
        Vector2 dir = (player.position - boltSpawnPoint.position).normalized;

        // Bolt 스크립트로 방향 전달 (기존 방식 유지)
        var boltScript = bolt.GetComponent<EnergyBolt>();
        if (boltScript != null)
            boltScript.SetDirection(dir);
    }

    private void FlipToPlayer()
    {
        if (!player) return;
        Vector3 s = visualRoot.localScale;

        if (player.position.x > transform.position.x)
            s.x = -Mathf.Abs(s.x);
        else
            s.x = Mathf.Abs(s.x);

        visualRoot.localScale = s;
    }

    // 사망/피격(기존 연결 유지)
    public void OnHurt()
    {
        if (!isDead && animator) animator.SetTrigger(hitTrig);
    }

    public void OnDie()
    {
        if (isDead) return;
        isDead = true;

        if (animator) animator.SetTrigger(dieTrig);
        this.enabled = false;
    }
}
