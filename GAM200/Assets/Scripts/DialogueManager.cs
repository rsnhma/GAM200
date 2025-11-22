using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;
    public Button dialogueButton;
    public TextMeshProUGUI buttonText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;
    [Tooltip("Play typing sound every N characters (1 = every character, 2 = every other character)")]
    public int typingSoundFrequency = 4;

    private Queue<DialogueData> dialogueQueue = new Queue<DialogueData>();
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private string currentFullText = "";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        if (dialogueButton != null)
        {
            dialogueButton.onClick.AddListener(OnDialogueButtonClick);
        }
    }

    public void StartDialogueSequence(string startDialogueID)
    {
        dialogueQueue.Clear();

        // Build dialogue chain
        string currentID = startDialogueID;
        while (!string.IsNullOrEmpty(currentID) && DialogueDatabase.dialogues.ContainsKey(currentID))
        {
            DialogueData data = DialogueDatabase.dialogues[currentID];
            dialogueQueue.Enqueue(data);
            currentID = data.nextDialogueID;
        }

        if (dialogueQueue.Count > 0)
        {
            isDialogueActive = true;
            dialoguePanel.SetActive(true);
            DisplayNextDialogue();
        }
    }

    private void DisplayNextDialogue()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueData currentDialogue = dialogueQueue.Dequeue();
        characterName.text = currentDialogue.speakerName;
        currentFullText = currentDialogue.dialogueText;

        // Update button text based on remaining dialogues
        UpdateButtonText();

        // Start typing effect
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentFullText));

        // Check if this dialogue should trigger a task
        if (!string.IsNullOrEmpty(currentDialogue.triggerTaskID))
        {
            TaskManager.Instance?.AddTask(currentDialogue.triggerTaskID);
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueArea.text = "";

        int charCount = 0;
        foreach (char c in text)
        {
            dialogueArea.text += c;

            // Play typing sound every N characters (skip spaces)
            if (c != ' ' && charCount % typingSoundFrequency == 0)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayDialogueSound();
                }
            }

            charCount++;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void OnDialogueButtonClick()
    {
        //if (SoundManager.Instance != null)
        //{
        //    SoundManager.Instance.PlayClickSound();
        //}

        if (isTyping)
        {
            // Skip typing animation
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueArea.text = currentFullText;
            isTyping = false;
        }
        else
        {
            // Move to next dialogue or close
            DisplayNextDialogue();
        }
    }

    private void UpdateButtonText()
    {
        if (dialogueQueue.Count > 0)
        {
            buttonText.text = "Continue";
        }
        else
        {
            buttonText.text = "Close";
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}