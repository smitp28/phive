using UnityEngine;

[CreateAssetMenu(fileName = "NewCompanionStats", menuName = "Companion/Stats")]
public class CompanionStats : ScriptableObject
{
    [Header("Movement")]
    public float speed = 4f;
    public float stopDistance = 1.5f;

    [Header("Detection")]
    public float detectionRadius = 5f;

    [Header("Health")]
    public int maxHealth = 100;
    public float invincibilityTime = 0.5f;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
}