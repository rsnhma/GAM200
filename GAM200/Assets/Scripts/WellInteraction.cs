using UnityEngine;
using UnityEngine.UI;
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

    [Header("Visual References")]
    public GameObject ropeVisual; // Visual rope on well
    public GameObject bucketVisual; // Visual bucket on well
    public GameObject keyPrefab; // Key to spawn after success
    public Transform keySpawnPoint; // Where to spawn the key

    [Header("Time Calibration")]
    public GameObject timeCalibrationUI; // The UI panel for time calibration
    public TimeCalibration timeCalibrationScript;

    [Header("Reeling Settings")]
    public float scrollSensitivity = 1f;
    private float reelingProgress = 0f;
    private float targetProgress = 100f; // Need to reach 100 to start calibration

    [Header("UI Elements")]
    public Slider reelingSlider; // Shows reeling progress
    public GameObject interactionPrompt; // Shows "Place Rope" or "Place Bucket" etc.

    [Header("Enemy Settings")]
    public EnemyManager enemyManager;
    public Transform[] tvSpawnPoints; // Assign all TV spawn points in scene

    private bool playerNearby = false;

    private void Start()
    {
        // Hide visuals at start
        if (ropeVisual) ropeVisual.SetActive(false);
        if (bucketVisual) bucketVisual.SetActive(false);
        if (timeCalibrationUI) timeCalibrationUI.SetActive(false);
        if (reelingSlider) reelingSlider.gameObject.SetActive(false);
        if (interactionPrompt) interactionPrompt.SetActive(false);

        // Get EnemyManager if not assigned
        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance;
        }
    }

    private void Update()
    {
        if (!playerNearby) return;

        // Handle different interaction states
        if (!ropeAttached)
        {
            // Check for rope placement
            if (Input.GetMouseButtonDown(0) && InventorySystem.Instance.HasItem(ropeItemID))
            {
                PlaceRope();
            }
        }
        else if (!bucketAttached)
        {
            // Check for bucket placement
            if (Input.GetMouseButtonDown(0))
            {
                if (InventorySystem.Instance.HasItem(bucketItemID))
                {
                    PlaceBucket();
                }
                else if (InventorySystem.Instance.HasItem(ropeItemID))
                {
                    // Player trying to place rope again - wrong sequence
                    ShowDialogue("wait_wrong_sequence");
                }
            }
        }
        else if (bucketAttached && !isReeling && !keyRetrieved)
        {
            // Check for reeling input (mouse wheel)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                StartReeling();
            }
        }

        // Handle reeling mechanics
        if (isReeling)
        {
            HandleReeling();
        }
    }

    private void PlaceRope()
    {
        ropeAttached = true;
        if (ropeVisual) ropeVisual.SetActive(true);

        // Remove rope from inventory
        InventorySystem.Instance.RemoveItem(ropeItemID);

        // Show dialogue
        ShowDialogue("rope_placed");

        Debug.Log("Rope attached to well");
        UpdateInteractionPrompt();
    }

    private void PlaceBucket()
    {
        bucketAttached = true;
        if (bucketVisual) bucketVisual.SetActive(true);

        // Remove bucket from inventory
        InventorySystem.Instance.RemoveItem(bucketItemID);

        // Show dialogue
        ShowDialogue("bucket_placed");

        Debug.Log("Bucket attached to well");
        UpdateInteractionPrompt();
    }

    private void StartReeling()
    {
        isReeling = true;
        reelingProgress = 0f;

        if (reelingSlider)
        {
            reelingSlider.gameObject.SetActive(true);
            reelingSlider.value = 0f;
        }

        ShowDialogue("start_reeling");
        Debug.Log("Started reeling bucket down");
    }

    private void HandleReeling()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0) // Scrolling up - reeling down
        {
            reelingProgress += scroll * scrollSensitivity * 100f;
            reelingProgress = Mathf.Clamp(reelingProgress, 0f, targetProgress);

            if (reelingSlider)
            {
                reelingSlider.value = reelingProgress / targetProgress;
            }

            // Check if reached bottom
            if (reelingProgress >= targetProgress)
            {
                StartTimeCalibration();
            }
        }
    }

    private void StartTimeCalibration()
    {
        isReeling = false;

        if (reelingSlider)
        {
            reelingSlider.gameObject.SetActive(false);
        }

        // Activate time calibration UI
        if (timeCalibrationUI)
        {
            timeCalibrationUI.SetActive(true);
        }

        if (timeCalibrationScript)
        {
            timeCalibrationScript.enabled = true;
            timeCalibrationScript.StartCalibration(OnCalibrationSuccess, OnCalibrationMiss);
        }

        Debug.Log("Time calibration started!");
    }

    public void OnCalibrationSuccess()
    {
        Debug.Log("Calibration successful! Spawning key...");
        keyRetrieved = true;

        // Hide calibration UI
        if (timeCalibrationUI)
        {
            timeCalibrationUI.SetActive(false);
        }

        // Spawn the key
        if (keyPrefab && keySpawnPoint)
        {
            GameObject key = Instantiate(keyPrefab, keySpawnPoint.position, Quaternion.identity);

            // Make sure the key has a Collectible component
            Collectible keyCollectible = key.GetComponent<Collectible>();
            if (keyCollectible != null)
            {
                keyCollectible.itemID = "well_key";
            }
        }

        ShowDialogue("key_retrieved");
    }

    public void OnCalibrationMiss()
    {
        Debug.Log("Player missed! Alerting enemy...");

        // EMIT PUZZLE FAIL NOISE - This will alert the enemy EVERY TIME player misses
        NoiseSystem.EmitNoise(transform.position, NoiseTypes.PuzzleFailRadius);
        Debug.Log($"Emitted puzzle fail noise at {transform.position}");

        // Spawn enemy if not already active (only on first miss)
        SpawnEnemyAtNearestTV();

        // Player continues the minigame - no reset, just pressure!
        // Enemy is now aware and heading to the well
    }

    private void SpawnEnemyAtNearestTV()
    {
        // Check if enemy is already active
        if (enemyManager != null && enemyManager.isEnemyActive)
        {
            Debug.Log("Enemy already active - noise will alert it");
            return;
        }

        // Find nearest TV spawn point to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        Transform nearestTV = null;
        float nearestDistance = Mathf.Infinity;

        // Find all TVs in scene if spawn points not assigned
        if (tvSpawnPoints == null || tvSpawnPoints.Length == 0)
        {
            GameObject[] tvObjects = GameObject.FindGameObjectsWithTag("TV");
            if (tvObjects.Length > 0)
            {
                foreach (GameObject tv in tvObjects)
                {
                    float distance = Vector2.Distance(player.transform.position, tv.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTV = tv.transform;
                    }
                }
            }
        }
        else
        {
            // Use assigned spawn points
            foreach (Transform spawnPoint in tvSpawnPoints)
            {
                float distance = Vector2.Distance(player.transform.position, spawnPoint.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTV = spawnPoint;
                }
            }
        }

        // Spawn enemy at nearest TV
        if (nearestTV != null && enemyManager != null)
        {
            Debug.Log($"Spawning enemy at nearest TV: {nearestTV.name}");
            enemyManager.ActivateEnemy(nearestTV.position);
        }
        else
        {
            Debug.LogWarning("Could not find TV spawn point or EnemyManager!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            UpdateInteractionPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (interactionPrompt) interactionPrompt.SetActive(false);
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (!interactionPrompt || !playerNearby) return;

        if (!ropeAttached)
        {
            interactionPrompt.SetActive(true);
            // Set text: "Place Rope"
        }
        else if (!bucketAttached)
        {
            interactionPrompt.SetActive(true);
            // Set text: "Place Bucket"
        }
        else if (!isReeling && !keyRetrieved)
        {
            interactionPrompt.SetActive(true);
            // Set text: "Scroll to Reel Bucket"
        }
        else if (keyRetrieved)
        {
            interactionPrompt.SetActive(false);
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