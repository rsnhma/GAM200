using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerSanity : MonoBehaviour
{
    [Header("Stats")]
    public float maxSanity = 10f;

    [Header("Events")]
    public UnityEvent<float> onSanityChanged;
    public UnityEvent onSanityDepleted;

    [Header("Light2D Reference")]
    public Light2D sanityLight;

    [Header("UI Reference")]
    public SanityUI sanityUI;

    [Header("Vision Settings")]
    public float fullOuterRadius = 12f;
    public float minOuterRadius = 2f;
    public float fullInnerRadius = 6f;
    public float minInnerRadius = 1f;
    public float fullIntensity = 1f;

    [Header("QTE Settings")]
    public float sanityLossPerQTEFail = 1f;
    public float sanityLossPerQTESuccess = 0.5f;

    [Header("Game Over Settings")]
    public float gameOverSanityThreshold = 0.2f; // 20% sanity
    public UnityEvent onGameOver;

    private float currentSanity;

    public static PlayerSanity Instance { get; private set; }

    private void Awake()
    {
        // Handle singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.Log("Destroying duplicate PlayerSanity instance");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentSanity = maxSanity;

        // Subscribe to scene load
        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log("PlayerSanity instance created and set as singleton");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Only clear instance if this is the current instance
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        UpdateSanityEffects();
        Debug.Log($"PlayerSanity Start called. Current sanity: {currentSanity}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}. Finding UI and Light components...");

        // Try to find UI + Light in the new scene
        if (sanityUI == null)
        {
            sanityUI = FindObjectOfType<SanityUI>();
            Debug.Log(sanityUI != null ? "SanityUI found in new scene" : "SanityUI not found in new scene");
        }

        if (sanityLight == null)
        {
            // Look for the light that's a child of the player
            sanityLight = GetComponentInChildren<Light2D>();
            if (sanityLight == null)
            {
                // If not found as child, try to find any Light2D in the scene that might be the player's light
                Light2D[] lights = FindObjectsOfType<Light2D>();
                foreach (Light2D light in lights)
                {
                    if (light.gameObject.CompareTag("Player") || light.transform.parent?.CompareTag("Player") == true)
                    {
                        sanityLight = light;
                        break;
                    }
                }
            }
            Debug.Log(sanityLight != null ? "SanityLight found in new scene" : "SanityLight not found in new scene");
        }

        UpdateSanityEffects(); // Apply sanity to the new scene objects
        Debug.Log("Sanity effects updated after scene load");
    }

    // Call this when player FAILS a QTE
    public void OnQTEFailed()
    {
        Debug.Log($"=== PlayerSanity.OnQTEFailed() CALLED ===");
        Debug.Log($"Current sanity BEFORE loss: {currentSanity}/{maxSanity}");
        Debug.Log($"Will lose {sanityLossPerQTEFail} sanity");

        LoseSanity(sanityLossPerQTEFail);

        Debug.Log($"Current sanity AFTER loss: {currentSanity}/{maxSanity}");

        // Force multiple UI updates
        UpdateSanityEffects();

        if (sanityUI != null)
        {
            sanityUI.SetSanity(GetSanityPercent());
            Debug.Log($"Forced sanity UI update to {GetSanityPercent():P0}");
        }
        else
        {
            Debug.LogError("SanityUI is null!");
        }

        // Update the QTE drain rate for the next attempt
        if (SpacebarQTESystem.Instance != null)
        {
            SpacebarQTESystem.Instance.RefreshDrainRate();
        }

        CheckGameOverCondition();
        Debug.Log($"=== PlayerSanity.OnQTEFailed() COMPLETE ===");
    }

    // Call this when player SUCCEEDS a QTE (but still loses a bit of sanity)
    public void OnQTESuccess()
    {
        Debug.Log($"=== PlayerSanity.OnQTESuccess() CALLED ===");
        Debug.Log($"Current sanity BEFORE loss: {currentSanity}/{maxSanity}");
        Debug.Log($"Will lose {sanityLossPerQTESuccess} sanity");

        LoseSanity(sanityLossPerQTESuccess);

        Debug.Log($"Current sanity AFTER loss: {currentSanity}/{maxSanity}");

        // Force multiple UI updates
        UpdateSanityEffects();

        if (sanityUI != null)
        {
            sanityUI.SetSanity(GetSanityPercent());
            Debug.Log($"Forced sanity UI update to {GetSanityPercent():P0}");
        }
        else
        {
            Debug.LogError("SanityUI is null!");
        }

        CheckGameOverCondition();
        Debug.Log($"=== PlayerSanity.OnQTESuccess() COMPLETE ===");
    }

    // Check if player should get game over when caught at low sanity
    private void CheckGameOverCondition()
    {
        float sanityPercent = GetSanityPercent();

        if (sanityPercent <= gameOverSanityThreshold)
        {
            Debug.Log($"GAME OVER! Player caught with sanity at {sanityPercent:P0}");
            onGameOver?.Invoke();
        }
    }

    public void LoseSanity(float amount)
    {
        Debug.Log($"LoseSanity called with amount: {amount}");

        currentSanity = Mathf.Clamp(currentSanity - amount, 0, maxSanity);
        Debug.Log($"Current sanity after loss: {currentSanity}");

        UpdateSanityEffects();

        if (currentSanity <= 0)
        {
            Debug.Log("Sanity depleted! Invoking event.");
            onSanityDepleted?.Invoke();
        }
    }

    public void GainSanity(float amount)
    {
        Debug.Log($"GainSanity called with amount: {amount}");

        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);
        Debug.Log($"Current sanity after gain: {currentSanity}");

        UpdateSanityEffects();
    }

    private void UpdateSanityEffects()
    {
        float sanityPercent = currentSanity / maxSanity;

        if (sanityUI != null)
        {
            sanityUI.SetSanity(sanityPercent);
        }

        if (sanityLight != null)
        {
            // Keep 360 degree vision at all times
            sanityLight.pointLightOuterAngle = 360f;

            // Adjust both inner and outer radius based on sanity
            sanityLight.pointLightOuterRadius = Mathf.Lerp(minOuterRadius, fullOuterRadius, sanityPercent);
            sanityLight.pointLightInnerRadius = Mathf.Lerp(minInnerRadius, fullInnerRadius, sanityPercent);
            sanityLight.intensity = fullIntensity * sanityPercent;

            Debug.Log($"Sanity: {currentSanity}/{maxSanity} ({sanityPercent:P0}) | Inner Radius: {sanityLight.pointLightInnerRadius:F1} | Outer Radius: {sanityLight.pointLightOuterRadius:F1} | Angle: 360°");
        }

        onSanityChanged?.Invoke(sanityPercent);
    }

    public float GetSanityPercent() => currentSanity / maxSanity;

    // Helper method to check if instance is valid
    public static bool IsInstanceValid()
    {
        return Instance != null;
    }

    public void ResetSanity()
    {
        currentSanity = maxSanity;
        UpdateSanityEffects();
        Debug.Log("Sanity reset to max");
    }
}