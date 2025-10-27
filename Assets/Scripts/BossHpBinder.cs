using System.Collections;
using UnityEngine;

// Damageable을 건드리지 않고, 주기적으로 HP를 읽어와 UI를 갱신하는 안전한 바인더
public class BossHPBinder : MonoBehaviour
{
    [SerializeField] private BossHPUI ui;         // BossHPUI 컴포넌트 드래그
    [SerializeField] private string bossTag = "Boss";
    [SerializeField] private float pollInterval = 0.05f; // 20fps로 폴링

    private Damageable target;
    private int lastHP = int.MinValue;
    private int lastMax = int.MinValue;
    private bool lastDead = false;

    void OnEnable()
    {
        StartCoroutine(PollLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator PollLoop()
    {
        while (true)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                target = FindBoss();
                if (target != null)
                {
                    lastHP = int.MinValue;
                    lastMax = int.MinValue;
                    lastDead = false;

                    if (ui != null)
                    {
                        ui.gameObject.SetActive(true);
                        ui.Bind(target.GetCurrentHP(), target.maxHP);
                    }
                }
                else
                {
                    if (ui != null) ui.gameObject.SetActive(false);
                }
            }
            else
            {
                int cur = target.GetCurrentHP();
                int max = target.maxHP;
                bool dead = target.IsDead(); // 확장메서드/기존메서드 중 하나 존재

                if (max != lastMax)
                {
                    if (ui != null) ui.Bind(cur, max);
                    lastMax = max;
                }
                else if (cur != lastHP)
                {
                    if (ui != null) ui.UpdateHP(cur);
                }
                lastHP = cur;

                if (!lastDead && dead)
                {
                    // UI만 처리 (포탈/기타 로직은 기존 시스템 그대로 동작)
                    if (ui != null)
                    {
                        ui.UpdateHP(0);
                        ui.gameObject.SetActive(false);
                    }
                }
                lastDead = dead;
            }

            yield return new WaitForSeconds(pollInterval);
        }
    }

    private Damageable FindBoss()
    {
        var all = FindObjectsOfType<Damageable>(true);
        foreach (var d in all)
            if (d.CompareTag(bossTag)) return d;
        return null;
    }

    // 보스가 지연 스폰될 때 외부에서 직접 연결하고 싶다면 사용
    public void Rebind(Damageable newBoss)
    {
        target = newBoss;
        lastHP = int.MinValue;
        lastMax = int.MinValue;
        lastDead = false;

        if (ui != null && target != null)
        {
            ui.gameObject.SetActive(true);
            ui.Bind(target.GetCurrentHP(), target.maxHP);
        }
    }
}
