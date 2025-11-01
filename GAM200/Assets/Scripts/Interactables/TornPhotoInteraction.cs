using System.Collections;
using UnityEngine;

public class TornPhotoInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PhotoPuzzleController puzzleController;
    [SerializeField] private float interactionRange = 2f;
    private KeyCode interactionKey = KeyCode.Mouse0; // Left click

    [Header("Dialogue Settings")]
    [SerializeField] private string previewDialogueID = "torn_class_photo_preview";

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private Transform playerTransform;
    private AudioSource audioSource;
    private bool hasBeenUsed = false;
    private bool playerInRange = false;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Find puzzle controller if not assigned
        if (puzzleController == null)
            puzzleController = FindObjectOfType<PhotoPuzzleController>();
    }

    void Update()
    {
        if (hasBeenUsed || playerTransform == null)
            return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionRange)
        {
            playerInRange = true;

            // Check for interaction input
            if (Input.GetKeyDown(interactionKey))
            {
                InteractWithPhoto();
            }
        }
        else
        {
            playerInRange = false;
        }
    }

    private void InteractWithPhoto()
    {
        if (hasBeenUsed)
            return;

        hasBeenUsed = true;

        Debug.Log("InteractWithPhoto called!");

        // Play pickup sound
        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);

        // Show dialogue first, then open puzzle when dialogue closes
        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(previewDialogueID))
        {
            Debug.Log($"Starting dialogue: {previewDialogueID}");
            DialogueManager.Instance.StartDialogueSequence(previewDialogueID);
            StartCoroutine(WaitForDialogueToClose());

            // DON'T hide the object yet - wait for coroutine to finish
        }
        else
        {
            Debug.Log("No dialogue, opening puzzle directly");
            // No dialogue, open puzzle directly
            OpenPuzzleDirectly();
            // Hide the torn photo object
            gameObject.SetActive(false);
        }
    }

    private IEnumerator WaitForDialogueToClose()
    {
        Debug.Log("Waiting for dialogue to close...");

        // Wait until dialogue is no longer active
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

        Debug.Log("Dialogue closed! Opening puzzle...");

        // Small delay after dialogue closes
        yield return new WaitForSeconds(0.3f);

        // Now open the puzzle
        OpenPuzzleDirectly();

        // NOW hide the torn photo object after puzzle opens
        gameObject.SetActive(false);
    }

    private void OpenPuzzleDirectly()
    {
        Debug.Log("OpenPuzzleDirectly called!");

        if (puzzleController != null)
        {
            Debug.Log("Puzzle controller found, calling OpenPuzzle()");
            puzzleController.OpenPuzzle();
        }
        else
        {
            Debug.LogError("Puzzle controller is NULL!");
        }
    }

    // Alternative: Click-based interaction
    private void OnMouseDown()
    {
        if (!hasBeenUsed && playerInRange)
        {
            InteractWithPhoto();
        }
    }

    // Visualize interaction range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}