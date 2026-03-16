using UnityEngine;

/// <summary>
/// Attach this to the Boss's MeleeHitbox child GameObject.
/// Forwards incoming damage to the BossController on the parent.
/// </summary>
public class BossDamageReceiver : MonoBehaviour
{
    private BossController bossController;

    void Start()
    {
        bossController = GetComponentInParent<BossController>();
        if (bossController == null)
            Debug.LogError("[BossDamageReceiver] Could not find BossController on parent!");
    }

    public void TakeDamage(int amount)
    {
        if (bossController != null)
            bossController.TakeDamage(amount);
    }
}