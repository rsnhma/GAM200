using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BulletinBoard : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bulletinBoardPanel;
    //public TextMeshProUGUI bulletinBoardText;

    [Header("Dialogue Settings")]
    public string initialDialogueID = "bulletin_board_intro";
    public float dialogueDelay = 2f;

    private KeyCode interactionKey = KeyCode.Mouse0; // Left click
    private float interactionRange = 2f;

    private bool isPlayerInRange = false;
    private bool hasBeenRead = false;
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
        if (bulletinBoardPanel != null)
        {
            bulletinBoardPanel.SetActive(false);
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

        // Handle interaction
        if (isPlayerInRange && Input.GetKeyDown(interactionKey) && !hasBeenRead)
        {
            Interact();
        }

        // Allow closing with ESC or clicking again
        if (bulletinBoardPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseBulletinBoard();
        }
    }

    private void Interact()
    {
        if (bulletinBoardPanel != null)
        {
            bulletinBoardPanel.SetActive(true);
            hasBeenRead = true;

            // Set bulletin board content
            //if (bulletinBoardText != null)
            //{
            //    bulletinBoardText.text = "MISSING PERSON\n\nLast seen at the AV room.\nIf you have any information, please report to the office.";
            //}

            // Start dialogue after delay
            StartCoroutine(ShowDialogueAfterDelay());
        }
    }

    private IEnumerator ShowDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDelay);

        // Close bulletin board panel
        if (bulletinBoardPanel != null)
        {
            bulletinBoardPanel.SetActive(false);
        }

        // Start dialogue sequence
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueSequence(initialDialogueID);
        }
    }

    public void CloseBulletinBoard()
    {
        if (bulletinBoardPanel != null)
        {
            bulletinBoardPanel.SetActive(false);
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