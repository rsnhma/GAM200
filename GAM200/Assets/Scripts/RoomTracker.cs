using UnityEngine;

public class RoomTracker : MonoBehaviour
{
    public static RoomTracker Instance { get; private set; }

    public GameObject currentRoom;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep between scenes if needed
    }

    public void SetPlayerRoom(GameObject room)
    {
        currentRoom = room;
        Debug.Log("Player entered room: " + (room != null ? room.name : "Hallway"));
    }

    public bool IsPlayerInRoom(GameObject room)
    {
        return currentRoom == room;
    }
}