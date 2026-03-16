using UnityEngine;

public class SwitchMenuUI : MonoBehaviour
{
    public PartyManager partyManager;
    public GameObject panel;

    public void SwitchTo(int index)
    {
        if (partyManager == null)
        {
            UnityEngine.Debug.LogError("PartyManager not assigned!");
            return;
        }
        UnityEngine.Debug.Log("Switching to character index: " + index);
        partyManager.SwitchToCharacter(index);
        CloseMenu();
    }

    public void CloseMenu()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
