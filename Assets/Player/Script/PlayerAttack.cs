using UnityEngine;

/// <summary>
/// Left-click attack. Works for melee (short range) or hitscan (long range).
/// Damage is applied by an Animation Event named "OnAttackHit" so it lines up
/// with the swing/shot. If you don't want to use an animation event, tick
/// 'applyDamageImmediately'.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Animator animator;
    public WeaponRigController rigController; // optional

    [Header("Attack")]
    public int damage = 20;
    public float range = 2.5f;          // ~2.5 for melee, 50+ for hitscan gun
    public float hitRadius = 0.3f;      // 0 = thin raycast, >0 = more forgiving spherecast
    public float cooldown = 0.5f;
    public LayerMask hitMask = ~0;
    public bool applyDamageImmediately = false;
    public bool allowHoldToAttack = true;

    float nextAttackTime;

    static readonly int AttackTrigger = Animator.StringToHash("Attack");

    void Update()
    {
        if (InventoryUI.IsOpen) return;

        bool pressed = allowHoldToAttack ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        if (pressed && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + cooldown;
            StartAttack();
        }
    }

    void StartAttack()
    {
        if (animator) animator.SetTrigger(AttackTrigger);
        if (rigController) rigController.PulseAttack();
        if (applyDamageImmediately) OnAttackHit();
    }

    // Call this from an Animation Event on the attack clip
    public void OnAttackHit()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        bool didHit = hitRadius > 0f
            ? Physics.SphereCast(ray, hitRadius, out hit, range, hitMask, QueryTriggerInteraction.Ignore)
            : Physics.Raycast(ray, out hit, range, hitMask, QueryTriggerInteraction.Ignore);

        if (!didHit) return;

        // Ignore ourselves
        if (hit.transform.root == transform.root) return;

        Health health = hit.collider.GetComponentInParent<Health>();
        if (health) health.TakeDamage(damage);
    }
}