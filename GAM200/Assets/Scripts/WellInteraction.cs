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

    [Header("Animation")]
    public Animator wellAnimator; // Reference to well's Animator component
    // Animation states should be: "Idle", "RopeAdded", "BucketAdded", "Reeling"

    [Header("Spawned Objects")]
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

    [Header("Enemy Settings")]
    public EnemyManager enemyManager;

    private bool playerNearby = false;

    private void Start()
    {
        // Hide UI elements at start
        if (timeCalibrationUI) timeCalibrationUI.SetActive(false);
        if (reelingSlider) reelingSlider.gameObject.SetActive(false);

        // Get EnemyManager if not assigned
        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance;
        }

        // Make sure animator is in idle state
        if (wellAnimator != null)
        {
            wellAnimator.Play("Idle");
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

        // Trigger rope animation
        if (wellAnimator != null)
        {
            wellAnimator.Play("RopeAdded");
        }

        // Remove rope from inventory
        InventorySystem.Instance.RemoveItem(ropeItemID);

        // Show dialogue
        ShowDialogue("rope_placed");

        Debug.Log("Rope attached to well");
    }

    private void PlaceBucket()
    {
        bucketAttached = true;

        // Trigger bucket animation
        if (wellAnimator != null)
        {
            wellAnimator.Play("BucketAdded");
        }

        // Remove bucket from inventory
        InventorySystem.Instance.RemoveItem(bucketItemID);

        // Show dialogue
        ShowDialogue("bucket_placed");

        Debug.Log("Bucket attached to well");
    }

    private void StartReeling()
    {
        isReeling = true;
        reelingProgress = 0f;

        // Start reeling animation
        if (wellAnimator != null)
        {
            wellAnimator.Play("Reeling");
        }

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

        // Get TV spawn points from EnemyManager
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

        // Find nearest TV spawn point to player
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

        // Spawn enemy at nearest TV
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