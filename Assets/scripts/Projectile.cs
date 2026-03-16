using UnityEngine;

public class Projectile : MonoBehaviour
{
    float speed;
    int damage;
    Vector2 direction;

    public float lifetime = 4f;

    public void Init(Vector2 dir, float spd, int dmg)
    {
        direction = dir;
        speed     = spd;
        damage    = dmg;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        BossController boss = other.GetComponentInParent<BossController>();
        if (boss != null) { boss.TakeDamage(damage); Destroy(gameObject); return; }

        SkeletonEnemy skeleton = other.GetComponentInParent<SkeletonEnemy>();
        if (skeleton != null) { skeleton.TakeDamage(damage); Destroy(gameObject); }
    }
}