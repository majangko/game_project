// Assets/Scripts/GameFlow/GameOverBackdrop.cs
using UnityEngine;
using UnityEngine.UI;

public class GameOverBackdrop : MonoBehaviour
{
    public RawImage target;
    Texture2D runtimeTex;

    void Start()
    {
        var tex = GameOverLoader.ConsumeShot();
        if (tex && target)
        {
            runtimeTex = tex;
            target.texture = runtimeTex;
            target.color = Color.white;

            // 화면 꽉 채우기(필요시)
            var rt = target.rectTransform;
            var parent = rt.rect;
            float canvasAspect = parent.width / parent.height;
            float texAspect    = (float)tex.width / tex.height;
            if (texAspect > canvasAspect)
                rt.sizeDelta = new Vector2(parent.height * texAspect, parent.height);
            else
                rt.sizeDelta = new Vector2(parent.width, parent.width / texAspect);
        }
    }

    void OnDisable()  { ClearTexture(); }
    void OnDestroy()  { ClearTexture(); }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 에디터에서 저장 시 런타임 텍스처가 참조되지 않도록
        if (!Application.isPlaying && target) target.texture = null;
    }
#endif

    void ClearTexture()
    {
        if (target) target.texture = null;
        if (runtimeTex)
        {
            if (Application.isPlaying) Destroy(runtimeTex);
            else DestroyImmediate(runtimeTex);
            runtimeTex = null;
        }
    }
}
