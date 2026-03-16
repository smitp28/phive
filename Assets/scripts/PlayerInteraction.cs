using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private List<IInteractable> interactables = new List<IInteractable>();

    private Vector2 lastMoveDirection = Vector2.down; // default facing

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (input != Vector2.zero)
            lastMoveDirection = input.normalized;
    }

    void OnEnable()
{
    interactables.Clear();

    Collider2D col = GetComponent<Collider2D>();
    if (col == null) return;

    Collider2D[] hits = Physics2D.OverlapBoxAll(
        transform.position,
        col.bounds.size,
        0f);

    foreach (var hit in hits)
    {
        if (hit.gameObject == gameObject) continue;
        IInteractable interactable = hit.GetComponent<IInteractable>();
        if (interactable != null && !interactables.Contains(interactable))
            interactables.Add(interactable);
    }
}

    public void Interact()
    {
        Debug.Log("INTERACT BUTTON PRESSED");
        IInteractable best = GetBestInteractable();

        if (best != null && best.CanInteract())
        {
            Debug.Log("CALLING INTERACT()");
            best.Interact();
        }
        else
        {
            Debug.Log("NO BEST INTERACTABLE");
        }
    }

    private IInteractable GetBestInteractable()
    {
        if (interactables.Count > 0)
            return interactables[0];

        return null;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        UnityEngine.Debug.Log("TRIGGER ENTERED with: " + other.name);
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            UnityEngine.Debug.Log("INTERACTABLE FOUND");
            interactables.Add(interactable);
        }
         else
        {
            UnityEngine.Debug.Log("NO IInteractable ON OBJECT");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactables.Remove(interactable);
        }
    }
}
