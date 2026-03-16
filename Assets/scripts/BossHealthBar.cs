using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BossHealthBar — drives a UI Slider to show boss HP.
/// 
/// SETUP:
///  1. Create a UI Slider in your Canvas. Set Min=0, Max=1, Interactable=OFF.
///  2. Attach this script to the Slider GameObject.
///  3. Assign the bossController reference in the Inspector (drag the Boss).
///  4. Optionally assign a Text/TMP label for numeric display.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    public BossController bossController;
    public Slider slider;
    public TMPro.TextMeshProUGUI hpLabel; // Optional — remove if not using TMP

    void Update()
    {
        if (bossController == null)
        {
            // Boss is dead, hide bar
            gameObject.SetActive(false);
            return;
        }

        // NOTE: BossController exposes currentHealth and maxHealth as public
        // for the bar to read. Make sure they are public in BossController.
        float ratio = (float)bossController.GetCurrentHealth() / bossController.maxHealth;
        slider.value = ratio;

        if (hpLabel != null)
            hpLabel.text = $"{bossController.GetCurrentHealth()} / {bossController.maxHealth}";
    }
}
