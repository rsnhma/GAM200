using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacultyRoomDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("Dialogue ID when player tries to enter without the key equipped")]
    public string lockedDialogueID = "faculty_room_locked";

    [Tooltip("The item ID required to open this door")]
    public string requiredItemID = "well_key";

    [Header("Door Components")]
    [Tooltip("The NON-TRIGGER collider that physically blocks the player")]
    public Collider2D blockingCollider;

    [Tooltip("The MapTransition script that handles teleportation")]
    public MapTransition mapTransitionScript;

    [Header("Debugging")]
    public bool showDebugLogs = true;

    private bool playerAtDoor = false;
    private bool doorUnlocked = false;
    private bool hasConsumedKeyPress = false;

    private void Awake()
    {
        // Make sure blocking collider is enabled at start
        if (blockingCollider != null)
        {
            blockingCollider.enabled = true;
            if (showDebugLogs)
                Debug.Log($"Faculty door blocking collider enabled on: {blockingCollider.gameObject.name}");
        }
        else
        {
            Debug.LogError("Blocking Collider not assigned! Please assign the NON-TRIGGER collider.");
        }

        if (mapTransitionScript == null)
        {
            Debug.LogError("MapTransition script not assigned!");
        }
    }

    private void Update()
    {
        // Check if MapTransition is trying to be used
        if (playerAtDoor && Input.GetKeyDown(KeyCode.E))
        {
            if (!doorUnlocked)
            {
                // Door is locked - intercept the keypress
                hasConsumedKeyPress = true;

                if (PlayerHasEquippedKey())
                {
                    if (showDebugLogs)
                        Debug.Log("Player used key - unlocking door!");

                    UnlockDoor();
                }
                else
                {
                    if (showDebugLogs)
                        Debug.Log("Door is locked - key not equipped.");

                    // Trigger locked dialogue
                    if (!string.IsNullOrEmpty(lockedDialogueID))
                    {
                        DialogueManager.Instance?.StartDialogueSequence(lockedDialogueID);
                    }
                }
            }
            else
            {
                // Door is unlocked - let MapTransition handle it
                if (showDebugLogs)
                    Debug.Log("Door unlocked - MapTransition should handle teleportation");
            }
        }

        // Reset key consumption flag
        if (!Input.GetKeyDown(KeyCode.E))
        {
            hasConsumedKeyPress = false;
        }
    }

    private void UnlockDoor()
    {
        if (doorUnlocked) return;

        doorUnlocked = true;

        // Remove key from inventory AND journal UI
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.RemoveItem(requiredItemID);
            if (JournalManager.Instance != null)
            {
                JournalManager.Instance.RemoveItemFromUI(requiredItemID);
            }

            if (showDebugLogs)
                Debug.Log($"Key '{requiredItemID}' removed from inventory and journal");
        }

        // Disable the blocking collider
        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
            if (showDebugLogs)
                Debug.Log($"Blocking collider disabled on: {blockingCollider.gameObject.name}");
        }

        if (showDebugLogs)
            Debug.Log("Faculty Room door is now unlocked! Press E again to enter.");

        // Play unlock sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayInteractSound();
        }

        // Show unlock dialogue
        string unlockDialogueID = "faculty_room_unlocked";
        if (DialogueDatabase.dialogues.ContainsKey(unlockDialogueID))
        {
            DialogueManager.Instance?.StartDialogueSequence(unlockDialogueID);
        }
    }

    private bool PlayerHasEquippedKey()
    {
        if (InventorySystem.Instance != null)
        {
            return InventorySystem.Instance.GetEquippedItemID() == requiredItemID;
        }

        if (showDebugLogs)
            Debug.LogWarning("InventorySystem not found! Defaulting to no key.");

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = true;
            if (showDebugLogs)
                Debug.Log("Player at faculty room door trigger");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = false;
            hasConsumedKeyPress = false;
            if (showDebugLogs)
                Debug.Log("Player left faculty room door trigger");
        }
    }

    // Public method to check if door blocks interaction
    public bool IsDoorLocked()
    {
        return !doorUnlocked;
    }
}