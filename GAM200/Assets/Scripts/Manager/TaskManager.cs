using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    [Header("UI References")]
    public Transform taskListContainer; // Parent transform for task UI elements
    public GameObject taskUIPrefab; // Prefab with TaskUI component

    [Header("Task Tracking")]
    private Dictionary<string, TaskUI> activeTasks = new Dictionary<string, TaskUI>();
    private List<string> completedTaskIDs = new List<string>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddTask(string taskID)
    {
        if (activeTasks.ContainsKey(taskID))
        {
            Debug.Log($"Task {taskID} already active");
            return;
        }

        if (!DialogueDatabase.tasks.ContainsKey(taskID))
        {
            Debug.LogError($"Task {taskID} not found in database");
            return;
        }

        TaskData taskData = DialogueDatabase.tasks[taskID];

        // Create task UI
        GameObject taskObj = Instantiate(taskUIPrefab, taskListContainer);
        TaskUI taskUI = taskObj.GetComponent<TaskUI>();

        if (taskUI != null)
        {
            taskUI.SetTask(taskID, taskData.taskDescription);
            activeTasks[taskID] = taskUI;

            // Show task popup
            TaskPopup.Instance?.ShowTaskPopup(taskData);
        }
    }

    public void CompleteTask(string taskID)
    {
        if (!activeTasks.ContainsKey(taskID))
        {
            Debug.Log($"Task {taskID} is not active");
            return;
        }

        // Mark task as completed in UI
        TaskUI taskUI = activeTasks[taskID];
        taskUI.MarkAsCompleted();

        // Add to completed list
        completedTaskIDs.Add(taskID);

        // Trigger completion dialogue if exists
        if (DialogueDatabase.tasks.ContainsKey(taskID))
        {
            TaskData taskData = DialogueDatabase.tasks[taskID];
            if (!string.IsNullOrEmpty(taskData.completionDialogueID))
            {
                DialogueManager.Instance?.StartDialogueSequence(taskData.completionDialogueID);
            }
        }

        Debug.Log($"Task {taskID} completed!");
    }

    public bool IsTaskActive(string taskID)
    {
        return activeTasks.ContainsKey(taskID);
    }

    public bool IsTaskCompleted(string taskID)
    {
        return completedTaskIDs.Contains(taskID);
    }

    public void RemoveTask(string taskID)
    {
        if (activeTasks.ContainsKey(taskID))
        {
            Destroy(activeTasks[taskID].gameObject);
            activeTasks.Remove(taskID);
        }
    }
}