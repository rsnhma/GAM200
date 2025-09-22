using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerSanity : MonoBehaviour
{
    [Header("Stats")]
    public float maxSanity = 100f;

    [Header("Events")]
    public UnityEvent<float> onSanityChanged;
    public UnityEvent onSanityDepleted;

    [Header("Light2D Reference")]
    public Light2D sanityLight;

    [Header("UI Reference")]
    public SanityUI sanityUI;

    [Header("Vision Settings")]
    public float fullRadius = 5f;
    public float minRadius = 1f;
    public float fullIntensity = 0.506f;

    [Header("Tunnel Vision Settings")]
    public float fullAngle = 360f;
    public float minAngle = 60f;
    public float tunnelVisionThreshold = 0.4f;

    private float currentSanity;

    public static PlayerSanity Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentSanity = maxSanity;

        // Subscribe to scene load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        UpdateSanityEffects();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try to find UI + Light in the new scene
        if (sanityUI == null)
            sanityUI = FindObjectOfType<SanityUI>();

        if (sanityLight == null)
            sanityLight = FindObjectOfType<Light2D>();

        UpdateSanityEffects(); // Apply sanity to the new scene objects
    }

    public void LoseSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity - amount, 0, maxSanity);
        UpdateSanityEffects();

        if (currentSanity <= 0)
            onSanityDepleted?.Invoke();
    }

    public void GainSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);
        UpdateSanityEffects();
    }

    private void UpdateSanityEffects()
    {
        float sanityPercent = currentSanity / maxSanity;

        if (sanityUI != null)
            sanityUI.SetSanity(sanityPercent);

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
        }

        onSanityChanged?.Invoke(sanityPercent);
    }

    public float GetSanityPercent() => currentSanity / maxSanity;
}
