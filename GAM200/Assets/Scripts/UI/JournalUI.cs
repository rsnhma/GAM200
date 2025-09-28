using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class JournalUI : MonoBehaviour
{
    [Header("References")]
    public GameObject journalPanel;       // Root journal panel
    public Button journalButton;          // Button to open/close journal

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

        // Open journal button
        journalButton.onClick.AddListener(ToggleJournal);

        // Start with controls by default
        ShowPanel("Controls");
    }

    void Update()
    {
        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseJournal();
                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI(journalPanel))
            {
                CloseJournal();
            }
        }
    }

    private void ToggleJournal()
    {
        if (isOpen) CloseJournal();
        else OpenJournal();
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

    private bool IsPointerOverUI(GameObject panel)
    {
        if (EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == panel || result.gameObject.transform.IsChildOf(panel.transform))
                return true;
        }

        return false;
    }
}
