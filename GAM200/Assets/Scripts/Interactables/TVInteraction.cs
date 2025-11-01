using UnityEngine;
using System.Collections;
using TMPro;

public class TVInteraction : MonoBehaviour
{
    [Header("Required Item")]
    public string vhsItemID = "vhs_tape";

    [Header("Enemy Spawn")]
    public MainEnemy enemyPrefab;
    public Transform spawnPoint;

    [Header("UI Elements")]
    public TextMeshProUGUI tvPromptText;

    private bool vhsInserted = false;
    private bool hasBeenUsed = false;
    private bool playerNearby = false;

    private void Start()
    {
        if (tvPromptText)
        {
            tvPromptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerNearby) return;

        UpdateInteractionPrompt();

        // Handle E key press for VHS insertion
        if (!vhsInserted && Input.GetKeyDown(KeyCode.E))
        {
            if (IsItemEquipped(vhsItemID))
            {
                HandleVHSUse();
            }
            else if (InventorySystem.Instance.GetEquippedItemID() != null)
            {
                // Player has wrong item equipped
                Debug.Log("Wrong item equipped. Need VHS tape!");
                // Optional: Add dialogue here if you want
            }
        }
    }

    private bool IsItemEquipped(string itemID)
    {
        string equippedID = InventorySystem.Instance.GetEquippedItemID();
        return equippedID == itemID;
    }

    private void UpdateInteractionPrompt()
    {
        if (tvPromptText == null) return;

        if (!vhsInserted && !hasBeenUsed)
        {
            if (IsItemEquipped(vhsItemID))
            {
                tvPromptText.text = "[E] Insert VHS Tape";
                tvPromptText.gameObject.SetActive(true);
            }
            else
            {
                tvPromptText.gameObject.SetActive(false);
            }
        }
        else
        {
            tvPromptText.gameObject.SetActive(false);
        }
    }

    public void HandleVHSUse()
    {
        if (hasBeenUsed) return;

        vhsInserted = true;

        // Remove VHS from inventory
        InventorySystem.Instance.RemoveItem(vhsItemID);
        JournalManager.Instance.RemoveItemFromUI(vhsItemID);

        StartCoroutine(InsertVHS());
    }

    private IEnumerator InsertVHS()
    {
        Debug.Log("VHS inserted into TV");
        hasBeenUsed = true;

        // Hide prompt immediately
        if (tvPromptText)
        {
            tvPromptText.gameObject.SetActive(false);
        }

        // Optional: Show dialogue or cutscene
        if (DialogueDatabase.dialogues.ContainsKey("vhs_inserted"))
        {
            DialogueManager.Instance.StartDialogueSequence("vhs_inserted");
        }

        // Wait before spawning enemy
        yield return new WaitForSeconds(2f);

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ActivateEnemy(spawnPoint.position);
            Debug.Log("Enemy spawned via EnemyManager!");
        }
        else if (enemyPrefab != null && spawnPoint != null)
        {
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity).BeginChase();
            Debug.Log("Enemy spawned directly (fallback)");
        }
        else
        {
            Debug.LogError("Cannot spawn enemy - missing enemyPrefab or spawnPoint!");
        }
      
        if (AVRoomController.Instance != null)
        {
            AVRoomController.Instance.OnEnemySpawned();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (tvPromptText)
            {
                tvPromptText.gameObject.SetActive(false);
            }
        }
    }
}