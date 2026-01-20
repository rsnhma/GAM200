using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableDesk : MonoBehaviour
{
    [Header("Desk Identity")]
    [SerializeField] private string deskInitials = ""; // For identification
    [SerializeField] private Vector3 correctPosition; // Set this in Inspector for each desk
    [SerializeField] private bool isKayoDesk = false; // Mark the special desk

    [Header("Push Settings")]
    [SerializeField] private float pushSpeed = 2f;
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private LayerMask obstacleLayer; // What blocks movement
    [SerializeField] private float stopThreshold = 0.1f; // When to stop moving

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer deskRenderer;
    [SerializeField] private GameObject correctPositionIndicator; // Optional glow effect

    private Color originalColor;
    private bool isLocked = false;
    private DeskPuzzleManager puzzleManager;
    private Rigidbody2D rb;
    private Vector3 lastPosition;
    private bool wasMovingLastFrame = false;
    private Vector3 startPosition; // Store the initial scattered position

    private void Awake()
    {
        if (deskRenderer == null)
            deskRenderer = GetComponent<SpriteRenderer>();

        originalColor = deskRenderer.color;

        // Setup Rigidbody2D for physics-based pushing
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Don't let it rotate
        rb.gravityScale = 0; // No gravity for top-down
        rb.linearDamping = 5f; // Friction so it stops when not pushed
        rb.mass = 10f; // Heavy object

        if (correctPositionIndicator != null)
            correctPositionIndicator.SetActive(false);

        lastPosition = transform.position;
        startPosition = transform.position; // Save starting position
    }

    public void Initialize(DeskPuzzleManager manager)
    {
        puzzleManager = manager;
    }

    private void FixedUpdate()
    {
        if (isLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Check if desk has stopped moving
        bool isMoving = rb.linearVelocity.magnitude > stopThreshold;

        if (wasMovingLastFrame && !isMoving)
        {
            // Desk just stopped moving
            OnDeskStopped();
        }

        wasMovingLastFrame = isMoving;
    }

    private void OnDeskStopped()
    {
        // Check if close to correct position and snap
        if (Vector3.Distance(transform.position, correctPosition) <= puzzleManager.GetSnapThreshold())
        {
            transform.position = correctPosition;
            rb.linearVelocity = Vector2.zero;

            if (correctPositionIndicator != null)
                correctPositionIndicator.SetActive(true);

            Debug.Log($"Desk {deskInitials} snapped to correct position!");
        }
        else
        {
            // Desk stopped but NOT in correct position - notify puzzle manager
            puzzleManager?.OnDeskPlaced(this);
        }

        // Check if puzzle is complete
        puzzleManager?.CheckPuzzleComplete();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isLocked) return;

        // Check if player is pushing this desk
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get player's movement direction
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (playerRb != null && playerRb.linearVelocity.magnitude > 0.1f)
            {
                // Push desk in the direction player is moving
                Vector2 pushDirection = playerRb.linearVelocity.normalized;
                rb.AddForce(pushDirection * pushForce, ForceMode2D.Force);
            }
        }
    }

    public bool IsInCorrectPosition()
    {
        return Vector3.Distance(transform.position, correctPosition) < 0.15f;
    }

    public void LockDesk()
    {
        isLocked = true;
        rb.bodyType = RigidbodyType2D.Static; // Make it immovable
        deskRenderer.color = originalColor;
    }

    public bool IsKayoDesk()
    {
        return isKayoDesk;
    }

    public bool IsMoving()
    {
        return rb.linearVelocity.magnitude > stopThreshold;
    }

    public void ResetToStartPosition()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        isLocked = false;
        rb.bodyType = RigidbodyType2D.Dynamic;

        if (correctPositionIndicator != null)
            correctPositionIndicator.SetActive(false);

        Debug.Log($"Desk {deskInitials} reset to starting position");
    }

    // Helper method to set correct position from editor
    [ContextMenu("Set Current Position as Correct")]
    private void SetCorrectPosition()
    {
        correctPosition = transform.position;
        Debug.Log($"Desk {deskInitials} correct position set to: {correctPosition}");
    }

    // Visualize correct position in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(correctPosition, 0.3f);
        Gizmos.DrawLine(transform.position, correctPosition);
    }
}