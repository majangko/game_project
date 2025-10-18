using UnityEngine;

public class SealProjectile : MonoBehaviour
{
    private float speed;
    private GameObject sealPrefab;
    private float sealDuration;
    private GameObject caster;

    private SpriteRenderer sr;            // 🔹 캐싱

    public interface ISealEffect
    {
        void Activate(Damageable target, float duration);
    }

    public void Init(float spd, GameObject seal, float dur, GameObject owner)
    {
        speed = spd;
        sealPrefab = seal;
        sealDuration = dur;
        caster = owner;

        // 🔹 SpriteRenderer 가져오기
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        // 🔹 발사 방향에 따라 flip
        if (sr != null)
        {
            // 오른쪽(+speed) → flipX = true, 왼쪽(-speed) → false
            sr.flipX = (speed > 0);
        }

        Destroy(gameObject, 5f); // 안전장치
    }

    void Update()
    {
        transform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == caster) return;

        Damageable d = other.GetComponent<Damageable>();
        if (d)
        {
            Vector3 pos = other.transform.position;
            GameObject seal = Instantiate(sealPrefab, pos, Quaternion.identity);

            ISealEffect effect = seal.GetComponent<ISealEffect>();
            if (effect != null)
                effect.Activate(d, sealDuration);

            Destroy(gameObject);
        }
    }
}
