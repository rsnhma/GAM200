using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BulletinBoard : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bulletinBoardPanel;

    [Header("Dialogue Settings")]
    public string initialDialogueID = "bulletin_board_intro";
    public float dialogueDelay = 2f;

    [Header("Memorabilia Settings")]
    public ItemData memorabiliaData; // ScriptableObject here
    public bool addToMemorabiliaOnFirstView = true;

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

        if (bulletinBoardPanel != null)
        {
            bulletinBoardPanel.SetActive(false);
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
        if (bulletinBoardPanel != null)
        {
            SoundManager.Instance.PlayInteractSound();
            bulletinBoardPanel.SetActive(true);
            isPanelOpen = true;

            // Add to memorabilia on first interaction
            if (addToMemorabiliaOnFirstView && !hasTriggeredInitialDialogue && memorabiliaData != null)
            {
                AddToMemorabilia();
            }

            if (!hasTriggeredInitialDialogue)
            {
                StartCoroutine(ShowDialogueAfterDelay(initialDialogueID, true));
            }
        }
    }

    private void AddToMemorabilia()
    {
        if (JournalManager.Instance != null && memorabiliaData != null)
        {
            JournalManager.Instance.AddMemorabilia(memorabiliaData.itemID);
            Debug.Log($"Added {memorabiliaData.itemName} to memorabilia");
        }
    }

    private IEnumerator ShowDialogueAfterDelay(string dialogueID, bool isFirstTime)
    {
        yield return new WaitForSeconds(dialogueDelay);

        CloseBulletinBoard();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueSequence(dialogueID);
        }

        if (isFirstTime)
        {
            hasTriggeredInitialDialogue = true;
        }
    }

    public void CloseBulletinBoard()
    {
        if (bulletinBoardPanel != null)
        {
            bulletinBoardPanel.SetActive(false);
            isPanelOpen = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
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