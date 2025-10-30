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

    [Header("Reeling Settings")]
    public float progressPerScroll = 10f; // Progress added per scroll (out of 100)
    private float reelingProgress = 0f;
    private float targetProgress = 100f;
    private int totalCalibrationsNeeded = 3;
    private int calibrationsCompleted = 0;

    [Header("UI Elements")]
    public GameObject reelingSliderCanvas;
    public Slider reelingSlider;
    public TextMeshProUGUI interactionPromptText;

    [Header("Enemy Settings")]
    public EnemyManager enemyManager;

    [Header("Sound Indicator")]
    public GameObject soundIndicatorUI;

    private bool playerNearby = false;
    private float lastScrollTime = 0f;
    private float scrollCooldown = 0.1f;

    private void Start()
    {
        if (timeCalibrationUI) timeCalibrationUI.SetActive(false);
        if (reelingSliderCanvas) reelingSliderCanvas.SetActive(false);
        if (interactionPromptText) interactionPromptText.gameObject.SetActive(false);
        if (soundIndicatorUI) soundIndicatorUI.SetActive(false);

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
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Only place rope if rope is equipped (not bucket or other items)
                if (IsItemEquipped(ropeItemID))
                {
                    PlaceRope();
                }
                else if (InventorySystem.Instance.GetEquippedItemID() != null)
                {
                    // Player has something else equipped - show wrong sequence dialogue
                    Debug.Log("Wrong item equipped. Need rope!");
                    if (DialogueDatabase.dialogues.ContainsKey("well_wait_wrong_sequence"))
                    {
                        DialogueManager.Instance.StartDialogueSequence("well_wait_wrong_sequence");
                    }
                }
            }
        }
        else if (!bucketAttached)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (IsItemEquipped(bucketItemID))
                {
                    PlaceBucket();
                }
                else if (InventorySystem.Instance.GetEquippedItemID() != null)
                {
                    // Player has something else equipped
                    Debug.Log("Wrong item equipped. Need bucket!");
                    if (DialogueDatabase.dialogues.ContainsKey("well_wait_wrong_sequence"))
                    {
                        DialogueManager.Instance.StartDialogueSequence("well_wait_wrong_sequence");
                    }
                }
            }
        }
        else if (bucketAttached && !isReeling && !keyRetrieved && !waitingForCalibration)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0 && Time.time >= lastScrollTime + scrollCooldown)
            {
                StartReeling();
            }
        }

        if (isReeling && !waitingForCalibration)
        {
            HandleReeling();
        }
    }

    private bool IsItemEquipped(string itemID)
    {
        string equippedID = InventorySystem.Instance.GetEquippedItemID();
        return equippedID == itemID;
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPromptText == null) return;

        if (!ropeAttached)
        {
            string equippedID = InventorySystem.Instance.GetEquippedItemID();

            if (IsItemEquipped(ropeItemID))
            {
                interactionPromptText.text = "[E] Attach Rope";
                interactionPromptText.gameObject.SetActive(true);
            }
            else if (!string.IsNullOrEmpty(equippedID) && equippedID != ropeItemID)
            {
                // Wrong item equipped
                interactionPromptText.text = "Wrong item! Equip rope from journal";
                interactionPromptText.gameObject.SetActive(true);
            }
            else if (InventorySystem.Instance.HasItem(ropeItemID))
            {
                interactionPromptText.text = "Equip rope from journal (right-click)";
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
        else if (!bucketAttached)
        {
            string equippedID = InventorySystem.Instance.GetEquippedItemID();

            if (IsItemEquipped(bucketItemID))
            {
                interactionPromptText.text = "[E] Attach Bucket";
                interactionPromptText.gameObject.SetActive(true);
            }
            else if (!string.IsNullOrEmpty(equippedID) && equippedID != bucketItemID)
            {
                // Wrong item equipped
                interactionPromptText.text = "Wrong item! Equip bucket from journal";
                interactionPromptText.gameObject.SetActive(true);
            }
            else if (InventorySystem.Instance.HasItem(bucketItemID))
            {
                interactionPromptText.text = "Equip bucket from journal (right-click)";
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
        else if (bucketAttached && !isReeling && !keyRetrieved && !waitingForCalibration)
        {
            interactionPromptText.text = "[Scroll Up] Start Reeling Down Bucket";
            interactionPromptText.gameObject.SetActive(true);
        }
        else if (isReeling && !waitingForCalibration)
        {
            interactionPromptText.text = "[Scroll Up] Keep Reeling...";
            interactionPromptText.gameObject.SetActive(true);
        }
        else if (waitingForCalibration)
        {
            interactionPromptText.text = "[Space] Press on Red Zone to Continue";
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

        // Remove from inventory AND journal UI
        InventorySystem.Instance.RemoveItem(ropeItemID);
        JournalManager.Instance.RemoveItemFromUI(ropeItemID);

        // Use dialogue ID: well_rope_placed
        if (DialogueDatabase.dialogues.ContainsKey("well_rope_placed"))
        {
            DialogueManager.Instance.StartDialogueSequence("well_rope_placed");
        }

        Debug.Log("Rope attached to well");
    }

    private void PlaceBucket()
    {
        bucketAttached = true;

        if (wellAnimator != null)
        {
            wellAnimator.Play("BucketAdded");
        }

        // Remove from inventory AND journal UI
        InventorySystem.Instance.RemoveItem(bucketItemID);
        JournalManager.Instance.RemoveItemFromUI(bucketItemID);

        // Use dialogue ID: well_bucket_placed
        if (DialogueDatabase.dialogues.ContainsKey("well_bucket_placed"))
        {
            DialogueManager.Instance.StartDialogueSequence("well_bucket_placed");
        }

        if (reelingSliderCanvas)
        {
            reelingSliderCanvas.SetActive(true);
        }

        if (reelingSlider)
        {
            reelingSlider.value = 0f;
        }

        Debug.Log("Bucket attached to well - Ready to reel!");
    }

    private void StartReeling()
    {
        if (!isReeling)
        {
            isReeling = true;
            reelingProgress = 0f;
            calibrationsCompleted = 0;
            waitingForCalibration = false;

            if (wellAnimator != null)
            {
                wellAnimator.Play("Reeling");
            }

            Debug.Log("Started reeling bucket down");
        }
    }

    private void HandleReeling()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0 && Time.time >= lastScrollTime + scrollCooldown)
        {
            lastScrollTime = Time.time;

            reelingProgress += progressPerScroll;
            reelingProgress = Mathf.Clamp(reelingProgress, 0f, targetProgress);

            if (reelingSlider)
            {
                reelingSlider.value = reelingProgress / targetProgress;
            }

            Debug.Log($"Progress: {reelingProgress:F1}/{targetProgress} | Calibrations: {calibrationsCompleted}/{totalCalibrationsNeeded}");

            // Trigger calibrations at 33%, 66%, and 100%
            float progressPercent = (reelingProgress / targetProgress) * 100f;
            int expectedCalibrations = Mathf.FloorToInt(progressPercent / (100f / totalCalibrationsNeeded));

            if (expectedCalibrations > calibrationsCompleted && calibrationsCompleted < totalCalibrationsNeeded)
            {
                TriggerTimeCalibration();
                return;
            }

            if (reelingProgress >= targetProgress && calibrationsCompleted >= totalCalibrationsNeeded)
            {
                RetrieveKey();
            }
        }
    }

    private void TriggerTimeCalibration()
    {
        waitingForCalibration = true;

        if (timeCalibrationUI)
        {
            timeCalibrationUI.SetActive(true);
        }

        if (timeCalibrationScript)
        {
            timeCalibrationScript.enabled = true;
            timeCalibrationScript.StartCalibration(OnCalibrationSuccess, OnCalibrationFail);
        }

        Debug.Log($"Time calibration triggered! Calibration {calibrationsCompleted + 1}/{totalCalibrationsNeeded}");
    }

    public void OnCalibrationSuccess()
    {
        Debug.Log("Calibration successful! Continuing reeling...");

        calibrationsCompleted++;
        waitingForCalibration = false;

        if (timeCalibrationUI)
        {
            timeCalibrationUI.SetActive(false);
        }

        if (reelingProgress >= targetProgress && calibrationsCompleted >= totalCalibrationsNeeded)
        {
            RetrieveKey();
        }
    }

    public void OnCalibrationFail()
    {
        Debug.Log("Player missed! Alerting enemy and resetting progress...");

        if (soundIndicatorUI)
        {
            soundIndicatorUI.SetActive(true);
        }

        // Show calibration failed dialogue
        if (DialogueDatabase.dialogues.ContainsKey("well_calibration_failed"))
        {
            DialogueManager.Instance.StartDialogueSequence("well_calibration_failed");
        }

        NoiseSystem.EmitNoise(transform.position, NoiseTypes.PuzzleFailRadius);
        SpawnEnemyAtNearestTV();

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
        calibrationsCompleted = 0;

        if (reelingSlider)
        {
            reelingSlider.value = 0f;
        }

        Debug.Log("Well progress reset - rope and bucket still attached");
    }

    private void RetrieveKey()
    {
        isReeling = false;
        keyRetrieved = true;

        if (reelingSliderCanvas)
        {
            reelingSliderCanvas.SetActive(false);
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

        // Use dialogue ID: well_key_retrieved
        if (DialogueDatabase.dialogues.ContainsKey("well_key_retrieved"))
        {
            DialogueManager.Instance.StartDialogueSequence("well_key_retrieved");
        }

        if (TaskManager.Instance != null && DialogueDatabase.tasks.ContainsKey("well_puzzle"))
        {
            TaskManager.Instance.CompleteTask("well_puzzle");
        }

        Debug.Log("Key retrieved successfully! Well puzzle completed!");
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

            if (bucketAttached && !keyRetrieved && reelingSliderCanvas)
            {
                reelingSliderCanvas.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (!keyRetrieved && reelingSliderCanvas)
            {
                reelingSliderCanvas.SetActive(false);
            }

            if ((isReeling || waitingForCalibration) && !keyRetrieved)
            {
                Debug.Log("Player left the well area - resetting progress");
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