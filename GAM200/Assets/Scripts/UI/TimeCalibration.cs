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

    public RectTransform[] hitZones;
    public float hitTolerance = 10f;

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

        float time = (Time.time - timeOffset) * rotationSpeed;
        float angle = Mathf.PingPong(time, maxAngle - minAngle) + minAngle;
        pointerPivot.localRotation = Quaternion.Euler(0, 0, angle);

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

        foreach (var zone in hitZones)
        {
            if (zone != null)
            {
                zone.gameObject.SetActive(false);
            }
        }

        currentHitZoneIndex = UnityEngine.Random.Range(0, hitZones.Length);

        if (hitZones[currentHitZoneIndex] != null)
        {
            hitZones[currentHitZoneIndex].gameObject.SetActive(true);
            Debug.Log("Showing random hit zone: " + currentHitZoneIndex);
        }

        if (keyAudio) keyAudio.Play();

        Debug.Log("Time Calibration Started! Press SPACE when pointer is in red zone!");
    }

    void CheckHit()
    {
        if (currentHitZoneIndex < 0 || currentHitZoneIndex >= hitZones.Length)
        {
            Debug.LogError("CheckHit: Invalid hitZone index " + currentHitZoneIndex);
            return;
        }

        RectTransform activeHitZone = hitZones[currentHitZoneIndex];

        if (activeHitZone == null)
        {
            Debug.LogError("CheckHit: hitZone at index " + currentHitZoneIndex + " is null!");
            return;
        }

        float pointerAngle = pointerPivot.localEulerAngles.z;
        if (pointerAngle > 180) pointerAngle -= 360;

        Vector2 hitZonePos = activeHitZone.anchoredPosition;
        float hitZoneAngle = Mathf.Atan2(hitZonePos.y, hitZonePos.x) * Mathf.Rad2Deg;

        hitZoneAngle -= 90f;
        if (hitZoneAngle < -180) hitZoneAngle += 360;
        if (hitZoneAngle > 180) hitZoneAngle -= 360;

        float angleDifference = Mathf.Abs(pointerAngle - hitZoneAngle);

        if (angleDifference > 180)
        {
            angleDifference = 360 - angleDifference;
        }

        Debug.Log("Pointer: " + pointerAngle + " | HitZone Angle: " + hitZoneAngle + " | Difference: " + angleDifference + " | Tolerance: " + hitTolerance);

        if (angleDifference <= hitTolerance)
        {
            Debug.Log("HIT! Angle difference: " + angleDifference);

            if (successAudio) successAudio.Play();

            CompleteCalibration(true);
        }
        else
        {
            Debug.Log("MISS! Angle difference: " + angleDifference + " (needed less than " + hitTolerance + ")");

            if (missAudio) missAudio.Play();

            CompleteCalibration(false);
        }
    }

    void CompleteCalibration(bool success)
    {
        isActive = false;

        if (keyAudio) keyAudio.Stop();

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