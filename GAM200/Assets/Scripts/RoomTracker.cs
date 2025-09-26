using UnityEngine;

public class RoomTracker : MonoBehaviour
{
    public static RoomTracker Instance { get; private set; }

    public GameObject currentRoom;
    private float lastUpdateTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerRoom(GameObject room)
    {
        // Only update if it's actually a change
        if (currentRoom != room)
        {
            currentRoom = room;
            lastUpdateTime = Time.time;
            Debug.Log($"Player room updated to: {(room != null ? room.name : "Hallway")}");
        }
    }

    public bool IsPlayerInRoom(GameObject room)
    {
        return currentRoom == room;
    }

    // Force update room based on player's actual position
    public void UpdateRoomBasedOnPosition(GameObject player)
    {
        if (player == null) return;

    }
}