using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeskPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private List<DraggableDesk> desks;
    [SerializeField] private Transform seatingChartUI; // Optional: UI showing correct layout
    [SerializeField] private GameObject hiddenClueCanvas; // The CANVAS with the memorabilia panel
    [SerializeField] private CanvasGroup hiddenClueCanvasGroup; // For fade effect
    [SerializeField] private AudioClip solvedSound;
    [SerializeField] private float snapThreshold = 0.5f; // How close desk needs to be to snap
    [SerializeField] private float fadeInDuration = 2f; // How long the fade-in takes

    [Header("Visual Feedback")]
    [SerializeField] private Color correctPositionColor = Color.green;
    [SerializeField] private Color incorrectPositionColor = Color.white;

    [Header("Memorabilia Settings")]
    [SerializeField] private ItemData memorabiliaData; // Optional: add to journal

    private bool puzzleSolved = false;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (hiddenClueCanvas != null)
        {
            hiddenClueCanvas.SetActive(false);

            // Setup CanvasGroup for fade effect
            if (hiddenClueCanvasGroup == null)
            {
                hiddenClueCanvasGroup = hiddenClueCanvas.GetComponent<CanvasGroup>();
                if (hiddenClueCanvasGroup == null)
                {
                    hiddenClueCanvasGroup = hiddenClueCanvas.AddComponent<CanvasGroup>();
                }
            }
        }

        // Initialize all desks
        foreach (var desk in desks)
        {
            desk.Initialize(this);
        }
    }

    public void CheckPuzzleComplete()
    {
        if (puzzleSolved) return;

        // Check if all desks are in correct positions (without Linq)
        bool allCorrect = true;
        foreach (var desk in desks)
        {
            if (!desk.IsInCorrectPosition())
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            OnPuzzleSolved();
        }
    }

    private void OnPuzzleSolved()
    {
        puzzleSolved = true;
        Debug.Log("Desk puzzle solved!");

        // Play sound effect
        if (audioSource != null && solvedSound != null)
            audioSource.PlayOneShot(solvedSound);

        // Add to memorabilia
        if (memorabiliaData != null && JournalManager.Instance != null)
        {
            JournalManager.Instance.AddMemorabilia(memorabiliaData.itemID);
            Debug.Log($"Added {memorabiliaData.itemName} to memorabilia");
        }

        // Lock all desks first
        foreach (var desk in desks)
        {
            desk.LockDesk();
        }

        // Fade in the hidden clue canvas
        if (hiddenClueCanvas != null)
        {
            StartCoroutine(FadeInCluePanel());
        }
    }

    private IEnumerator FadeInCluePanel()
    {
        // Activate the canvas
        hiddenClueCanvas.SetActive(true);

        // Start fully transparent
        if (hiddenClueCanvasGroup != null)
        {
            hiddenClueCanvasGroup.alpha = 0f;
        }

        // Wait a moment for dramatic effect
        yield return new WaitForSeconds(0.5f);

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (hiddenClueCanvasGroup != null)
            {
                hiddenClueCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            }
            yield return null;
        }

        // Ensure it's fully visible
        if (hiddenClueCanvasGroup != null)
        {
            hiddenClueCanvasGroup.alpha = 1f;
        }
    }

    // Call this from the Close Button
    public void CloseHiddenClue()
    {
        if (hiddenClueCanvas != null)
        {
            hiddenClueCanvas.SetActive(false);
        }
    }

    public float GetSnapThreshold()
    {
        return snapThreshold;
    }
}