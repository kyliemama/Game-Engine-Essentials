using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Left-click attack. Slash mode hits everything inside a box directly in front of the character.
/// Ray mode fires a single ray or spherecast (for guns and bows).
///
/// Damage only lands while the attack animation is actually in its "hit" part:
///  - AnimationWindow (default): the part of the attack state between Hit Window Start and End.
///  - AnimationEvents: between HitStart and HitEnd Animation Events in the clip.
///  - Timer: a single check Hit Delay seconds after the swing starts.
/// Each target is damaged once per swing.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    public enum AttackOrigin { Camera, Body }
    public enum HitTiming { Timer, AnimationWindow, AnimationEvents }

    [Header("References")]
    public Camera playerCamera;
    public Animator animator;
    public WeaponRigController rigController; // optional

    [Header("Attack")]
    public AttackShape attackShape = AttackShape.Slash;
    public int damage = 20;             // damage dealt to every target hit
    public float range = 2.5f;          // slash: depth of the hitbox. ray: how far the ray travels
    public float cooldown = 0.5f;
    public LayerMask hitMask = ~0;
    public bool allowHoldToAttack = true;

    [Header("Hit Timing")]
    [Tooltip("AnimationWindow: damage only lands while the attack animation is inside the window below.\n" +
             "AnimationEvents: damage lands between HitStart and HitEnd events in the clip.\n" +
             "Timer: one check after Hit Delay seconds.")]
    public HitTiming hitTiming = HitTiming.AnimationWindow;
    [Tooltip("AnimationWindow: how far through the attack animation (0 to 1) the hit begins.")]
    [Range(0f, 1f)] public float hitWindowStart = 0.3f;
    [Tooltip("AnimationWindow: how far through the attack animation (0 to 1) the hit ends.")]
    [Range(0f, 1f)] public float hitWindowEnd = 0.6f;
    [Tooltip("Timer mode only. Set automatically from the equipped item.")]
    public float hitDelay = 0.25f;

    [Header("Animation Flow")]
    [Tooltip("The exact name of the Attack state in your Animator.")]
    public string attackStateName = "Attack";
    [Tooltip("The next attack can start once the current attack animation is this far through (0 to 1). " +
             "Higher = animations play more fully. Lower = snappier chaining.")]
    [Range(0.3f, 1f)] public float chainAt = 0.7f;
    public int animatorLayer = 0;

    [Header("Perspective")]
    [Tooltip("Body = third person: the slash starts at the character and faces the way the character faces. Camera = first person.")]
    public AttackOrigin attackOrigin = AttackOrigin.Body;
    [Tooltip("Body mode: where the slash starts, relative to the player's feet (about chest height).")]
    public Vector3 bodyOffset = new Vector3(0f, 1f, 0f);

    [Header("Combo")]
    [Tooltip("Attacks in the combo. 1 = no combo. The Animator float 'AttackVariant' is set to 0, 1, 2...")]
    public int comboLength = 1;
    [Tooltip("Seconds after an attack before the combo starts over.")]
    public float comboResetTime = 1.5f;
    [Tooltip("Damage multiplier for the last attack of the combo.")]
    public float finisherMultiplier = 1.5f;
    public string variantParameter = "AttackVariant";

    [Header("Slash Hitbox")]
    [Tooltip("Optional. A BoxCollider on a child of the PLAYER that defines the slash area (third person). " +
             "Resize and move it with the Scene handles. If set, the width, height, range and offsets below are ignored for slashes.")]
    public BoxCollider slashHitbox;
    public float slashWidth = 2f;
    public float slashHeight = 1.5f;
    [Tooltip("Gap between the character (or camera) and where the hitbox begins.")]
    public float slashStartDistance = 0.2f;
    [Tooltip("First person only: shifts the box relative to the camera (right, up, forward).")]
    public Vector3 slashOffset = new Vector3(0f, -0.3f, 0f);

    [Header("Ray Mode")]
    public float hitRadius = 0.3f;      // 0 = thin raycast, >0 = more forgiving spherecast

    [Header("Debug")]
    public bool logAttacks = false;     // prints what each attack does in the Console
    public bool drawLastStrike = true;  // shows a red shape in the Scene view (and Game view with Gizmos on)

    /// <summary>Raised each time an attack starts (used by CombatStance).</summary>
    public event Action OnAttackStarted;

    float nextAttackTime;
    int comboIndex = -1;
    float lastAttackTime = -999f;
    float pendingMultiplier = 1f;

    bool swingActive;        // a swing has started and has not finished its hit window yet
    bool windowOpen;         // damage may be dealt right now
    bool rayFiredThisSwing;
    float swingStartTime;
    bool sawAttackState;      // the Animator has reached the attack state during this swing
    bool windowEverOpened;
    bool awaitingRestart;     // swing began while the previous attack was still playing in the same state
    float lastObservedT;

    readonly Collider[] overlapBuffer = new Collider[32];
    readonly HashSet<Health> alreadyHit = new HashSet<Health>();

    Vector3 lastBoxCenter;
    Quaternion lastBoxRotation;
    Vector3 lastBoxSize;
    float lastStrikeTime = -999f;

    static readonly int AttackTrigger = Animator.StringToHash("Attack");

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>();
        if (!playerCamera) playerCamera = Camera.main;

        if (!animator)
            Debug.LogWarning("PlayerAttack: no Animator found, so attack animations won't play. Assign the model's Animator.", this);

        if (slashHitbox)
        {
            slashHitbox.isTrigger = true;                // it is only a shape, it must never block anything
            slashHitbox.gameObject.layer = 2;            // "Ignore Raycast"

            if (attackOrigin == AttackOrigin.Body && !slashHitbox.transform.IsChildOf(transform))
                Debug.LogWarning("PlayerAttack: the Slash Hitbox is not a child of the Player, so it will not follow the " +
                                 "character. Move it under the Player (or clear the field).", this);
        }
    }

    void Update()
    {
        // The hit window is driven by the animation, so keep it running even if a menu opens mid-swing
        if (hitTiming == HitTiming.AnimationWindow) UpdateHitWindow();
        if (windowOpen) DoHitCheck();

        if (InventoryUI.IsOpen) return;

        // No attacks while the settings panel is open.
        if (PersistentHUD.SettingsOpen) return;

        bool pressed = allowHoldToAttack ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        if (pressed && CanStartAttack())
        {
            nextAttackTime = Time.time + cooldown;
            StartAttack();
        }
    }

    // ------------------------------------------------------------ starting an attack

    /// <summary>
    /// An attack may start when the cooldown is over AND the current attack animation has played
    /// far enough. This stops a new swing cutting the last one off halfway.
    /// </summary>
    bool CanStartAttack()
    {
        if (Time.time < nextAttackTime) return false;
        if (!animator || string.IsNullOrEmpty(attackStateName)) return true;

        // Still blending into an attack: wait for it
        if (animator.IsInTransition(animatorLayer))
        {
            AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(animatorLayer);
            if (next.IsName(attackStateName)) return false;
        }

        // Mid-attack: only chain once the animation has played far enough
        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(animatorLayer);
        if (current.IsName(attackStateName)) return current.normalizedTime >= chainAt;

        return true;
    }

    void StartAttack()
    {
        // Attack sound from AudioManager's "Attack" entry.
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Attack");

        UpdateCombo();
        BeginSwing();

        if (logAttacks)
            Debug.Log($"PlayerAttack: attack started (combo {comboIndex}, {attackShape}, {hitTiming}, {DamageThisHit} damage)");

        if (animator)
        {
            if (!string.IsNullOrEmpty(variantParameter)) animator.SetFloat(variantParameter, comboIndex);
            animator.SetTrigger(AttackTrigger);
        }
        if (rigController) rigController.PulseAttack();
        OnAttackStarted?.Invoke();

        // Without an Animator there is no animation to follow, so fall back to the timer
        if (hitTiming == HitTiming.Timer || !animator) Invoke(nameof(OnAttackHit), hitDelay);
    }

    void UpdateCombo()
    {
        int length = Mathf.Max(1, comboLength);
        bool expired = Time.time - lastAttackTime > comboResetTime;

        comboIndex = (expired || comboIndex < 0) ? 0 : (comboIndex + 1) % length;
        lastAttackTime = Time.time;

        bool finisher = length > 1 && comboIndex == length - 1;
        pendingMultiplier = finisher ? finisherMultiplier : 1f;
    }

    int DamageThisHit => Mathf.RoundToInt(damage * pendingMultiplier);

    void BeginSwing()
    {
        alreadyHit.Clear();
        rayFiredThisSwing = false;
        windowOpen = false;
        swingActive = true;

        swingStartTime = Time.time;
        sawAttackState = false;
        windowEverOpened = false;

        // A chained attack starts while the previous attack animation is still playing. Remember that,
        // so we don't mistake the old animation's time for the new one's.
        lastObservedT = -1f;
        awaitingRestart = animator && !string.IsNullOrEmpty(attackStateName) &&
                          animator.GetCurrentAnimatorStateInfo(animatorLayer).IsName(attackStateName);
    }

    // ------------------------------------------------------------ hit window

    void UpdateHitWindow()
    {
        if (!swingActive || !animator || string.IsNullOrEmpty(attackStateName)) return;

        AnimatorStateInfo info;
        bool inAttackState;
        bool blendingIntoAttack = false;

        if (animator.IsInTransition(animatorLayer) &&
            animator.GetNextAnimatorStateInfo(animatorLayer).IsName(attackStateName))
        {
            // Blending into the new attack: follow the new attack, not the one it is leaving
            info = animator.GetNextAnimatorStateInfo(animatorLayer);
            inAttackState = true;
            blendingIntoAttack = true;
        }
        else
        {
            info = animator.GetCurrentAnimatorStateInfo(animatorLayer);
            inAttackState = info.IsName(attackStateName);
        }

        if (blendingIntoAttack) awaitingRestart = false;

        if (!inAttackState)
        {
            SetWindow(false);

            if (sawAttackState)
            {
                // The attack animation has finished
                swingActive = false;
                if (!windowEverOpened)
                    Debug.LogWarning($"PlayerAttack: the attack animation ended before reaching the hit window " +
                                     $"({hitWindowStart:0.00} to {hitWindowEnd:0.00}), so nothing was hit. " +
                                     "Lower Hit Window Start, or check the Attack state's exit transition.", this);
            }
            else if (Time.time - swingStartTime > 0.5f)
            {
                // The Animator never entered a state with this name. Don't leave the player unable to deal damage.
                swingActive = false;
                Debug.LogWarning($"PlayerAttack: no Animator state named '{attackStateName}' was found, so the hit " +
                                 "window could not follow the animation. Set 'Attack State Name' to the exact name of " +
                                 "your Attack state. Falling back to a single hit check for this swing.", this);
                DoHitCheck();
            }
            return;
        }

        // This swing began while the previous attack was still playing in the same state. Wait until that
        // animation has actually restarted (its time drops back down), otherwise we would be reading the
        // OLD attack's time and the hit window would never open.
        if (awaitingRestart)
        {
            float now = info.normalizedTime;
            if (lastObservedT >= 0f && now < lastObservedT - 0.01f) awaitingRestart = false;
            lastObservedT = now;

            if (awaitingRestart)
            {
                SetWindow(false);

                if (Time.time - swingStartTime > 0.4f)
                {
                    swingActive = false;
                    Debug.LogWarning("PlayerAttack: the attack animation did not restart for a chained attack. In the Animator, " +
                                     "select the transition 'Any State -> Attack' and tick 'Can Transition To Self'. " +
                                     "Using a single hit check for this swing.", this);
                    DoHitCheck();
                }
                return;
            }
        }

        sawAttackState = true;

        float t = info.normalizedTime;
        bool open = t >= hitWindowStart && t <= hitWindowEnd;
        if (open) windowEverOpened = true;
        SetWindow(open);

        if (t > hitWindowEnd) swingActive = false;   // this swing's hit part is over
    }

    void SetWindow(bool open)
    {
        if (open == windowOpen) return;
        windowOpen = open;

        if (logAttacks)
        {
            if (open) Debug.Log("PlayerAttack: hit window opened");
            else Debug.Log($"PlayerAttack: hit window closed, {alreadyHit.Count} target(s) hit this swing");
        }
    }

    /// <summary>Animation Event (AnimationEvents mode): the hit part of the swing begins.</summary>
    public void HitStart()
    {
        if (hitTiming == HitTiming.AnimationEvents) SetWindow(true);
    }

    /// <summary>Animation Event (AnimationEvents mode): the hit part of the swing ends.</summary>
    public void HitEnd()
    {
        if (hitTiming == HitTiming.AnimationEvents)
        {
            SetWindow(false);
            swingActive = false;
        }
    }

    /// <summary>A single hit check. Used by Timer mode, or by a one-off "OnAttackHit" Animation Event.</summary>
    public void OnAttackHit() => DoHitCheck();

    void DoHitCheck()
    {
        if (attackShape == AttackShape.Slash)
        {
            SlashHit();
        }
        else if (!rayFiredThisSwing)
        {
            rayFiredThisSwing = true;
            RayHit();
        }
    }

    // ------------------------------------------------------------ slash (box in front)

    void SlashHit()
    {
        GetSlashBox(out Vector3 center, out Quaternion rotation, out Vector3 size);

        lastBoxCenter = center;
        lastBoxRotation = rotation;
        lastBoxSize = size;
        lastStrikeTime = Time.time;

        int count = Physics.OverlapBoxNonAlloc(
            center, size * 0.5f, overlapBuffer, rotation, hitMask, QueryTriggerInteraction.Ignore);

        DamageOverlaps(count);
    }

    // ------------------------------------------------------------ applying damage

    void DamageOverlaps(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];

            // Never hit ourselves
            if (col.transform.IsChildOf(transform)) continue;

            Health health = col.GetComponentInParent<Health>();
            if (!health || alreadyHit.Contains(health)) continue; // one hit per target per swing

            alreadyHit.Add(health);
            health.TakeDamage(DamageThisHit);

            if (logAttacks)
                Debug.Log($"PlayerAttack: hit {health.name} for {DamageThisHit}");
        }
    }

    void GetSlashBox(out Vector3 center, out Quaternion rotation, out Vector3 size)
    {
        if (slashHitbox)
        {
            Transform t = slashHitbox.transform;
            rotation = t.rotation;
            center = t.TransformPoint(slashHitbox.center);

            Vector3 s = slashHitbox.size;
            Vector3 scale = t.lossyScale;
            size = new Vector3(s.x * Mathf.Abs(scale.x), s.y * Mathf.Abs(scale.y), s.z * Mathf.Abs(scale.z));
            return;
        }

        if (attackOrigin == AttackOrigin.Body)
        {
            // Third person: a box directly in front of the character, facing the way it faces
            rotation = transform.rotation;
            Vector3 bodyCenter = bodyOffset + Vector3.forward * (slashStartDistance + range * 0.5f);
            center = transform.TransformPoint(bodyCenter);
            size = new Vector3(slashWidth, slashHeight, range);
            return;
        }

        Transform origin = playerCamera ? playerCamera.transform : transform;
        rotation = origin.rotation;

        Vector3 localCenter = slashOffset + Vector3.forward * (slashStartDistance + range * 0.5f);
        center = origin.position + rotation * localCenter;
        size = new Vector3(slashWidth, slashHeight, range);
    }

    // ------------------------------------------------------------ ray (guns and bows)

    void RayHit()
    {
        Transform origin = playerCamera ? playerCamera.transform : transform;
        Vector3 start = origin.position;

        // Third person: start the shot level with the character, so the camera's ray doesn't hit the player's own body
        if (attackOrigin == AttackOrigin.Body && playerCamera)
            start += origin.forward * Mathf.Max(0f, Vector3.Dot(transform.position + bodyOffset - origin.position, origin.forward));

        Ray ray = new Ray(start, origin.forward);
        RaycastHit hit;

        bool didHit = hitRadius > 0f
            ? Physics.SphereCast(ray, hitRadius, out hit, range, hitMask, QueryTriggerInteraction.Ignore)
            : Physics.Raycast(ray, out hit, range, hitMask, QueryTriggerInteraction.Ignore);

        if (logAttacks) Debug.Log(didHit ? "PlayerAttack: ray hit " + hit.collider.name : "PlayerAttack: ray hit nothing");

        if (!didHit) return;
        if (hit.collider.transform.IsChildOf(transform)) return;

        Health health = hit.collider.GetComponentInParent<Health>();
        if (health) health.TakeDamage(DamageThisHit);
    }

    // ------------------------------------------------------------ gizmos

    void OnDrawGizmos()
    {
        if (!drawLastStrike || attackShape != AttackShape.Slash) return;
        if (Time.time - lastStrikeTime > 0.15f) return;

        DrawBox(lastBoxCenter, lastBoxRotation, lastBoxSize, new Color(1f, 0.2f, 0.2f, 0.35f), true);
    }

    void OnDrawGizmosSelected()
    {
        if (attackShape != AttackShape.Slash) return;

        GetSlashBox(out Vector3 center, out Quaternion rotation, out Vector3 size);
        DrawBox(center, rotation, size, Color.cyan, false);
    }

    static void DrawBox(Vector3 center, Quaternion rotation, Vector3 size, Color color, bool filled)
    {
        Matrix4x4 previous = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
        Gizmos.color = color;
        if (filled) Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = previous;
    }
}