using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Toggle with Tab (or I). Builds the slot grid from slotPrefab.
/// Put on a Canvas object. 'panel' is the grid parent (give it a Grid Layout Group).
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("References")]
    public Inventory inventory;
    public GameObject panelRoot;      // the whole inventory window (toggled on/off)
    public Transform slotParent;      // object with Grid Layout Group
    public InventorySlotUI slotPrefab;

    [Header("Tooltip (optional)")]
    public GameObject tooltipRoot;
    public TMP_Text tooltipName;
    public TMP_Text tooltipDescription;

    [Header("Equipped HUD (optional, place outside panelRoot to always show)")]
    public TMP_Text equippedLabel;

    [Header("Keys")]
    public KeyCode toggleKey = KeyCode.Tab;
    public KeyCode altToggleKey = KeyCode.I;

    InventorySlotUI[] slotUIs;

    void Start()
    {
        slotUIs = new InventorySlotUI[inventory.slots.Length];
        for (int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i] = Instantiate(slotPrefab, slotParent);
            slotUIs[i].Init(this, i);
        }

        inventory.OnChanged += Refresh;
        SetOpen(false);
        Refresh();
    }

    void OnDestroy()
    {
        if (inventory) inventory.OnChanged -= Refresh;
        IsOpen = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(altToggleKey))
            SetOpen(!IsOpen);
    }

    public void SetOpen(bool open)
    {
        IsOpen = open;
        if (inventory) inventory.InteractionEnabled = !open;
        panelRoot.SetActive(open);
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
        if (!open) ShowTooltip(-1);
    }

    public void Refresh()
    {
        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].SetStack(inventory.slots[i], i == inventory.equippedIndex);

        if (equippedLabel)
        {
            ItemData eq = inventory.EquippedItem;
            equippedLabel.text = eq ? "Equipped: " + eq.itemName : "";
        }
    }

    // Slot callbacks
    public void OnSlotLeftClick(int i) => inventory.UseAt(i);
    public void OnSlotRightClick(int i, bool shift) => inventory.DropAt(i, shift);
    public void OnSlotDropped(int from, int to) => inventory.Swap(from, to);

    public void ShowTooltip(int i)
    {
        if (!tooltipRoot) return;
        if (i < 0 || inventory.slots[i].IsEmpty) { tooltipRoot.SetActive(false); return; }

        ItemData item = inventory.slots[i].item;
        tooltipRoot.SetActive(true);
        if (tooltipName) tooltipName.text = item.itemName;
        if (tooltipDescription) tooltipDescription.text = item.description;
    }
}