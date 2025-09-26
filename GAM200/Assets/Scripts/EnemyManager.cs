using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Enemy Settings")]
    public MainEnemy enemyPrefab;
    public Transform[] tvSpawnPoints; 

    [Header("Enemy State")]
    public bool isEnemyActive = false;
    public EnemyState currentEnemyState;
    public Vector2 lastKnownPosition;
    public string currentScene;
    public float lingerTimer;

    private MainEnemy currentEnemyInstance;

    public enum EnemyState { Inactive, Chasing, Suspicious, Patrolling }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;

        // If enemy was active in previous scene, spawn it in the new scene
        /*if (isEnemyActive)
        {
            SpawnEnemyInNewScene();
        }*/
    }

    public void ActivateEnemy(Vector2 spawnPosition)
    {
        isEnemyActive = true;
        currentEnemyState = EnemyState.Chasing;
        lastKnownPosition = spawnPosition;

        SpawnEnemyAtPosition(spawnPosition);
    }

    private void SpawnEnemyInNewScene()
    {
        // Find the nearest TV spawn point in the new scene
        Transform nearestTV = FindNearestTVToPlayer();
        Vector2 spawnPosition = nearestTV != null ? nearestTV.position : GetPlayerPosition();

        SpawnEnemyAtPosition(spawnPosition);

        // Apply saved state to the new enemy instance
        if (currentEnemyInstance != null)
        {
            currentEnemyInstance.ApplySavedState(currentEnemyState, lastKnownPosition, lingerTimer);
        }
    }

    private void SpawnEnemyAtPosition(Vector2 position)
    {
        if (currentEnemyInstance != null)
        {
            Destroy(currentEnemyInstance.gameObject);
        }

        currentEnemyInstance = Instantiate(enemyPrefab, position, Quaternion.identity);
        currentEnemyInstance.enemyManager = this;
    }

    private Transform FindNearestTVToPlayer()
    {
        GameObject[] tvs = GameObject.FindGameObjectsWithTag("TV");
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null || tvs.Length == 0) return null;

        Transform nearestTV = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject tv in tvs)
        {
            float distance = Vector2.Distance(player.position, tv.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTV = tv.transform;
            }
        }

        return nearestTV;
    }

    private Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position : Vector2.zero;
    }

    public void UpdateEnemyState(EnemyState state, Vector2 position, float timer)
    {
        currentEnemyState = state;
        lastKnownPosition = position;
        lingerTimer = timer;
    }

    public void DeactivateEnemy()
    {
        isEnemyActive = false;
        currentEnemyState = EnemyState.Inactive;

        if (currentEnemyInstance != null)
        {
            Destroy(currentEnemyInstance.gameObject);
            currentEnemyInstance = null;
        }
    }

    public void TeleportEnemyToHallway()
    {
        if (currentEnemyInstance == null) return;

        GameObject hallway = GameObject.Find("Hallway");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (hallway != null && player != null)
        {
            Vector3 spawnPos = EnemySpawnPointManager.Instance.GetNearestSpawnPosition(player.transform.position);
            currentEnemyInstance.transform.SetParent(hallway.transform, true);
            currentEnemyInstance.transform.position = spawnPos;

            currentEnemyInstance.OnTeleportedToHallway();
            Debug.Log($"Enemy teleported to hallway at {spawnPos}");
        }
    }

}