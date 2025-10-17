using UnityEngine;

public class MageBossAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform player;
    [SerializeField] private Transform boltSpawnPoint; // 🔮 볼트 생성 위치
    [SerializeField] private GameObject boltPrefab;    // 🔮 에너지 볼트 프리팹

    [Header("Attack Settings")]
    [SerializeField] private float detectRadius = 10f;   // 플레이어 감지 거리
    [SerializeField] private float attackRange = 8f;     // 사거리
    [SerializeField] private float attackCooldown = 2f;  // 공격 쿨타임
    [SerializeField] private float shootDelay = 0.4f;    // 발사 모션 후 딜레이

    private float lastAttackTime;
    private bool isDead;

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
        // 플레이어 자동 탐색
        if (!player)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    private void Update()
    {
        if (isDead || !player) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            TryAttack();
        }

        FlipToPlayer();
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        animator.SetTrigger(attackTrig);
        Invoke(nameof(ShootBolt), shootDelay);

        lastAttackTime = Time.time;
    }

    private void ShootBolt()
    {
        if (boltPrefab == null || boltSpawnPoint == null || player == null) return;

        // 생성
        GameObject bolt = Instantiate(boltPrefab, boltSpawnPoint.position, Quaternion.identity);

        // 방향 계산
        Vector2 dir = (player.position - boltSpawnPoint.position).normalized;

        // Bolt 스크립트로 방향 전달
        EnergyBolt boltScript = bolt.GetComponent<EnergyBolt>();
        if (boltScript != null)
        {
            boltScript.SetDirection(dir);
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
    }

    // 사망 처리 (Damageable에서 호출될 수 있음)
    public void OnHurt()
    {
        if (!isDead && animator) animator.SetTrigger(hitTrig);
    }

    public void OnDie()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger(dieTrig);
        this.enabled = false;
    }
}
