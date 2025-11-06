using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class JournalUI : MonoBehaviour
{
    [Header("References")]
    public GameObject journalPanel;
    public Image journalIconImage;

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

    [Header("Main Menu Settings")]
    public Button mainMenuButton;
    public string mainMenuSceneName = "MainMenu";

    private bool isOpen = false;
    private Dictionary<string, GameObject> panels;
    private string currentPanel = "Settings";

    void Start()
    {
        journalPanel.SetActive(false);

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

        // Main Menu button listener
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }

        // Add click listener to journal icon
        if (journalIconImage != null)
        {
            EventTrigger trigger = journalIconImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = journalIconImage.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { ToggleJournal(); });
            trigger.triggers.Add(entry);
        }

        // Start with settings by default
        ShowPanel("Settings", playSound: false);
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

        // Toggle memorabilia tab with M
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

        // Toggle tasks tab with T
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

        // Notify that journal was opened
        if (JournalNotificationManager.Instance != null)
        {
            JournalNotificationManager.Instance.OnJournalOpened();
        }
    }

    private void CloseJournal()
    {
        journalPanel.SetActive(false);
        isOpen = false;
    }

    private void ShowPanel(string panelName, bool playSound = true)
    {
        // Only play sound if requested (not during initialization)
        if (playSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayJournalTabSound();
        }

        // Track which panel we're showing
        currentPanel = panelName;

        // Notify the notification manager when viewing specific tabs
        if (JournalNotificationManager.Instance != null)
        {
            if (panelName == "Inventory")
            {
                JournalNotificationManager.Instance.OnInventoryTabViewed();
            }
            else if (panelName == "Memorabilia")
            {
                JournalNotificationManager.Instance.OnMemorabiliaTabViewed();
            }
        }

        foreach (var kvp in panels)
            kvp.Value.SetActive(kvp.Key == panelName);
    }

    public void LoadMainMenu()
    {
        // Play click sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClickSound();
        }

        Debug.Log($"Loading Main Menu scene: {mainMenuSceneName}");

        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
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