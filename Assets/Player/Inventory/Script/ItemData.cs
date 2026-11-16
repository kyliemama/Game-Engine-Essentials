using UnityEngine;

/// <summary>
/// Define items as assets: Right click in Project -> Create -> Inventory -> Item.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon;
    [TextArea] public string description;

    [Header("Stacking")]
    public int maxStack = 1;

    [Header("Use")]
    public bool consumable;
    public int healAmount;

    [Header("Equip")]
    public bool equippable;
    public GameObject heldPrefab;                 // model shown in the player's hand (can be the same as worldPrefab)
    public Vector3 heldLocalPosition;             // offset from the hand socket
    public Vector3 heldLocalEuler;                // rotation from the hand socket
    public Vector3 heldLocalScale = Vector3.one;

    [Header("Weapon Type")]
    [Tooltip("Picks the attack animation and, unless overridden, the attack shape.")]
    public WeaponType weaponType = WeaponType.OneHandedMelee;
    [Tooltip("Off: the shape follows the weapon type (melee = Slash, Pistol/Rifle/Bow = Ray).")]
    public bool overrideAttackShape;
    public AttackShape attackShapeOverride = AttackShape.Slash;

    [Header("Attack Stats")]
    public int damage = 20;                       // damage dealt per target
    public float range = 2.5f;                    // Slash: hitbox depth (~2.5). Ray: distance (50+ for a gun)
    public float cooldown = 0.5f;
    public float hitDelay = 0.25f;                // seconds from swing start until damage lands
    [Tooltip("Attacks in this weapon's combo. 1 = no combo.")]
    public int comboLength = 1;
    public float finisherMultiplier = 1.5f;       // damage multiplier for the last hit of the combo

    [Header("Slash Hitbox (only used by Slash attacks)")]
    public float slashWidth = 2f;
    public float slashHeight = 1.5f;

    [Header("World")]
    public GameObject worldPrefab; // spawned when dropped (needs an ItemPickup on it)

    /// <summary>The attack this item uses: chosen from its weapon type unless overridden.</summary>
    public AttackShape AttackShapeResolved
    {
        get
        {
            if (overrideAttackShape) return attackShapeOverride;
            switch (weaponType)
            {
                case WeaponType.Pistol:
                case WeaponType.Rifle:
                case WeaponType.Bow:
                    return AttackShape.Ray;
                default:
                    return AttackShape.Slash;
            }
        }
    }

    /// <summary>Returns true if the item was used up (should be removed from inventory).</summary>
    public virtual bool Use(GameObject user)
    {
        if (healAmount > 0)
        {
            Health h = user.GetComponent<Health>();
            if (h) h.Heal(healAmount);
        }
        return consumable;
    }
}