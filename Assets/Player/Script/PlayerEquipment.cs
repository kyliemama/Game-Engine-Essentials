using UnityEngine;

/// <summary>
/// Shows the equipped inventory item in the player model's hand and applies its attack stats.
/// Put on the Player.
///
/// handSocket: an empty object parented to the model's hand bone (e.g. RightHand).
/// Held prefabs may contain children named "LeftHandGrip" / "RightHandGrip";
/// if found, the IK hand targets in WeaponRigController snap to them.
/// </summary>
public class PlayerEquipment : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public Transform handSocket;
    public PlayerAttack attack;
    public WeaponRigController rigController; // optional
    public Animator animator;                 // optional

    [Header("Unarmed Punch")]
    public int unarmedDamage = 8;
    public float unarmedRange = 1.8f;
    public float unarmedCooldown = 0.45f;
    public float unarmedHitDelay = 0.18f;
    public float unarmedSlashWidth = 0.9f;
    public float unarmedSlashHeight = 0.8f;
    [Tooltip("Punches in the unarmed combo (jab, jab, finisher). 1 = no combo.")]
    public int unarmedComboLength = 3;
    public float unarmedFinisherMultiplier = 1.5f;

    [Header("Animator (optional)")]
    public string armedBoolParameter = "Armed";
    public string weaponTypeParameter = "WeaponType"; // Float parameter used by the Attack blend tree

    /// <summary>The weapon type of whatever is equipped (Unarmed when nothing is).</summary>
    public WeaponType CurrentWeaponType { get; private set; } = WeaponType.Unarmed;

    /// <summary>True while a weapon or item is equipped.</summary>
    public bool IsArmed => heldItem != null;

    /// <summary>Raised whenever the equipped weapon type changes (including back to Unarmed).</summary>
    public event System.Action<WeaponType> OnWeaponTypeChanged;

    GameObject heldObject;
    ItemData heldItem;
    bool initialised;   // so the unarmed stats are applied on the very first Refresh

    void Start()
    {
        if (!inventory) inventory = GetComponent<Inventory>();
        if (!attack) attack = GetComponent<PlayerAttack>();

        inventory.OnEquippedChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (inventory) inventory.OnEquippedChanged -= Refresh;
    }

    void Refresh()
    {
        ItemData item = inventory.EquippedItem;
        if (initialised && item == heldItem && (item == null || heldObject != null)) return;
        initialised = true;

        ClearHeld();
        heldItem = item;

        if (item == null)
        {
            ApplyStats(unarmedDamage, unarmedRange, unarmedCooldown, unarmedHitDelay,
                       AttackShape.Slash, unarmedSlashWidth, unarmedSlashHeight,
                       unarmedComboLength, unarmedFinisherMultiplier);
            if (rigController) rigController.SetGrips(null, null);
        }
        else
        {
            SpawnHeld(item);
            ApplyStats(item.damage, item.range, item.cooldown, item.hitDelay,
                       item.AttackShapeResolved, item.slashWidth, item.slashHeight,
                       item.comboLength, item.finisherMultiplier);
        }

        if (animator && !string.IsNullOrEmpty(armedBoolParameter))
            animator.SetBool(armedBoolParameter, item != null);

        WeaponType type = item != null ? item.weaponType : WeaponType.Unarmed;
        bool typeChanged = type != CurrentWeaponType;
        CurrentWeaponType = type;

        if (animator && !string.IsNullOrEmpty(weaponTypeParameter))
            animator.SetFloat(weaponTypeParameter, (float)type);

        if (typeChanged) OnWeaponTypeChanged?.Invoke(type);
    }

    void SpawnHeld(ItemData item)
    {
        if (!item.heldPrefab || !handSocket) return;

        heldObject = Instantiate(item.heldPrefab, handSocket);
        Transform t = heldObject.transform;
        t.localPosition = item.heldLocalPosition;
        t.localRotation = Quaternion.Euler(item.heldLocalEuler);
        t.localScale = item.heldLocalScale;

        // Make it a purely visual object
        foreach (Collider c in heldObject.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (Rigidbody rb in heldObject.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
        foreach (ItemPickup p in heldObject.GetComponentsInChildren<ItemPickup>()) Destroy(p);

        if (rigController)
        {
            Transform left = FindDeep(t, "LeftHandGrip");
            Transform right = FindDeep(t, "RightHandGrip");
            rigController.SetGrips(left, right);
        }
    }

    void ClearHeld()
    {
        if (heldObject) Destroy(heldObject);
        heldObject = null;
    }

    void ApplyStats(int damage, float range, float cooldown, float hitDelay,
                    AttackShape shape, float slashWidth, float slashHeight,
                    int comboLength, float finisherMultiplier)
    {
        if (!attack) return;
        attack.damage = damage;
        attack.range = range;
        attack.cooldown = cooldown;
        attack.hitDelay = hitDelay;
        attack.attackShape = shape;
        attack.slashWidth = slashWidth;
        attack.slashHeight = slashHeight;
        attack.comboLength = comboLength;
        attack.finisherMultiplier = finisherMultiplier;
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>())
            if (child.name == name) return child;
        return null;
    }
}