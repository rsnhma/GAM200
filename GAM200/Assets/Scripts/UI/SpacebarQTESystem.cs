using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpacebarQTESystem : MonoBehaviour
{
    public static SpacebarQTESystem Instance { get; private set; }

    [Header("UI References")]
    private Text spacebarKey;
    public Slider progressBar; // The bar that fills
    public Image progressFillWhite; // White fill: player's progress
    public Image progressBackgroundRed; // Red background: resistance

    [Header("QTE Settings")]
    public float timeLimit = 5f;
    public float fillAmountPerPress = 0.05f; // How much bar fills per spacebar press
    public float baseDrainRate = 0.04f; // Base drain per second (enemy resistance)
    public float drainMultiplierAtLowSanity = 3f; // How much harder at low sanity

    [Header("Timer")]
    public Slider timerSlider;

    [Header("Visual Feedback")]
    public Color playerFillColor = Color.white; // White = player progress
    public Color resistanceColor = Color.red; // Red = enemy resistance
    public Color lowSanityFillColor = Color.grey;

    private float currentProgress = 0f; // 0 = empty, 1 = full/escaped
    private float currentTime;
    private bool isQTEActive = false;
    private Action onSuccess;
    private Action onFail;

    private float currentDrainRate;
    private Vector3 originalKeyScale;
    private float pulseTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Configure slider
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }

        // Set colors
        if (progressFillWhite != null)
        {
            progressFillWhite.color = playerFillColor;
        }

        if (progressBackgroundRed != null)
        {
            progressBackgroundRed.color = resistanceColor;
        }

    }

    public void BeginQTE(Action successCallback, Action failCallback = null)
    {
        Debug.Log("=== SPACEBAR QTE STARTED ===");
        Debug.Log("SPAM SPACEBAR TO FILL THE BAR!");

        onSuccess = successCallback;
        onFail = failCallback;

        currentTime = timeLimit;
        currentProgress = 0f; // Start empty
        isQTEActive = true;

        // Calculate drain rate based on sanity
        UpdateDrainRate();

        // Setup timer
        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(true);
            timerSlider.maxValue = timeLimit;
            timerSlider.value = timeLimit;
        }

        // Setup progress bar
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0f;
        }

        // Show white fill
        if (progressFillWhite != null)
        {
            progressFillWhite.gameObject.SetActive(true);
        }

        // Show red background (resistance)
        if (progressBackgroundRed != null)
        {
            progressBackgroundRed.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
        Debug.Log($"Drain rate: {currentDrainRate:F3}/s | Time: {timeLimit}s");
    }

    void Update()
    {
        if (!isQTEActive) return;

        // Update timer
        currentTime -= Time.deltaTime;
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;
        }

        // Check for timeout
        if (currentTime <= 0f)
        {
            Debug.Log("QTE Timeout - Failed!");
            FailQTE();
            return;
        }

        // Handle spacebar input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSpacebarPressed(); // This will check for completion inside
            return; // Don't process drain on the same frame as input
        }

        // Apply resistance drain (enemy fighting back)
        ApplyDrain();

        // Update progress bar visual
        if (progressBar != null)
        {
            progressBar.value = currentProgress;
        }
    }

    void OnSpacebarPressed()
    {
        if (!isQTEActive) return; // Safety check

        // Add progress
        currentProgress += fillAmountPerPress;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 1f);

        Debug.Log($"Spacebar pressed! Progress: {currentProgress:F2} ({currentProgress * 100f:F0}%)");

        // CHECK FOR SUCCESS IMMEDIATELY after spacebar press
        if (currentProgress >= 1f)
        {
            Debug.Log("BAR FILLED - PLAYER ESCAPED!");
            CompleteQTE();
        }
    }

    void ApplyDrain()
    {
        // Enemy/resistance drains the bar
        currentProgress -= currentDrainRate * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 1f);
    }

    void UpdateDrainRate()
    {
        if (PlayerSanity.Instance != null)
        {
            float sanityPercent = PlayerSanity.Instance.GetSanityPercent();

            // Lower sanity = stronger resistance/drain
            float sanityMultiplier = Mathf.Lerp(drainMultiplierAtLowSanity, 1f, sanityPercent);
            currentDrainRate = baseDrainRate * sanityMultiplier;

            // Update fill color based on sanity (gets dimmer at low sanity)
            if (progressFillWhite != null)
            {
                progressFillWhite.color = Color.Lerp(lowSanityFillColor, playerFillColor, sanityPercent);
            }

            Debug.Log($"Sanity: {sanityPercent:P0} | Drain multiplier: {sanityMultiplier:F2}x");
        }
        else
        {
            currentDrainRate = baseDrainRate;
        }
    }

    void CompleteQTE()
    {
        Debug.Log("=== QTE SUCCESS - ESCAPED! ===");
        Debug.Log($"Invoking success callback... (onSuccess is null? {onSuccess == null})");

        // IMMEDIATELY set to false to prevent multiple triggers
        isQTEActive = false;

        try
        {
            if (onSuccess != null)
            {
                Debug.Log("Calling onSuccess callback NOW");
                onSuccess.Invoke();
                Debug.Log("onSuccess callback completed");
            }
            else
            {
                Debug.LogError("onSuccess callback is NULL!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in success callback: {e.Message}\n{e.StackTrace}");
        }

        ResetQTE();
    }

    void FailQTE()
    {
        Debug.Log("=== QTE FAILED - STAYED CAUGHT ===");
        Debug.Log($"Invoking fail callback... (onFail is null? {onFail == null})");

        isQTEActive = false;

        try
        {
            if (onFail != null)
            {
                Debug.Log("Calling onFail callback NOW");
                onFail.Invoke();
                Debug.Log("onFail callback completed");
            }
            else
            {
                Debug.LogError("onFail callback is NULL!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in fail callback: {e.Message}\n{e.StackTrace}");
        }

        ResetQTE();
    }

    void ResetQTE()
    {
        Debug.Log("=== RESETTING QTE ===");

        currentProgress = 0f;
        currentTime = 0f;
        isQTEActive = false;
        pulseTimer = 0f;

        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(false);
        }

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(false);
            progressBar.value = 0f;
        }

        if (progressFillWhite != null)
        {
            progressFillWhite.gameObject.SetActive(false);
        }

        if (progressBackgroundRed != null)
        {
            progressBackgroundRed.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
        Debug.Log("QTE System fully disabled");
    }

    public void RefreshDrainRate()
    {
        if (isQTEActive)
        {
            UpdateDrainRate();
        }
    }
}