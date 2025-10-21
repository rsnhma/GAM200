using System;
using System.Collections.Generic;

[Serializable]
public class DialogueData
{
    public string dialogueID;
    public string speakerName;
    public string dialogueText;
    public string nextDialogueID;
    public string triggerTaskID;
}

[Serializable]
public class TaskData
{
    public string taskID;
    public string taskName;
    public string taskDescription;
    public string category;
    public string completionDialogueID;
}

public static class DialogueDatabase
{
    public static Dictionary<string, DialogueData> dialogues = new Dictionary<string, DialogueData>();
    public static Dictionary<string, TaskData> tasks = new Dictionary<string, TaskData>();

    public static void LoadDialogues(string csvText)
    {
        dialogues.Clear();
        string[] lines = csvText.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            if (values.Length < 5) continue;

            DialogueData data = new DialogueData
            {
                dialogueID = values[0].Trim(),
                speakerName = values[1].Trim(),
                dialogueText = values[2].Trim().Trim('"'),
                nextDialogueID = values[3].Trim(),
                triggerTaskID = values[4].Trim()
            };

            dialogues[data.dialogueID] = data;
        }
    }

    public static void LoadTasks(string csvText)
    {
        tasks.Clear();
        string[] lines = csvText.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            if (values.Length < 5) continue;

            TaskData data = new TaskData
            {
                taskID = values[0].Trim(),
                taskName = values[1].Trim(),
                taskDescription = values[2].Trim(),
                category = values[3].Trim(),
                completionDialogueID = values[4].Trim()
            };

            tasks[data.taskID] = data;
        }
    }
}