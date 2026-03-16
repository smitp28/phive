using UnityEngine;

/// <summary>
/// Attach this to the AttackHitbox child GameObject.
/// It forwards trigger events up to PlayerAttack on the parent.
/// </summary>
public class MeleeHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    void Start()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();

        if (playerAttack == null)
            Debug.LogError("MeleeHitbox: Could not find PlayerAttack on parent!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[MeleeHitbox] Hit: {other.gameObject.name} | Tag: {other.tag}");
        if (playerAttack == null)
        {
            Debug.Log("[MeleeHitbox] PlayerAttack is NULL!");
            return;
        }
        if (!other.CompareTag("Enemy"))
        {
            Debug.Log($"[MeleeHitbox] {other.gameObject.name} is not tagged Enemy, skipping.");
            return;
        }
        playerAttack.OnHitboxTrigger(other);
    }
}