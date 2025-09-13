using UnityEngine;

[ExecuteAlways]
public class ParallaxLayer2D : MonoBehaviour
{
    [Tooltip("비워두면 자동으로 MainCamera를 찾습니다.")]
    public Transform cameraTarget;

    [Tooltip("카메라 이동에 곱해지는 비율 (0이면 고정, 1이면 동일 이동)")]
    public Vector2 parallax = new Vector2(0.3f, 0.0f);

    [Tooltip("사이드스크롤이면 Y 고정 권장")]
    public bool lockY = true;

    private Vector3 startPos;
    private Vector3 cameraStart;

    void OnEnable()
    {
        if (cameraTarget == null && Camera.main != null)
            cameraTarget = Camera.main.transform;

        startPos   = transform.position;
        cameraStart = cameraTarget != null ? cameraTarget.position : Vector3.zero;
    }

    void LateUpdate()
    {
        if (cameraTarget == null) return;

        var delta = cameraTarget.position - cameraStart;
        float yMul = lockY ? 0f : parallax.y;

        transform.position = new Vector3(
            startPos.x + delta.x * parallax.x,
            startPos.y + delta.y * yMul,
            startPos.z
        );
    }
}
