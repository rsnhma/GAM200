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

    private bool isActive = false;
    private float timeOffset;
    private int currentHitZoneIndex = -1;

    [Header("Input Cooldown")]
    public float inputCooldown = 0.3f;
    private float lastInputTime = 0f;

    [Header("Collision Detection")]
    public Collider2D pointerCollider; // Reference to the pointer's collider
    private bool isPointerInHitZone = false; // Track if pointer is currently in hit zone

    public AudioSource keyAudio;

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
        if (pointerCollider == null)
        {
            Debug.LogError("TimeCalibration: pointerCollider is not assigned!");
        }

        // Add PointerTriggerDetector component to the pointer collider
        if (pointerCollider != null && pointerCollider.GetComponent<PointerTriggerDetector>() == null)
        {
            PointerTriggerDetector detector = pointerCollider.gameObject.AddComponent<PointerTriggerDetector>();
            detector.timeCalibration = this;
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
        isPointerInHitZone = false;

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

    // Called by PointerTriggerDetector when pointer enters a hit zone
    public void OnPointerEnterHitZone(bool inHitZone)
    {
        if (!isActive) return;

        // Check if this is the active hit zone
        //if (currentHitZoneIndex >= 0 && currentHitZoneIndex < hitZones.Length)
        //{
        //    RectTransform activeHitZone = hitZones[currentHitZoneIndex];
        //    if (activeHitZone != null && hitZone.transform == activeHitZone.transform)
        //    {
                isPointerInHitZone = inHitZone;
                Debug.Log("POINTER ENTERED HIT ZONE!");
            //}
        //}
    }

    // Called by PointerTriggerDetector when pointer exits a hit zone
    public void OnPointerExitHitZone(bool inHitZone)
    {
        if (!isActive) return;

        //if (currentHitZoneIndex >= 0 && currentHitZoneIndex < hitZones.Length)
        //{
        //    RectTransform activeHitZone = hitZones[currentHitZoneIndex];
        //    if (activeHitZone != null && hitZone.transform == activeHitZone.transform)
        //    {
                isPointerInHitZone = inHitZone;
                Debug.Log("POINTER EXITED HIT ZONE!");
        //    }
        //}
    }

    void CheckHit()
    {
        Debug.Log($"Space Pressed! Pointer in hit zone: {isPointerInHitZone}");

        if (isPointerInHitZone)
        {
            Debug.Log("HIT! Pointer was in the hit zone!");
            SoundManager.Instance.PlayPuzzleSuccessSound();
            CompleteCalibration(true);
        }
        else
        {
            Debug.Log("MISS! Pointer was not in the hit zone!");
            SoundManager.Instance.PlayWellFailSound();
            CompleteCalibration(false);
        }
    }

    void CompleteCalibration(bool success)
    {
        isActive = false;
        isPointerInHitZone = false;

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
        isPointerInHitZone = false;
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

public class PointerTriggerDetector : MonoBehaviour
{
    public TimeCalibration timeCalibration;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (timeCalibration != null)
        {
            timeCalibration.OnPointerEnterHitZone(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (timeCalibration != null)
        {
            timeCalibration.OnPointerExitHitZone(false);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Optional: Log continuously while overlapping
        // Debug.Log($"Pointer staying in: {other.name}");
    }
}