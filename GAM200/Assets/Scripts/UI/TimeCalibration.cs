using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeCalibration : MonoBehaviour
{
    public Transform pointerPivot;
    public float rotationSpeed = 100f;
    public float minAngle = -80f;
    public float maxAngle = 80f;

    public RectTransform[] hitZones; // Array of hit zones
    public float hitZoneTolerance = 15f;

    private bool isActive = false;
    private float timeOffset;
    private int currentHitZoneIndex = -1;

    [Header("Input Cooldown")]
    public float inputCooldown = 0.3f;
    private float lastInputTime = 0f;

    public AudioSource successAudio;
    public AudioSource keyAudio;
    public AudioSource missAudio;

    private Action onSuccess;
    private Action onFail;

    void Start()
    {
        if (hitZones == null || hitZones.Length == 0)
        {
            Debug.LogError("TimeCalibration: hitZones array is empty!");
        }
        if (pointerPivot == null)
        {
            Debug.LogError("TimeCalibration: pointerPivot is not assigned!");
        }
    }

    void Update()
    {
        if (!isActive || pointerPivot == null) return;

        // Move pointer back and forth
        float time = (Time.time - timeOffset) * rotationSpeed;
        float angle = Mathf.PingPong(time, maxAngle - minAngle) + minAngle;
        pointerPivot.localRotation = Quaternion.Euler(0, 0, angle);

        // Detect player input with cooldown
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastInputTime + inputCooldown)
        {
            lastInputTime = Time.time;
            CheckHit();
        }
    }

    public void StartCalibration(Action successCallback, Action failCallback)
    {
        if (hitZones == null || hitZones.Length == 0)
        {
            Debug.LogError("Cannot start calibration: hitZones array is empty!");
            return;
        }

        if (pointerPivot == null)
        {
            Debug.LogError("Cannot start calibration: pointerPivot is not assigned!");
            return;
        }

        isActive = true;
        timeOffset = Time.time;
        lastInputTime = 0f;

        onSuccess = successCallback;
        onFail = failCallback;

        // Hide all hit zones first
        foreach (var zone in hitZones)
        {
            if (zone != null)
            {
                zone.gameObject.SetActive(false);
            }
        }

        // Pick a random hit zone to show
        currentHitZoneIndex = UnityEngine.Random.Range(0, hitZones.Length);

        if (hitZones[currentHitZoneIndex] != null)
        {
            hitZones[currentHitZoneIndex].gameObject.SetActive(true);
            Debug.Log($"Showing random hit zone: {currentHitZoneIndex}");
        }

        if (keyAudio) keyAudio.Play();

        Debug.Log("Time Calibration Started! Press SPACE when pointer is in red zone!");
    }

    void CheckHit()
    {
        if (currentHitZoneIndex < 0 || currentHitZoneIndex >= hitZones.Length)
        {
            Debug.LogError($"CheckHit: Invalid hitZone index {currentHitZoneIndex}");
            return;
        }

        RectTransform activeHitZone = hitZones[currentHitZoneIndex];

        if (activeHitZone == null)
        {
            Debug.LogError($"CheckHit: hitZone at index {currentHitZoneIndex} is null!");
            return;
        }

        float pointerAngle = pointerPivot.localEulerAngles.z;
        float hitZoneAngle = activeHitZone.localEulerAngles.z;

        // Normalize angles to -180 to 180
        if (pointerAngle > 180) pointerAngle -= 360;
        if (hitZoneAngle > 180) hitZoneAngle -= 360;

        float angleDifference = Mathf.Abs(pointerAngle - hitZoneAngle);

        if (angleDifference < hitZoneTolerance)
        {
            // SUCCESS!
            Debug.Log($"HIT! Angle difference: {angleDifference:F1}°");

            if (successAudio) successAudio.Play();

            CompleteCalibration(true);
        }
        else
        {
            // MISS - fail the calibration
            Debug.Log($"MISS! Angle difference: {angleDifference:F1}° (needed < {hitZoneTolerance}°)");

            if (missAudio) missAudio.Play();

            CompleteCalibration(false);
        }
    }

    void CompleteCalibration(bool success)
    {
        isActive = false;

        if (keyAudio) keyAudio.Stop();

        // Hide all hit zones
        foreach (var zone in hitZones)
        {
            if (zone != null)
            {
                zone.gameObject.SetActive(false);
            }
        }

        if (success)
        {
            StartCoroutine(DelayedCallback(onSuccess));
        }
        else
        {
            StartCoroutine(DelayedCallback(onFail));
        }
    }

    IEnumerator DelayedCallback(Action callback)
    {
        yield return new WaitForSeconds(0.2f);
        callback?.Invoke();
    }

    public void StopCalibration()
    {
        isActive = false;
        if (keyAudio) keyAudio.Stop();

        // Hide all hit zones
        if (hitZones != null)
        {
            foreach (var zone in hitZones)
            {
                if (zone != null)
                {
                    zone.gameObject.SetActive(false);
                }
            }
        }
    }
}