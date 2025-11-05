using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;

public class TVInteraction : MonoBehaviour
{
    [Header("Required Item")]
    public string vhsItemID = "vhs_tape";

    [Header("Enemy Spawn")]
    public MainEnemy enemyPrefab;
    public Transform spawnPoint;

    [Header("UI Elements")]
    public TextMeshProUGUI tvPromptText;

    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public GameObject videoPanel;

    [Header("Dialogue After Video")]
    public string dialogueAfterVideoID = "tv_video_end";

    private bool vhsInserted = false;
    private bool hasBeenUsed = false;
    private bool playerNearby = false;
    private bool videoPlaying = false;

    private void Start()
    {
        if (tvPromptText)
        {
            tvPromptText.gameObject.SetActive(false);
        }

        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
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

        // Optional: Show initial dialogue before video
        if (DialogueDatabase.dialogues.ContainsKey("vhs_inserted"))
        {
            DialogueManager.Instance.StartDialogueSequence("vhs_inserted");
            // Wait for dialogue to finish (adjust timing as needed)
            yield return new WaitForSeconds(3f);
        }

        // Play the video
        PlayVideo();

        // Wait for video to finish (the OnVideoFinished callback will handle the rest)
        yield return null;
    }

    private void PlayVideo()
    {
        Debug.Log("Playing VHS video");

        if (videoPlayer != null && videoPlayer.clip != null)
        {
            // Show video panel
            if (videoPanel != null)
            {
                videoPanel.SetActive(true);
                Debug.Log($"VideoPanel active: {videoPanel.activeSelf}");

                // Ensure RawImage is enabled
                RawImage rawImage = videoPanel.GetComponentInChildren<RawImage>();
                if (rawImage != null)
                {
                    Debug.Log($"RawImage found. Texture: {rawImage.texture != null}");
                    rawImage.enabled = true;
                }
                else
                {
                    Debug.LogError("NO RAW IMAGE FOUND ON VIDEO PANEL!");
                }
            }

            videoPlaying = true;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("VideoPlayer or VideoClip is missing!");
            // Fallback: spawn enemy immediately if video fails
            OnVideoFinished(null);
        }
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("VHS video prepared and ready to play");
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("VHS video finished");
        videoPlaying = false;

        // Hide video panel
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        // Start the post-video sequence
        StartCoroutine(PostVideoSequence());
    }

    private IEnumerator PostVideoSequence()
    {
        // Show dialogue after video ends
        if (DialogueDatabase.dialogues.ContainsKey(dialogueAfterVideoID))
        {
            DialogueManager.Instance.StartDialogueSequence(dialogueAfterVideoID);
            Debug.Log("Playing post-video dialogue");

            // Wait a moment for dialogue to start
            yield return new WaitForSeconds(1f);
        }

        // Spawn enemy (can happen during or after dialogue)
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

    private void OnDestroy()
    {
        // Clean up video player events
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}
