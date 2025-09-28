using UnityEngine;

public class RoomResetManager : MonoBehaviour
{
    [Header("Hallway (always active at start)")]
    public GameObject hallway;

    private GameObject[] rooms;

    private void Awake()
    {
        // Automatically find all rooms tagged "Room"
        rooms = GameObject.FindGameObjectsWithTag("Room");
        Debug.Log($"RoomResetManager found {rooms.Length} rooms in the scene.");
    }

    public void ResetRooms()
    {
        if (rooms != null)
        {
            // Deactivate all rooms
            foreach (GameObject room in rooms)
            {
                if (room != null)
                    room.SetActive(false);
            }
        }

        // Hallway is always active at start
        if (hallway != null)
            hallway.SetActive(true);

        // Notify doors to skip first deactivation after restart
        DoorBehaviour[] doors = FindObjectsOfType<DoorBehaviour>();
        foreach (var door in doors)
        {
            door.SetIgnoreNextDeactivation();
        }

        Debug.Log("Rooms and doors reset for new round");
    }
}
