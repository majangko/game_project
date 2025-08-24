// Assets/Scripts/UI/FullScreenCoverImage.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(Image))]
public class FullScreenCoverImage : UIBehaviour
{
    protected override void OnEnable()  { base.OnEnable();  Refit(); }
    protected override void OnRectTransformDimensionsChange() { Refit(); }

    void Refit()
    {
        var rt = (RectTransform)transform;
        var img = GetComponent<Image>();
        if (img.sprite == null || rt.parent == null) return;

        var parentRect = ((RectTransform)rt.parent).rect;
        float targetW = parentRect.width;
        float targetH = parentRect.height;
        if (targetW <= 0 || targetH <= 0) return;

        var sr = img.sprite.rect;
        float imgAspect = sr.width / sr.height;
        float targetAspect = targetW / targetH;

        float width, height;
        if (imgAspect > targetAspect) { height = targetH; width  = height * imgAspect; }
        else                          { width  = targetW; height = width  / imgAspect; }

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   height);
        rt.anchoredPosition = Vector2.zero;
    }
}
