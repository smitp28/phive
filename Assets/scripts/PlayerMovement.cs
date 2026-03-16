using System.Numerics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, ICompanionMovement
{
    public float speed = 5f;
    private UnityEngine.Vector2 moveInput;
    private Animator animator;
    private Rigidbody2D rb;
    public bool isActive = false;
    SpriteRenderer spriteRenderer;

    // ── ICompanionMovement ────────────────────────────────────────
    public UnityEngine.Vector2 Position => rb.position;

    [Header("Sprite")]
    public bool facingRightByDefault = true;

    public void SetMoveDirection(UnityEngine.Vector2 direction)
    {
        if (isActive) return;
        moveInput = direction;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Called by PartyManager.OnMove
    public void OnMove(UnityEngine.Vector2 value)
    {
        if (!isActive) return;
        moveInput = value;
        animator?.SetBool("isMoving", moveInput != UnityEngine.Vector2.zero);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
        animator?.SetBool("isMoving", moveInput != UnityEngine.Vector2.zero);
        FlipSprite();
    }

    void FlipSprite()
    {
        if (moveInput.x > 0f)
            spriteRenderer.flipX = !facingRightByDefault; // facing right
        else if (moveInput.x < 0f)
            spriteRenderer.flipX = facingRightByDefault;  // facing left
    }

    public void StopMovement()
    {
        moveInput = UnityEngine.Vector2.zero;
        rb.linearVelocity = UnityEngine.Vector2.zero;
        animator?.SetBool("isMoving", false);
    }

    void OnDisable()
    {
        StopMovement();
    }
}