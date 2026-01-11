using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeatingChart : MonoBehaviour
{
    [Header("UI References")]
    public GameObject seatingChartPanel; // The enlarged, readable seating chart UI

    [Header("Dialogue Settings")]
    public string initialDialogueID = "seating_chart_intro";
    public float dialogueDelay = 2f;

    
    private KeyCode interactionKey = KeyCode.Mouse0;
    private float interactionRange = 2f;
    private bool isPlayerInRange = false;
    private bool hasTriggeredInitialDialogue = false;
    private bool isPanelOpen = false;
    private Transform playerTransform;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (seatingChartPanel != null)
        {
            seatingChartPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerInRange = distance <= interactionRange;
        }

        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
            {
                return;
            }

            if (!isPanelOpen)
            {
                Interact();
            }
        }
    }

    private void Interact()
    {
        if (seatingChartPanel != null)
        {
            SoundManager.Instance.PlayInteractSound();
            seatingChartPanel.SetActive(true);
            isPanelOpen = true;

       

            // Show dialogue after viewing (optional)
            if (!hasTriggeredInitialDialogue && !string.IsNullOrEmpty(initialDialogueID))
            {
                StartCoroutine(ShowDialogueAfterDelay(initialDialogueID, true));
            }
        }
    }


    private IEnumerator ShowDialogueAfterDelay(string dialogueID, bool isFirstTime)
    {
        yield return new WaitForSeconds(dialogueDelay);
        CloseSeatingChart();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueSequence(dialogueID);
        }

        if (isFirstTime)
        {
            hasTriggeredInitialDialogue = true;
        }
    }

    public void CloseSeatingChart()
    {
        if (seatingChartPanel != null)
        {
            seatingChartPanel.SetActive(false);
            isPanelOpen = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

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