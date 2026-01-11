using UnityEngine;
using System.Collections.Generic;

public class DeskPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private List<DraggableDesk> desks;
    [SerializeField] private Transform seatingChartUI; // Optional: UI showing correct layout
    [SerializeField] private GameObject hiddenClue; // The memorabilia that appears when solved
    [SerializeField] private AudioClip solvedSound;
    [SerializeField] private float snapThreshold = 0.5f; // How close desk needs to be to snap

    [Header("Visual Feedback")]
    [SerializeField] private Color correctPositionColor = Color.green;
    [SerializeField] private Color incorrectPositionColor = Color.white;

    private bool puzzleSolved = false;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (hiddenClue != null)
            hiddenClue.SetActive(false);

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

        // Reveal the hidden clue
        if (hiddenClue != null)
        {
            hiddenClue.SetActive(true);
        }

        // Lock all desks
        foreach (var desk in desks)
        {
            desk.LockDesk();
        }
    }

    public float GetSnapThreshold()
    {
        return snapThreshold;
    }
}