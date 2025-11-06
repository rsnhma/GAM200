using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    //public GameObject creditsPanel; //Don't have it yet

    [Header("Cutscene Reference")]
    public IntroCutsceneUI introCutsceneUI;  //Reference to cutscene script

    void Start()
    {
        // Show main menu, hide other panel at the start
        ShowMainMenu();
    }

    // === MAIN MENU BUTTONS ===
    public void StartGame()
    {
        // Instead of loading scene directly, play cutscene first
        if (introCutsceneUI != null)
        {
            introCutsceneUI.PlayIntroCutscene();
        }
        else
        {
            // Fallback: Load game directly if cutscene not set up
            Debug.LogWarning("IntroCutsceneUI not assigned! Loading game directly.");
            SceneManager.LoadScene("CCAM LEVEL 1");
        }
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        //creditsPanel.SetActive(false);
    }

    //public void OpenCredits()
    //{
    //    mainMenuPanel.SetActive(false);
    //    settingsPanel.SetActive(false);
    //    creditsPanel.SetActive(true);
    //}

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    // === BACK BUTTONS ===
    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        //creditsPanel.SetActive(false);
    }
}
