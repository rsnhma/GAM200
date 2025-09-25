using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class QTESystem : MonoBehaviour
{
    public static QTESystem Instance { get; private set; }

    public Image[] arrowImages;
    public Sprite upArrow, downArrow, leftArrow, rightArrow;

    [Header("QTE Timer Settings")]
    public float timeLimit = 5f; // Time limit in seconds
    public Slider timerSlider; 

    private KeyCode[] possibleKeys = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
    private List<KeyCode> qteSequence = new List<KeyCode>();
    private int currentKeyIndex = 0;

    private Action onSuccess;
    private Action onFail;

    private float currentTime; // Current time remaining
    private bool isQTEActive = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Don't disable the entire GameObject, just start disabled
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Start a QTE sequence of given length
    /// </summary>
    public void BeginQTE(int sequenceLength, Action successCallback, Action failCallback = null)
    {
        // Validate sequence length
        if (sequenceLength <= 0 || sequenceLength > arrowImages.Length)
        {
            Debug.LogError($"Invalid QTE sequence length: {sequenceLength}. Must be between 1 and {arrowImages.Length}");
            failCallback?.Invoke();
            return;
        }

        onSuccess = successCallback;
        onFail = failCallback;

        currentTime = timeLimit;
        isQTEActive = true;

        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(true);
            timerSlider.maxValue = timeLimit;
            timerSlider.value = timeLimit;
        }

        gameObject.SetActive(true);
        GenerateQTESequence(sequenceLength);
    }

    void Update()
    {
        if (!isQTEActive) return;

        // Update timer
        currentTime -= Time.deltaTime;

        // Update timer slider
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;

        }

        // Check for timeout
        if (currentTime <= 0f)
        {
            Debug.Log("QTE Timeout!");
            FailQTE();
            return;
        }

        if (currentKeyIndex < qteSequence.Count)
        {
            KeyCode expectedKey = qteSequence[currentKeyIndex];

            // Check only the specific expected key
            if (Input.GetKeyDown(expectedKey))
            {
                arrowImages[currentKeyIndex].gameObject.SetActive(false);
                currentKeyIndex++;

                if (currentKeyIndex >= qteSequence.Count)
                {
                    CompleteQTE();
                }
            }
            else
            {
                // Check if any OTHER arrow key was pressed (which would be wrong)
                foreach (KeyCode key in possibleKeys)
                {
                    if (key != expectedKey && Input.GetKeyDown(key))
                    {
                        FailQTE();
                        return; // Exit early on wrong key press
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate random arrow sequence
    /// </summary>
    void GenerateQTESequence(int length)
    {
        qteSequence.Clear();
        currentKeyIndex = 0;

        // Reset all arrow images first
        foreach (var image in arrowImages)
        {
            image.gameObject.SetActive(false);
        }

        for (int i = 0; i < length; i++)
        {
            // Safety check for array bounds
            if (i >= arrowImages.Length) break;

            int randomIndex = UnityEngine.Random.Range(0, possibleKeys.Length);
            KeyCode randomKey = possibleKeys[randomIndex];
            qteSequence.Add(randomKey);

            switch (randomKey)
            {
                case KeyCode.UpArrow: arrowImages[i].sprite = upArrow; break;
                case KeyCode.DownArrow: arrowImages[i].sprite = downArrow; break;
                case KeyCode.LeftArrow: arrowImages[i].sprite = leftArrow; break;
                case KeyCode.RightArrow: arrowImages[i].sprite = rightArrow; break;
            }

            arrowImages[i].gameObject.SetActive(true);
        }
    }

    void CompleteQTE()
    {
        Debug.Log("QTE Completed!");
        gameObject.SetActive(false);

        // Safe callback invocation
        try
        {
            onSuccess?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during QTE success callback: {e.Message}");
        }

        ResetQTE(); 
    }

    void FailQTE()
    {
        Debug.Log("QTE Failed!");
        gameObject.SetActive(false);

        // Safe callback invocation
        try
        {
            onFail?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during QTE fail callback: {e.Message}");
        }

        ResetQTE(); 
    }

    void ResetQTE()
    {
        currentKeyIndex = 0;
        qteSequence.Clear();
        currentTime = 0f;
        isQTEActive = false;

        // Reset all arrow images
        foreach (var image in arrowImages)
        {
            if (image != null)
                image.gameObject.SetActive(false);
        }

        // Hide timer slider
        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(false);
        }
    }
}