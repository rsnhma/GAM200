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
    public Button TabButton2;
    public Button itemsTabButton;

    [Header("Panels")]
    public GameObject ContentPanel1;
    public GameObject ContentPanel2;
    public GameObject itemsPanel;

    private bool isOpen = false;
    private Dictionary<string, GameObject> panels;

    void Start()
    {
        journalPanel.SetActive(false);

        // Tab setup
        panels = new Dictionary<string, GameObject>
        {
            {"Settings", ContentPanel1},
            {"TabButton2", ContentPanel2},
            {"Items", itemsPanel},
        };

        // Tab listeners
        settingsTabButton.onClick.AddListener(() => ShowPanel("Settings"));
        TabButton2.onClick.AddListener(() => ShowPanel("Content2"));
        itemsTabButton.onClick.AddListener(() => ShowPanel("Items"));

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

        // Start with controls by default
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
                CloseJournal(); // close if open
            else
            {
                OpenJournal();
                ShowPanel("Items");
            }
        }

        // Toggle journal with Escape (always goes to Settings)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen)
                CloseJournal(); // close if open
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
}