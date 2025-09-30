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
    public Button controlsTabButton;
    public Button mapTabButton;
    public Button itemsTabButton;

    [Header("Panels")]
    public GameObject controlsPanel;
    public GameObject mapPanel;
    public GameObject itemsPanel;

    private bool isOpen = false;
    private Dictionary<string, GameObject> panels;

    void Start()
    {
        journalPanel.SetActive(false);

        // Tab setup
        panels = new Dictionary<string, GameObject>
        {
            {"Controls", controlsPanel},
            {"Map", mapPanel},
            {"Items", itemsPanel},
        };

        // Tab listeners
        controlsTabButton.onClick.AddListener(() => ShowPanel("Controls"));
        mapTabButton.onClick.AddListener(() => ShowPanel("Map"));
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
        ShowPanel("Controls");
    }

    void Update()
    {
        // Toggle journal with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleJournal();
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