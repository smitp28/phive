using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour, ICompanionAttack
{
    [Header("Stats asset")]
    public CompanionStats stats;

    [Header("References")]
    public Animator animator;
    public Collider2D meleeHitbox;

    [Header("Animation Timing")]
    public float meleeWindupTime = 0.15f;
    public float meleeActiveTime = 0.2f;

    float lastAttackTime = -999f;
    bool isAttacking;

    static readonly int AnimAttack = Animator.StringToHash("attack");

    // ── ICompanionAttack ──────────────────────────────────────────
    public float AttackRange => stats.attackRange;
    public bool IsReady => Time.time >= lastAttackTime + stats.attackCooldown && !isAttacking;

    public void TriggerAttack(Transform target)
    {
        if (!IsReady) return;
        StartCoroutine(AttackRoutine());
    }

    // ── Lifecycle ─────────────────────────────────────────────────
    void Start() => meleeHitbox.enabled = false;

    void OnDisable()
    {
        isAttacking = false;
        meleeHitbox.enabled = false;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || !IsReady) return;
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetTrigger(AnimAttack);

        yield return new WaitForSeconds(meleeWindupTime);
        meleeHitbox.enabled = true;
        yield return new WaitForSeconds(meleeActiveTime);
        meleeHitbox.enabled = false;

        yield return new WaitForSeconds(0.1f);
        isAttacking = false;
    }

    public void OnHitboxTrigger(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        BossController boss = other.GetComponentInParent<BossController>();
        if (boss != null) { boss.TakeDamage((int)stats.attackDamage); return; }

        SkeletonEnemy skeleton = other.GetComponentInParent<SkeletonEnemy>();
        if (skeleton != null) skeleton.TakeDamage((int)stats.attackDamage);
    }
}