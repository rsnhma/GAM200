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

    void Awake()
    {
        // Get components in Awake - they should be available immediately
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        photoCollider = GetComponent<Collider2D>();
        if (photoCollider == null)
        {
            photoCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        if (photoCollider != null)
        {
            photoCollider.enabled = true;
        }
    }

    void Start()
    {
        // Try to find player in Start (might be more reliable)
        FindPlayer();

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
    }

    void OnEnable()
    {
        // Also try to find player when this object becomes enabled
        FindPlayer();
    }

    void Update()
    {
        if (hasBeenUsed)
        {
            if (showDebugLogs && Time.frameCount % 60 == 0)
                Debug.Log($"TornPhoto: Update - hasBeenUsed = true, skipping interaction checks");
            return;
        }

        // Ensure we have a player reference
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null)
            {
                // Still no player, skip interaction checks
                if (showDebugLogs && Time.frameCount % 120 == 0) // Log every 2 seconds to avoid spam
                    Debug.LogWarning("TornPhoto: Still waiting for player reference...");
                return;
            }
        }

        // Now we can safely use playerTransform
        CheckPlayerDistanceAndInteract();
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return; // Already found

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            if (showDebugLogs)
                Debug.Log("TornPhoto: Player found successfully in FindPlayer()");
        }
        else
        {
            // Try alternative methods to find player
            player = GameObject.Find("Player"); // Try by name
            if (player != null)
            {
                playerTransform = player.transform;
                if (showDebugLogs)
                    Debug.Log("TornPhoto: Player found by name 'Player'");
                return;
            }

            // Try finding any object with PlayerController or similar
            MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour obj in allObjects)
            {
                if (obj.gameObject.name.ToLower().Contains("player"))
                {
                    playerTransform = obj.transform;
                    if (showDebugLogs)
                        Debug.Log($"TornPhoto: Player found by name containing 'player': {obj.gameObject.name}");
                    return;
                }
            }

            if (showDebugLogs)
                Debug.LogWarning("TornPhoto: Could not find player object");
        }
    }

    private void CheckPlayerDistanceAndInteract()
    {
        // Double-check player reference right before using it
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null)
            {
                if (showDebugLogs)
                    Debug.LogError("TornPhoto: Player reference is still null in CheckPlayerDistanceAndInteract!");
                return;
            }
        }

        // Safe distance calculation
        Vector2 photoPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos = new Vector2(playerTransform.position.x, playerTransform.position.y);
        float distance = Vector2.Distance(photoPos, playerPos);

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

        // Debug current state periodically
        if (showDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log($"TornPhoto: State - InRange: {playerInRange}, Distance: {distance:F2}, HasBeenUsed: {hasBeenUsed}");
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

    // Debug method to check current state
    public void DebugState()
    {
        Debug.Log($"=== TORN PHOTO DEBUG STATE ===");
        Debug.Log($"Player Transform: {playerTransform}");
        Debug.Log($"Has Been Used: {hasBeenUsed}");
        Debug.Log($"Player In Range: {playerInRange}");
        Debug.Log($"Collider Enabled: {photoCollider != null && photoCollider.enabled}");
        Debug.Log($"GameObject Active: {gameObject.activeInHierarchy}");

        if (playerTransform != null)
        {
            Vector2 photoPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(playerTransform.position.x, playerTransform.position.y);
            float distance = Vector2.Distance(photoPos, playerPos);
            Debug.Log($"Distance to Player: {distance:F2}");
            Debug.Log($"Interaction Range: {interactionRange}");
            Debug.Log($"In Range: {distance <= interactionRange}");
        }

        Debug.Log($"=== END DEBUG ===");
    }
}