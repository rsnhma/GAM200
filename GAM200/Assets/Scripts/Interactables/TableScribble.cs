using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TableScribble : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tableScribblePanel;

    [Header("Dialogue Settings")]
    public string initialDialogueID = "table_scribble";
    public float dialogueDelay = 2f;

    private KeyCode interactionKey = KeyCode.Mouse0; // Left click
    private float interactionRange = 2f;
    private bool isPlayerInRange = false;
    private bool hasTriggeredInitialDialogue = false;
    private bool isPanelOpen = false;
    private Transform playerTransform;

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Hide panel initially
        if (tableScribblePanel != null)
        {
            tableScribblePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Check if player is in range
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerInRange = distance <= interactionRange;
        }

        // Handle interaction - works every time player clicks
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            // Don't interact if dialogue is currently active
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
            {
                return;
            }

            // Don't interact if panel is already open
            if (!isPanelOpen)
            {
                Interact();
            }
        }
    }

    private void Interact()
    {
        if (tableScribblePanel != null)
        {
            tableScribblePanel.SetActive(true);
            isPanelOpen = true;


            // Determine which dialogue to show
            if (!hasTriggeredInitialDialogue)
            {
                // First time interaction - trigger main dialogue and task after delay
                StartCoroutine(ShowDialogueAfterDelay(initialDialogueID, true));
            }
        }
    }

    private IEnumerator ShowDialogueAfterDelay(string dialogueID, bool isFirstTime)
    {
        yield return new WaitForSeconds(dialogueDelay);

        // Close bulletin board panel
        CloseTableInspect();

        // Start dialogue sequence
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueSequence(dialogueID);
        }

        // Mark as triggered after first dialogue starts
        if (isFirstTime)
        {
            hasTriggeredInitialDialogue = true;
        }
    }

    public void CloseTableInspect()
    {
        if (tableScribblePanel != null)
        {
            tableScribblePanel.SetActive(false);
            isPanelOpen = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    // Trigger-based interaction (alternative)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
