using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static NoiseSystem;

public class DeskPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private List<DraggableDesk> desks;
    [SerializeField] private Transform seatingChartUI; // Optional: UI showing correct layout
    [SerializeField] private GameObject hiddenClueCanvas; // The CANVAS with the memorabilia panel
    [SerializeField] private CanvasGroup hiddenClueCanvasGroup; // For fade effect
    [SerializeField] private AudioClip solvedSound;
    [SerializeField] private AudioClip wrongPlacementSound;
    [SerializeField] private float snapThreshold = 0.5f; // How close desk needs to be to snap
    [SerializeField] private float fadeInDuration = 2f; // How long the fade-in takes

    [Header("Fail Condition Settings")]
    [SerializeField] private int maxWrongPlacements = 3; // How many wrong placements before fail
    [SerializeField] private float wrongPlacementCheckDelay = 1f; // Delay before checking if placement is wrong

    [Header("Visual Feedback")]
    [SerializeField] private Color correctPositionColor = Color.green;
    [SerializeField] private Color incorrectPositionColor = Color.white;

    [Header("Memorabilia Settings")]
    [SerializeField] private ItemData memorabiliaData; // Optional: add to journal

    [Header("Sanity Settings")]
    [SerializeField] private float sanityReward = 2f; // Sanity gained on success
    [SerializeField] private float sanityLossPerWrongPlacement = 0.5f; // Small loss per wrong placement
    [SerializeField] private float sanityLossOnFail = 2f; // Big loss when puzzle fails

    [Header("Enemy Settings")]
    [SerializeField] private EnemyManager enemyManager;

    [Header("Dialogue Settings")]
    [SerializeField] private string wrongPlacementDialogueID = "desk_wrong_placement";
    [SerializeField] private string puzzleFailedDialogueID = "desk_puzzle_failed";

    private bool puzzleSolved = false;
    private bool puzzleFailed = false;
    private int wrongPlacementCount = 0;
    private AudioSource audioSource;
    private HashSet<DraggableDesk> checkedDesks = new HashSet<DraggableDesk>(); // Track which desks we've checked

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

        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance;
        }

        // Initialize all desks
        foreach (var desk in desks)
        {
            desk.Initialize(this);
        }
    }

    public void CheckPuzzleComplete()
    {
        if (puzzleSolved || puzzleFailed) return;

        // Check if all desks are in correct positions
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

    // Called by DraggableDesk when a desk stops moving
    public void OnDeskPlaced(DraggableDesk desk)
    {
        if (puzzleSolved || puzzleFailed) return;

        // Don't check the same desk multiple times
        if (checkedDesks.Contains(desk)) return;

        StartCoroutine(CheckDeskPlacementAfterDelay(desk));
    }

    private IEnumerator CheckDeskPlacementAfterDelay(DraggableDesk desk)
    {
        yield return new WaitForSeconds(wrongPlacementCheckDelay);

        // If desk is NOT in correct position and it has stopped moving
        if (!desk.IsInCorrectPosition() && !desk.IsMoving())
        {
            checkedDesks.Add(desk); // Mark this desk as checked
            OnWrongPlacement(desk);
        }
    }

    private void OnWrongPlacement(DraggableDesk desk)
    {
        wrongPlacementCount++;

        Debug.Log($"Wrong placement! Count: {wrongPlacementCount}/{maxWrongPlacements}");

        // Play wrong placement sound
        if (audioSource != null && wrongPlacementSound != null)
        {
            audioSource.PlayOneShot(wrongPlacementSound);
        }

        // Lose sanity
        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.LoseSanity(sanityLossPerWrongPlacement);
        }

        // Show dialogue hint
        if (!string.IsNullOrEmpty(wrongPlacementDialogueID))
        {
            DialogueManager.Instance?.StartDialogueSequence(wrongPlacementDialogueID);
        }

        // Make noise to alert enemy
        NoiseSystem.EmitNoise(desk.transform.position, NoiseTypes.PuzzleFailRadius);

        // Check if max wrong placements reached
        if (wrongPlacementCount >= maxWrongPlacements)
        {
            OnPuzzleFailed();
        }
    }

    private void OnPuzzleFailed()
    {
        puzzleFailed = true;

        Debug.Log("Desk puzzle failed! Too many wrong placements!");

        // Big sanity loss
        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.LoseSanity(sanityLossOnFail);
        }

        // Show failure dialogue
        if (!string.IsNullOrEmpty(puzzleFailedDialogueID))
        {
            DialogueManager.Instance?.StartDialogueSequence(puzzleFailedDialogueID);
        }

        // Spawn enemy at nearest TV
        SpawnEnemyAtNearestTV();

        // Reset puzzle after delay
        StartCoroutine(ResetPuzzleAfterDelay(3f));
    }

    private IEnumerator ResetPuzzleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset all desks to their starting scattered positions
        foreach (var desk in desks)
        {
            desk.ResetToStartPosition();
        }

        // Reset counters
        wrongPlacementCount = 0;
        puzzleFailed = false;
        checkedDesks.Clear();

        Debug.Log("Puzzle reset - try again!");
    }

    private void OnPuzzleSolved()
    {
        puzzleSolved = true;
        Debug.Log("Desk puzzle solved!");

        // Play sound effect
        if (audioSource != null && solvedSound != null)
            audioSource.PlayOneShot(solvedSound);

        // Gain sanity reward
        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.GainSanity(sanityReward);
            Debug.Log($"Player gained {sanityReward} sanity for solving the desk puzzle!");
        }

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

    private void SpawnEnemyAtNearestTV()
    {
        if (enemyManager != null && enemyManager.isEnemyActive)
        {
            Debug.Log("Enemy already active - noise will alert it");
            return;
        }

        if (enemyManager == null)
        {
            Debug.LogError("EnemyManager not found!");
            return;
        }

        Transform[] tvSpawnPoints = enemyManager.GetTVSpawnPoints();
        if (tvSpawnPoints == null || tvSpawnPoints.Length == 0)
        {
            Debug.LogError("No TV spawn points found in EnemyManager!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        Transform nearestTV = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Transform spawnPoint in tvSpawnPoints)
        {
            float distance = Vector2.Distance(player.transform.position, spawnPoint.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTV = spawnPoint;
            }
        }

        if (nearestTV != null)
        {
            Debug.Log($"Spawning enemy at nearest TV: {nearestTV.name}");
            enemyManager.ActivateEnemy(nearestTV.position);
        }
        else
        {
            Debug.LogWarning("Could not find TV spawn point!");
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

    public bool IsPuzzleSolved()
    {
        return puzzleSolved;
    }

    public bool IsPuzzleFailed()
    {
        return puzzleFailed;
    }
}