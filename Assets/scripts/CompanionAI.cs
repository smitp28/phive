using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class CompanionAI : MonoBehaviour
{
    [Header("Stats asset — drag SO here")]
    public CompanionStats stats;

    [Header("References")]
    public Transform followTarget;
    public LayerMask enemyLayer;

    ICompanionMovement movement;
    ICompanionAttack attack;      // ← interface, works for both melee and ranged
    PlayerHealth health;
    Transform currentEnemy;

    void Awake()
    {
        health   = GetComponent<PlayerHealth>();
        movement = GetComponent<ICompanionMovement>();
        attack   = GetComponent<ICompanionAttack>(); // picks up whichever attack script is on this character

        if (movement == null)
            Debug.LogError($"[CompanionAI] No ICompanionMovement on {name}.");
        if (attack == null)
            Debug.LogError($"[CompanionAI] No ICompanionAttack on {name}.");
    }

    void OnEnable()  => movement?.SetMoveDirection(Vector2.zero);
    void OnDisable() => movement?.SetMoveDirection(Vector2.zero);

    void FixedUpdate()
    {
        if (health.IsDead || movement == null || stats == null) return;

        Debug.Log($"[{name}] FixedUpdate running, isEnabled: {enabled}");

        if (PartyManager.Instance != null)
            followTarget = PartyManager.Instance.ActiveMember?.transform;

        if (followTarget == transform)
        {
            movement.SetMoveDirection(Vector2.zero);
            return;
        }

        currentEnemy = GetClosestEnemy();

        if (currentEnemy != null)
            ChaseAndAttack(currentEnemy);
        else
            FollowPlayer();
    }

    Transform GetClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            movement.Position, stats.detectionRadius, enemyLayer);

        Debug.Log($"[{name}] Enemies found in radius: {hits.Length}");
        Transform closest     = null;
        float     closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            var enemyHealth = hit.GetComponent<PlayerHealth>();
            if (enemyHealth != null && enemyHealth.IsDead) continue;

            float d = Vector2.Distance(movement.Position, hit.transform.position);
            if (d < closestDist) { closestDist = d; closest = hit.transform; }
        }

        return closest;
    }

    void ChaseAndAttack(Transform enemy)
    {
        // Use attack script's own range instead of stats.attackRange
        float range = attack != null ? attack.AttackRange : stats.attackRange;
        float dist  = Vector2.Distance(movement.Position, enemy.position);

        if (dist > range)
        {
            Vector2 dir = ((Vector2)enemy.position - movement.Position).normalized;
            movement.SetMoveDirection(dir);
        }
        else
        {
            movement.SetMoveDirection(Vector2.zero);
            attack?.TriggerAttack(enemy);
        }
    }

    void FollowPlayer()
    {
        if (followTarget == null) { movement.SetMoveDirection(Vector2.zero); return; }

        float dist = Vector2.Distance(movement.Position, followTarget.position);

        if (dist > stats.stopDistance)
        {
            Vector2 dir = ((Vector2)followTarget.position - movement.Position).normalized;
            movement.SetMoveDirection(dir);
        }
        else
        {
            movement.SetMoveDirection(Vector2.zero);
        }
    }

    public void SetPlayer(Transform target) => followTarget = target;

    void OnDrawGizmosSelected()
    {
        if (stats == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.detectionRadius);
        Gizmos.color = Color.yellow;
        float range = attack != null ? attack.AttackRange : stats.attackRange;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}