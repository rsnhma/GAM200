using UnityEngine;
using System.Collections;

public class DoorBehaviour : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private GameObject doorVisual;      // Door sprite/visual
    [SerializeField] private Collider2D doorCollider;    // Door collider (blocking - NOT trigger)
    [SerializeField] private bool isLocked = false;

    [Header("Room Management")]
    [SerializeField] private GameObject hallway;         // Hallway GameObject
    [SerializeField] private GameObject targetRoom;      // Room this door leads to

    private bool playerNearby;
    private bool isOpen = false;

    void Start()
    {
        // Auto-setup trigger collider
        SetupTriggerCollider();

        // Start with door closed
        CloseDoor();

        // Make sure hallway is active and target room is INACTIVE at start
        if (hallway != null) hallway.SetActive(true);
        if (targetRoom != null) targetRoom.SetActive(false);

        Debug.Log("Door initialized. isOpen = " + isOpen);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed near door. Current isOpen = " + isOpen);
            ToggleDoor();
        }
    }

    private void SetupTriggerCollider()
    {
        // Check if we have a trigger collider
        Collider2D[] colliders = GetComponents<Collider2D>();
        bool hasTrigger = false;

        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                Debug.Log("Trigger collider found");
                break;
            }
        }

        // Create trigger if missing
        if (!hasTrigger)
        {
            BoxCollider2D trigger = gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(3f, 4f);
            Debug.Log("Created trigger collider");
        }
        
        
    }

    private void ToggleDoor()
    {
        if (isLocked)
        {
            Debug.Log("Door is locked!");
            return;
        }

        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorVisual != null)
            doorVisual.SetActive(false);

        if (targetRoom != null && !targetRoom.activeInHierarchy)
        {
            targetRoom.SetActive(true);
            Debug.Log("Room activated: " + targetRoom.name);
        }

        Debug.Log("Door opened!");
    }

    private void CloseDoor()
    {
        isOpen = false;

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (doorVisual != null)
            doorVisual.SetActive(true);

        Debug.Log("Door closed");

        // Check if we should deactivate the room when door closes
        CheckRoomDeactivation();
    }

    private void CheckRoomDeactivation()
    {
        // If player is not in the room and all doors are closed, deactivate the room
        if (targetRoom != null && targetRoom.activeInHierarchy)
        {
            bool playerInThisRoom = RoomTracker.Instance != null &&
                                   RoomTracker.Instance.IsPlayerInRoom(targetRoom);
            bool anyDoorOpen = CheckIfAnyDoorToRoomIsOpen();

            if (!playerInThisRoom && !anyDoorOpen)
            {
                targetRoom.SetActive(false);
                Debug.Log("Room deactivated: " + targetRoom.name);
            }
        }
    }

    private bool CheckIfAnyDoorToRoomIsOpen()
    {
        // Find all doors that lead to this room
        DoorBehaviour[] allDoors = FindObjectsOfType<DoorBehaviour>();
        foreach (DoorBehaviour door in allDoors)
        {
            if (door.targetRoom == targetRoom && door.isOpen)
            {
                return true; // Found an open door to this room
            }
        }
        return false; // All doors to this room are closed
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Player near door");

            // Update room tracker based on which way player is moving
            UpdatePlayerRoomTracker(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Player left door area");

            // Update room tracker based on which way player is moving
            UpdatePlayerRoomTracker(false);

            if (isOpen)
            {
                StartCoroutine(AutoCloseDoor());
            }
        }
    }

    private void UpdatePlayerRoomTracker(bool isEntering)
    {
        if (RoomTracker.Instance == null) return;

        // Determine player's movement direction and update room tracker
        if (isEntering)
        {
            // Player is approaching the door from one side
            if (RoomTracker.Instance.currentRoom == hallway)
            {
                // Player is leaving hallway, entering room
                RoomTracker.Instance.SetPlayerRoom(targetRoom);
            }
            else
            {
                // Player is leaving room, entering hallway
                RoomTracker.Instance.SetPlayerRoom(hallway);
            }
        }
    }

    private IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(0.5f);
        if (!playerNearby && isOpen)
        {
            CloseDoor();
        }
    }

    public void UnlockDoor() { isLocked = false; }
    public void LockDoor() { isLocked = true; CloseDoor(); }
}