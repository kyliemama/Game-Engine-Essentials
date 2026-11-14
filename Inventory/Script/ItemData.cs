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

    [Header("Equipped Attack Stats")]
    public WeaponType weaponType = WeaponType.OneHandedMelee; // decides which attack animation plays
    public float hitDelay = 0.25f;                // seconds from swing start until damage lands
    public int damage = 20;
    public float range = 2.5f;                    // ~2.5 melee, 50+ for a gun
    public float cooldown = 0.5f;

    [Header("World")]
    public GameObject worldPrefab; // spawned when dropped (needs an ItemPickup on it)

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