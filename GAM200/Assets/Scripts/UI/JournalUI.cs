using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class JournalUI : MonoBehaviour
{
    [Header("References")]
    public GameObject journalPanel;       // Root journal panel

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

        // Start with controls by default
        ShowPanel("Controls");
    }

    void Update()
    {
        // Toggle journal with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isOpen) CloseJournal();
            else OpenJournal();
        }
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
