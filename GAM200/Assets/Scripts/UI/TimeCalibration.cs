using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeCalibration : MonoBehaviour
{
    public Transform pointerPivot; // Assign PointerPivot
    public float rotationSpeed = 100f;
    public float minAngle = -80f;
    public float maxAngle = 80f;

    public RectTransform[] hitZones; // Assign all hit zones
    public Slider progressSlider; // Assign Slider

    private int successfulHits = 0;
    private int maxHits = 3; // Need 3 successful hits to retrieve key
    private bool isActive = false;
    private float timeOffset;
    private int currentHitZoneIndex = 0;

    public AudioSource successAudio; // When players complete the time cali
    public AudioSource keyAudio; // When key spawns in game

    // Callbacks for well interaction
    private Action onSuccess;
    private Action onMiss; // Called every time player misses

    void Update()
    {
        if (!isActive || !pointerPivot) return;

        // Move pointer back and forth
        float time = (Time.time - timeOffset) * rotationSpeed;
        float angle = Mathf.PingPong(time, maxAngle - minAngle) + minAngle;
        pointerPivot.localRotation = Quaternion.Euler(0, 0, angle);

        // Detect player input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }
    }

    public void StartCalibration(Action successCallback, Action missCallback)
    {
        // Reset everything
        successfulHits = 0;
        isActive = true;
        timeOffset = Time.time;
        currentHitZoneIndex = 0;

        // Store callbacks
        onSuccess = successCallback;
        onMiss = missCallback;

        // Reset UI
        if (progressSlider) progressSlider.value = 0f;

        // Hide all hit zones first
        foreach (var zone in hitZones)
        {
            zone.gameObject.SetActive(false);
        }

        // Activate first hit zone
        ActivateHitZone(currentHitZoneIndex);

        // Play audio
        if (keyAudio) keyAudio.Play();

        Debug.Log("Time Calibration Started! Get 3 hits before enemy arrives!");
    }

    void CheckHit()
    {
        RectTransform activeHitZone = hitZones[currentHitZoneIndex];
        float pointerAngle = pointerPivot.localEulerAngles.z;
        float hitZoneAngle = activeHitZone.localEulerAngles.z;

        // Normalize angles to -180 to 180
        if (pointerAngle > 180) pointerAngle -= 360;
        if (hitZoneAngle > 180) hitZoneAngle -= 360;

        float angleDifference = Mathf.Abs(pointerAngle - hitZoneAngle);

        if (angleDifference < 10f) // Successful hit!
        {
            successfulHits++;
            if (progressSlider) progressSlider.value = (float)successfulHits / maxHits;

            Debug.Log($"Hit! Progress: {successfulHits}/{maxHits}");

            if (successfulHits >= maxHits)
            {
                CompleteCalibration();
            }
            else
            {
                NextHitZone();
            }
        }
        else
        {
            // Missed! Alert enemy but keep trying
            Debug.Log("Missed! Enemy alerted to your position!");

            // Call miss callback - this will alert the enemy
            onMiss?.Invoke();

            // Player continues trying - no game over, just pressure!
        }
    }

    void NextHitZone()
    {
        hitZones[currentHitZoneIndex].gameObject.SetActive(false);

        // Pick a different random hit zone
        int newIndex;
        do
        {
            newIndex = UnityEngine.Random.Range(0, hitZones.Length);
        } while (newIndex == currentHitZoneIndex && hitZones.Length > 1);

        currentHitZoneIndex = newIndex;
        ActivateHitZone(currentHitZoneIndex);
    }

    void ActivateHitZone(int index)
    {
        hitZones[index].gameObject.SetActive(true);
    }

    void CompleteCalibration()
    {
        isActive = false;
        Debug.Log("Time Calibration Successful! Key retrieved!");

        if (keyAudio) keyAudio.Stop();
        if (successAudio) successAudio.Play();

        StartCoroutine(DelayedSuccess());
    }

    IEnumerator DelayedSuccess()
    {
        // Wait for unlock audio to finish
        if (successAudio != null)
        {
            yield return new WaitForSeconds(successAudio.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Call success callback
        onSuccess?.Invoke();

        // Reset for potential future use
        ResetCalibration();
    }

    void ResetCalibration()
    {
        successfulHits = 0;

        if (progressSlider) progressSlider.value = 0f;

        // Hide all hit zones
        foreach (var zone in hitZones)
        {
            zone.gameObject.SetActive(false);
        }
    }

    // Public method to stop calibration externally (e.g., if player gets caught by enemy)
    public void StopCalibration()
    {
        isActive = false;
        if (keyAudio) keyAudio.Stop();
        ResetCalibration();
    }
}