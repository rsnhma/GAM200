using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Data Files")]
    public TextAsset dialogueCSV;
    public TextAsset taskCSV;

    private void Awake()
    {
        // Load data at game start
        LoadGameData();
    }

    private void LoadGameData()
    {
        if (dialogueCSV != null)
        {
            DialogueDatabase.LoadDialogues(dialogueCSV.text);
            Debug.Log($"Loaded {DialogueDatabase.dialogues.Count} dialogues");
        }
        else
        {
            Debug.LogError("Dialogue CSV not assigned!");
        }

        if (taskCSV != null)
        {
            DialogueDatabase.LoadTasks(taskCSV.text);
            Debug.Log($"Loaded {DialogueDatabase.tasks.Count} tasks");
        }
        else
        {
            Debug.LogError("Task CSV not assigned!");
        }
    }
}