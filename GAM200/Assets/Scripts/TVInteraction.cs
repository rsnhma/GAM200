using UnityEngine;
using TMPro;
using System.Collections;

public class TVInteraction : MonoBehaviour
{
    private bool playerNearby = false;
    [SerializeField] private TextMeshProUGUI interactText;

    [Header("Enemy Spawn")]
    public MainEnemy enemyPrefab;   // Assign your enemy prefab here
    public Transform spawnPoint;    // Position of TV (enemy crawls out here)

    private bool hasBeenUsed = false;

    private void Start()
    {
        // Check if enemy is already active
        if (EnemyManager.Instance != null && EnemyManager.Instance.isEnemyActive)
        {
            DisableTVInteraction();
        }
    }

    private void Update()
    {
        if (hasBeenUsed) return;

        if (playerNearby && VHSItem.hasVHS)
        {
            interactText.text = "Left Click to put tape in";
            interactText.gameObject.SetActive(true);

            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(HandleCutscene());
            }
        }
        else
        {
            interactText.gameObject.SetActive(false);
        }
    }

    private IEnumerator HandleCutscene()
    {
        Debug.Log("Tape inserted. Cutscene start (skipped for now)");
        hasBeenUsed = true;
        interactText.gameObject.SetActive(false);

        // TODO: Play actual cutscene here
        yield return new WaitForSeconds(2f); // Cutscene time

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            // SAFE CHECK: Make sure EnemyManager exists
            if (EnemyManager.Instance == null)
            {
                Debug.LogError("EnemyManager.Instance is null! Creating temporary enemy...");

                // Fallback: Spawn enemy directly
                MainEnemy enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                enemy.BeginChase();
                Debug.Log("Enemy spawned directly (fallback)");
            }
            else
            {
                // Use the EnemyManager to spawn the enemy
                EnemyManager.Instance.ActivateEnemy(spawnPoint.position);
                Debug.Log("Enemy spawned via EnemyManager!");
            }

            ChangeTVAppearance();
        }
        else
        {
            Debug.LogError("EnemyPrefab or SpawnPoint not set on TVInteraction");
        }
    }

    private void DisableTVInteraction()
    {
        hasBeenUsed = true;
        interactText.gameObject.SetActive(false);
        ChangeTVAppearance();
    }

    private void ChangeTVAppearance()
    {
        // Optional: Change TV material, color, or add visual effect
        Renderer tvRenderer = GetComponent<Renderer>();
        if (tvRenderer != null)
        {
            tvRenderer.material.color = Color.gray;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenUsed)
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            interactText.gameObject.SetActive(false);
        }
    }
}