using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI taskDescriptionText;
    public Image background;

    private string taskID;
    private string taskName;
    private string taskDescription;
    private bool isCompleted = false;

    public void SetTask(string id, string description)
    {
        taskID = id;
        // Get full task data from database
        if (DialogueDatabase.tasks.ContainsKey(id))
        {
            TaskData data = DialogueDatabase.tasks[id];
            taskName = data.taskName;
            taskDescription = data.taskDescription;
        }
        else
        {
            taskName = "Unknown Task";
            taskDescription = description;
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update description
        if (taskDescriptionText != null)
        {
            taskDescriptionText.text = taskDescription;
        }
    }

    public void MarkAsCompleted()
    {
        isCompleted = true;
        // Just destroy it immediately
        Destroy(gameObject);
    }

  
    // Public getters
    public string GetTaskID() => taskID;
    public string GetTaskName() => taskName;
    public string GetTaskDescription() => taskDescription;
    public bool IsCompleted() => isCompleted;
}