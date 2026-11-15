using UnityEngine;

/// <summary>
/// Automatic combat mode. Put on the Player.
///  - Attacking (left click) enters combat mode straight away.
///  - Every attack (and optionally every hit you take) restarts the timer.
///  - After 'exitDelay' seconds with no combat you relax back to normal movement.
/// Drives the Animator bool "InCombat".
/// </summary>
public class CombatStance : MonoBehaviour
{
    [Header("References (auto-found on this object if left empty)")]
    public Animator animator;     // the model's Animator
    public PlayerAttack attack;
    public Health health;         // optional: taking damage keeps combat active
    public Inventory inventory;   // optional: only needed for 'stayInCombatWhileArmed'

    [Header("Timing")]
    [Tooltip("Seconds without attacking (or being hit) before the stance relaxes.")]
    public float exitDelay = 5f;
    public bool damageKeepsCombatActive = true;

    [Header("Weapons")]
    [Tooltip("If ticked, the stance stays on for as long as a weapon is equipped.")]
    public bool stayInCombatWhileArmed = false;

    [Header("Animator")]
    public string inCombatParameter = "InCombat";

    public bool InCombat => active || (stayInCombatWhileArmed && WeaponEquipped);

    bool active;
    float combatTimer;

    bool WeaponEquipped => inventory && inventory.EquippedItem != null;

    void Start()
    {
        if (!attack) attack = GetComponent<PlayerAttack>();
        if (!health) health = GetComponent<Health>();
        if (!inventory) inventory = GetComponent<Inventory>();

        if (attack) attack.OnAttackStarted += EnterCombat;
        if (health && damageKeepsCombatActive) health.onDamaged.AddListener(OnDamaged);

        Apply();
    }

    void OnDestroy()
    {
        if (attack) attack.OnAttackStarted -= EnterCombat;
        if (health) health.onDamaged.RemoveListener(OnDamaged);
    }

    void Update()
    {
        if (active)
        {
            combatTimer -= Time.deltaTime;
            if (combatTimer <= 0f) active = false;
        }

        Apply();
    }

    void OnDamaged(int amount) => EnterCombat();

    /// <summary>Enter combat mode (or restart the timer). Other scripts can call this too.</summary>
    public void EnterCombat()
    {
        active = true;
        combatTimer = exitDelay;
        Apply(); // set the Animator bool right away so the attack returns to the combat stance
    }

    /// <summary>Leave combat mode immediately.</summary>
    public void ExitCombat()
    {
        active = false;
        Apply();
    }

    void Apply()
    {
        if (animator && !string.IsNullOrEmpty(inCombatParameter))
            animator.SetBool(inCombatParameter, InCombat);
    }
}
