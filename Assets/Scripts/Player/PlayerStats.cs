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

    [Header("Skills")]
    public Sprite[] skillIcons;         // 캐릭터별 스킬 아이콘
    public float[] skillCooldownMax;    // 각 스킬의 최대 쿨다운
    public float[] skillCooldownRemain; // 남은 쿨다운 (독립적으로 유지됨)

    void Awake()
    {
        _hp = Mathf.Clamp(_hp <= 0 ? maxHP : _hp, 0, maxHP);
        _mp = Mathf.Clamp(_mp <= 0 ? maxMP : _mp, 0, maxMP);

        // 쿨다운 배열 초기화
        if (skillCooldownMax != null)
        {
            skillCooldownRemain = new float[skillCooldownMax.Length];
            for (int i = 0; i < skillCooldownRemain.Length; i++)
                skillCooldownRemain[i] = 0;
        }

        OnHPChanged?.Invoke(_hp, maxHP);
        OnMPChanged?.Invoke(_mp, maxMP);
    }

    void Update()
    {
        // 쿨다운 시간 감소
        if (skillCooldownRemain == null) return;
        for (int i = 0; i < skillCooldownRemain.Length; i++)
        {
            if (skillCooldownRemain[i] > 0)
                skillCooldownRemain[i] = Mathf.Max(0, skillCooldownRemain[i] - Time.deltaTime);
        }
    }

    public void Damage(int amt)
    {
        if (amt <= 0) return;
        int prev = _hp;
        _hp = Mathf.Max(0, _hp - amt);
        OnHPChanged?.Invoke(_hp, maxHP);
        if (prev > 0 && _hp == 0) OnDied?.Invoke();
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

    // 🔹 스킬 쿨다운 발동
    public void TriggerSkillCooldown(int index)
    {
        if (index < 0 || index >= skillCooldownMax.Length) return;
        skillCooldownRemain[index] = skillCooldownMax[index];
    }
    public void SetHPMP(int hp, int mp)
    {
        _hp = Mathf.Clamp(hp, 0, maxHP);
        _mp = Mathf.Clamp(mp, 0, maxMP);
        OnHPChanged?.Invoke(_hp, maxHP);
        OnMPChanged?.Invoke(_mp, maxMP);
        if (_hp == 0) OnDied?.Invoke();
    }
}
