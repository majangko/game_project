using UnityEngine;

public class PromptBillboard : MonoBehaviour
{
    public Camera cam;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        // 캔버스가 카메라를 향하도록
        transform.forward = cam.transform.forward;
    }
}
