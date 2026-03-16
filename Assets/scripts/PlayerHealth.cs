using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats asset")]
    public CompanionStats stats;

    [Header("Reload scene on death — check only on default character")]
    public bool reloadOnDeath = true;

    [Header("Events")]
    public UnityEvent<int> onHealthChanged;
    public UnityEvent onDeath;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    float invincibilityTimer;

    void Start()
    {
        CurrentHealth = stats.maxHealth;
    }

    void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || invincibilityTimer > 0f) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        invincibilityTimer = stats.invincibilityTime;
        onHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0) Die();
    }

    public void TakeDamage(float amount) => TakeDamage((int)amount);

    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(stats.maxHealth, CurrentHealth + amount);
        onHealthChanged?.Invoke(CurrentHealth);
    }

    void Die()
    {
        IsDead = true;
        onDeath?.Invoke();

        if (reloadOnDeath)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            gameObject.SetActive(false);
    }

    public int GetCurrentHealth() => CurrentHealth;
    public int GetMaxHealth() => stats.maxHealth;
}