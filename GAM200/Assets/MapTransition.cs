using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Direction direction;
    [SerializeField] float additivePos = 2f;

    [Header("References")]
    [SerializeField] Collider2D doorCollider;
    [SerializeField] Collider2D triggerCollider;

    private CinemachineConfiner confiner;
    private bool playerAtDoor = false;
    private GameObject player;
    private bool transitioning = false; // prevent spam E during transition

    enum Direction { Up, Down, Left, Right }

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void Update()
    {
        if (playerAtDoor && Input.GetKeyDown(KeyCode.E) && !transitioning)
        {
            StartCoroutine(HandleTransition());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = true;
            player = collision.gameObject;
            Debug.Log("Player is at the door. Press E to enter.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = false;
            player = null;
            Debug.Log("Player left the door area.");
        }
    }

    private System.Collections.IEnumerator HandleTransition()
    {
        transitioning = true;
        doorCollider.enabled = false;

        // Move player slightly inward
        UpdatePlayerPosition(player);

        // Switch Cinemachine confiner
        confiner.m_BoundingShape2D = mapBoundary;

        Debug.Log("Camera transitioned to new room.");

        // Wait a short time before re-enabling door
        yield return new WaitForSeconds(1f);
        doorCollider.enabled = true;
        transitioning = false;
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