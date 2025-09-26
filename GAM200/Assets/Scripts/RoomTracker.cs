using UnityEngine;
using System.Collections.Generic;

public class RoomTracker : MonoBehaviour
{
    public static RoomTracker Instance { get; private set; }

    public GameObject currentRoom;
    private float lastUpdateTime;

    // Store all rooms dynamically
    private List<GameObject> allRooms = new List<GameObject>();
    private GameObject hallway;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-find rooms and hallway by tag or parent
        GameObject[] roomObjects = GameObject.FindGameObjectsWithTag("Room");
        allRooms.AddRange(roomObjects);

        hallway = GameObject.FindGameObjectWithTag("Hallway");
    }

    public void SetPlayerRoom(GameObject room)
    {
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

    public void ResetTracker()
    {
        currentRoom = null;
        lastUpdateTime = 0f;
        Debug.Log("RoomTracker reset");

        // Reset all rooms
        foreach (GameObject room in allRooms)
        {
            if (room != null)
                room.SetActive(false);
        }

        if (hallway != null)
            hallway.SetActive(true);

        // Notify doors to ignore first deactivation
        DoorBehaviour[] doors = FindObjectsOfType<DoorBehaviour>();
        foreach (var door in doors)
            door.SetIgnoreNextDeactivation();
    }
}
