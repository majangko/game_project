using UnityEngine;
using UnityEngine.UI;

public class GameOverBackdrop : MonoBehaviour
{
    public RawImage target;
    [Range(0f,1f)] public float vignette = 0.4f;

    void Start()
    {
        var tex = GameOverLoader.ConsumeShot();
        if (tex && target)
        {
            target.texture = tex;
            target.color = Color.white;

            // 해상도 커버(비율 유지)
            var rt = target.rectTransform;
            float canvasAspect = rt.rect.width / rt.rect.height;
            float texAspect = (float)tex.width / tex.height;
            if (texAspect > canvasAspect) {
                // 가로가 넓다 → 높이를 맞추고 좌우가 넘치게
                rt.sizeDelta = new Vector2(rt.rect.height * texAspect, rt.rect.height);
            } else {
                rt.sizeDelta = new Vector2(rt.rect.width, rt.rect.width / texAspect);
            }
        }
    }
}
