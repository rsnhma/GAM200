using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEntranceBlocker : MonoBehaviour
{
    [Header("Blocker Settings")]
    [Tooltip("Dialogue ID when player tries to enter before finding AV room")]
    public string blockedDialogueID = "explore_av_room_first";

    [Tooltip("The physical barricade collider (non-trigger) that blocks player")]
    public Collider2D barricadeCollider;

    [Tooltip("Should this trigger only once?")]
    public bool triggerOnce = false;

    [Header("Debugging")]
    public bool showDebugLogs = true;

    private bool playerAtEntrance = false;
    private bool hasTriggered = false;

    private void Start()
    {
        // Check if AV room has already been found
        if (GameStateManager.Instance != null && GameStateManager.Instance.hasEnteredAVRoom)
        {
            // AV room already found - remove barricade immediately
            RemoveBarricade();
        }
        else
        {
            // AV room not found yet - ensure barricade is active
            if (barricadeCollider != null)
            {
                barricadeCollider.enabled = true;

                if (showDebugLogs)
                {
                    Debug.Log($"Barricade enabled for {gameObject.name}");
                }
            }
        }
    }

    private void Update()
    {
        // Continuously check if player has found AV room
        UpdateBarricadeState();

        // If player is at entrance and tries to interact
        if (playerAtEntrance && Input.GetKeyDown(KeyCode.E))
        {
            // Check if already triggered and should only trigger once
            if (triggerOnce && hasTriggered)
                return;

            bool canEnter = CanEnterRoom();

            if (!canEnter)
            {
                hasTriggered = true;

                if (showDebugLogs)
                {
                    Debug.Log($"Blocking entrance to {gameObject.name}");
                }

                // Show dialogue
                if (!string.IsNullOrEmpty(blockedDialogueID))
                {
                    DialogueManager.Instance?.StartDialogueSequence(blockedDialogueID);
                }
            }
            else if (showDebugLogs)
            {
                Debug.Log($"Entrance to {gameObject.name} allowed");
            }
        }
    }

    private void UpdateBarricadeState()
    {
        if (barricadeCollider == null) return;

        bool canEnter = CanEnterRoom();

        // Enable barricade if player can't enter, disable if they can
        if (barricadeCollider.enabled && canEnter)
        {
            // Remove the barricade - player can now enter
            RemoveBarricade();
        }
        else if (!barricadeCollider.enabled && !canEnter)
        {
            // Re-enable barricade
            barricadeCollider.enabled = true;

            if (showDebugLogs)
            {
                Debug.Log($"Barricade re-enabled for {gameObject.name}");
            }
        }
    }

    private bool CanEnterRoom()
    {
        // Player can enter if they've found the AV room
        if (GameStateManager.Instance != null)
        {
            return GameStateManager.Instance.hasEnteredAVRoom;
        }

        // If no GameStateManager, allow entry as fallback
        if (showDebugLogs)
        {
            Debug.LogWarning("GameStateManager not found! Allowing entry as fallback.");
        }
        return true;
    }

    private void RemoveBarricade()
    {
        if (barricadeCollider != null && barricadeCollider.enabled)
        {
            barricadeCollider.enabled = false;

            if (showDebugLogs)
            {
                Debug.Log($"Barricade removed for {gameObject.name}");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtEntrance = true;

            if (showDebugLogs)
            {
                Debug.Log($"Player at entrance trigger for {gameObject.name}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtEntrance = false;

            if (showDebugLogs)
            {
                Debug.Log($"Player left entrance trigger for {gameObject.name}");
            }
        }
    }
}