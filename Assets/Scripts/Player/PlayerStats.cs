using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Max Stats")]
    public int maxHP = 100;
    public int maxMP = 50;

    [SerializeField] private int _hp;
    [SerializeField] private int _mp;

    // ✔ 단 하나의 프로퍼티만 유지 (동일 이름의 필드/메서드/프로퍼티 존재 금지)
    public int HP => _hp;
    public int MP => _mp;

    public event Action<int,int> OnHPChanged; // (cur, max)
    public event Action<int,int> OnMPChanged; // (cur, max)
    public event Action OnDied;

    void Awake()
    {
        _hp = Mathf.Clamp(_hp <= 0 ? maxHP : _hp, 0, maxHP);
        _mp = Mathf.Clamp(_mp <= 0 ? maxMP : _mp, 0, maxMP);
        OnHPChanged?.Invoke(_hp, maxHP);
        OnMPChanged?.Invoke(_mp, maxMP);
    }

    public void SetHPMP(int hp, int mp)
    {
        _hp = Mathf.Clamp(hp, 0, maxHP);
        _mp = Mathf.Clamp(mp, 0, maxMP);
        OnHPChanged?.Invoke(_hp, maxHP);
        OnMPChanged?.Invoke(_mp, maxMP);
        if (_hp == 0) OnDied?.Invoke();
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
}
