using UnityEngine;
using System;

public class DayNightTimer : MonoBehaviour
{
    public static DayNightTimer Instance;

    [Header("Settings")]
    public float switchInterval = 10f; // ¿¸»Ø ¡÷±‚ (√ )
    private float timer;

    public bool isDay = true; // ≥∑¿Ã∏È true, π„¿Ã∏È false
    public event Action<bool> OnDayNightChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            isDay = !isDay;
            OnDayNightChanged?.Invoke(isDay);
            Debug.Log("≥∑/π„ ¿¸»Øµ : " + (isDay ? "≥∑" : "π„"));
        }
    }
}
