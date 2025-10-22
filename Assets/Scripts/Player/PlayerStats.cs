using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Basic Info")]
    public string characterName = "Default";
    public Sprite portrait;

    [Header("HP / MP")]
    public int maxHP = 100;
    public int maxMP = 50;

    [SerializeField] private int _hp;
    [SerializeField] private int _mp;

    public int HP => _hp;
    public int MP => _mp;

    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnMPChanged;
    public event Action OnDied;

    [Header("Combat Stats")]
    [Tooltip("기본 공격력 (모든 공격의 기반)")]
    public float baseAttack = 10f;

    [Tooltip("공격력 배율 (1.0 = 기본, 1.5 = +50%)")] // HP·패시브 기반
    public float attackMultiplier = 1f;

    public float tempAttackBuffMultiplier = 1f; // 스킬 기반 추가 배율

    // ✅ 공격력 배율 변경 이벤트 (컨트롤러와 자동 연동)
    public event Action<float> OnAttackMultiplierChanged;

    /// <summary>
    /// 현재 실제 공격력 계산 (HUD 및 스킬용)
    /// </summary>
    public float GetAttackPower()
    {
        float totalMul = Mathf.Max(0.1f, attackMultiplier * tempAttackBuffMultiplier);
        return baseAttack * totalMul;
    }


    /// <summary>
    /// 공격력 배율을 변경하고 이벤트 발동
    /// </summary>
    public void SetAttackMultiplier(float value)
    {
        attackMultiplier = Mathf.Max(0.1f, value);
        OnAttackMultiplierChanged?.Invoke(attackMultiplier); // ✅ 이벤트 발동
    }

    // HUD 표시용 (선택적으로 사용할 수 있음)
    public event Action<float> OnAttackChanged;

    [Header("Skills")]
    public Sprite[] skillIcons;         // 캐릭터별 스킬 아이콘
    public float[] skillCooldownMax;    // 각 스킬 최대 쿨다운
    public float[] skillCooldownRemain; // 남은 쿨다운

    void Awake()
    {
        // HP/MP 초기화
        _hp = Mathf.Clamp(_hp <= 0 ? maxHP : _hp, 0, maxHP);
        _mp = Mathf.Clamp(_mp <= 0 ? maxMP : _mp, 0, maxMP);

        // 스킬 쿨타임 배열 초기화
        if (skillCooldownMax != null)
        {
            skillCooldownRemain = new float[skillCooldownMax.Length];
            for (int i = 0; i < skillCooldownRemain.Length; i++)
                skillCooldownRemain[i] = 0;
        }

        // 초기 상태 HUD 갱신
        OnHPChanged?.Invoke(_hp, maxHP);
        OnMPChanged?.Invoke(_mp, maxMP);
    }

    void Update()
    {
        // 쿨타임 감소
        if (skillCooldownRemain == null) return;

        for (int i = 0; i < skillCooldownRemain.Length; i++)
        {
            if (skillCooldownRemain[i] > 0)
                skillCooldownRemain[i] = Mathf.Max(0, skillCooldownRemain[i] - Time.deltaTime);
        }
    }

    // ============================================================
    // HP / MP 관련 메서드
    // ============================================================

    public void Damage(int amt)
    {
        if (amt <= 0) return;
        int prev = _hp;
        _hp = Mathf.Max(0, _hp - amt);
        OnHPChanged?.Invoke(_hp, maxHP);
        if (prev > 0 && _hp == 0)
            OnDied?.Invoke();
    }

    public void Heal(int amt)
    {
        if (amt <= 0) return;
        _hp = Mathf.Min(maxHP, _hp + amt);
        OnHPChanged?.Invoke(_hp, maxHP);
    }

    public bool UseMP(int amt)
    {
        if (amt <= 0) return true;
        if (_mp < amt) return false;
        _mp -= amt;
        OnMPChanged?.Invoke(_mp, maxMP);
        return true;
    }

    public void RestoreMP(int amt)
    {
        if (amt <= 0) return;
        _mp = Mathf.Min(maxMP, _mp + amt);
        OnMPChanged?.Invoke(_mp, maxMP);
    }

    // ============================================================
    // 스킬 쿨타임 관리
    // ============================================================

    public void TriggerSkillCooldown(int index)
    {
        if (index < 0 || skillCooldownMax == null || index >= skillCooldownMax.Length) return;
        skillCooldownRemain[index] = skillCooldownMax[index];
    }

    // ============================================================
    // 직접 수치 세팅용 (로드 / 관리자 기능)
    // ============================================================

    public void SetHPMP(int hp, int mp)
    {
        _hp = Mathf.Clamp(hp, 0, maxHP);
        _mp = Mathf.Clamp(mp, 0, maxMP);
        OnHPChanged?.Invoke(_hp, maxHP);
        OnMPChanged?.Invoke(_mp, maxMP);
        if (_hp == 0)
            OnDied?.Invoke();
    }

    public void SetHP(int hp)
    {
        _hp = Mathf.Clamp(hp, 0, maxHP);
        OnHPChanged?.Invoke(_hp, maxHP);
        if (_hp == 0)
            OnDied?.Invoke();
    }
}
