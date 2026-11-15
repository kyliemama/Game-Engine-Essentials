using UnityEngine;

/// <summary>
/// Put on any world object (needs a Collider) to make it collectable with E.
/// For drops, also add a Rigidbody to the prefab.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public int count = 1;

    public void TryCollect(Inventory inventory)
    {
        int leftover = inventory.AddItem(item, count);
        if (leftover <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            count = leftover; // inventory was full, leave the rest in the world
        }
    }
}
