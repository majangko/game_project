using UnityEngine;

public class WarningEffect : MonoBehaviour
{
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private SpriteRenderer sprite;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (sprite)
        {
            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
            sprite.color = new Color(1f, 0.2f, 0.2f, alpha);
        }

        if (timer >= duration)
            Destroy(gameObject);
    }
}
