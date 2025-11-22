using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroCutsceneUI : MonoBehaviour
{
    [Header("References")]
    public GameObject mainMenuPanel;
    public GameObject cutscenePanel;

    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public GameObject videoPanel;

    [Header("Skip Button")]
    public Button skipButton;
    public string gameSceneName = "CCAM LEVEL 1";

    [Header("Audio")]
    public float bgmFadeOutDuration = 1f;

    private bool videoPlaying = false;

    private void Start()
    {
        // Hide cutscene elements at start
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);
        if (videoPanel != null)
            videoPanel.SetActive(false);

        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        // Setup skip button
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipCutscene);
            skipButton.gameObject.SetActive(false);
        }
    }

    // Call this from main menu's START button
    public void PlayIntroCutscene()
    {
        Debug.Log("Starting intro cutscene");

        // STOP MENU BGM WHEN CUTSCENE STARTS
        if (SoundManager.Instance != null)
        {
            // Choose one: instant stop or smooth fade
            // SoundManager.Instance.StopMenuBGM(); // Instant
            SoundManager.Instance.FadeOutMenuBGM(bgmFadeOutDuration); // Smooth
        }

        // Hide main menu
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // Show cutscene panel
        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        // Play video
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            if (videoPanel != null)
            {
                videoPanel.SetActive(true);
                Debug.Log($"VideoPanel active: {videoPanel.activeSelf}");

                RawImage rawImage = videoPanel.GetComponentInChildren<RawImage>();
                if (rawImage != null)
                {
                    rawImage.enabled = true;
                    Debug.Log("RawImage enabled for cutscene");
                }
                else
                {
                    Debug.LogError("NO RAW IMAGE FOUND ON VIDEO PANEL!");
                }
            }

            if (skipButton != null)
                skipButton.gameObject.SetActive(true);

            videoPlaying = true;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("VideoPlayer or VideoClip is missing! Loading game directly...");
            LoadGameScene();
        }
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Intro cutscene video prepared");
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("Intro cutscene finished - loading game");
        videoPlaying = false;
        LoadGameScene();
    }

    public void SkipCutscene()
    {
        Debug.Log("Cutscene skipped by player");

        if (videoPlayer != null)
            videoPlayer.Stop();

        videoPlaying = false;
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        // Ensure menu BGM is stopped before loading game
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMenuBGM();
        }

        // Hide everything
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);
        if (videoPanel != null)
            videoPanel.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}