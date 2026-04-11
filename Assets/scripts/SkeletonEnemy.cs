using System.Collections;
using UnityEngine.Events;
using UnityEngine;

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
    public HealthBar healthBar;

    private Transform player;
    private int currentHealth;
    private float damageTimer;
    private bool isDead;

    private static readonly int AnimMoving = Animator.StringToHash("isMoving");

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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0f)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(contactDamage);
                damageTimer = damageCooldown;
            }
        }
    }

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