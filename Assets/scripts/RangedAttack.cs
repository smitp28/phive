using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedAttack : MonoBehaviour, ICompanionAttack
{
    [Header("Stats asset")]
    public CompanionStats stats;

    [Header("References")]
    public Animator animator;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Animation Timing")]
    public float castWindupTime = 0.2f;
    public float projectileSpeed = 8f;

    float lastAttackTime = -999f;
    bool isCasting;

    static readonly int AnimCast = Animator.StringToHash("cast");

    // ── ICompanionAttack ──────────────────────────────────────────
    public float AttackRange => stats.attackRange;
    public bool IsReady => Time.time >= lastAttackTime + stats.attackCooldown && !isCasting;

    public void TriggerAttack(Transform target)
    {
        if (!IsReady) return;
        StartCoroutine(CastRoutine(target));
    }

    // ── Lifecycle ─────────────────────────────────────────────────
    void OnDisable()
    {
        isCasting = false;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || !IsReady) return;
        StartCoroutine(CastRoutine(null));
    }

    IEnumerator CastRoutine(Transform target)
    {
        isCasting = true;
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(castWindupTime);

        SpawnProjectile(target);

        yield return new WaitForSeconds(0.1f);
        isCasting = false;
    }

    void SpawnProjectile(Transform target)
    {
        if (projectilePrefab == null || firePoint == null) return;

        Vector2 dir = target != null
            ? ((Vector2)target.position - (Vector2)firePoint.position).normalized
            : GetAimDirection();

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.Init(dir, projectileSpeed, (int)stats.attackDamage);
    }

    Vector2 GetAimDirection()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos  = Camera.main.ScreenToWorldPoint(
                                new Vector3(screenPos.x, screenPos.y, 0f));
        return ((Vector2)worldPos - (Vector2)firePoint.position).normalized;
    }
}