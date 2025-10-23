using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance;

    [Header("Tabs")]
    public GameObject ContentPanel1;
    public GameObject ContentPanel2;
    public GameObject itemsTabPanel;

    [Header("Item Grid")]
    public GameObject itemEntryPrefab;
    public Transform itemsContentParent;

    [Header("Item Details")]
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;

    [Header("Item Data ScriptableObjects")]
    public List<ItemData> allItemData = new List<ItemData>(); // Drag all ItemData SOs here in Inspector

    // Dictionary to cache item icons 
    private Dictionary<string, Sprite> itemIconCache = new Dictionary<string, Sprite>();

    // Dictionary to track item slot UI elements
    private Dictionary<string, ItemSlotUI> itemSlots = new Dictionary<string, ItemSlotUI>();

    // Dictionary for quick ItemData lookup
    private Dictionary<string, ItemData> itemDataLookup = new Dictionary<string, ItemData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        PrecacheAllItemIcons();
        BuildItemDataLookup();
        ClearItemDetails();
    }

    private void BuildItemDataLookup()
    {
        // Build dictionary from ScriptableObject list for fast lookup
        foreach (ItemData data in allItemData)
        {
            if (!string.IsNullOrEmpty(data.itemID))
            {
                itemDataLookup[data.itemID] = data;
                Debug.Log($"Registered ItemData: {data.itemID}");
            }
        }
    }

    public void UpdateJournalTab(string itemID, System.Action useAction = null, GameObject itemPrefab = null)
    {
        // Don't add duplicate items
        if (itemSlots.ContainsKey(itemID))
        {
            Debug.Log($"Item {itemID} already exists in journal");
            return;
        }

        // Add item to items tab
        GameObject entry = Instantiate(itemEntryPrefab, itemsContentParent);

        ItemSlotUI slotUI = entry.GetComponent<ItemSlotUI>();
        slotUI.itemID = itemID;
        slotUI.onUse = useAction;

        Image icon = entry.transform.Find("ItemIcon").GetComponent<Image>();

        // Get sprite from cache
        Sprite itemSprite = GetItemSprite(itemID);
        if (itemSprite != null && icon != null)
        {
            icon.sprite = itemSprite;
            icon.enabled = true;
            Debug.Log("Icon sprite assigned: " + itemSprite.name + " for item: " + itemID);
        }
        else
        {
            Debug.LogWarning("Could not find sprite for item: " + itemID);
        }

        // Store reference to this slot
        itemSlots[itemID] = slotUI;

        // Register in inventory system (no longer called separately from Collectible)
        InventorySystem.Instance.AddItem(itemID, useAction, itemPrefab);
    }

    public void UpdateItemDetailsPanel(string itemID)
    {
        // Check if we have ItemData for this item
        if (itemDataLookup.ContainsKey(itemID))
        {
            ItemData data = itemDataLookup[itemID];

            // Update icon
            if (itemIcon != null && data.itemIcon != null)
            {
                itemIcon.sprite = data.itemIcon;
                itemIcon.enabled = true;
            }

            // Update name
            if (itemName != null)
            {
                itemName.text = data.itemName;
            }

            // Update description
            if (itemDescription != null)
            {
                itemDescription.text = data.itemDescription;
            }

            Debug.Log($"Updated details panel for: {itemID}");
        }
        else
        {
            // Fallback if ItemData not found
            Debug.LogWarning($"No ItemData found for: {itemID}");

            if (itemName != null)
                itemName.text = itemID;

            if (itemDescription != null)
                itemDescription.text = "No description available.";

            // Try to get icon from cache
            Sprite itemSprite = GetItemSprite(itemID);
            if (itemIcon != null && itemSprite != null)
            {
                itemIcon.sprite = itemSprite;
                itemIcon.enabled = true;
            }
        }
    }

    public void UpdateItemSelectionHighlight(string selectedItemID)
    {
        // Update all slots to show only the selected one highlighted
        foreach (var kvp in itemSlots)
        {
            kvp.Value.SetSelected(kvp.Key == selectedItemID);
        }
    }

    public void RemoveItemFromUI(string itemID)
    {
        if (itemSlots.ContainsKey(itemID))
        {
            Destroy(itemSlots[itemID].gameObject);
            itemSlots.Remove(itemID);
            Debug.Log($"Removed {itemID} from journal UI");
        }
    }

    public void HideItemDetailsPanel()
    {
        ClearItemDetails();
    }

    private void ClearItemDetails()
    {
        // Clear the details panel (don't hide, just clear content)
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

    private Sprite GetItemSprite(string itemID)
    {
        // Check if we already have this icon cached
        if (itemIconCache.ContainsKey(itemID))
        {
            Debug.Log("Found cached icon for: " + itemID);
            return itemIconCache[itemID];
        }

        // If not cached, try to find ANY collectible with this ID (even inactive ones)
        Collectible[] allCollectibles = Resources.FindObjectsOfTypeAll<Collectible>();
        foreach (Collectible col in allCollectibles)
        {
            if (col.itemID == itemID && col.itemIcon != null)
            {
                // Cache the icon for future use
                itemIconCache[itemID] = col.itemIcon;
                Debug.Log("Cached new icon for: " + itemID);
                return col.itemIcon;
            }
        }

        Debug.LogWarning("No collectible found with ID: " + itemID);
        return null;
    }

    private void PrecacheAllItemIcons()
    {
        // Find all collectibles in the scene (including inactive ones)
        Collectible[] allCollectibles = Resources.FindObjectsOfTypeAll<Collectible>();

        Debug.Log("Pre-caching icons from " + allCollectibles.Length + " collectibles found in scene");

        foreach (Collectible col in allCollectibles)
        {
            if (!string.IsNullOrEmpty(col.itemID) && col.itemIcon != null)
            {
                if (!itemIconCache.ContainsKey(col.itemID))
                {
                    itemIconCache[col.itemID] = col.itemIcon;
                    Debug.Log("Pre-cached icon for: " + col.itemID);
                }
                else
                {
                    Debug.Log("Icon already cached for: " + col.itemID);
                }
            }
            else
            {
                Debug.LogWarning("Collectible missing itemID or itemIcon: " + col.name);
            }
        }

        Debug.Log("Total icons cached: " + itemIconCache.Count);
    }
}