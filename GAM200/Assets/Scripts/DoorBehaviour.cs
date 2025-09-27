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

    [Header("Exit Detection")]
    [SerializeField] private float exitCheckDelay = 2f;  // Time to wait before checking if player really left

    private bool playerNearby;
    private bool isOpen = false;
    private bool playerExitingRoom = false;
    private Coroutine exitCheckCoroutine;

    private bool ignoreNextDeactivation = false;


    private void Start()
    {
        SetupTriggerCollider();
        CloseDoor();

        if (hallway != null)
            hallway.SetActive(true);

        if (targetRoom != null)
        {
            // Only deactivate if this room isn't the starting room
            if (!ShouldKeepRoomActiveOnStart())
                targetRoom.SetActive(false);
            else
                targetRoom.SetActive(true);
        }
    }


    private bool ShouldKeepRoomActiveOnStart()
    {
        // Add logic to determine if this room should stay active at start
        // For example, if this is the room where the game starts
        // or if this room contains important initial objects

        // You might want to:
        // 1. Check if this room has the TV that spawns the enemy
        // 2. Check if this is the player's starting room
        // 3. Use a specific tag or component to identify rooms that should stay active

        return false; // Change this based on your game logic
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
            trigger.size = new Vector2(1f, 1.5f); // Smaller size
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
        StartCoroutine(DelayedRoomDeactivationCheck());
    }

    private IEnumerator DelayedRoomDeactivationCheck()
    {
        // Wait a moment before checking to ensure player has actually left
        yield return new WaitForSeconds(0.5f);
        CheckRoomDeactivation();
    }
    public void SetIgnoreNextDeactivation()
    {
        ignoreNextDeactivation = true;
    }

    private void CheckRoomDeactivation()
    {
        if (ignoreNextDeactivation)
        {
            ignoreNextDeactivation = false;
            Debug.Log($"Skipping deactivation for {targetRoom.name} after restart");
            return;
        }

        // Existing deactivation logic...
        if (ShouldDeactivateRoom())
        {
            Debug.Log($"Deactivating room: {targetRoom.name}");
            EnemyManager.Instance.TeleportEnemyToHallway();
            targetRoom.SetActive(false);
        }
        else
        {
            Debug.Log($"Room {targetRoom.name} kept active - conditions not met");
        }
    }

    private bool ShouldDeactivateRoom()
    {
        // Don't deactivate if door is still open
        if (isOpen)
        {
            Debug.Log("Room kept active - door is open");
            return false;
        }

        // Don't deactivate if player is still in the room
        if (IsPlayerActuallyInRoom())
        {
            Debug.Log("Room kept active - player is in room");
            return false;
        }

        // Don't deactivate if any other door to this room is open
        if (CheckIfAnyDoorToRoomIsOpen())
        {
            Debug.Log("Room kept active - another door to this room is open");
            return false;
        }

        // Don't deactivate if this is a special room that should stay active
        if (ShouldKeepRoomActiveOnStart())
        {
            Debug.Log("Room kept active - special room designation");
            return false;
        }

        // All conditions met - safe to deactivate
        return true;
    }

    private bool IsPlayerActuallyInRoom()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || targetRoom == null)
        {
            Debug.Log("IsPlayerActuallyInRoom: player or targetRoom is null");
            return false;
        }

        // Method 1: Check if player is within the room's collider bounds
        Collider2D roomCollider = targetRoom.GetComponent<Collider2D>();
        if (roomCollider != null)
        {
            bool inBounds = roomCollider.bounds.Contains(player.transform.position);
            Debug.Log($"Player in room bounds: {inBounds}");
            return inBounds;
        }

        // Method 2: Fallback to RoomTracker
        if (RoomTracker.Instance != null)
        {
            bool inRoom = RoomTracker.Instance.IsPlayerInRoom(targetRoom);
            Debug.Log($"RoomTracker says player in room: {inRoom}");
            return inRoom;
        }

        Debug.Log("IsPlayerActuallyInRoom: No reliable method found, defaulting to false");
        return false;
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

            // Cancel any pending exit checks
            if (exitCheckCoroutine != null)
            {
                StopCoroutine(exitCheckCoroutine);
                exitCheckCoroutine = null;
            }

            UpdatePlayerRoomTracker(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Player left door area");

            UpdatePlayerRoomTracker(false);

            // Start delayed check to see if player actually left the room
            if (gameObject.activeInHierarchy)
                exitCheckCoroutine = StartCoroutine(CheckPlayerExit());


            if (isOpen)
            {
                StartCoroutine(AutoCloseDoor());
            }
        }
    }

    private IEnumerator CheckPlayerExit()
    {
        yield return new WaitForSeconds(exitCheckDelay);

        // Double-check if player is really gone from the room
        if (!IsPlayerActuallyInRoom() && !playerNearby)
        {
            Debug.Log("Player confirmed to have left the room");

            // Only now check for room deactivation and enemy teleportation
            CheckRoomDeactivation();
        }
        else
        {
            Debug.Log("Player still in room or returned");
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
                Debug.Log("Player entering room from hallway");
            }
            else
            {
                // Player is leaving room, entering hallway
                RoomTracker.Instance.SetPlayerRoom(hallway);
                Debug.Log("Player exiting room to hallway");
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

    private void TeleportEnemyToHallway()
    {
        Debug.Log("Attempting to teleport enemies to hallway...");

        int enemiesTeleported = 0;

        // Method 1: Find ALL enemies, including inactive ones
        MainEnemy[] allEnemies = Resources.FindObjectsOfTypeAll<MainEnemy>();
        Debug.Log($"Found {allEnemies.Length} total enemies (including inactive)");

        foreach (MainEnemy enemy in allEnemies)
        {
            // Skip prefabs and enemies in other scenes
            if (enemy == null || enemy.gameObject.scene.name != gameObject.scene.name)
                continue;

            if (IsEnemyInTargetRoom(enemy))
            {
                Debug.Log($"Found enemy {enemy.name} in target room, attempting teleport...");
                if (TeleportSingleEnemyToHallway(enemy))
                {
                    enemiesTeleported++;
                }
            }
        }

        Debug.Log($"Teleported {enemiesTeleported} enemies to hallway");
    }

    private bool IsEnemyInTargetRoom(MainEnemy enemy)
    {
        if (enemy == null || targetRoom == null) return false;

        // Check if enemy is a child of the target room
        if (enemy.transform.IsChildOf(targetRoom.transform))
        {
            Debug.Log($"Enemy {enemy.name} is child of target room");
            return true;
        }

        // Check if enemy is physically inside the target room bounds
        Collider2D roomCollider = targetRoom.GetComponent<Collider2D>();
        if (roomCollider != null && roomCollider.bounds.Contains(enemy.transform.position))
        {
            Debug.Log($"Enemy {enemy.name} is inside target room bounds");
            return true;
        }

        // Additional check: if enemy's current room reference matches target room
        // (This requires adding a public method to MainEnemy to get its current room)
        // if (enemy.GetCurrentRoom() == targetRoom) return true;

        return false;
    }

    private bool TeleportSingleEnemyToHallway(MainEnemy enemy)
    {
        if (enemy == null) return false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && EnemySpawnPointManager.Instance != null)
        {
            Vector3 spawnPosition = EnemySpawnPointManager.Instance.GetNearestSpawnPosition(player.transform.position);

            Debug.Log($"Teleporting enemy {enemy.name} from room {targetRoom.name} to hallway near player at {spawnPosition}");

            // Move enemy to hallway
            enemy.transform.SetParent(hallway.transform, true);
            enemy.transform.position = spawnPosition;

            // Notify enemy
            enemy.OnTeleportedToHallway();

            return true;
        }
        else
        {
            Debug.LogError("Could not find player or EnemySpawnPointManager for teleportation!");
            return false;
        }
    }

}