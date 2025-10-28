// Assets/Scripts/System/PlayerInventoryLocator.cs
using UnityEngine;

public static class PlayerInventoryLocator
{
    public static PlayerInventory Ensure()
    {
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance;

        // 씬에 이미 붙어있는지 한번 더 탐색
        var existing = Object.FindAnyObjectByType<PlayerInventory>();
        if (existing != null)
        {
            // 싱글톤 초기화
            var go = existing.gameObject;
            if (PlayerInventory.Instance == null)
            {
                // Awake에서 Instance 세팅이 이루어지도록 강제 활성화
                if (!go.activeSelf) go.SetActive(true);
            }
            return existing;
        }

        // 완전 없으면 새로 만든다
        var holder = new GameObject("PlayerInventory(Auto)");
        Object.DontDestroyOnLoad(holder);
        var inv = holder.AddComponent<PlayerInventory>();
        Debug.LogWarning("[PlayerInventoryLocator] PlayerInventory가 없어 자동 생성했습니다.");
        return inv;
    }
}
