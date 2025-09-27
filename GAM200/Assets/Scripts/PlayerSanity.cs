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
    public float fullRadius = 12f;
    public float minRadius = 5f;
    public float fullIntensity = 1f;

    [Header("Tunnel Vision Settings")]
    public float fullAngle = 360f;
    public float minAngle = 60f;
    public float tunnelVisionThreshold = 0.4f;

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
        //DontDestroyOnLoad(gameObject);

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
            sanityLight.pointLightOuterRadius = Mathf.Lerp(minRadius, fullRadius, sanityPercent);
            sanityLight.intensity = fullIntensity * sanityPercent;

            if (sanityPercent < tunnelVisionThreshold)
            {
                float t = sanityPercent / tunnelVisionThreshold;
                sanityLight.pointLightOuterAngle = Mathf.Lerp(minAngle, fullAngle, t);
            }
            else
            {
                sanityLight.pointLightOuterAngle = fullAngle;
            }

            Debug.Log($"Sanity: {currentSanity}/{maxSanity} ({sanityPercent:P0}) | Radius: {sanityLight.pointLightOuterRadius:F1}");
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