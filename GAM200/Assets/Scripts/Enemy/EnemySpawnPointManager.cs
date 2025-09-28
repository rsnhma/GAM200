using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnPointManager : MonoBehaviour
{
    public static EnemySpawnPointManager Instance { get; private set; }

    private List<Transform> tvSpawnPoints = new List<Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        FindAllSpawnPoints();
    }

    private void FindAllSpawnPoints()
    {
        // Clear existing list
        tvSpawnPoints.Clear();

        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("TVSpawnPoint");
        foreach (GameObject spawnPoint in spawnPointObjects)
        {
            // Only add active spawn points
            if (spawnPoint.activeInHierarchy)
            {
                tvSpawnPoints.Add(spawnPoint.transform);
                Debug.Log("Found TV spawn point: " + spawnPoint.name + " in " + spawnPoint.transform.parent.name);
            }
        }

        Debug.Log("Total TV spawn points found: " + tvSpawnPoints.Count);
    }

    public Transform GetNearestSpawnPoint(Vector3 position)
    {
        if (tvSpawnPoints.Count == 0) return null;

        Transform nearestSpawnPoint = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Transform spawnPoint in tvSpawnPoints)
        {
            float distance = Vector3.Distance(position, spawnPoint.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestSpawnPoint = spawnPoint;
            }
        }

        return nearestSpawnPoint;
    }

    public Vector3 GetNearestSpawnPosition(Vector3 position)
    {
        Transform nearest = GetNearestSpawnPoint(position);
        return nearest != null ? nearest.position : position;
    }
}
