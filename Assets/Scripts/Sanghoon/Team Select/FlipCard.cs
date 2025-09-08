// FILE: FlipCard.cs
using UnityEngine;

public class FlipCard : MonoBehaviour
{
    public RectTransform front;
    public RectTransform back;

    // 단순 토글(애니메이션은 나중에)
    public void Toggle()
    {
        if (!front || !back) return;
        bool toBack = front.gameObject.activeSelf;
        front.gameObject.SetActive(!toBack);
        back.gameObject.SetActive(toBack);
    }

    public void ShowFront()
    {
        if (!front || !back) return;
        front.gameObject.SetActive(true);
        back.gameObject.SetActive(false);
    }

    public void ShowBack()
    {
        if (!front || !back) return;
        front.gameObject.SetActive(false);
        back.gameObject.SetActive(true);
    }
}
