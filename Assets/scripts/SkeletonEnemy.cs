using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Skeleton Mini-Enemy — Top-Down 2D
/// 
/// Spawned by the Boss. Chases the player, deals contact damage, and dies.
/// 
/// SETUP INSTRUCTIONS:
///  1. Attach this to your Skeleton prefab.
///  2. Assign Animator and Rigidbody2D.
///  3. Tag Skeleton with "Enemy", Player with "Player".
///  4. Add a Collider2D to the skeleton (not trigger) for physics collision.
///  5. Optionally add a second child Collider2D set as Trigger for damage detection.
/// </summary>
public class SkeletonEnemy : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;

    [Header("Stats")]
    public int maxHealth = 50;
    public float moveSpeed = 2f;
    public int contactDamage = 10;
    public float damageCooldown = 1f;

    [Header("Events")]
    public UnityEvent<int> onHealthChanged;

    [Header("Detection")]
    public float detectionRange = 8f;
    public HealthBar healthBar; // Fixed: was "Healthbar"

    // ── Private ───────────────────────────────────────────────────────────────

    private Transform player;
    private int currentHealth;
    private float damageTimer;
    private bool isDead;

    private static readonly int AnimMoving = Animator.StringToHash("isMoving");

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        damageTimer -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectionRange)
            ChasePlayer();
        else
            Idle();
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    void ChasePlayer()
    {
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        float dir = player.position.x - transform.position.x;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * (dir < 0 ? -1 : 1),
            transform.localScale.y,
            transform.localScale.z
        );

        if (animator != null) animator.SetBool(AnimMoving, true);
    }

    void Idle()
    {
        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool(AnimMoving, false);
    }

    // ── Contact Damage ────────────────────────────────────────────────────────

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            Debug.Log("Skeleton hit player! Attempting to deal damage...");
            if (ph != null)
            {
                ph.TakeDamage(contactDamage);
                damageTimer = damageCooldown;
            }
        }
    }

    // ── Damage & Death ────────────────────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth);
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
            StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool(AnimMoving, false);

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}