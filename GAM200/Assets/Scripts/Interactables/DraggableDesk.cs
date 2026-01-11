using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableDesk : MonoBehaviour
{
    [Header("Desk Identity")]
    [SerializeField] private string deskInitials = "M.K."; // For identification
    [SerializeField] private Vector3 correctPosition; // Set this in Inspector for each desk
    [SerializeField] private bool isKayoDesk = false; // Mark the special desk

    [Header("Push Settings")]
    [SerializeField] private float pushSpeed = 3f;
    [SerializeField] private float pushDistance = 0.5f; // Distance per "push"
    [SerializeField] private LayerMask obstacleLayer; // What blocks movement

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer deskRenderer;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private GameObject correctPositionIndicator; // Optional glow effect

    private Vector3 startPosition;
    private Color originalColor;
    private bool isDragging = false;
    private bool isLocked = false;
    private Vector3 targetPosition;
    private DeskPuzzleManager puzzleManager;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (deskRenderer == null)
            deskRenderer = GetComponent<SpriteRenderer>();

        originalColor = deskRenderer.color;
        startPosition = transform.position;
        targetPosition = transform.position;

        if (correctPositionIndicator != null)
            correctPositionIndicator.SetActive(false);
    }

    public void Initialize(DeskPuzzleManager manager)
    {
        puzzleManager = manager;
    }

    private void Update()
    {
        // Smooth movement to target position
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, pushSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    private void OnMouseEnter()
    {
        if (!isLocked && !isDragging)
        {
            deskRenderer.color = highlightColor;
        }
    }

    private void OnMouseExit()
    {
        if (!isLocked && !isDragging)
        {
            deskRenderer.color = originalColor;
        }
    }

    private void OnMouseDown()
    {
        if (isLocked) return;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isLocked || !isDragging) return;

        // Get mouse position in world space
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Calculate push direction
        Vector3 pushDirection = (mousePos - transform.position).normalized;

        // Try to push in the dominant direction (4-directional movement)
        Vector3 snapDirection = GetSnapDirection(pushDirection);

        // Check if we can move in that direction
        Vector3 newTargetPos = transform.position + snapDirection * pushDistance;

        if (!IsPositionBlocked(newTargetPos))
        {
            targetPosition = newTargetPos;
        }
    }

    private void OnMouseUp()
    {
        if (isLocked) return;
        isDragging = false;
        deskRenderer.color = originalColor;

        // Snap to grid if close to correct position
        if (Vector3.Distance(transform.position, correctPosition) <= puzzleManager.GetSnapThreshold())
        {
            targetPosition = correctPosition;

            if (correctPositionIndicator != null)
                correctPositionIndicator.SetActive(true);
        }

        // Check if puzzle is complete
        puzzleManager?.CheckPuzzleComplete();
    }

    private Vector3 GetSnapDirection(Vector3 direction)
    {
        // Snap to 4 cardinal directions
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absX > absY)
        {
            return new Vector3(Mathf.Sign(direction.x), 0, 0);
        }
        else
        {
            return new Vector3(0, Mathf.Sign(direction.y), 0);
        }
    }

    private bool IsPositionBlocked(Vector3 position)
    {
        // Check if there's an obstacle at the target position
        Collider2D hit = Physics2D.OverlapCircle(position, 0.3f, obstacleLayer);

        // Also check other desks
        Collider2D[] desks = Physics2D.OverlapCircleAll(position, 0.3f);
        foreach (var desk in desks)
        {
            if (desk.gameObject != gameObject && desk.GetComponent<DraggableDesk>() != null)
            {
                return true;
            }
        }

        return hit != null;
    }

    public bool IsInCorrectPosition()
    {
        return Vector3.Distance(transform.position, correctPosition) < 0.1f;
    }

    public void LockDesk()
    {
        isLocked = true;
        deskRenderer.color = originalColor;
    }

    public bool IsKayoDesk()
    {
        return isKayoDesk;
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