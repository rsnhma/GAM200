using System.Collections;
using UnityEngine;

public class TornPhotoInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PhotoPuzzleController puzzleController;
    [SerializeField] private float interactionRange = 2f;

    [Header("Interaction Settings")]
    [SerializeField] private bool useClickInteraction = true; // Click on object
    [SerializeField] private bool useKeyInteraction = true;   // Press E when near
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Dialogue Settings")]
    [SerializeField] private string previewDialogueID = "torn_class_photo_preview";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Transform playerTransform;
    private AudioSource audioSource;
    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private Collider2D photoCollider;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            if (showDebugLogs)
                Debug.Log("TornPhoto: Player found");
        }
        else
        {
            Debug.LogError("TornPhoto: Player not found!");
        }

        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Find puzzle controller if not assigned
        if (puzzleController == null)
        {
            puzzleController = FindObjectOfType<PhotoPuzzleController>();
            if (puzzleController == null)
            {
                Debug.LogError("TornPhoto: PhotoPuzzleController not found!");
            }
            else if (showDebugLogs)
            {
                Debug.Log("TornPhoto: PhotoPuzzleController found");
            }
        }

        // Get collider
        photoCollider = GetComponent<Collider2D>();
        if (photoCollider == null)
        {
            Debug.LogError("TornPhoto: No Collider2D found! Adding one...");
            photoCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // Ensure collider is enabled
        if (photoCollider != null)
        {
            photoCollider.enabled = true;
            if (showDebugLogs)
                Debug.Log($"TornPhoto: Collider enabled = {photoCollider.enabled}");
        }
    }

    void Update()
    {
        if (hasBeenUsed || playerTransform == null)
            return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        // Debug when player enters/exits range
        if (showDebugLogs && playerInRange != wasInRange)
        {
            if (playerInRange)
                Debug.Log($"TornPhoto: Player entered range (distance: {distance:F2})");
            else
                Debug.Log($"TornPhoto: Player left range (distance: {distance:F2})");
        }

        // Key-based interaction (E key)
        if (useKeyInteraction && playerInRange && Input.GetKeyDown(interactionKey))
        {
            if (showDebugLogs)
                Debug.Log("TornPhoto: E key pressed while in range");
            InteractWithPhoto();
        }

        // Mouse click interaction (backup method)
        if (useClickInteraction && playerInRange && Input.GetMouseButtonDown(0))
        {
            // Check if mouse is over this object
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (photoCollider != null && photoCollider.OverlapPoint(mousePos))
            {
                if (showDebugLogs)
                    Debug.Log("TornPhoto: Mouse clicked on photo collider");
                InteractWithPhoto();
            }
        }
    }

    private void InteractWithPhoto()
    {
        if (hasBeenUsed)
        {
            if (showDebugLogs)
                Debug.Log("TornPhoto: Already used, ignoring interaction");
            return;
        }

        hasBeenUsed = true;

        if (showDebugLogs)
            Debug.Log("TornPhoto: InteractWithPhoto called!");

        // Play interaction sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayInteractSound();
        }

        // Show dialogue first, then open puzzle when dialogue closes
        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(previewDialogueID))
        {
            if (showDebugLogs)
                Debug.Log($"TornPhoto: Starting dialogue: {previewDialogueID}");

            DialogueManager.Instance.StartDialogueSequence(previewDialogueID);
            StartCoroutine(WaitForDialogueToClose());
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("TornPhoto: No dialogue, opening puzzle directly");

            OpenPuzzleDirectly();
            gameObject.SetActive(false);
        }
    }

    private IEnumerator WaitForDialogueToClose()
    {
        if (showDebugLogs)
            Debug.Log("TornPhoto: Waiting for dialogue to close...");

        // Wait until dialogue is no longer active
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

        if (showDebugLogs)
            Debug.Log("TornPhoto: Dialogue closed! Opening puzzle...");

        // Small delay after dialogue closes
        yield return new WaitForSeconds(0.3f);

        // Now open the puzzle
        OpenPuzzleDirectly();

        // Hide the torn photo object after puzzle opens
        gameObject.SetActive(false);
    }

    private void OpenPuzzleDirectly()
    {
        if (showDebugLogs)
            Debug.Log("TornPhoto: OpenPuzzleDirectly called!");

        if (puzzleController != null)
        {
            if (showDebugLogs)
                Debug.Log("TornPhoto: Puzzle controller found, calling OpenPuzzle()");

            puzzleController.OpenPuzzle();
        }
        else
        {
            Debug.LogError("TornPhoto: Puzzle controller is NULL!");
        }
    }

    // OnMouseDown can be blocked by UI - use as backup only
    private void OnMouseDown()
    {
        if (showDebugLogs)
            Debug.Log($"TornPhoto: OnMouseDown called - hasBeenUsed: {hasBeenUsed}, playerInRange: {playerInRange}");

        if (!hasBeenUsed && playerInRange)
        {
            InteractWithPhoto();
        }
        else if (!playerInRange)
        {
            if (showDebugLogs)
                Debug.Log("TornPhoto: OnMouseDown ignored - player not in range");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (showDebugLogs)
                Debug.Log("TornPhoto: Player entered trigger zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (showDebugLogs)
                Debug.Log("TornPhoto: Player left trigger zone");
        }
    }

    // Visualize interaction range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    // Public method to check if photo can be interacted with
    public bool CanInteract()
    {
        return !hasBeenUsed && playerInRange;
    }

    // Public method to force reset (for debugging)
    public void ResetInteraction()
    {
        hasBeenUsed = false;
        if (showDebugLogs)
            Debug.Log("TornPhoto: Interaction reset");
    }
}