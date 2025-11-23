using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The dialogue ID to trigger from DialogueDatabase")]
    public string dialogueID;

    [Tooltip("Should this trigger only work once?")]
    public bool triggerOnce = true;

    [Tooltip("Can this trigger be activated?")]
    public bool canTrigger = true;

    [Header("Optional: Auto-disable after event")]
    [Tooltip("Disable this trigger after player enters AV room?")]
    public bool disableAfterAVRoomEntry = false;

    private bool hasTriggered = false;

    private void Start()
    {
        // If this should be disabled after AV room entry, check the game state
        if (disableAfterAVRoomEntry && GameStateManager.Instance != null)
        {
            if (GameStateManager.Instance.hasEnteredAVRoom)
            {
                canTrigger = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if player entered and dialogue can be triggered
        if (collision.CompareTag("Player") && canTrigger)
        {
            // Check if should only trigger once
            if (triggerOnce && hasTriggered)
                return;

            // Trigger the dialogue
            if (!string.IsNullOrEmpty(dialogueID))
            {
                DialogueManager.Instance?.StartDialogueSequence(dialogueID);
                hasTriggered = true;
            }
        }
    }

    // Public method to enable/disable this trigger
    public void SetCanTrigger(bool value)
    {
        canTrigger = value;
    }

    // Public method to reset the trigger
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
