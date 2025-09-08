using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameAction
{
    MoveUp, MoveDown, MoveLeft, MoveRight,
    Attack, Skill, Team1, Team2
}

public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance { get; private set; }

    Dictionary<GameAction, KeyCode> _keys = new Dictionary<GameAction, KeyCode>();

    // ±âº»°ª
    readonly Dictionary<GameAction, KeyCode> _defaults = new Dictionary<GameAction, KeyCode>
    {
        { GameAction.MoveUp, KeyCode.W },
        { GameAction.MoveDown, KeyCode.S },
        { GameAction.MoveLeft, KeyCode.A },
        { GameAction.MoveRight, KeyCode.D },
        { GameAction.Attack, KeyCode.H },
        { GameAction.Skill, KeyCode.J },
        { GameAction.Team1, KeyCode.K },
        { GameAction.Team2, KeyCode.L },
    };

    const string PREF_PREFIX = "KEY_";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromPrefs();
    }

    public KeyCode GetKey(GameAction action) => _keys[action];

    public void SetKey(GameAction action, KeyCode key) => _keys[action] = key;

    public bool IsUsing(KeyCode key)
    {
        foreach (var kv in _keys) if (kv.Value == key) return true;
        return false;
    }

    public void SaveToPrefs()
    {
        foreach (var kv in _keys)
            PlayerPrefs.SetInt(PREF_PREFIX + kv.Key, (int)kv.Value);
        PlayerPrefs.Save();
    }

    public void LoadFromPrefs()
    {
        _keys.Clear();
        foreach (var kv in _defaults)
        {
            var prefKey = PREF_PREFIX + kv.Key;
            var k = (KeyCode)PlayerPrefs.GetInt(prefKey, (int)kv.Value);
            _keys[kv.Key] = k;
        }
    }
}
