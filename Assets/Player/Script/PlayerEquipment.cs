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

    [Header("Unarmed Stats")]
    public int unarmedDamage = 5;
    public float unarmedRange = 2f;
    public float unarmedCooldown = 0.6f;

    [Header("Animator (optional)")]
    public string armedBoolParameter = "Armed";

    GameObject heldObject;
    ItemData heldItem;

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
        if (item == heldItem && (item == null || heldObject != null)) return;

        ClearHeld();
        heldItem = item;

        if (item == null)
        {
            ApplyStats(unarmedDamage, unarmedRange, unarmedCooldown);
            if (rigController) rigController.SetGrips(null, null);
        }
        else
        {
            SpawnHeld(item);
            ApplyStats(item.damage, item.range, item.cooldown);
        }

        if (animator && !string.IsNullOrEmpty(armedBoolParameter))
            animator.SetBool(armedBoolParameter, item != null);
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

    void ApplyStats(int damage, float range, float cooldown)
    {
        if (!attack) return;
        attack.damage = damage;
        attack.range = range;
        attack.cooldown = cooldown;
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>())
            if (child.name == name) return child;
        return null;
    }
}
