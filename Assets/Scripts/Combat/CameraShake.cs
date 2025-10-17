using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;
    private Coroutine shakeRoutine;
    private Transform target; // 플레이어 추적용

    private void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    public void SetTarget(Transform player)
    {
        target = player;
    }

    public void Shake(float duration = 0.2f, float magnitude = 0.15f)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        Vector3 basePos = target ? target.position : transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = (Mathf.PerlinNoise(Time.time * 10f, 0f) - 0.5f) * magnitude;
            float y = (Mathf.PerlinNoise(0f, Time.time * 10f) - 0.5f) * magnitude;

            // ✅ 플레이어 중심 유지 + 살짝 진동만
            transform.position = basePos + new Vector3(x, y, -10f);

            yield return null;
        }

        if (target)
            transform.position = new Vector3(target.position.x, target.position.y, -10f);
        else
            transform.localPosition = originalPos;
    }
}
