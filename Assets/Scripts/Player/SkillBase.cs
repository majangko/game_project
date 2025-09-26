using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    [Header("Common")]
    public string skillName = "Skill";
    public float cooldown = 1f;
    public float duration = 0f;
    public GameObject effectPrefab;
    public string animTrigger = "";

    protected Animator anim;
    protected SpumPlatformerController ctrl;
    protected Rigidbody2D rb;

    private bool isOnCooldown = false;

    protected virtual void Awake()
    {
        ctrl = GetComponent<SpumPlatformerController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    public void Activate()
    {
        if (isOnCooldown) return;
        StartCoroutine(CoActivate());
    }

    private IEnumerator CoActivate()
    {
        OnActivate();
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    protected abstract void OnActivate();
}
