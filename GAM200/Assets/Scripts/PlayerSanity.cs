using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal; // For Light2D

public class PlayerSanity : MonoBehaviour
{
    [Header("Stats Reference")]
    public PlayerStats playerStats;

    [Header("Events")]
    public UnityEvent<float> onSanityChanged;
    public UnityEvent onSanityDepleted;

    [Header("Light2D Reference")]
    public Light2D sanityLight;

    [Header("UI Reference")]
    public SanityUI sanityUI;

    [Header("Vision Settings")]
    public float fullRadius = 5f;        // radius at 100% sanity
    public float minRadius = 1f;         // radius at 0% sanity
    public float fullIntensity = 0.506f;

    [Header("Tunnel Vision Settings")]
    public float fullAngle = 360f;       // full field of view
    public float minAngle = 60f;         // tunnel vision minimum
    public float tunnelVisionThreshold = 0.4f; // 40% sanity

    private float currentSanity;

    void Start()
    {
        currentSanity = playerStats.maxSanity;
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

    private void UpdateSanityEffects()
    {
        float sanityPercent = currentSanity / playerStats.maxSanity;

        // Update UI
        if (sanityUI != null)
            sanityUI.SetSanity(sanityPercent);

        // Update Light radius and intensity
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

        // Invoke sanity changed event
        onSanityChanged?.Invoke(sanityPercent);
    }

    public float GetSanityPercent() => currentSanity / playerStats.maxSanity;
}
