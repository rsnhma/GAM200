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
    private bool hasShownFirstEncounter = false;

    [Header("Dialogue Settings")]
    public string firstEncounterDialogueID = "well_first_encounter";

    [Header("Animation")]
    public Animator wellAnimator;

    [Header("Spawned Objects")]
    public GameObject keyPrefab;
    public Transform keySpawnPoint;

    [Header("Time Calibration")]
    public GameObject timeCalibrationUI;
    public TimeCalibration timeCalibrationScript;

    [Header("Reeling Settings")]
    public float progressPerScroll = 10f;
    private float reelingProgress = 0f;
    private float targetProgress = 100f;
    private int totalCalibrationsNeeded = 3;
    private int calibrationsCompleted = 0;

    [Header("UI Elements")]
    public GameObject reelingSliderCanvas;
    public Slider reelingSlider;
    public TextMeshProUGUI interactionPromptText;

    [Header("Audio Settings")]
    public AudioSource keySpawnAudio;

    [Header("Enemy Settings")]
    public EnemyManager enemyManager;

    [Header("Sanity Reward")]
    public float sanityReward = 1f;
    public float sanityLoss = 2f;

    private bool playerNearby = false;
    private float lastScrollTime = 0f;
    private float scrollCooldown = 0.1f;

    private void Start()
    {
        if (timeCalibrationUI) timeCalibrationUI.SetActive(false);
        if (reelingSliderCanvas) reelingSliderCanvas.SetActive(false);
        if (interactionPromptText) interactionPromptText.gameObject.SetActive(false);

        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance;
        }

        if (wellAnimator != null)
        {
            wellAnimator.Play("WellIdle");
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
                // Check if rope is equipped
                if (IsItemEquipped(ropeItemID))
                {
                    PlaceRope();
                }
                else
                {
                    // Show wrong sequence dialogue (even if nothing equipped)
                    Debug.Log("Wrong item equipped or no item. Need rope!");
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
                // Check if bucket is equipped
                if (IsItemEquipped(bucketItemID))
                {
                    PlaceBucket();
                }
                else
                {
                    // Show wrong sequence dialogue
                    Debug.Log("Wrong item equipped or no item. Need bucket!");
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
                // Correct item equipped
                interactionPromptText.text = "[E] Attach Rope";
                interactionPromptText.gameObject.SetActive(true);
            }
            else if (equippedID != null)
            {
                // Wrong item equipped - show hint
                interactionPromptText.text = "[E] Wrong Item (Need Rope)";
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                // Nothing equipped
                interactionPromptText.text = "";
                interactionPromptText.gameObject.SetActive(true);
            }
        }
        else if (!bucketAttached)
        {
            string equippedID = InventorySystem.Instance.GetEquippedItemID();

            if (IsItemEquipped(bucketItemID))
            {
                // Correct item equipped
                interactionPromptText.text = "[E] Attach Bucket";
                interactionPromptText.gameObject.SetActive(true);
            }
            else if (equippedID != null)
            {
                // Wrong item equipped - show hint
                interactionPromptText.text = "[E] Wrong Item (Need Bucket)";
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                // Nothing equipped
                interactionPromptText.text = "";
                interactionPromptText.gameObject.SetActive(true);
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

        InventorySystem.Instance.RemoveItem(ropeItemID);
        JournalManager.Instance.RemoveItemFromUI(ropeItemID);

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

        InventorySystem.Instance.RemoveItem(bucketItemID);
        JournalManager.Instance.RemoveItemFromUI(bucketItemID);

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
                Debug.Log("Setting IsReeling = true in animator");
                wellAnimator.SetBool("isReeling", true);
            }
            else
            {
                Debug.LogError("Well Animator is NULL!");
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
        PlayerSanity.Instance.LoseSanity(sanityLoss);
        Debug.Log("Player missed! Alerting enemy and resetting progress...");

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

        if (wellAnimator != null)
        {
            wellAnimator.SetBool("isReeling", false);
        }

        Debug.Log("Well progress reset - rope and bucket still attached");
    }

    private void RetrieveKey()
    {
        isReeling = false;
        keyRetrieved = true;

        if (wellAnimator != null)
        {
            wellAnimator.SetBool("isReeling", false);
        }

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

            if (keySpawnAudio != null)
            {
                keySpawnAudio.Play();
            }
            else
            {
                Debug.LogWarning("Key spawn audio not assigned in Inspector!");
            }
        }

        if (TaskManager.Instance != null && DialogueDatabase.tasks.ContainsKey("well_puzzle"))
        {
            TaskManager.Instance.CompleteTask("well_puzzle");
        }

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.GainSanity(sanityReward);
            Debug.Log($"Player gained {sanityReward} sanity for completing the well puzzle!");
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

            if (!hasShownFirstEncounter)
            {
                hasShownFirstEncounter = true;

                if (!string.IsNullOrEmpty(firstEncounterDialogueID))
                {
                    DialogueManager.Instance?.StartDialogueSequence(firstEncounterDialogueID);
                    Debug.Log("Player discovered the well - showing first encounter dialogue");
                }
            }

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