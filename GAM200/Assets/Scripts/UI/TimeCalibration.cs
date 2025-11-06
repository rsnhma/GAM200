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

    [Header("UI References")]
    [SerializeField] private GameObject calibrationUI; // The parent GameObject containing all UI elements

    [Header("Input Cooldown")]
    public float inputCooldown = 0.3f;
    private float lastInputTime = 0f;

    [Header("Collision Detection")]
    public Collider2D pointerCollider;
    private bool isPointerInHitZone = false;

    private Action onSuccess;
    private Action onFail;

    void Awake()
    {
        // Hide the entire calibration UI on start
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(false);
            Debug.Log("Time Calibration UI hidden on Awake");
        }
        else if (transform.parent != null)
        {
            // If calibrationUI not assigned, try to hide the parent GameObject
            transform.parent.gameObject.SetActive(false);
            Debug.Log("Time Calibration parent UI hidden on Awake");
        }
        else
        {
            // Last resort: hide this GameObject
            gameObject.SetActive(false);
            Debug.Log("Time Calibration GameObject hidden on Awake");
        }
    }

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

        // Hide all hit zones on start
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

        // Show the UI when calibration starts
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(true);
        }
        else if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
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

        Debug.Log("Time Calibration Started! Press SPACE when pointer is in red zone!");
    }

    public void OnPointerEnterHitZone(bool inHitZone)
    {
        if (!isActive) return;
        isPointerInHitZone = inHitZone;
        Debug.Log("POINTER ENTERED HIT ZONE!");
    }

    public void OnPointerExitHitZone(bool inHitZone)
    {
        if (!isActive) return;
        isPointerInHitZone = inHitZone;
        Debug.Log("POINTER EXITED HIT ZONE!");
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

        // Hide UI after completion
        StartCoroutine(HideUIAfterDelay(0.3f));
    }

    IEnumerator HideUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (calibrationUI != null)
        {
            calibrationUI.SetActive(false);
        }
        else if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
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

        // Hide UI when stopped
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(false);
        }
        else if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
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
}