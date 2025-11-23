using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class JournalNotificationManager : MonoBehaviour
{
    public static JournalNotificationManager Instance;

    [Header("Notification Indicators")]
    public GameObject journalIconNotification; // Yellow circle on the journal icon button
    public GameObject inventoryTabNotification; // Yellow circle on inventory tab
    public GameObject memorabiliaTabNotification; // Yellow circle on memorabilia tab
    public GameObject tasksTabNotification; // Yellow circle on tasks tab (optional)

    private HashSet<string> unviewedInventoryItems = new HashSet<string>();
    private HashSet<string> unviewedMemorabiliaItems = new HashSet<string>();
    private bool hasViewedInventoryTab = false;
    private bool hasViewedMemorabiliaTab = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Hide all notifications initially
        HideAllNotifications();
    }

    // Called when a new inventory item is added
    public void NotifyNewInventoryItem(string itemID)
    {
        unviewedInventoryItems.Add(itemID);
        hasViewedInventoryTab = false;
        UpdateNotifications();
        Debug.Log($"New inventory item notification: {itemID}");
    }

    // Called when a new memorabilia is added
    public void NotifyNewMemorabilia(string itemID)
    {
        unviewedMemorabiliaItems.Add(itemID);
        hasViewedMemorabiliaTab = false;
        UpdateNotifications();
        Debug.Log($"New memorabilia notification: {itemID}");
    }

    // Called when player opens the journal
    public void OnJournalOpened()
    {
        // Hide the journal icon notification when they open it
        if (journalIconNotification != null)
        {
            journalIconNotification.SetActive(false);
        }
    }

    // Called when player clicks on inventory tab
    public void OnInventoryTabViewed()
    {
        hasViewedInventoryTab = true;
        unviewedInventoryItems.Clear();
        UpdateNotifications();
        Debug.Log("Inventory tab viewed - notifications cleared");
    }

    // Called when player clicks on memorabilia tab
    public void OnMemorabiliaTabViewed()
    {
        hasViewedMemorabiliaTab = true;
        unviewedMemorabiliaItems.Clear();
        UpdateNotifications();
        Debug.Log("Memorabilia tab viewed - notifications cleared");
    }

    // Update all notification indicators
    private void UpdateNotifications()
    {
        bool hasUnviewedInventory = unviewedInventoryItems.Count > 0;
        bool hasUnviewedMemorabilia = unviewedMemorabiliaItems.Count > 0;
        bool hasAnyUnviewed = hasUnviewedInventory || hasUnviewedMemorabilia;

        // Show journal icon notification if there are ANY unviewed items
        if (journalIconNotification != null)
        {
            journalIconNotification.SetActive(hasAnyUnviewed);
        }

        // Show inventory tab notification
        if (inventoryTabNotification != null)
        {
            inventoryTabNotification.SetActive(hasUnviewedInventory);
        }

        // Show memorabilia tab notification
        if (memorabiliaTabNotification != null)
        {
            memorabiliaTabNotification.SetActive(hasUnviewedMemorabilia);
        }

        Debug.Log($"Notifications updated - Inventory: {hasUnviewedInventory}, Memorabilia: {hasUnviewedMemorabilia}");
    }

    private void HideAllNotifications()
    {
        if (journalIconNotification != null)
            journalIconNotification.SetActive(false);

        if (inventoryTabNotification != null)
            inventoryTabNotification.SetActive(false);

        if (memorabiliaTabNotification != null)
            memorabiliaTabNotification.SetActive(false);

        if (tasksTabNotification != null)
            tasksTabNotification.SetActive(false);
    }

    // Clear all notifications (useful for testing or resetting)
    public void ClearAllNotifications()
    {
        unviewedInventoryItems.Clear();
        unviewedMemorabiliaItems.Clear();
        hasViewedInventoryTab = false;
        hasViewedMemorabiliaTab = false;
        HideAllNotifications();
    }

    // Check if there are any unviewed items
    public bool HasUnviewedItems()
    {
        return unviewedInventoryItems.Count > 0 || unviewedMemorabiliaItems.Count > 0;
    }
}