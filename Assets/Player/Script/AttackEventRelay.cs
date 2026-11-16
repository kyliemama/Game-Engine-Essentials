using UnityEngine;

/// <summary>
/// Optional. Put on the model (the object with the Animator) if you use Animation Events.
/// Animation Events can only call methods on the object that has the Animator, so this passes
/// them up to PlayerAttack on the Player.
///
/// AnimationEvents hit timing: add "HitStart" and "HitEnd" events to each attack clip.
/// A one-off "OnAttackHit" event also works (a single hit check at that frame).
/// </summary>
public class AttackEventRelay : MonoBehaviour
{
    PlayerAttack attack;

    void Awake() => attack = GetComponentInParent<PlayerAttack>();

    public void HitStart() { if (attack) attack.HitStart(); }
    public void HitEnd() { if (attack) attack.HitEnd(); }
    public void OnAttackHit() { if (attack) attack.OnAttackHit(); }
}
