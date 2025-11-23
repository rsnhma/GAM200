using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static string nextEntryPointId;  // Which spawn point to use
    public Transform playerPrefab;          // Assign your Player prefab in Inspector

    void Start()
    {
        if (!string.IsNullOrEmpty(nextEntryPointId))
        {
            // Find all spawn points in the scene
            PlayerSpawnPoint[] spawnPoints = FindObjectsOfType<PlayerSpawnPoint>();

            foreach (var sp in spawnPoints)
            {
                if (sp.entryPointId == nextEntryPointId)
                {
                    // Move player here
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = sp.transform.position;
                    }
                    else if (playerPrefab != null)
                    {
                        Instantiate(playerPrefab, sp.transform.position, Quaternion.identity);
                    }
                    break;
                }
            }
        }
    }
}
