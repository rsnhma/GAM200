using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI taskDescriptionText;
    public Image background;

    [Header("Visual Settings")]
    public Color activeColor = Color.white;
    public Color completedColor = Color.green;
    [Range(0.3f, 1f)]
    public float completedAlpha = 0.5f;

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

        UpdateVisuals();
    }

    public void MarkAsCompleted()
    {
        isCompleted = true;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (isCompleted)
        {
            // Update description styling
            if (taskDescriptionText != null)
            {
                taskDescriptionText.color = completedColor;
                taskDescriptionText.fontStyle = FontStyles.Strikethrough;
            }

            // Fade background
            if (background != null)
            {
                Color bgColor = background.color;
                bgColor.a = completedAlpha;
                background.color = bgColor;
            }
        }
        else
        {

            // Reset description styling
            if (taskDescriptionText != null)
            {
                taskDescriptionText.color = activeColor;
                taskDescriptionText.fontStyle = FontStyles.Normal;
            }

            // Reset background
            if (background != null)
            {
                Color bgColor = background.color;
                bgColor.a = 1f;
                background.color = bgColor;
            }
        }
    }

    // Public getters
    public string GetTaskID() => taskID;
    public string GetTaskName() => taskName;
    public string GetTaskDescription() => taskDescription;
    public bool IsCompleted() => isCompleted;
}