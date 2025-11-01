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

//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//    public class DialogueCharacter
//    {
//        public string name;
//        public Sprite icon;
//    }

//    [System.Serializable]
//    public class DialogueLine
//    {
//        public DialogueCharacter character;
//        [TextArea(3,10)]
//        public string line;
//    }

//     [System.Serializable]
//    public class Dialogue
//    {
//        public List<DialogueLine> dialogueLines = new List<DialogueLine>();
//    }

//public class DialogueTrigger : MonoBehaviour
//{
//    public Dialogue dialogue;
//    public GameObject triggerObject; 

//    public void TriggerDialogue()
//    {
//        DialogueManager.Instance.StartDialogue(dialogue);
//        DialogueManager.Instance.currentTrigger = this;
//    }
//    private bool hasTriggered = false;

//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Player") && !hasTriggered)
//        {
//            hasTriggered = true;
//            TriggerDialogue();
//        }
//    }

//    private void OnTriggerExit2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Player"))
//        {
//            hasTriggered = false; // reset so it can be triggered again later
//        }
//    }
//}