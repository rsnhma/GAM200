using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
    public class DialogueCharacter
    {
        public string name;
        public Sprite icon;
    }

    [System.Serializable]
    public class DialogueLine
    {
        public DialogueCharacter character;
        [TextArea(3,10)]
        public string line;
    }

     [System.Serializable]
    public class Dialogue
    {
        public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    }

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public GameObject triggerObject; 

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
        DialogueManager.Instance.currentTrigger = this;
    }
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hasTriggered = false; // reset so it can be triggered again later
        }
    }
}
