using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundIndicatorUI : MonoBehaviour
{
    public static SoundIndicatorUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject background; // Reference to "Background" GameObject
    public Image indicatorImage; // Reference to "Image" (the ear icon)
    public Text warningText; 

    [Header("Animation Settings")]
    public float displayDuration = 2f; // How long the indicator shows
    public float fadeInSpeed = 5f;
    public float fadeOutSpeed = 3f;
    public float pulseSpeed = 2f; // Speed of pulse animation
    public float pulseScale = 1.2f; // Max scale during pulse

    [Header("Colors")]
    public Color alertColor = Color.red;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Coroutine displayCoroutine;
    private Vector3 originalScale;

    // Track if player has been detected this "cycle"
    private bool hasBeenDetected = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Get or add CanvasGroup to the Background for fading
            if (background != null)
            {
                canvasGroup = background.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = background.AddComponent<CanvasGroup>();
                }

                // Store original scale of background
                originalScale = background.transform.localScale;

                // Start hidden
                canvasGroup.alpha = 0f;
                background.SetActive(false);
            }
            else
            {
                Debug.LogError("SoundIndicatorUI: Background reference not assigned!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

  
    /// Show the sound indicator when player is heard (only if not already detected)
    public void ShowIndicator()
    {
        if (background == null) return;

        // Only show if player hasn't been detected yet this cycle
        if (hasBeenDetected)
        {
            Debug.Log("SoundIndicatorUI: Already detected, not showing again");
            return;
        }

        // Mark as detected
        hasBeenDetected = true;

        // If already showing, restart the display
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        displayCoroutine = StartCoroutine(DisplayIndicator());
    }


    /// Reset detection state - call this when line of sight is broken
    public void ResetDetection()
    {
        hasBeenDetected = false;
        Debug.Log("SoundIndicatorUI: Detection state reset");
    }


    /// Check if player is currently in a detected state
    public bool IsDetected()
    {
        return hasBeenDetected;
    }

    private IEnumerator DisplayIndicator()
    {
        isShowing = true;
        background.SetActive(true);

        // Apply alert color to the ear icon
        if (indicatorImage != null)
        {
            indicatorImage.color = alertColor;
        }

        // Optional: Set warning text
        if (warningText != null)
        {
            warningText.text = "DETECTED!";
            warningText.color = alertColor;
        }

        // Fade in
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeInSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Pulse animation during display
        float elapsedTime = 0f;
        while (elapsedTime < displayDuration)
        {
            float pulseValue = Mathf.Sin(elapsedTime * pulseSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
            float scale = Mathf.Lerp(1f, pulseScale, pulseValue);
            background.transform.localScale = originalScale * scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset scale
        background.transform.localScale = originalScale;

        // Fade out
        alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeOutSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        background.SetActive(false);
        isShowing = false;
        displayCoroutine = null;
    }

    /// Force hide the indicator immediately
    public void HideIndicator()
    {
        if (background == null) return;

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        canvasGroup.alpha = 0f;
        background.transform.localScale = originalScale;
        background.SetActive(false);
        isShowing = false;
    }

    public bool IsShowing()
    {
        return isShowing;
    }
}