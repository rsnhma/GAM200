using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JournalUI : MonoBehaviour
{
    [Header("References")]
    public GameObject journalPanel;   // The UI panel for your journal
    public Button journalButton;      // The button that toggles the journal

    private bool isOpen = false;

    void Start()
    {
        journalPanel.SetActive(false); // start hidden
        journalButton.onClick.AddListener(ToggleJournal);
    }

    void Update()
    {
        if (isOpen)
        {
            // Close with ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseJournal();
                return;
            }

            // Close if left-click outside panel
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

    // Checks if mouse is over the panel or its children
    private bool IsPointerOverUI(GameObject panel)
    {
        if (EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == panel || result.gameObject.transform.IsChildOf(panel.transform))
                return true;
        }

        return false;
    }
}
