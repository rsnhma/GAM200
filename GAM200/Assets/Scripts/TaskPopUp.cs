using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskPopup : MonoBehaviour
{
    public static TaskPopup Instance;

    [Header("UI References")]
    [SerializeField] private GameObject popUpObj;
    [SerializeField] private TextMeshProUGUI popupText;

    [Header("Animation Settings")]
    [SerializeField] private float minY = -200f;
    [SerializeField] private float maxY = 100f;
    [SerializeField] private float animDuration = 0.5f;
    [SerializeField] private float stopDuration = 3f;

    private Queue<TaskData> taskDisplayQueue = new Queue<TaskData>();
    private bool isShowing = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        isShowing = false;

        // Set initial position offscreen
        if (popUpObj != null)
        {
            RectTransform rt = popUpObj.GetComponent<RectTransform>();
            Vector2 pos = rt.anchoredPosition;
            pos.y = minY;
            rt.anchoredPosition = pos;
        }
    }

    public void ShowTaskPopup(string taskID)
    {
        if (DialogueDatabase.tasks.ContainsKey(taskID))
        {
            TaskData taskData = DialogueDatabase.tasks[taskID];
            AddTaskToQueue(taskData);
        }
    }

    public void ShowTaskPopup(TaskData taskData)
    {
        AddTaskToQueue(taskData);
    }

    private void AddTaskToQueue(TaskData taskData)
    {
        taskDisplayQueue.Enqueue(taskData);

        if (!isShowing)
        {
            StartCoroutine(ShowTaskCoroutine());
        }
    }

    private IEnumerator ShowTaskCoroutine()
    {
        isShowing = true;

        while (taskDisplayQueue.Count > 0)
        {
            TaskData task = taskDisplayQueue.Dequeue();

            // Set popup text
            if (popupText != null)
            {
                popupText.text = $"New Task:\n{task.taskName}";
            }

            RectTransform rt = popUpObj.GetComponent<RectTransform>();

            // Animate from minY to maxY (slide up)
            float timer = 0f;
            while (timer < animDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, timer / animDuration);
                Vector2 pos = rt.anchoredPosition;
                pos.y = Mathf.Lerp(minY, maxY, t);
                rt.anchoredPosition = pos;
                yield return null;
            }

            // Ensure at maxY
            Vector2 maxPos = rt.anchoredPosition;
            maxPos.y = maxY;
            rt.anchoredPosition = maxPos;

            // Pause at maxY
            yield return new WaitForSeconds(stopDuration);

            // Animate from maxY to minY (slide down)
            timer = 0f;
            while (timer < animDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, timer / animDuration);
                Vector2 pos = rt.anchoredPosition;
                pos.y = Mathf.Lerp(maxY, minY, t);
                rt.anchoredPosition = pos;
                yield return null;
            }

            // Ensure back at minY
            Vector2 minPos = rt.anchoredPosition;
            minPos.y = minY;
            rt.anchoredPosition = minPos;
        }

        isShowing = false;
    }
}