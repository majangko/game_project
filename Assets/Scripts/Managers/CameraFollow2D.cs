using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector2 offset;
    public float smooth = 5f;
    void LateUpdate()
    {
        if (!target) return;
        var p = transform.position;
        var t = new Vector3(target.position.x + offset.x, target.position.y + offset.y, p.z);
        transform.position = Vector3.Lerp(p, t, Time.deltaTime * smooth);
    }
}
