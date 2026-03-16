using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Cinemachine;
public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }
    PlayerInteraction activeInteraction;

    public List<GameObject> partyMembers;
    public GameObject ActiveMember { get; private set; }
    public CinemachineCamera virtualCamera;

    public float switchCooldown = 2f;
    float cooldown;
    int activeIndex = 0;

    // Cached components of active character
    PlayerMovement activeMovement;
    PlayerAttack activeAttack;
    RangedAttack activeRangedAttack;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetActiveCharacter(0);
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

        if (ActiveMember != null)
        {
            var health = ActiveMember.GetComponent<PlayerHealth>();
            if (health != null && health.IsDead)
                SwitchToNextAlive();
        }
    }

    // ── Input callbacks (on this GameObject's PlayerInput) ────────

    public void OnMove(InputValue value)
    {
        activeMovement?.OnMove(value.Get<UnityEngine.Vector2>());
    }

    public void OnAttack(InputValue value)
    {
        if (activeAttack != null) activeAttack.OnAttack(value);
        else activeRangedAttack?.OnAttack(value);
    }

    public void OnInteract(InputValue value)
{
    if (!value.isPressed) return;
    activeInteraction?.Interact(); // calls the new public method
}

    // ── Switch ────────────────────────────────────────────────────

    public void SwitchToCharacter(int index)
    {
        if (cooldown > 0f) return;
        if (index < 0 || index >= partyMembers.Count) return;

        activeIndex = index;
        SetActiveCharacter(activeIndex);
        cooldown = switchCooldown;
    }

    void SetActiveCharacter(int index)
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            PlayerMovement player     = partyMembers[i].GetComponent<PlayerMovement>();
            CompanionAI    ai         = partyMembers[i].GetComponent<CompanionAI>();
            PlayerInteraction interact = partyMembers[i].GetComponent<PlayerInteraction>();
            Rigidbody2D    rb         = partyMembers[i].GetComponent<Rigidbody2D>();

            if (rb != null) rb.linearVelocity = Vector2.zero;

            bool isActive = i == index;

            if (player != null)
            {
                player.StopMovement();
                player.isActive = isActive;
                player.enabled  = true;
            }

            if (ai != null)
            {
                ai.enabled = !isActive;
                if (!isActive)
                    ai.SetPlayer(partyMembers[index].transform);
            }

            if (interact != null)
                interact.enabled = isActive;

            if (isActive)
            {
                ActiveMember       = partyMembers[i];
                activeMovement     = partyMembers[i].GetComponent<PlayerMovement>();
                activeAttack       = partyMembers[i].GetComponent<PlayerAttack>();
                activeRangedAttack = partyMembers[i].GetComponent<RangedAttack>();
                activeInteraction  = partyMembers[i].GetComponent<PlayerInteraction>(); 

                if (virtualCamera != null)
                    {
                        virtualCamera.Target.TrackingTarget = partyMembers[i].transform;
                        Debug.Log($"Camera now following: {partyMembers[i].name}");
                    }
                    else
                    {
                        Debug.Log("virtualCamera is null!");
                    }
            }
        }
    }

    void SwitchToNextAlive()
    {
        for (int i = 1; i < partyMembers.Count; i++)
        {
            int next = (activeIndex + i) % partyMembers.Count;
            var health = partyMembers[next].GetComponent<PlayerHealth>();
            if (health != null && !health.IsDead)
            {
                SwitchToCharacter(next);
                return;
            }
        }
        Debug.Log("All party members dead — Game Over");
    }
}