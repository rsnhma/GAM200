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

    public void BeginQTE(Action successCallback, Action failCallback = null)
    {
        onSuccess = successCallback;
        onFail = failCallback;

        gameObject.SetActive(true);
        GenerateQTESequence();
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

    void GenerateQTESequence()
    {
        qteSequence.Clear();
        currentKeyIndex = 0;

        for (int i = 0; i < 5; i++)
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
