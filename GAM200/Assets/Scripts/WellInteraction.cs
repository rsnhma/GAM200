using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static NoiseSystem;

public class WellInteraction : MonoBehaviour
{
    [Header("Required Items")]
    public string ropeItemID = "rope";
    public string bucketItemID = "bucket";

    [Header("Well States")]
    private bool ropeAttached = false;
    private bool bucketAttached = false;
    private bool isReeling = false;
    private bool keyRetrieved = false;
    private bool waitingForCalibration = false;

    [Header("Animation")]
    public Animator wellAnimator;

    [Header("Spawned Objects")]
    public GameObject keyPrefab;
    public Transform keySpawnPoint;

    [Header("Time Calibration")]
    public GameObject timeCalibrationUI;
    public TimeCalibration timeCalibrationScript;
    public float[] calibrationCheckpoints = { 33f, 66f, 100f }; // Progress points where calibration appears
    private int currentCheckpointIndex = 0;

    [Header("Reeling Settings")]
    public float scrollSensitivity = 1f;
    private float reelingProgress = 0f;
    private float targetProgress = 100f;

    [Header("UI Elements")]
    public Slider reelingSlider;
    public TextMeshProUGUI interactionPromptText;

    [Header("Enemy Settings")]
    public EnemyManager enemyManager;

    private bool playerNearby = false;

    private void Start()
    {
        if (timeCalibrationUI) timeCalibrationUI.SetActive(false);
        if (reelingSlider) reelingSlider.gameObject.SetActive(false);
        if (interactionPromptText) interactionPromptText.gameObject.SetActive(false);

        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance;
        }

        if (wellAnimator != null)
        {
            wellAnimator.Play("Idle");
        }
    }

    private void Update()
    {
        if (!playerNearby) return;

        UpdateInteractionPrompt();

        if (!ropeAttached)
        {
            if (Input.GetKeyDown(KeyCode.E) && InventorySystem.Instance.HasItem(ropeItemID))
            {
                PlaceRope();
            }
        }
        else if (!bucketAttached)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (InventorySystem.Instance.HasItem(bucketItemID))
                {
                    PlaceBucket();
                }
                else if (InventorySystem.Instance.HasItem(ropeItemID))
                {
                    ShowDialogue("wait_wrong_sequence");
                }
            }
        }
        else if (bucketAttached && !isReeling && !keyRetrieved && !waitingForCalibration)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0)
            {
                StartReeling();
            }
        }

        // Handle reeling only when not waiting for calibration
        if (isReeling && !waitingForCalibration)
        {
            HandleReeling();
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPromptText == null) return;

        if (!ropeAttached)
        {
            if (InventorySystem.Instance.HasItem(ropeItemID))
            {
                interactionPromptText.text = "[E] Attach Rope";
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
        else if (!bucketAttached)
        {
            if (InventorySystem.Instance.HasItem(bucketItemID))
            {
                interactionPromptText.text = "[E] Attach Bucket";
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
        else if (bucketAttached && !isReeling && !keyRetrieved && !waitingForCalibration)
        {
            interactionPromptText.text = "[Scroll Up] Reel Down Bucket";
            interactionPromptText.gameObject.SetActive(true);
        }
        else if (isReeling && !waitingForCalibration)
        {
            interactionPromptText.text = "[Scroll Up] Keep Reeling...";
            interactionPromptText.gameObject.SetActive(true);
        }
        else
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void PlaceRope()
    {
        ropeAttached = true;

        if (wellAnimator != null)
        {
            wellAnimator.Play("RopeAdded");
        }

        InventorySystem.Instance.RemoveItem(ropeItemID);
        ShowDialogue("rope_placed");

        Debug.Log("Rope attached to well");
    }

    private void PlaceBucket()
    {
        bucketAttached = true;

        if (wellAnimator != null)
        {
            wellAnimator.Play("BucketAdded");
        }

        InventorySystem.Instance.RemoveItem(bucketItemID);
        ShowDialogue("bucket_placed");

        Debug.Log("Bucket attached to well");
    }

    private void StartReeling()
    {
        isReeling = true;
        reelingProgress = 0f;
        currentCheckpointIndex = 0;
        waitingForCalibration = false;

        if (wellAnimator != null)
        {
            wellAnimator.Play("Reeling");
        }

        if (reelingSlider)
        {
            reelingSlider.gameObject.SetActive(true);
            reelingSlider.value = 0f;
        }

        Debug.Log("Started reeling bucket down");
    }

    private void HandleReeling()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0)
        {
            reelingProgress += scroll * scrollSensitivity * 100f;
            reelingProgress = Mathf.Clamp(reelingProgress, 0f, targetProgress);

            if (reelingSlider)
            {
                reelingSlider.value = reelingProgress / targetProgress;
            }

            // Check if we've reached a calibration checkpoint
            if (currentCheckpointIndex < calibrationCheckpoints.Length)
            {
                float nextCheckpoint = calibrationCheckpoints[currentCheckpointIndex];

                if (reelingProgress >= nextCheckpoint)
                {
                    TriggerTimeCalibration();
                    return;
                }
            }

            // If we've completed all checkpoints and reached 100%
            if (reelingProgress >= targetProgress && currentCheckpointIndex >= calibrationCheckpoints.Length)
            {
                RetrieveKey();
            }
        }
    }

    private void TriggerTimeCalibration()
    {
        waitingForCalibration = true;

        if (interactionPromptText)
        {
            interactionPromptText.gameObject.SetActive(false);
        }

        if (timeCalibrationUI)
        {
            timeCalibrationUI.SetActive(true);
        }

        if (timeCalibrationScript)
        {
            timeCalibrationScript.enabled = true;
            timeCalibrationScript.StartCalibration(OnCalibrationSuccess, OnCalibrationFail);
        }

        Debug.Log($"Time calibration triggered at checkpoint {currentCheckpointIndex + 1}/{calibrationCheckpoints.Length}");
    }

    public void OnCalibrationSuccess()
    {
        Debug.Log("Calibration successful! Continuing reeling...");

        currentCheckpointIndex++; // Move to next checkpoint
        waitingForCalibration = false;

        if (timeCalibrationUI)
        {
            timeCalibrationUI.SetActive(false);
        }

        // Check if this was the final checkpoint
        if (reelingProgress >= targetProgress && currentCheckpointIndex >= calibrationCheckpoints.Length)
        {
            RetrieveKey();
        }
    }

    public void OnCalibrationFail()
    {
        Debug.Log("Player missed! Resetting progress and alerting enemy...");

        // Alert enemy
        NoiseSystem.EmitNoise(transform.position, NoiseTypes.PuzzleFailRadius);
        SpawnEnemyAtNearestTV();

        // Reset everything
        ResetWellProgress();

        if (timeCalibrationUI)
        {
            timeCalibrationUI.SetActive(false);
        }
    }

    private void ResetWellProgress()
    {
        isReeling = false;
        waitingForCalibration = false;
        reelingProgress = 0f;
        currentCheckpointIndex = 0;

        if (reelingSlider)
        {
            reelingSlider.value = 0f;
            reelingSlider.gameObject.SetActive(false);
        }

        Debug.Log("Well progress reset");
    }

    private void RetrieveKey()
    {
        isReeling = false;
        keyRetrieved = true;

        if (reelingSlider)
        {
            reelingSlider.gameObject.SetActive(false);
        }

        if (keyPrefab && keySpawnPoint)
        {
            GameObject key = Instantiate(keyPrefab, keySpawnPoint.position, Quaternion.identity);

            Collectible keyCollectible = key.GetComponent<Collectible>();
            if (keyCollectible != null)
            {
                keyCollectible.itemID = "well_key";
            }
        }

        ShowDialogue("key_retrieved");

        if (TaskManager.Instance != null && DialogueDatabase.tasks.ContainsKey("well_puzzle"))
        {
            TaskManager.Instance.CompleteTask("well_puzzle");
        }

        Debug.Log("Key retrieved successfully!");
    }

    private void SpawnEnemyAtNearestTV()
    {
        if (enemyManager != null && enemyManager.isEnemyActive)
        {
            Debug.Log("Enemy already active - noise will alert it");
            return;
        }

        if (enemyManager == null)
        {
            Debug.LogError("EnemyManager not found!");
            return;
        }

        Transform[] tvSpawnPoints = enemyManager.GetTVSpawnPoints();
        if (tvSpawnPoints == null || tvSpawnPoints.Length == 0)
        {
            Debug.LogError("No TV spawn points found in EnemyManager!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        Transform nearestTV = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Transform spawnPoint in tvSpawnPoints)
        {
            float distance = Vector2.Distance(player.transform.position, spawnPoint.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTV = spawnPoint;
            }
        }

        if (nearestTV != null)
        {
            Debug.Log($"Spawning enemy at nearest TV: {nearestTV.name}");
            enemyManager.ActivateEnemy(nearestTV.position);
        }
        else
        {
            Debug.LogWarning("Could not find TV spawn point!");
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

            // If player leaves while reeling, reset everything
            if (isReeling || waitingForCalibration)
            {
                ResetWellProgress();

                if (timeCalibrationUI)
                {
                    timeCalibrationUI.SetActive(false);
                }
            }

            if (interactionPromptText)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    private void ShowDialogue(string dialogueSuffix)
    {
        string dialogueID = "well_" + dialogueSuffix;
        if (DialogueDatabase.dialogues.ContainsKey(dialogueID))
        {
            DialogueManager.Instance.StartDialogueSequence(dialogueID);
        }
    }
}