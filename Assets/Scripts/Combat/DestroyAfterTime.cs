using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifetime = 0.3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
