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

    [Header("Settings")]
    [SerializeField] private bool hasAnimation = true;
    [SerializeField] private bool autoOpenForEnemies = true; // Allow enemies to pass through
    [SerializeField] private float autoCloseDelay = 2f; // Time before door closes after enemy passes

    private CinemachineConfiner confiner;
    private GameObject player;
    private bool playerAtDoor = false;
    private bool transitioning = false;
    private bool isDoorOpen = false;
    private bool canInteract = false;
    private Coroutine autoCloseCoroutine;

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
        else if (collision.CompareTag("Enemy") && autoOpenForEnemies)
        {
            // Enemy enters - open door automatically if not already open
            if (!isDoorOpen && !transitioning)
            {
                StartCoroutine(OpenDoorForEnemy(collision.gameObject));
            }
            
            // Cancel any pending auto-close since enemy is at door
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
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
        else if (collision.CompareTag("Enemy") && autoOpenForEnemies)
        {
            // Enemy left the trigger - start auto-close timer
            if (isDoorOpen && autoCloseCoroutine == null)
            {
                autoCloseCoroutine = StartCoroutine(AutoCloseDoorAfterDelay());
            }
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

        // Close the door immediately after transition
        yield return ForceDoorClosed();

        isDoorOpen = false;
        transitioning = false;
    }

    // Enemy opens door automatically - NO camera transition
    private IEnumerator OpenDoorForEnemy(GameObject enemy)
    {
        transitioning = true;

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

        isDoorOpen = true;

        // Disable door collider temporarily so enemy can pass
        if (doorCollider != null)
            doorCollider.enabled = false;

        // Move enemy through door
        UpdateEnemyPosition(enemy);

        yield return new WaitForSeconds(0.3f);

        // Re-enable collider
        if (doorCollider != null)
            doorCollider.enabled = true;

        transitioning = false;
    }

    private IEnumerator AutoCloseDoorAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        
        if (isDoorOpen)
        {
            yield return CloseDoor();
        }
        
        autoCloseCoroutine = null;
    }

    private IEnumerator CloseDoor()
    {
        transitioning = true;

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

    private void UpdateEnemyPosition(GameObject enemyObj)
    {
        if (enemyObj == null) return;

        if (direction == Direction.Teleport && teleportTargetPosition != null)
        {
            enemyObj.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector3 newPos = enemyObj.transform.position;
        switch (direction)
        {
            case Direction.Up: newPos.y += additivePos; break;
            case Direction.Down: newPos.y -= additivePos; break;
            case Direction.Left: newPos.x -= additivePos; break;
            case Direction.Right: newPos.x += additivePos; break;
        }

        enemyObj.transform.position = newPos;
    }
}