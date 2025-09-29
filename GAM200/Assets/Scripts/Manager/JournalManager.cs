using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance;

    [Header("Tabs")]
    public GameObject mapTabPanel;
    public GameObject itemsTabPanel;

    [Header("Prefabs")]
    public GameObject itemEntryPrefab;
    public Transform itemsContentParent;

    // Dictionary to cache item icons so we don't need to find destroyed collectibles
    private Dictionary<string, Sprite> itemIconCache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        PrecacheAllItemIcons();
    }

    public void UpdateJournalTab(string itemID, System.Action useAction = null)
    {
        if (itemID == "map")
        {
            mapTabPanel.SetActive(true);
            return;
        }

        // Add item to items tab
        GameObject entry = Instantiate(itemEntryPrefab, itemsContentParent);

        ItemSlotUI slotUI = entry.GetComponent<ItemSlotUI>();
        slotUI.itemID = itemID;
        slotUI.onUse = useAction;

        Image icon = entry.transform.Find("ItemIcon").GetComponent<Image>();
        Debug.Log("Is Icon Assigned? :" + icon);

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

        // Register in inventory system
        InventorySystem.Instance.AddItem(itemID, useAction);
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

    private Collectible FindCollectibleByID(string id)
    {
        Collectible[] all = FindObjectsOfType<Collectible>();
        foreach (var c in all)
            if (c.itemID == id)
                return c;
        return null;
    }
}