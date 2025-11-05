using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public string itemID;
    public System.Action onUse;

    [Header("UI References")]
    public Image itemIconImage;
    public Image slotBackgroundImage;

    [Header("Slot Background Sprites")]
    public Sprite defaultSlotSprite;
    public Sprite selectedSlotSprite;

    // Explicitly set by JournalManager
    [HideInInspector]
    public bool isMemorabilia = false;

    private void Start()
    {
        if (itemIconImage == null)
        {
            itemIconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
        }

        if (slotBackgroundImage == null)
        {
            slotBackgroundImage = GetComponent<Image>();
        }

        SetSelected(false);

        Debug.Log($"ItemSlotUI initialized: {itemID}, isMemorabilia: {isMemorabilia}");
    }

    private void SelectItem()
    {
        SoundManager.Instance.PlayClickSound();

        JournalManager.Instance.UpdateItemSelectionHighlight(itemID);

        if (isMemorabilia)
        {
            JournalManager.Instance.UpdateMemorabiliaDetailsPanel(itemID);
            Debug.Log($"Selected memorabilia: {itemID}");
        }
        else
        {
            InventorySystem.Instance.SelectItem(itemID);
            Debug.Log($"Selected inventory item: {itemID}");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SelectItem();
        }
        else if (eventData.button == PointerEventData.InputButton.Right && !isMemorabilia)
        {
            SoundManager.Instance.PlayEquipSound();
            InventorySystem.Instance.EquipItem(itemID);
            Debug.Log($"Equipped item: {itemID}");
        }
    }

    public void SetSelected(bool selected)
    {
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