using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AVRoomController : MonoBehaviour
{
    public static AVRoomController Instance;

    [Header("AV Room State")]
    [Tooltip("Has the enemy been spawned in this room?")]
    public bool enemySpawned = false;

    [Header("Debugging")]
    public bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Check if player can leave the AV room
    public bool CanLeaveAVRoom()
    {
        // Player can only leave after enemy has spawned
        bool canLeave = enemySpawned;

        if (showDebugLogs && !canLeave)
        {
            Debug.Log("Cannot leave AV Room yet - enemy hasn't spawned");
        }

        return canLeave;
    }

    // Call this when enemy spawns (call from TVInteraction or EnemyManager)
    public void OnEnemySpawned()
    {
        enemySpawned = true;

        if (showDebugLogs)
        {
            Debug.Log("Enemy spawned! Player can now leave AV Room.");
        }
    }

    // Reset the room state (useful for testing or scene reload)
    public void ResetRoomState()
    {
        enemySpawned = false;

        if (showDebugLogs)
        {
            Debug.Log("AV Room state reset");
        }
    }
}