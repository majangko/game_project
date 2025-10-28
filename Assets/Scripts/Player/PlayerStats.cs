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

    [Tooltip("공격력 배율 (1.0 = 기본, 1.5 = +50%)")]
    public float attackMultiplier = 1f;

    public float tempAttackBuffMultiplier = 1f; // 스킬 기반 추가 배율
    public event Action<float> OnAttackMultiplierChanged;

    public event Action<float> OnAttackChanged;

    [Header("Skills")]
    public Sprite[] skillIcons;         // 캐릭터별 스킬 아이콘
    public float[] skillCooldownMax;    // 각 스킬 최대 쿨다운
    public float[] skillCooldownRemain; // 남은 쿨다운

    // ======================================================================
    // [추가] 아이템/런 효과용 런타임 버프 (기존 로직 건드리지 않기 위한 최소 추가)
    // ======================================================================
    [Header("Runtime Bonuses (Items / Run Only)")]
    [Tooltip("아이템 등으로 얻는 고정 공격력 보너스(평면) 누적값")]
    [SerializeField] private float extraAttackFlat = 0f;

    [Tooltip("아이템 등으로 얻는 이동속도 가산 누적 (0.015 = +1.5%)")]
    [SerializeField] private float speedBonusPercent = 0f;

    [Tooltip("아이템 등으로 이번 런 동안 추가된 최대체력 보너스 누적")]
    [SerializeField] private int maxHPBonus = 0;

    /// <summary>현재 최종 이동 속도 배율(= 1 + speedBonusPercent). 이동 스크립트가 필요시 참조</summary>
    public float CurrentSpeedMultiplier => 1f + speedBonusPercent;

    // ======================================================================
    // Unity Lifecycle
    // ======================================================================

    void Awake()
    {
        // ✅ HP/MP 초기화 — 씬이 시작될 때 항상 풀 상태로 복원
        _hp = maxHP;
        _mp = maxMP;

        // 쿨타임 배열 초기화
        if (skillCooldownMax != null && skillCooldownMax.Length > 0)
        {
            skillCooldownRemain = new float[skillCooldownMax.Length];
            for (int i = 0; i < skillCooldownRemain.Length; i++)
                skillCooldownRemain[i] = 0;
        }

        // 초기 상태 HUD 반영
        OnHPChanged?.Invoke(_hp, maxHP);
        OnMPChanged?.Invoke(_mp, maxMP);
    }

    void Update()
    {
        // 스킬 쿨타임 감소
        if (skillCooldownRemain == null) return;

        for (int i = 0; i < skillCooldownRemain.Length; i++)
        {
            if (skillCooldownRemain[i] > 0)
                skillCooldownRemain[i] = Mathf.Max(0, skillCooldownRemain[i] - Time.deltaTime);
        }
    }

    // ======================================================================
    // 공격력 계산
    // ======================================================================

    public float GetAttackPower()
    {
        float totalMul = Mathf.Max(0.1f, attackMultiplier * tempAttackBuffMultiplier);
        // [변경점 - 최소 수정] 평면(고정) 보너스를 마지막에 더해줌.
        // extraAttackFlat이 0이면 기존과 완전히 동일한 값이 반환됩니다.
        return baseAttack * totalMul + extraAttackFlat;
    }

    public void SetAttackMultiplier(float value)
    {
        attackMultiplier = Mathf.Max(0.1f, value);
        OnAttackMultiplierChanged?.Invoke(attackMultiplier);
        OnAttackChanged?.Invoke(GetAttackPower());
    }

    // ======================================================================
    // HP / MP 관리
    // ======================================================================

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

    // ======================================================================
    // 스킬 쿨타임 관리
    // ======================================================================

    public void TriggerSkillCooldown(int index)
    {
        if (index < 0 || skillCooldownMax == null || index >= skillCooldownMax.Length) return;
        skillCooldownRemain[index] = skillCooldownMax[index];
    }

    // ======================================================================
    // 직접 세팅용 (로드 / 관리자 기능)
    // ======================================================================

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

    // ======================================================================
    // [추가] 아이템/런 효과 적용용 API (외부에서 호출)
    // ======================================================================

    /// <summary>
    /// 공격력 평면 보너스를 누적/해제합니다. (예: +1, +3, +5, +7, +10)
    /// 음수를 넣으면 해제(되돌리기)입니다.
    /// </summary>
    public void AddAttackBonus(float v)
    {
        if (Mathf.Approximately(v, 0f)) return;
        extraAttackFlat += v;
        OnAttackChanged?.Invoke(GetAttackPower());
    }

    /// <summary>
    /// 이동속도 가산 퍼센트를 누적/해제합니다. 0.015 = +1.5%
    /// 음수를 넣으면 해제(되돌리기)입니다.
    /// </summary>
    public void AddSpeedMultiplier(float v)
    {
        if (Mathf.Approximately(v, 0f)) return;
        // 너무 큰 음수로 속도가 역전되지 않도록 하한선(예: -0.9 = -90%) 방어
        speedBonusPercent = Mathf.Clamp(speedBonusPercent + v, -0.9f, 10f);
        // 필요 시 이동 컴포넌트가 이 이벤트를 구독하도록 확장 가능
        // OnSpeedChanged?.Invoke(CurrentSpeedMultiplier); // 원하면 이벤트 추가
    }

    /// <summary>
    /// 런 동안 최대체력을 증감합니다. (예: +10, +20, ..., -10)
    /// 증가 시 현재 체력도 그만큼 회복, 감소 시 현재 체력을 새 max로 클램프합니다.
    /// </summary>
    public void AddMaxHP(int v)
    {
        if (v == 0) return;

        int prevMax = maxHP;
        maxHP = Mathf.Max(1, maxHP + v);
        maxHPBonus += v; // 누적 기록 (원복 시 참고용으로 남겨도 됨)

        if (v > 0)
        {
            // 최대체력 증가분만큼 즉시 회복 (자연스러운 체감)
            _hp = Mathf.Min(maxHP, _hp + v);
        }
        else
        {
            // 최대체력 감소면 현재 체력도 클램프
            _hp = Mathf.Clamp(_hp, 0, maxHP);
        }

        OnHPChanged?.Invoke(_hp, maxHP);
    }
}
