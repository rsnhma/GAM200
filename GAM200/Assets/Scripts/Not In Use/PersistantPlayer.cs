using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal; // For Light2D

public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance;

    [Header("Stats")]
    public PlayerStats playerStats;

    [Header("Sanity")]
    public float currentSanity;

    [Header("Events")]
    public UnityEvent<float> onSanityChanged;
    public UnityEvent onSanityDepleted;

    [Header("UI + Light")]
    public SanityUI sanityUI;
    public Light2D sanityLight;

    [Header("Vision Settings")]
    public float fullRadius = 5f;
    public float minRadius = 1f;
    public float fullIntensity = 0.506f;

    [Header("Tunnel Vision Settings")]
    public float fullAngle = 360f;
    public float minAngle = 60f;
    public float tunnelVisionThreshold = 0.4f; // 40% sanity

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize sanity
        currentSanity = playerStats.maxSanity;
    }

    private void Start()
    {
        UpdateSanityEffects();
    }

    public void LoseSanity(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, playerStats.maxSanity);

        UpdateSanityEffects();

        if (currentSanity <= 0)
            onSanityDepleted?.Invoke();
    }

    public void GainSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, playerStats.maxSanity);

        UpdateSanityEffects();
    }

    public float GetSanityPercent() => currentSanity / playerStats.maxSanity;

    private void UpdateSanityEffects()
    {
        float sanityPercent = GetSanityPercent();

        // UI
        if (sanityUI != null)
            sanityUI.SetSanity(sanityPercent);

        // Light2D
        if (sanityLight != null)
        {
            sanityLight.pointLightOuterRadius = Mathf.Lerp(minRadius, fullRadius, sanityPercent);
            sanityLight.intensity = fullIntensity * sanityPercent;

            // Tunnel vision
            if (sanityPercent < tunnelVisionThreshold)
            {
                float t = sanityPercent / tunnelVisionThreshold;
                sanityLight.pointLightOuterAngle = Mathf.Lerp(minAngle, fullAngle, t);
            }
            else
            {
                sanityLight.pointLightOuterAngle = fullAngle;
            }
        }

        onSanityChanged?.Invoke(sanityPercent);
    }
}
