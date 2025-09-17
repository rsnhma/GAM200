using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System; // For Action callbacks

public class QTESystem : MonoBehaviour
{
    public Image[] arrowImages;
    public Sprite upArrow, downArrow, leftArrow, rightArrow;

    private KeyCode[] possibleKeys = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
    private List<KeyCode> qteSequence = new List<KeyCode>();
    private int currentKeyIndex = 0;

    private Action onSuccess; // Callback for success
    private Action onFail;    // Callback for fail

    /// <summary>
    /// Start a QTE sequence of given length
    /// </summary>
    public void BeginQTE(int sequenceLength, Action successCallback, Action failCallback = null)
    {
        onSuccess = successCallback;
        onFail = failCallback;

        gameObject.SetActive(true);
        GenerateQTESequence(sequenceLength);
    }

    void Update()
    {
        if (Input.anyKeyDown && currentKeyIndex < qteSequence.Count)
        {
            KeyCode expectedKey = qteSequence[currentKeyIndex];

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
                // Wrong key pressed
                FailQTE();
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

        for (int i = 0; i < length; i++)
        {
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
        onSuccess?.Invoke();
    }

    void FailQTE()
    {
        Debug.Log("QTE Failed!");
        gameObject.SetActive(false);
        onFail?.Invoke();
    }
}
