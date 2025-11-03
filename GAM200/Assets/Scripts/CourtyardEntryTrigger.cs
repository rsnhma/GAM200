using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtyardEntryTrigger : MonoBehaviour
{
    [Header("Entry Settings")]
    public string entryDialogueID = "courtyard_entry";
    private string taskToCompleteID = "head_to_the_courtyard_afterwards";

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
            Debug.Log("Player entered Courtyard for the first time!");
        }

        // Trigger entry dialogue
        if (!string.IsNullOrEmpty(entryDialogueID))
        {
            DialogueManager.Instance?.StartDialogueSequence(entryDialogueID);
        }

        // Complete the courtyard task
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
            GameStateManager.Instance.OnCourtyardEntered();
        }


    }

}