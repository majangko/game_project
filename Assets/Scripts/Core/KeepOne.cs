using UnityEngine;

public class KeepOne : MonoBehaviour
{
    private static KeepOne _instance;
    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
