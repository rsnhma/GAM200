using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [Header("Player Hand Reference")]
    public Transform playerHandTransform;

    private HashSet<string> items = new HashSet<string>();
    private Dictionary<string, Action> useActions = new Dictionary<string, Action>();
    private Dictionary<string, GameObject> itemPrefabs = new Dictionary<string, GameObject>();

    private string currentlyEquippedItemID = null;
    private string currentlySelectedItemID = null;
    private GameObject currentHandItemObject = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Press E to use currently SELECTED item 
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!string.IsNullOrEmpty(currentlySelectedItemID))
            {
                UseItem(currentlySelectedItemID);
            }
        }
    }

    public void AddItem(string itemID, Action useAction = null, GameObject itemPrefab = null)
    {
        bool isFirstItem = items.Count == 0;

        items.Add(itemID);
        if (useAction != null) useActions[itemID] = useAction;
        if (itemPrefab != null) itemPrefabs[itemID] = itemPrefab;

        // Auto-equip AND select first item picked up
        if (isFirstItem)
        {
            EquipItem(itemID);
            SelectItem(itemID);
        }

        Debug.Log($"Added item: {itemID}. Total items: {items.Count}");
    }

    public void SelectItem(string itemID)
    {
        if (!items.Contains(itemID)) return;

        currentlySelectedItemID = itemID;

        // Update UI to show selected item details
        JournalManager.Instance.UpdateItemDetailsPanel(itemID);

        // Update all item slots to show selection highlight
        JournalManager.Instance.UpdateItemSelectionHighlight(itemID);

        Debug.Log($"Selected item: {itemID}");
    }

    public void EquipItem(string itemID)
    {
        if (!items.Contains(itemID)) return;

        SoundManager.Instance.PlayEquipSound();

        // Remove previous hand item if exists
        if (currentHandItemObject != null)
        {
            Destroy(currentHandItemObject);
        }

        currentlyEquippedItemID = itemID;

        // Instantiate item prefab in player's hand
        if (itemPrefabs.ContainsKey(itemID) && playerHandTransform != null)
        {
            currentHandItemObject = Instantiate(itemPrefabs[itemID], playerHandTransform);
            currentHandItemObject.transform.localPosition = Vector3.zero;
            currentHandItemObject.transform.localRotation = Quaternion.identity;
        }

        // Also select the item when equipped (so E key works on it)
        SelectItem(itemID);

        Debug.Log($"Equipped item to hands: {itemID}");
    }

    public bool HasItem(string itemID)
    {
        return items.Contains(itemID);
    }

    public void UseItem(string itemID)
    {
        if (items.Contains(itemID) && useActions.ContainsKey(itemID))
        {
            useActions[itemID]?.Invoke();
            items.Remove(itemID);
            useActions.Remove(itemID);
            itemPrefabs.Remove(itemID);

            // Clear equipped item if it was the one used
            if (currentlyEquippedItemID == itemID)
            {
                if (currentHandItemObject != null)
                {
                    Destroy(currentHandItemObject);
                }
                currentlyEquippedItemID = null;
            }

            // Clear selection if it was the one used
            if (currentlySelectedItemID == itemID)
            {
                currentlySelectedItemID = null;

                // Hide details panel when item is used
                JournalManager.Instance.HideItemDetailsPanel();
            }

            // Remove from journal UI - THIS WAS ALREADY HERE
            JournalManager.Instance.RemoveItemFromUI(itemID);

            Debug.Log($"Used and removed item: {itemID}");
        }
    }

    public void RemoveItem(string itemID)
    {
        items.Remove(itemID);
        useActions.Remove(itemID);
        itemPrefabs.Remove(itemID);

        if (currentlyEquippedItemID == itemID)
        {
            if (currentHandItemObject != null)
            {
                Destroy(currentHandItemObject);
            }
            currentlyEquippedItemID = null;
        }

        if (currentlySelectedItemID == itemID)
        {
            currentlySelectedItemID = null;
            JournalManager.Instance.HideItemDetailsPanel();
        }

        
        JournalManager.Instance.RemoveItemFromUI(itemID);

        Debug.Log($"Removed item: {itemID}");
    }

    public List<string> GetAllItems()
    {
        return new List<string>(items);
    }

    public string GetEquippedItemID()
    {
        return currentlyEquippedItemID;
    }

    public string GetSelectedItemID()
    {
        return currentlySelectedItemID;
    }

    public void ClearAll()
    {
        items.Clear();
        useActions.Clear();
        itemPrefabs.Clear();

        if (currentHandItemObject != null)
        {
            Destroy(currentHandItemObject);
        }

        currentlyEquippedItemID = null;
        currentlySelectedItemID = null;
    }
}