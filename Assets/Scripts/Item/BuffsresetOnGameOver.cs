using UnityEngine;

public class BuffsResetOnGameOver : MonoBehaviour
{
    private void OnEnable()
    {
        // GameOver UI가 켜질 때 버프 초기화
        if (ItemBuffRuntime.Instance != null)
        {
            ItemBuffRuntime.Instance.ResetAll();
        }
    }
}
