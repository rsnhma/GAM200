using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] PolygonCollider2D mapBoundary; // New room bounds
    [SerializeField] Direction direction;
    [SerializeField] float additivePos = 2f;

    [Header("References")]
    [SerializeField] Collider2D doorCollider;     // Solid door collider
    [SerializeField] Collider2D triggerCollider;  // Small trigger in front of door

    private CinemachineConfiner confiner;
    private bool playerAtDoor = false;
    private GameObject player;

    enum Direction { Up, Down, Left, Right }

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
        if (confiner == null)
            Debug.LogWarning("No CinemachineConfiner found in scene!");

        if (doorCollider == null)
            Debug.LogWarning("Door Collider not assigned!");
        if (triggerCollider == null)
            Debug.LogWarning("Trigger Collider not assigned!");
    }

    private void Update()
    {
        if (playerAtDoor && Input.GetKeyDown(KeyCode.E))
        {
            EnterRoom();
        }
    }

    // Detect when player enters the trigger area
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = true;
            player = collision.gameObject;
            Debug.Log("Player is at the door. Press E to enter.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = false;
            player = null;
            Debug.Log("Player left the door area.");
        }
    }

    private void EnterRoom()
    {
        if (player == null || confiner == null || mapBoundary == null || doorCollider == null)
            return;

        // Temporarily disable the solid door so the player can pass
        doorCollider.enabled = false;
        Debug.Log("Door temporarily disabled. Player can enter.");

        // Move the player slightly inward
        UpdatePlayerPosition(player);

        // Switch Cinemachine confiner to the new room
        confiner.m_BoundingShape2D = mapBoundary;
        Debug.Log("Camera transitioned to new room.");
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;
        switch (direction)
        {
            case Direction.Up: newPos.y += additivePos; break;
            case Direction.Down: newPos.y -= additivePos; break;
            case Direction.Left: newPos.x -= additivePos; break;
            case Direction.Right: newPos.x += additivePos; break;
        }
        player.transform.position = newPos;
    }
}