using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider sanitySlider;

    [Header("Lerp Settings")]
    public float lerpSpeed = 5f; // smooth transition speed

    private float targetValue = 1f;

    public void SetSanity(float percent)
    {
        targetValue = Mathf.Clamp01(percent);
    }

    void Update()
    {
        if (sanitySlider != null)
            sanitySlider.value = Mathf.Lerp(sanitySlider.value, targetValue, Time.deltaTime * lerpSpeed);
    }
}
