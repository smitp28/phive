using System.Collections;
using UnityEngine;

/// <summary>
/// Robot Boss Controller — Top-Down 2D
/// 
/// States: Idle → Chase → MeleeAttack / Summon → repeat
/// 
/// SETUP INSTRUCTIONS:
///  1. Attach this script to your Boss GameObject.
///  2. Assign the Animator, Rigidbody2D, and skeletonPrefab in the Inspector.
///  3. Create an empty child GameObject called "MeleeHitbox", add a CircleCollider2D
///     (set as Trigger), and assign it to meleeHitbox. This is your invisible melee hit zone.
///  4. Set the Boss layer to "Enemy" and Player layer to "Player".
///  5. Tag your Player GameObject as "Player".
/// </summary>
public class BossController : MonoBehaviour
{
    // ── Inspector Fields ─────────────────────────────────────────────────────

    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;
    public Transform skeletonSpawnPoint;   // Where skeletons spawn (assign a child Transform)
    public GameObject skeletonPrefab;       // Drag your Skeleton prefab here
    public Collider2D meleeHitbox;          // Child GameObject with CircleCollider2D (Trigger)

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseRange = 10f;          // Distance at which boss starts chasing
    public float stopDistance = 1.5f;       // Distance at which boss stops to attack

    [Header("Melee Attack")]
    public float meleeCooldown = 2f;
    public float meleeWindupTime = 0.3f;    // Pause before hitbox activates
    public float meleeActiveTime = 0.25f;   // How long hitbox stays on
    public int meleeDamage = 20;

    [Header("Summon")]
    public float summonCooldown = 8f;
    public int skeletonsPerSummon = 2;
    public float summonSpreadRadius = 2f;

    [Header("Boss Stats")]
    public int maxHealth = 500;

    // ── Private State ─────────────────────────────────────────────────────────

    private enum BossState { Idle, Chase, MeleeAttack, Summon, Dead }
    private BossState currentState = BossState.Idle;

    private Transform player;
    private int currentHealth;
    private float meleeTimer;
    private float summonTimer;
    private bool isActing; // Prevents overlapping actions

    // ── Animator Parameter Hashes (performance) ───────────────────────────────
    private static readonly int AnimMoving   = Animator.StringToHash("isMoving");
    private static readonly int AnimAttack   = Animator.StringToHash("meleeAttack");
    private static readonly int AnimSummon   = Animator.StringToHash("summon");
    private static readonly int AnimDead     = Animator.StringToHash("isDead");

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        currentHealth = maxHealth;
        meleeHitbox.enabled = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (currentState == BossState.Dead || player == null) return;

        meleeTimer  -= Time.deltaTime;
        summonTimer -= Time.deltaTime;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isActing)
            DecideState(distToPlayer);

        HandleMovement(distToPlayer);
    }

    // ── State Decision ────────────────────────────────────────────────────────

    void DecideState(float dist)
    {
        // Priority: Summon > Melee > Chase > Idle
        if (summonTimer <= 0f && dist <= chaseRange)
        {
            StartCoroutine(SummonRoutine());
            return;
        }

        if (dist <= stopDistance && meleeTimer <= 0f)
        {
            StartCoroutine(MeleeRoutine());
            return;
        }

        currentState = dist <= chaseRange ? BossState.Chase : BossState.Idle;
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    void HandleMovement(float dist)
    {
        if (currentState == BossState.Chase && dist > stopDistance)
        {
            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
            FlipTowardsPlayer();
            animator.SetBool(AnimMoving, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool(AnimMoving, false);
        }
    }

    void FlipTowardsPlayer()
    {
        if (player == null) return;
        float dir = player.position.x - transform.position.x;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * (dir < 0 ? -1 : 1),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    // ── Melee Attack ──────────────────────────────────────────────────────────

    IEnumerator MeleeRoutine()
    {
        isActing = true;
        currentState = BossState.MeleeAttack;
        meleeTimer = meleeCooldown;

        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger(AnimAttack);

        // Wind-up pause (animation charging)
        yield return new WaitForSeconds(meleeWindupTime);

        // Activate the invisible hitbox
        meleeHitbox.enabled = true;
        yield return new WaitForSeconds(meleeActiveTime);
        meleeHitbox.enabled = false;

        // Short recovery before next action
        yield return new WaitForSeconds(0.3f);

        isActing = false;
    }

    // Detect player inside melee hitbox
    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == BossState.MeleeAttack && other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(meleeDamage);
        }
    }

    // ── Summon ────────────────────────────────────────────────────────────────

    IEnumerator SummonRoutine()
    {
        isActing = true;
        currentState = BossState.Summon;
        summonTimer = summonCooldown;

        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger(AnimSummon);

        yield return new WaitForSeconds(0.6f); // Wait for summon animation

        for (int i = 0; i < skeletonsPerSummon; i++)
        {
            Vector2 spawnOffset = Random.insideUnitCircle * summonSpreadRadius;
            Vector2 spawnPos = skeletonSpawnPoint != null
                ? (Vector2)skeletonSpawnPoint.position + spawnOffset
                : (Vector2)transform.position + spawnOffset;

            Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.4f);
        isActing = false;
    }

    // ── Damage & Death ────────────────────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= amount;
        Debug.Log($"[Boss] Took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            StartCoroutine(DieRoutine());
    }

    public int GetCurrentHealth() => currentHealth;

    IEnumerator DieRoutine()
    {
        currentState = BossState.Dead;
        isActing = true;
        rb.linearVelocity = Vector2.zero;

        animator.SetBool(AnimDead, true);

        // Wait for death animation before destroying
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}