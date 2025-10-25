using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID;
    public Sprite itemIcon;
    public GameObject itemPrefabForHands; // Assign the prefab that will appear in player's hands
    public bool isPuzzleReward = false; // False = Goes into inventory

    [Header("Mandatory TV Interaction")]
    public TVInteraction tvInteraction;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool playerNearby = false;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;


        // Check if item already collected (works for both types)
        if (isPuzzleReward)
        {
            if (JournalManager.Instance.HasMemorabilia(itemID))
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            if (InventorySystem.Instance.HasItem(itemID))
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Left click to pick up item when player is nearby
        if (playerNearby && Input.GetMouseButtonDown(0))
        {
            Pickup();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    public void Pickup()
    {
        // Show dialogue if it exists
        string dialogueID = itemID + "_pickup";
        if (DialogueDatabase.dialogues.ContainsKey(dialogueID))
        {
           DialogueManager.Instance.StartDialogueSequence(dialogueID);
        }

        gameObject.SetActive(false);

        if (isPuzzleReward)
        {
            // This is memorabilia - add to memorabilia tab
            JournalManager.Instance.AddMemorabilia(itemID);
            Debug.Log($"Collected memorabilia: {itemID}");
        }
        else
        {
            // This is an inventory item - add to inventory tab
            System.Action useAction = null;
            if (tvInteraction != null)
            {
                useAction = () => tvInteraction.HandleVHSUse();
            }

            JournalManager.Instance.UpdateJournalTab(itemID, useAction, itemPrefabForHands);
            Debug.Log($"Collected inventory item: {itemID}");
        }
    }

    public void ResetCollectible()
    {
        if (!isPuzzleReward)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            gameObject.SetActive(true);
        }
    }
}