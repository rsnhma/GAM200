using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID;
    public Sprite itemIcon;
    public GameObject itemPrefabForHands; // Assign the prefab that will appear in player's hands
    public bool isPuzzleReward = false;

    [Header("Mandatory TV Interaction")]
    public TVInteraction tvInteraction;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool playerNearby = false;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (InventorySystem.Instance.HasItem(itemID))
        {
            gameObject.SetActive(false);
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
        gameObject.SetActive(false);

        System.Action useAction = null;
        if (tvInteraction != null)
        {
            useAction = () => tvInteraction.HandleVHSUse();
        }

        // Update journal UI (which handles adding to inventory internally)
        JournalManager.Instance.UpdateJournalTab(itemID, useAction, itemPrefabForHands);

        Debug.Log($"Collected {itemID}");
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