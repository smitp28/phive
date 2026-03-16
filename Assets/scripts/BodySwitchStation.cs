using System.Diagnostics;
using UnityEngine;

public class BodySwitchStation : MonoBehaviour, IInteractable
{
    public GameObject switchUI; // assign in inspector

    public void Interact()
    {
        UnityEngine.Debug.Log("Opening panel");

        if (switchUI.activeSelf) return;   // prevent reopening

        switchUI.SetActive(true);
        Time.timeScale = 0f;

        foreach (Transform child in switchUI.transform)
        {
            UnityEngine.Debug.Log("Child inside panel: " + child.name +
                    " | Active: " + child.gameObject.activeSelf);
        }
    }
    public void ClosePanel()
    {
        switchUI.SetActive(false);
        Time.timeScale = 1f;
    }


    public bool CanInteract()
    {
        return true;
    }
}
