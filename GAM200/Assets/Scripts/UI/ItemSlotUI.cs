using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public string itemID;
    public System.Action onUse; // Action to perform when item is used (null for memorabilia)
    [Header("UI References")]
    public Image itemIconImage; // The actual item icon
    public Image slotBackgroundImage; // The slot background that changes sprites
    [Header("Slot Background Sprites")]
    public Sprite defaultSlotSprite; // Drag default slot sprite here
    public Sprite selectedSlotSprite; // Drag selected slot sprite here
    private bool isMemorabilia = false;
    private void Start()
    {
        // Determine if this is memorabilia based on whether it has a use action
        isMemorabilia = (onUse == null);
        // Get icon reference if not assigned
        if (itemIconImage == null)
        {
            itemIconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
        }
        // Get background image if not assigned (it should be on this GameObject)
        if (slotBackgroundImage == null)
        {
            slotBackgroundImage = GetComponent<Image>();
        }
        // Set to default sprite initially
        SetSelected(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // Left click to select and view details
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SelectItem();
        }
        // Right click to use/equip (only for inventory items, not memorabilia)
        else if (eventData.button == PointerEventData.InputButton.Right && !isMemorabilia)
        {
            // For inventory items, use the inventory system's equip method
            InventorySystem.Instance.EquipItem(itemID);
            Debug.Log($"Equipped item: {itemID}");
        }
    }
    private void SelectItem()
    {
        // Update selection highlight for all items
        JournalManager.Instance.UpdateItemSelectionHighlight(itemID);
        // Update the appropriate details panel based on item type
        if (isMemorabilia)
        {
            JournalManager.Instance.UpdateMemorabiliaDetailsPanel(itemID);
            Debug.Log($"Selected memorabilia: {itemID}");
        }
        else
        {
            // For inventory items, use the inventory system's select method
            InventorySystem.Instance.SelectItem(itemID);
            Debug.Log($"Selected inventory item: {itemID}");
        }
    }
    public void SetSelected(bool selected)
    {
        // Change background sprite based on selection state
        if (slotBackgroundImage != null)
        {
            if (selected && selectedSlotSprite != null)
            {
                slotBackgroundImage.sprite = selectedSlotSprite;
            }
            else if (!selected && defaultSlotSprite != null)
            {
                slotBackgroundImage.sprite = defaultSlotSprite;
            }
        }
    }
}