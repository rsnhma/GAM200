using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance;

    [Header("Item Grid (Inventory)")]
    public GameObject itemEntryPrefab;
    public Transform inventoryContentParent;

    [Header("Memorabilia Grid")]
    public GameObject memorabiliaEntryPrefab;
    public Transform memorabiliaContentParent;

    [Header("Inventory Item Details")]
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;

    [Header("Memorabilia Item Details")]
    public Image memorabiliaitemIcon;
    public TextMeshProUGUI memorabiliaitemName;
    public TextMeshProUGUI memorabiliaitemDescription;

    [Header("Item Data ScriptableObjects")]
    public List<ItemData> allItemData = new List<ItemData>();

    private Dictionary<string, Sprite> itemIconCache = new Dictionary<string, Sprite>();
    private Dictionary<string, ItemSlotUI> itemSlots = new Dictionary<string, ItemSlotUI>();
    private Dictionary<string, ItemSlotUI> memorabiliaSlots = new Dictionary<string, ItemSlotUI>();
    private HashSet<string> collectedMemorabilia = new HashSet<string>();
    private Dictionary<string, ItemData> itemDataLookup = new Dictionary<string, ItemData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        BuildItemDataLookup();
        PrecacheAllItemIcons();
        ClearItemDetails();
        ClearMemorabiliaDetails();
    }

    private void BuildItemDataLookup()
    {
        foreach (ItemData data in allItemData)
        {
            if (!string.IsNullOrEmpty(data.itemID))
            {
                itemDataLookup[data.itemID] = data;

                // IMPORTANT: Cache the icon from ItemData
                if (data.itemIcon != null)
                {
                    itemIconCache[data.itemID] = data.itemIcon;
                    Debug.Log($"Registered ItemData with icon: {data.itemID}");
                }
                else
                {
                    Debug.LogWarning($"ItemData {data.itemID} is missing an icon!");
                }
            }
        }
    }

    public void UpdateJournalTab(string itemID, System.Action useAction = null, GameObject itemPrefab = null)
    {
        if (itemSlots.ContainsKey(itemID))
        {
            Debug.Log($"Item {itemID} already exists in journal");
            return;
        }

        GameObject entry = Instantiate(itemEntryPrefab, inventoryContentParent);

        ItemSlotUI slotUI = entry.GetComponent<ItemSlotUI>();
        slotUI.itemID = itemID;
        slotUI.onUse = useAction;
        slotUI.isMemorabilia = false;

        Image icon = entry.transform.Find("ItemIcon").GetComponent<Image>();

        Sprite itemSprite = GetItemSprite(itemID);
        if (itemSprite != null && icon != null)
        {
            icon.sprite = itemSprite;
            icon.enabled = true;
            Debug.Log($"Icon sprite assigned: {itemSprite.name} for item: {itemID}");
        }
        else
        {
            Debug.LogWarning($"Could not find sprite for item: {itemID}");
        }

        itemSlots[itemID] = slotUI;
        InventorySystem.Instance.AddItem(itemID, useAction, itemPrefab);

        // Notify that a new inventory item was added
        if (JournalNotificationManager.Instance != null)
        {
            JournalNotificationManager.Instance.NotifyNewInventoryItem(itemID);
        }
    }

    public void AddMemorabilia(string itemID)
    {
        if (memorabiliaSlots.ContainsKey(itemID))
        {
            Debug.Log($"Memorabilia {itemID} already exists in journal");
            return;
        }

        collectedMemorabilia.Add(itemID);

        GameObject entry = Instantiate(memorabiliaEntryPrefab, memorabiliaContentParent);

        ItemSlotUI slotUI = entry.GetComponent<ItemSlotUI>();
        slotUI.itemID = itemID;
        slotUI.onUse = null;
        slotUI.isMemorabilia = true;

        Image icon = entry.transform.Find("ItemIcon").GetComponent<Image>();

        Sprite itemSprite = GetItemSprite(itemID);
        if (itemSprite != null && icon != null)
        {
            icon.sprite = itemSprite;
            icon.enabled = true;
            Debug.Log($"Memorabilia icon assigned: {itemSprite.name} for item: {itemID}");
        }
        else
        {
            Debug.LogWarning($"Could not find sprite for memorabilia: {itemID}");
        }

        memorabiliaSlots[itemID] = slotUI;

        // NOTIFY that a new memorabilia was added
        if (JournalNotificationManager.Instance != null)
        {
            JournalNotificationManager.Instance.NotifyNewMemorabilia(itemID);
        }
    }

    public bool HasMemorabilia(string itemID)
    {
        return collectedMemorabilia.Contains(itemID);
    }

    // Update INVENTORY item details panel
    public void UpdateItemDetailsPanel(string itemID)
    {
        if (itemDataLookup.ContainsKey(itemID))
        {
            ItemData data = itemDataLookup[itemID];

            if (itemIcon != null && data.itemIcon != null)
            {
                itemIcon.sprite = data.itemIcon;
                itemIcon.enabled = true;
            }

            if (itemName != null)
            {
                itemName.text = data.itemName;
            }

            if (itemDescription != null)
            {
                itemDescription.text = data.itemDescription;
            }

            Debug.Log($"Updated inventory details panel for: {itemID}");
        }
        else
        {
            Debug.LogWarning($"No ItemData found for: {itemID}");

            if (itemName != null)
                itemName.text = itemID;

            if (itemDescription != null)
                itemDescription.text = "No description available.";

            Sprite itemSprite = GetItemSprite(itemID);
            if (itemIcon != null && itemSprite != null)
            {
                itemIcon.sprite = itemSprite;
                itemIcon.enabled = true;
            }
        }
    }

    // Update MEMORABILIA item details panel
    public void UpdateMemorabiliaDetailsPanel(string itemID)
    {
        if (itemDataLookup.ContainsKey(itemID))
        {
            ItemData data = itemDataLookup[itemID];

            if (memorabiliaitemIcon != null && data.itemIcon != null)
            {
                memorabiliaitemIcon.sprite = data.itemIcon;
                memorabiliaitemIcon.enabled = true;
            }

            if (memorabiliaitemName != null)
            {
                memorabiliaitemName.text = data.itemName;
            }

            if (memorabiliaitemDescription != null)
            {
                memorabiliaitemDescription.text = data.itemDescription;
            }

            Debug.Log($"Updated memorabilia details panel for: {itemID}");
        }
        else
        {
            Debug.LogWarning($"No ItemData found for memorabilia: {itemID}");

            if (memorabiliaitemName != null)
                memorabiliaitemName.text = itemID;

            if (memorabiliaitemDescription != null)
                memorabiliaitemDescription.text = "No description available.";

            Sprite itemSprite = GetItemSprite(itemID);
            if (memorabiliaitemIcon != null && itemSprite != null)
            {
                memorabiliaitemIcon.sprite = itemSprite;
                memorabiliaitemIcon.enabled = true;
            }
        }
    }

    public void UpdateItemSelectionHighlight(string selectedItemID)
    {
        foreach (var kvp in itemSlots)
        {
            kvp.Value.SetSelected(kvp.Key == selectedItemID);
        }

        foreach (var kvp in memorabiliaSlots)
        {
            kvp.Value.SetSelected(kvp.Key == selectedItemID);
        }
    }

    // Only Inventory item gets removed as it can be used
    public void RemoveItemFromUI(string itemID)
    {
        if (itemSlots.ContainsKey(itemID))
        {
            Destroy(itemSlots[itemID].gameObject);
            itemSlots.Remove(itemID);
            Debug.Log($"Removed {itemID} from inventory UI");
        }
    }

    public void HideItemDetailsPanel()
    {
        ClearItemDetails();
    }

    public void HideMemorabiliaDetailsPanel()
    {
        ClearMemorabiliaDetails();
    }

    private void ClearItemDetails()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (itemName != null)
        {
            itemName.text = "";
        }

        if (itemDescription != null)
        {
            itemDescription.text = "Select an item to view details";
        }
    }

    private void ClearMemorabiliaDetails()
    {
        if (memorabiliaitemIcon != null)
        {
            memorabiliaitemIcon.sprite = null;
            memorabiliaitemIcon.enabled = false;
        }

        if (memorabiliaitemName != null)
        {
            memorabiliaitemName.text = "";
        }

        if (memorabiliaitemDescription != null)
        {
            memorabiliaitemDescription.text = "Select a memorabilia to view details";
        }
    }

    private Sprite GetItemSprite(string itemID)
    {
        // First check if we have it cached
        if (itemIconCache.ContainsKey(itemID))
        {
            Debug.Log($"Found cached icon for: {itemID}");
            return itemIconCache[itemID];
        }

        // Try to get from ItemData lookup (should already be cached in BuildItemDataLookup)
        if (itemDataLookup.ContainsKey(itemID))
        {
            Sprite icon = itemDataLookup[itemID].itemIcon;
            if (icon != null)
            {
                itemIconCache[itemID] = icon;
                Debug.Log($"Found icon from ItemData for: {itemID}");
                return icon;
            }
        }

        // Fallback: try to find from Collectible components in scene
        Collectible[] allCollectibles = Resources.FindObjectsOfTypeAll<Collectible>();
        foreach (Collectible col in allCollectibles)
        {
            if (col.itemID == itemID && col.itemIcon != null)
            {
                itemIconCache[itemID] = col.itemIcon;
                Debug.Log($"Found icon from Collectible for: {itemID}");
                return col.itemIcon;
            }
        }

        Debug.LogWarning($"No sprite found for: {itemID}");
        return null;
    }

    private void PrecacheAllItemIcons()
    {
        // Cache icons from Collectibles in scene (for backward compatibility)
        Collectible[] allCollectibles = Resources.FindObjectsOfTypeAll<Collectible>();

        Debug.Log($"Pre-caching icons from {allCollectibles.Length} collectibles found in scene");

        foreach (Collectible col in allCollectibles)
        {
            if (!string.IsNullOrEmpty(col.itemID) && col.itemIcon != null)
            {
                if (!itemIconCache.ContainsKey(col.itemID))
                {
                    itemIconCache[col.itemID] = col.itemIcon;
                    Debug.Log($"Pre-cached icon from Collectible: {col.itemID}");
                }
            }
        }

        Debug.Log($"Total icons cached: {itemIconCache.Count}");
    }

    public void ClearAllJournalData()
    {
        itemSlots.Clear();
        memorabiliaSlots.Clear();
        collectedMemorabilia.Clear();
        ClearItemDetails();
        ClearMemorabiliaDetails();

        Debug.Log("All journal data cleared");
    }
}