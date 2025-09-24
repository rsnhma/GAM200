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

    private void Start()
    {
        // If enemy is already active from previous scene, don't allow interaction
        if (EnemyManager.Instance != null && EnemyManager.Instance.isEnemyActive)
        {
            this.enabled = false; // Disable this TV interaction
        }
    }

    private void Update()
    {
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

        interactText.gameObject.SetActive(false);

        // TODO: Play actual cutscene here
        yield return new WaitForSeconds(4f); // Cutscene time

        // Buffer time for player to react
        yield return new WaitForSeconds(2f);

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            // Use the EnemyManager to spawn the enemy
            EnemyManager.Instance.ActivateEnemy(spawnPoint.position);
            Debug.Log("Enemy spawned and begins chase!");
        }
        else
        {
            Debug.LogError("EnemyPrefab or SpawnPoint not set on TVInteraction");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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