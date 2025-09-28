using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance;

    [Header("Tabs")]
    public GameObject mapTabPanel;
    public GameObject itemsTabPanel;

    [Header("Prefabs")]
    public GameObject itemEntryPrefab; // prefab with Text + Button
    public Transform itemsContentParent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        // Register right-click action
        slotUI.onRightClick = useAction;

        Image icon = entry.transform.Find("ItemIcon").GetComponent<Image>();
        Collectible col = FindCollectibleByID(itemID); // helper to get the collectible
        if (col != null && icon != null)
        {
            icon.sprite = col.itemIcon;
            icon.enabled = true;
        }

        // Register in inventory system
        InventorySystem.Instance.AddItem(itemID, useAction);
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
