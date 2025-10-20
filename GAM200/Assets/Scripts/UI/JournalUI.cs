using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class JournalUI : MonoBehaviour
{
    [Header("References")]
    public GameObject journalPanel;       // Root journal panel
    public Image journalIconImage;        // The journal icon/image players can click

    [Header("Tabs")]
    public Button settingsTabButton;
    public Button tasksTabButton;         
    public Button memorabiliaTabButton;   
    public Button inventoryTabButton;    

    [Header("Panels")]
    public GameObject settingsPanel;      
    public GameObject tasksPanel;         
    public GameObject memorabiliaPanel;   
    public GameObject inventoryPanel;    

    private bool isOpen = false;
    private Dictionary<string, GameObject> panels;

    void Start()
    {
        journalPanel.SetActive(false);

        // Tab setup - now includes all 4 tabs
        panels = new Dictionary<string, GameObject>
        {
            {"Settings", settingsPanel},
            {"Tasks", tasksPanel},
            {"Memorabilia", memorabiliaPanel},
            {"Inventory", inventoryPanel}
        };

        // Tab listeners
        settingsTabButton.onClick.AddListener(() => ShowPanel("Settings"));
        tasksTabButton.onClick.AddListener(() => ShowPanel("Tasks"));
        memorabiliaTabButton.onClick.AddListener(() => ShowPanel("Memorabilia"));
        inventoryTabButton.onClick.AddListener(() => ShowPanel("Inventory"));

        // Add click listener to journal icon
        if (journalIconImage != null)
        {
            // Add EventTrigger component if not present
            EventTrigger trigger = journalIconImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = journalIconImage.gameObject.AddComponent<EventTrigger>();
            }

            // Create pointer click entry
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { ToggleJournal(); });
            trigger.triggers.Add(entry);
        }

        // Start with settings by default
        ShowPanel("Settings");
    }

    void Update()
    {
        // Toggle journal with J
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleJournal();
        }

        // Toggle inventory tab with I
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isOpen)
                CloseJournal();
            else
            {
                OpenJournal();
                ShowPanel("Inventory");
            }
        }

        // NEW: Toggle memorabilia tab with M
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isOpen)
                CloseJournal();
            else
            {
                OpenJournal();
                ShowPanel("Memorabilia");
            }
        }

        // NEW: Toggle tasks tab with T
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isOpen)
                CloseJournal();
            else
            {
                OpenJournal();
                ShowPanel("Tasks");
            }
        }

        // Toggle journal with Escape (always goes to Settings)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen)
                CloseJournal();
            else
            {
                OpenJournal();
                ShowPanel("Settings");
            }
        }
    }

    private void ToggleJournal()
    {
        if (isOpen)
            CloseJournal();
        else
            OpenJournal();
    }

    private void OpenJournal()
    {
        journalPanel.SetActive(true);
        isOpen = true;
    }

    private void CloseJournal()
    {
        journalPanel.SetActive(false);
        isOpen = false;
    }

    private void ShowPanel(string panelName)
    {
        foreach (var kvp in panels)
            kvp.Value.SetActive(kvp.Key == panelName);
    }

    public void OpenToInventory()
    {
        OpenJournal();
        ShowPanel("Inventory");
    }

    public void OpenToMemorabilia()
    {
        OpenJournal();
        ShowPanel("Memorabilia");
    }

    public void OpenToTasks()
    {
        OpenJournal();
        ShowPanel("Tasks");
    }

    public void OpenToSettings()
    {
        OpenJournal();
        ShowPanel("Settings");
    }
}