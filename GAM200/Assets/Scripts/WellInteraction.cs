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
    public int scrollsBeforeCalibration = 3; // Number of scrolls before each calibration appears

    [Header("Reeling Settings")]
    public float scrollSensitivity = 10f; // Increased to make progress more noticeable
    private float reelingProgress = 0f;
    private float targetProgress = 100f;
    private int scrollCount = 0;
    private int totalCalibrationsNeeded = 3; // Total number of calibrations needed
    private int calibrationsCompleted = 0;

    [Header("UI Elements")]
    public GameObject reelingSliderCanvas; // Separate canvas for the progress slider
    public Slider reelingSlider;
    public TextMeshProUGUI interactionPromptText;

    [Header("Enemy Settings")]
    public EnemyManager enemyManager;

    [Header("Sound Indicator")]
    public GameObject soundIndicatorUI; // The sound indicator UI to show on fail

    private bool playerNearby = false;

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

        // Show the slider canvas after bucket is attached
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
            scrollCount = 0;
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

        if (scroll > 0)
        {
            scrollCount++;
            reelingProgress += scroll * scrollSensitivity;
            reelingProgress = Mathf.Clamp(reelingProgress, 0f, targetProgress);

            if (reelingSlider)
            {
                reelingSlider.value = reelingProgress / targetProgress;
            }

            Debug.Log($"Scroll count: {scrollCount}/{scrollsBeforeCalibration} | Progress: {reelingProgress:F1}% | Calibrations: {calibrationsCompleted}/{totalCalibrationsNeeded}");

            // Check if we've reached the required number of scrolls for calibration
            if (scrollCount >= scrollsBeforeCalibration)
            {
                TriggerTimeCalibration();
                return;
            }

            // If we've completed all calibrations and reached 100%
            if (reelingProgress >= targetProgress && calibrationsCompleted >= totalCalibrationsNeeded)
            {
                RetrieveKey();
            }
        }
    }

    private void TriggerTimeCalibration()
    {
        waitingForCalibration = true;
        scrollCount = 0; // Reset scroll count for next round

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

        // Check if we've completed all calibrations and progress
        if (reelingProgress >= targetProgress && calibrationsCompleted >= totalCalibrationsNeeded)
        {
            RetrieveKey();
        }
    }

    public void OnCalibrationFail()
    {
        Debug.Log("Player missed! Alerting enemy and resetting progress...");

        // Show sound indicator UI
        if (soundIndicatorUI)
        {
            soundIndicatorUI.SetActive(true);
        }

        // Alert enemy via noise system
        NoiseSystem.EmitNoise(transform.position, NoiseTypes.PuzzleFailRadius);
        SpawnEnemyAtNearestTV();

        // Reset progress but keep well items attached
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
        scrollCount = 0;
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

        ShowDialogue("key_retrieved");

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

            // Show slider if bucket is attached and key not retrieved
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

            // Always hide slider canvas when player leaves (unless key is retrieved)
            if (!keyRetrieved && reelingSliderCanvas)
            {
                reelingSliderCanvas.SetActive(false);
            }

            // If player leaves while reeling or in calibration, reset everything
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