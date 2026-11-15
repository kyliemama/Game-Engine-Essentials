using System;
using UnityEngine;

[Serializable]
public struct ItemStack
{
    public ItemData item;
    public int count;
    public bool IsEmpty => item == null || count <= 0;
}

/// <summary>
/// Slot-based inventory with stacking. Put on the Player.
/// Press E while looking at an ItemPickup to collect it.
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("Setup")]
    public int slotCount = 20;
    public Camera playerCamera;

    [Header("Interact")]
    public KeyCode pickupKey = KeyCode.E;
    public float pickupRange = 3f;
    public Transform dropPoint; // optional, defaults to in front of the camera

    public ItemStack[] slots;
    public event Action OnChanged;

    /// <summary>
    /// Set to false by anything that wants to block world pickup (a UI, a cutscene, a menu).
    /// Inventory knows nothing about who sets it.
    /// </summary>
    public bool InteractionEnabled { get; set; } = true;

    [Header("Equipment")]
    public int equippedIndex = -1;
    public event Action OnEquippedChanged;

    public ItemData EquippedItem =>
        InRange(equippedIndex) && !slots[equippedIndex].IsEmpty ? slots[equippedIndex].item : null;

    /// <summary>Equips the item in this slot, or unequips it if it is already equipped.</summary>
    public void EquipAt(int index)
    {
        if (!InRange(index) || slots[index].IsEmpty || !slots[index].item.equippable) return;
        equippedIndex = (equippedIndex == index) ? -1 : index;
        OnEquippedChanged?.Invoke();
        OnChanged?.Invoke();
    }

    public void Unequip()
    {
        if (equippedIndex == -1) return;
        equippedIndex = -1;
        OnEquippedChanged?.Invoke();
        OnChanged?.Invoke();
    }

    void Awake()
    {
        slots = new ItemStack[slotCount];
    }

    void Update()
    {
        if (!InteractionEnabled) return;
        if (Input.GetKeyDown(pickupKey)) TryPickupLookedAt();
    }

    void TryPickupLookedAt()
    {
        if (!playerCamera) return;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, ~0, QueryTriggerInteraction.Collide))
        {
            ItemPickup pickup = hit.collider.GetComponentInParent<ItemPickup>();
            if (pickup) pickup.TryCollect(this);
        }
    }

    /// <summary>Adds items, returns how many could NOT fit.</summary>
    public int AddItem(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return count;

        // 1) fill existing stacks
        for (int i = 0; i < slots.Length && count > 0; i++)
        {
            if (slots[i].item == item && slots[i].count < item.maxStack)
            {
                int space = item.maxStack - slots[i].count;
                int add = Mathf.Min(space, count);
                slots[i].count += add;
                count -= add;
            }
        }

        // 2) use empty slots
        for (int i = 0; i < slots.Length && count > 0; i++)
        {
            if (slots[i].IsEmpty)
            {
                int add = Mathf.Min(item.maxStack, count);
                slots[i].item = item;
                slots[i].count = add;
                count -= add;
            }
        }

        OnChanged?.Invoke();
        return count;
    }

    public bool HasItem(ItemData item, int count = 1)
    {
        int total = 0;
        foreach (var s in slots) if (s.item == item) total += s.count;
        return total >= count;
    }

    public void RemoveAt(int index, int count = 1)
    {
        if (!InRange(index) || slots[index].IsEmpty) return;
        slots[index].count -= count;
        if (slots[index].count <= 0)
        {
            slots[index] = default;
            if (equippedIndex == index)
            {
                equippedIndex = -1;
                OnEquippedChanged?.Invoke();
            }
        }
        OnChanged?.Invoke();
    }

    public void UseAt(int index)
    {
        if (!InRange(index) || slots[index].IsEmpty) return;
        if (slots[index].item.equippable)
        {
            EquipAt(index);
            return;
        }

        bool used = slots[index].item.Use(gameObject);
        if (used) RemoveAt(index, 1);
        else OnChanged?.Invoke();
    }

    public void DropAt(int index, bool wholeStack = false)
    {
        if (!InRange(index) || slots[index].IsEmpty) return;

        ItemStack s = slots[index];
        int amount = wholeStack ? s.count : 1;

        if (s.item.worldPrefab)
        {
            Transform cam = playerCamera ? playerCamera.transform : transform;
            Vector3 pos = dropPoint ? dropPoint.position : cam.position + cam.forward * 1.2f;
            GameObject go = Instantiate(s.item.worldPrefab, pos, Quaternion.identity);
            ItemPickup p = go.GetComponent<ItemPickup>();
            if (p) { p.item = s.item; p.count = amount; }
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb) rb.AddForce(cam.forward * 3f, ForceMode.Impulse);
        }

        RemoveAt(index, amount);
    }

    public void Swap(int a, int b)
    {
        if (!InRange(a) || !InRange(b) || a == b) return;

        // merge if same item
        if (slots[a].item != null && slots[a].item == slots[b].item)
        {
            int space = slots[b].item.maxStack - slots[b].count;
            int move = Mathf.Min(space, slots[a].count);
            slots[b].count += move;
            slots[a].count -= move;
            if (slots[a].count <= 0)
            {
                slots[a] = default;
                if (equippedIndex == a) { equippedIndex = b; OnEquippedChanged?.Invoke(); }
            }
        }
        else
        {
            (slots[a], slots[b]) = (slots[b], slots[a]);
            if (equippedIndex == a) equippedIndex = b;
            else if (equippedIndex == b) equippedIndex = a;
        }
        OnChanged?.Invoke();
    }

    bool InRange(int i) => i >= 0 && i < slots.Length;
}
