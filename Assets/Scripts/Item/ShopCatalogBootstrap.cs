using System.Collections;
using UnityEngine;

public class ShopCatalogBootstrap : MonoBehaviour
{
    [Tooltip("true면 Resources/Data/Shop에서 ShopItemSO를 스캔합니다. (Play 중 1프레임 뒤 안전 호출)")]
    public bool scanResources = true;

    private void OnEnable()
    {
        // 매니저들에서 연결된 SO 먼저 읽기(즉시 OK)
        ShopCatalog.WarmupFromManagers();

        if (scanResources && Application.isPlaying)
            StartCoroutine(CoWarmupResourcesNextFrame());
    }

    private IEnumerator CoWarmupResourcesNextFrame()
    {
        yield return null; // 한 프레임 대기 → 직렬화 구간 회피
        ShopCatalog.WarmupFromResources();
    }
}
