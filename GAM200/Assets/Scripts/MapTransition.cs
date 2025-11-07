using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class MapTransition : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private PolygonCollider2D mapBoundary;
    [SerializeField] private Direction direction;
    [SerializeField] private Transform teleportTargetPosition;
    [SerializeField] private float additivePos = 2f;

    [Header("References")]
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private DoorInteraction doorInteraction;

    [Header("Settings")]
    [SerializeField] private bool hasAnimation = true;
    [SerializeField] private float enemyFollowDelay = 0.5f; // Delay before enemy follows player

    private CinemachineConfiner confiner;
    private GameObject player;
    private bool playerAtDoor = false;
    private bool transitioning = false;
    private bool isDoorOpen = false;
    private bool canInteract = false;

    private enum Direction { Up, Down, Left, Right, Teleport }

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
        if (confiner == null)
            Debug.LogError("CinemachineConfiner not found in scene!");

        playerAtDoor = false;
        transitioning = false;
        isDoorOpen = false;
        canInteract = false;

        ResetDoorToClosed();
    }

    private void Update()
    {
        if (playerAtDoor && canInteract && Input.GetKeyDown(KeyCode.E) && !transitioning)
        {
            if (!isDoorOpen)
                StartCoroutine(OpenDoorForPlayer());
            else
                StartCoroutine(CloseDoor());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = true;
            player = collision.gameObject;
            StartCoroutine(EnableInteractionNextFrame());

            Debug.Log("Player at door. Press E to open/close.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtDoor = false;
            player = null;
            canInteract = false;

            if (isDoorOpen)
                StartCoroutine(CloseDoor());

            Debug.Log("Player left the door area.");
        }
    }

    private IEnumerator EnableInteractionNextFrame()
    {
        yield return null;
        canInteract = true;
    }

    // Player opens door with E key - includes camera transition
    private IEnumerator OpenDoorForPlayer()
    {
        transitioning = true;

        // Play door open sound
        if (doorInteraction != null)
        {
            doorInteraction.PlayDoorOpenSound();
        }

        if (hasAnimation && doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");

            yield return new WaitUntil(() =>
                doorAnimator.GetCurrentAnimatorStateInfo(0).IsName("Door_Open"));

            float animLength = doorAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Handle the player/camera transition
        yield return HandleTransition();

        // Check if enemy should follow player through door
        StartCoroutine(CheckForChasingEnemy());

        // Close the door immediately after transition
        yield return ForceDoorClosed();

        isDoorOpen = false;
        transitioning = false;
    }

    // Check if an enemy is chasing and teleport them through the door
    private IEnumerator CheckForChasingEnemy()
    {
        yield return new WaitForSeconds(enemyFollowDelay);

        // Find all enemies in the scene
        MainEnemy[] enemies = FindObjectsOfType<MainEnemy>();

        foreach (MainEnemy enemy in enemies)
        {
            if (enemy == null) continue;
            // Check if enemy is chasing and close to the door
            float distanceToDoor = Vector2.Distance(enemy.transform.position, transform.position);

            if (distanceToDoor < 5f) // Enemy is close to the door
            {
                Debug.Log($"Enemy {enemy.name} is close to door (distance: {distanceToDoor}), teleporting through!");

                // Calculate target position for enemy
                Vector2 enemyTargetPos = CalculateEnemyTargetPosition(enemy.transform.position);

                // Teleport enemy
                enemy.OnTeleportedThroughDoor(enemyTargetPos);

                Debug.Log($"Enemy teleported to {enemyTargetPos}");
            }
        }
    }

    private Vector2 CalculateEnemyTargetPosition(Vector2 enemyCurrentPos)
    {
        if (direction == Direction.Teleport && teleportTargetPosition != null)
        {
            // Add slight offset so enemy doesn't spawn exactly on player
            return (Vector2)teleportTargetPosition.position + (Random.insideUnitCircle * 1.5f);
        }

        Vector2 targetPos = enemyCurrentPos;
        switch (direction)
        {
            case Direction.Up: targetPos.y += additivePos; break;
            case Direction.Down: targetPos.y -= additivePos; break;
            case Direction.Left: targetPos.x -= additivePos; break;
            case Direction.Right: targetPos.x += additivePos; break;
        }

        return targetPos;
    }

    private IEnumerator CloseDoor()
    {
        transitioning = true;

        // Play door close sound
        if (doorInteraction != null)
        {
            doorInteraction.PlayDoorOpenSound();
        }

        if (hasAnimation && doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");

            yield return new WaitUntil(() =>
                doorAnimator.GetCurrentAnimatorStateInfo(0).IsName("Door_Closed"));

            float animLength = doorAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        isDoorOpen = false;
        transitioning = false;
    }

    private IEnumerator HandleTransition()
    {
        if (doorCollider != null)
            doorCollider.enabled = false;

        UpdatePlayerPosition(player);

        if (confiner != null && mapBoundary != null)
        {
            confiner.m_BoundingShape2D = mapBoundary;
            Debug.Log("Camera transitioned to new room.");
        }
        else
        {
            Debug.LogWarning("Confiner or map boundary not assigned!");
        }

        yield return new WaitForSeconds(0.5f);

        if (doorCollider != null)
            doorCollider.enabled = true;
    }

    private IEnumerator ForceDoorClosed()
    {
        if (doorAnimator != null)
        {
            doorAnimator.ResetTrigger("Open");
            doorAnimator.ResetTrigger("Close");
            doorAnimator.Play("Door_Close", 0, 0f);
        }
        yield return null;
    }

    private void ResetDoorToClosed()
    {
        if (doorAnimator != null)
        {
            doorAnimator.ResetTrigger("Open");
            doorAnimator.ResetTrigger("Close");
            doorAnimator.Play("Door_Close", 0, 0f);
        }
        isDoorOpen = false;
    }

    private void UpdatePlayerPosition(GameObject playerObj)
    {
        if (playerObj == null) return;

        if (direction == Direction.Teleport && teleportTargetPosition != null)
        {
            playerObj.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector3 newPos = playerObj.transform.position;
        switch (direction)
        {
            case Direction.Up: newPos.y += additivePos; break;
            case Direction.Down: newPos.y -= additivePos; break;
            case Direction.Left: newPos.x -= additivePos; break;
            case Direction.Right: newPos.x += additivePos; break;
        }

        playerObj.transform.position = newPos;
    }
}