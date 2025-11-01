using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AVRoomEntryTrigger : MonoBehaviour
{
    [Header("Entry Settings")]
    [Tooltip("Dialogue ID when player enters AV room")]
    public string entryDialogueID = "av_room_entry";

    [Tooltip("Task ID to complete when entering (e.g., 'find_av_room')")]
    public string taskToCompleteID = "find_av_room";

    [Tooltip("Should this only trigger once?")]
    public bool triggerOnce = true;

    [Header("Debugging")]
    public bool showDebugLogs = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // Check if already triggered
        if (triggerOnce && hasTriggered)
            return;

        // Mark as triggered
        hasTriggered = true;

        if (showDebugLogs)
        {
            Debug.Log("Player entered AV Room for the first time!");
        }

        // Trigger entry dialogue
        if (!string.IsNullOrEmpty(entryDialogueID))
        {
            DialogueManager.Instance?.StartDialogueSequence(entryDialogueID);
        }

        // Complete the "find AV room" task
        if (!string.IsNullOrEmpty(taskToCompleteID) && TaskManager.Instance != null)
        {
            if (TaskManager.Instance.IsTaskActive(taskToCompleteID))
            {
                TaskManager.Instance.CompleteTask(taskToCompleteID);

                if (showDebugLogs)
                {
                    Debug.Log($"Completed task: {taskToCompleteID}");
                }
            }
        }

        // Update game state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnAVRoomEntered();
        }

        // Disable all "explore AV room first" triggers
        DisableOtherRoomBlockers();
    }

    private void DisableOtherRoomBlockers()
    {
        // Find all DialogueTriggers that block other rooms
        DialogueTrigger[] allTriggers = FindObjectsOfType<DialogueTrigger>();

        foreach (DialogueTrigger trigger in allTriggers)
        {
            // Disable triggers that were blocking access until AV room found
            if (trigger.disableAfterAVRoomEntry)
            {
                trigger.SetCanTrigger(false);

                if (showDebugLogs)
                {
                    Debug.Log($"Disabled room blocker: {trigger.gameObject.name}");
                }
            }
        }
    }
}