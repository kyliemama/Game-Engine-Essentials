using UnityEngine;
using UnityEngine.Animations.Rigging;
 
/// <summary>
/// Drives Animation Rigging for a first-person character:
///  - Moves an "aim target" to wherever the camera is looking (for Multi-Aim / Two Bone IK constraints)
///  - Blends rig weights (e.g. hands locked to weapon, spine aiming) on and off
///  - PulseAttack() briefly boosts the weapon rig so IK stays solid during the swing/shot
///
/// Setup:
///  1. Package Manager -> install "Animation Rigging".
///  2. On the character root: Add Component -> Rig Builder.
///  3. Create child object "Rig" -> add Rig component; add it to Rig Builder's Rig Layers.
///  4. Under Rig: add child objects with Two Bone IK Constraint (hands on weapon grips)
///     and/or Multi-Aim Constraint (spine/head/weapon aims at aimTarget).
///  5. Create an empty "AimTarget" object (NOT a child of the camera). Assign it below.
///  6. Assign the Rig(s) below, then click Rig Builder -> Build / run.
/// </summary>
public class WeaponRigController : MonoBehaviour
{
    [Header("Aim Target")]
    public Camera playerCamera;
    public Transform aimTarget;
    public float aimDistance = 30f;
    public LayerMask aimMask = ~0;
 
    [Header("Rigs")]
    public Rig handsRig;      // IK holding the weapon
    public Rig aimRig;        // multi-aim for spine/head/weapon
    public float rigBlendSpeed = 10f;
 
    [Header("Hand IK Targets (targets of your Two Bone IK constraints)")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;
 
    Transform leftGrip;
    Transform rightGrip;
 
    [Header("Attack Pulse")]
    public float attackPulseTime = 0.3f;
 
    float handsTarget = 1f;
    float aimTargetWeight = 1f;
    float pulseTimer;
 
    void LateUpdate()
    {
        UpdateAimTarget();
        UpdateGrips();
        UpdateWeights();
    }
 
    void UpdateAimTarget()
    {
        if (!playerCamera || !aimTarget) return;
 
        Transform cam = playerCamera.transform;
        Vector3 point = cam.position + cam.forward * aimDistance;
 
        // Snap to whatever the crosshair is hitting so arms don't over/under-aim close up
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, aimDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.root != transform.root) point = hit.point;
        }
 
        aimTarget.position = point;
    }
 
    void UpdateWeights()
    {
        if (pulseTimer > 0f) pulseTimer -= Time.deltaTime;
 
        float t = rigBlendSpeed * Time.deltaTime;
        if (handsRig) handsRig.weight = Mathf.Lerp(handsRig.weight, handsTarget, t);
 
        // Aim rig fully on while attacking, otherwise follows aimTargetWeight
        float desiredAim = pulseTimer > 0f ? 1f : aimTargetWeight;
        if (aimRig) aimRig.weight = Mathf.Lerp(aimRig.weight, desiredAim, t);
    }
 
    void UpdateGrips()
    {
        if (leftGrip && leftHandTarget)
            leftHandTarget.SetPositionAndRotation(leftGrip.position, leftGrip.rotation);
        if (rightGrip && rightHandTarget)
            rightHandTarget.SetPositionAndRotation(rightGrip.position, rightGrip.rotation);
    }
 
    /// <summary>Called by PlayerEquipment. Pass null for a hand that has no grip point.</summary>
    public void SetGrips(Transform left, Transform right)
    {
        leftGrip = left;
        rightGrip = right;
        SetHandsIK(left != null || right != null);
    }
 
    // --- API ---
    public void PulseAttack() => pulseTimer = attackPulseTime;
    public void SetHandsIK(bool on) => handsTarget = on ? 1f : 0f;     // e.g. off while unarmed / reloading
    public void SetAimWeight(float w) => aimTargetWeight = Mathf.Clamp01(w);
}