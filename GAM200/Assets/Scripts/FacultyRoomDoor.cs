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

    [Tooltip("The physical barricade collider (non-trigger) that blocks player")]
    public Collider2D barricadeCollider;

    [Header("Debugging")]
    public bool showDebugLogs = true;

    private BoxCollider2D triggerCollider;
    private bool playerAtDoor = false;
    private bool doorUnlocked = false;

    private void Awake()
    {
        // This script's collider should be the TRIGGER
        triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        // Make sure barricade is enabled at start (blocking entry)
        if (barricadeCollider != null)
        {
            barricadeCollider.enabled = true;
            if (showDebugLogs)
                Debug.Log("Faculty Room door barricade enabled - door is locked");
        }
        else
        {
            Debug.LogError("Barricade Collider not assigned! Please assign the non-trigger collider.");
        }
    }

    private void Update()
    {
        // Player interaction with E key
        if (playerAtDoor && Input.GetKeyDown(KeyCode.E))
        {
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
    }

    private void UnlockDoor()
    {
        if (doorUnlocked) return; // Already unlocked

        doorUnlocked = true;

        // Remove key from inventory AND journal UI (same as rope/bucket in well)
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.RemoveItem(requiredItemID);
            JournalManager.Instance.RemoveItemFromUI(requiredItemID);

            if (showDebugLogs)
                Debug.Log($"Key '{requiredItemID}' removed from inventory and journal");
        }

        // Disable the barricade to allow passage
        if (barricadeCollider != null)
        {
            barricadeCollider.enabled = false;
            if (showDebugLogs)
                Debug.Log("Faculty Room door barricade removed - door is now open!");
        }

        // Optional: Play unlock sound
        SoundManager.Instance?.PlayInteractSound();

        // Optional: Show unlock dialogue
        if (!string.IsNullOrEmpty(lockedDialogueID))
        {
            string unlockDialogueID = "faculty_room_unlocked"; // Create this dialogue if you want
            if (DialogueDatabase.dialogues.ContainsKey(unlockDialogueID))
            {
                DialogueManager.Instance?.StartDialogueSequence(unlockDialogueID);
            }
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
            if (showDebugLogs)
                Debug.Log("Player left faculty room door trigger");
        }
    }
}