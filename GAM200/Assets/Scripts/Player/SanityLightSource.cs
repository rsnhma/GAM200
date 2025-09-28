using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SanityLightSource : MonoBehaviour
{
    [Header("Settings")]
    public float sanityPerSecond = 2f;
    public float flickerIntervalMin = 1f;
    public float flickerIntervalMax = 3f;

    [Header("References")]
    public Light2D light2D;
    private bool isPlayerInside = false;
    private bool isLightOn = true;

    private void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        StartCoroutine(FlickerRoutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            StartCoroutine(SanityRegenRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private IEnumerator SanityRegenRoutine()
    {
        while (isPlayerInside)
        {
            if (isLightOn && PlayerSanity.IsInstanceValid())
            {
                PlayerSanity.Instance.GainSanity(sanityPerSecond);
            }
            yield return new WaitForSeconds(1f); // 1 second intervals
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(flickerIntervalMin, flickerIntervalMax));
            isLightOn = !isLightOn;
            light2D.enabled = isLightOn; // makes it visibly flicker
        }
    }
}
