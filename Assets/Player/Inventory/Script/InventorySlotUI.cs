using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One slot. Prefab layout: root (Image + this script) with child "Icon" (Image)
/// and child "Count" (TextMeshPro).
/// Left click = use, Right click = drop one (Shift+Right = drop stack), drag to swap/merge.
/// </summary>
public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TMP_Text countText;
    public GameObject equippedMarker; // e.g. a green border or "E" badge, child of the slot

    InventoryUI ui;
    int index;

    // drag ghost
    static GameObject dragGhost;

    public void Init(InventoryUI owner, int i)
    {
        ui = owner;
        index = i;
    }

    public void SetStack(ItemStack stack, bool equipped = false)
    {
        bool has = !stack.IsEmpty;
        if (equippedMarker) equippedMarker.SetActive(has && equipped);
        icon.enabled = has;
        icon.sprite = has ? stack.item.icon : null;
        countText.text = has && stack.count > 1 ? stack.count.ToString() : "";
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.dragging) return;
        if (e.button == PointerEventData.InputButton.Left) ui.OnSlotLeftClick(index);
        else if (e.button == PointerEventData.InputButton.Right)
            ui.OnSlotRightClick(index, Input.GetKey(KeyCode.LeftShift));
    }

    public void OnPointerEnter(PointerEventData e) => ui.ShowTooltip(index);
    public void OnPointerExit(PointerEventData e) => ui.ShowTooltip(-1);

    public void OnBeginDrag(PointerEventData e)
    {
        if (!icon.enabled) return;
        dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
        dragGhost.transform.SetParent(ui.GetComponentInParent<Canvas>().transform, false);
        var img = dragGhost.GetComponent<Image>();
        img.sprite = icon.sprite;
        img.raycastTarget = false;
        ((RectTransform)dragGhost.transform).sizeDelta = ((RectTransform)icon.transform).rect.size;
    }

    public void OnDrag(PointerEventData e)
    {
        if (dragGhost) dragGhost.transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (dragGhost) Destroy(dragGhost);
    }

    public void OnDrop(PointerEventData e)
    {
        var from = e.pointerDrag ? e.pointerDrag.GetComponent<InventorySlotUI>() : null;
        if (from != null) ui.OnSlotDropped(from.index, index);
    }
}