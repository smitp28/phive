using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image potraitImage;

    private int dialogueIndex;
    private bool isDialogueActive;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private Coroutine autoProgressCoroutine;

    // CanInteract always returns true so PlayerInteraction
    // allows both starting AND advancing dialogue
    public bool CanInteract() => true;

    public void Interact()
    {
        if (dialogueData == null) return; // fixed: was != null

        if (isDialogueActive)
        {
            if (isTyping)
                SkipTyping();   // first press skips typing animation
            else
                NextLine();     // second press advances to next line
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        potraitImage.sprite = dialogueData.npcPortrait;
        dialoguePanel.SetActive(true);

        ShowLine();
    }

    void ShowLine()
    {
        // Cancel any running coroutines before starting new ones
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoProgressCoroutine != null) StopCoroutine(autoProgressCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(dialogueData.dialogueLines[dialogueIndex]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        // Check if this line should auto-progress
        bool shouldAutoProgress = dialogueData.autoProgressLines != null
            && dialogueIndex < dialogueData.autoProgressLines.Length
            && dialogueData.autoProgressLines[dialogueIndex];

        if (shouldAutoProgress)
            autoProgressCoroutine = StartCoroutine(AutoProgress());
    }

    IEnumerator AutoProgress()
    {
        yield return new WaitForSeconds(dialogueData.autoProgressDelay);
        NextLine();
    }

    void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        isTyping = false;
        dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);

        // Still check for auto-progress after skip
        bool shouldAutoProgress = dialogueData.autoProgressLines != null
            && dialogueIndex < dialogueData.autoProgressLines.Length
            && dialogueData.autoProgressLines[dialogueIndex];

        if (shouldAutoProgress)
            autoProgressCoroutine = StartCoroutine(AutoProgress());
    }

    void NextLine()
    {
        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        isTyping = false;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoProgressCoroutine != null) StopCoroutine(autoProgressCoroutine);

        dialoguePanel.SetActive(false);
        dialogueText.SetText("");
    }
}