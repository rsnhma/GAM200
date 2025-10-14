using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public string itemID;
    public System.Action onUse;

    [Header("UI References")]
    public Image itemIconImage;
    public GameObject selectionHighlight; // Optional: visual feedback for selected item

    private void Start()
    {
        // Get icon reference if not assigned
        if (itemIconImage == null)
        {
            itemIconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
        }

        // Disable highlight by default
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click: Select and view item details
            InventorySystem.Instance.SelectItem(itemID);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click: Equip item to hands
            InventorySystem.Instance.EquipItem(itemID);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(selected);
        }
        else
        {
            // Optional: Change color/scale if no highlight object exists
            // itemIconImage.color = selected ? Color.yellow : Color.white;
        }
    }
}