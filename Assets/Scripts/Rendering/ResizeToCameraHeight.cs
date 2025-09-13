using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResizeToCameraHeight : MonoBehaviour
{
    public Camera cam;          // 비워두면 main camera
    public bool keepAspect = true;

    void Start()
    {
        if (!cam) cam = Camera.main;
        var sr = GetComponent<SpriteRenderer>();
        if (!sr || !sr.sprite || !cam) return;

        // 현재 스프라이트의 월드 사이즈
        var b = sr.bounds;
        float worldH = b.size.y;

        // 카메라 화면의 월드 높이 = orthoSize * 2
        float camH = cam.orthographicSize * 2f;

        // 스케일 비율
        float scale = camH / worldH;
        transform.localScale *= scale;

        if (!keepAspect)
        {
            // 가로도 꽉 채우고 싶으면 카메라 폭 기준으로 추가 보정
            float worldW = sr.bounds.size.x * scale;
            float camW = camH * cam.aspect;
            transform.localScale = new Vector3(transform.localScale.x * (camW / worldW), transform.localScale.y, 1f);
        }
    }
}
