using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public GameObject videoPanel; // Optional: Panel to show video on
   
    private bool videoPlaying = false;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (videoPanel != null)
            videoPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(MainMenu);

        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    public void ShowGameOver()
    {
        Debug.Log("ShowGameOver called - starting jumpscare video");

        if (videoPlayer != null && videoPlayer.clip != null)
        {
            // FORCE VideoPanel to be visible and on top
            if (videoPanel != null)
            {
                videoPanel.SetActive(true);

                // Debug checks
                Debug.Log($"VideoPanel active: {videoPanel.activeSelf}");

                RawImage rawImage = videoPanel.GetComponentInChildren<RawImage>();
                if (rawImage != null)
                {
                    Debug.Log($"RawImage found. Texture: {rawImage.texture != null}");
                    rawImage.enabled = true; // Force enable
                }
                else
                {
                    Debug.LogError("NO RAW IMAGE FOUND ON VIDEO PANEL!");
                }
            }

            videoPlaying = true;
            videoPlayer.Play();
        }
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Video prepared and ready to play");
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("Jumpscare video finished - showing game over panel");
        videoPlaying = false;
        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        // Hide video panel
        if (videoPanel != null)
            videoPanel.SetActive(false);

        // Show game over UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Pause game
        Time.timeScale = 0f;

        Debug.Log("Game Over panel displayed");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (PlayerSanity.IsInstanceValid())
            PlayerSanity.Instance.ResetSanity();

        RoomTracker.Instance?.ResetTracker();

        // Stop video if playing
        if (videoPlayer != null)
            videoPlayer.Stop();

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        // Stop video if playing
        if (videoPlayer != null)
            videoPlayer.Stop();

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Clean up video player events
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}