using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text goldText;       // 현재 골드 표시
    [SerializeField] private TMP_Text gainTextPrefab; // +5G 효과용 프리팹 (World Space 아님!)

    [Header("Effect Settings")]
    [SerializeField] private Transform effectParent;  // 이펙트 표시할 부모 (보통 Canvas)
    [SerializeField] private float floatSpeed = 40f;  // 위로 뜨는 속도
    [SerializeField] private float fadeTime = 1f;     // 사라지는 시간

    void Start() => Refresh();

    public void Refresh()
    {
        if (goldText == null) return;

        if (GoldManager.Instance != null)
            goldText.text = $"{GoldManager.Instance.CurrentGold} G";
        else
            goldText.text = "0 G";
    }

    // ✅ 골드 획득 시 호출 (AddGold()에서 자동 호출)
    public void ShowGainEffect(int amount)
    {
        if (gainTextPrefab == null || effectParent == null) return;

        TMP_Text gainText = Instantiate(gainTextPrefab, effectParent);
        gainText.text = $"+{amount}G";
        gainText.alpha = 1f;
        gainText.transform.localPosition = Vector3.zero;

        // 위로 뜨면서 사라지는 코루틴 시작
        StartCoroutine(FloatAndFade(gainText));
    }

    private System.Collections.IEnumerator FloatAndFade(TMP_Text txt)
    {
        Vector3 start = txt.transform.localPosition;
        float elapsed = 0f;
        Color c = txt.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            // 위로 이동
            txt.transform.localPosition = start + Vector3.up * floatSpeed * elapsed;
            // 알파값 감소
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            txt.color = c;
            yield return null;
        }

        Destroy(txt.gameObject);
    }
}
