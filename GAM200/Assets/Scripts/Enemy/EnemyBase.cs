using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Scriptable Object Data")]
    public EnemyData baseData;

    [Header("Detection Settings")]
    protected float lineOfSightRange;
    public LayerMask obstacleMask;

    [Header("Chase Settings")]
    protected float chaseSpeed;
    protected Transform player;
    protected bool isChasing = false;

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceRadius = 0.2f; // Adjust based on enemy size, right now using basic cricle sprite
    public float obstacleAvoidanceDistance = 1.5f; // How far ahead to look

    // Private obstacle avoidance variables
    private RaycastHit2D[] obstacleHits;
    private float obstacleAvoidanceCooldown;
    private Vector2 avoidanceDirection;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (baseData != null)
            lineOfSightRange = baseData.lineOfSightRange;

        // Initialize obstacle detection array
        obstacleHits = new RaycastHit2D[10];
    }
    protected virtual void Update()
    {
        // Derived class handle their own update logic 
    }

    protected bool HasLineOfSight()
    {
        if (player == null) return false;

        // If player is hiding in locker, they can't be seen
        if (IsPlayerHiding())
        {
            return false;
        }

        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > lineOfSightRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleMask);

        return hit.collider == null; // true = player visible
    }

    protected bool IsPlayerHiding()
    {
        return Locker.IsPlayerInsideLocker;
    }

    protected virtual void ChasePlayer()
    {
        if (player == null) return;

        Vector2 targetDirection = (player.position - transform.position).normalized;

        // Check for obstacles and adjust direction
        targetDirection = GetObstacleAvoidanceDirection(targetDirection);

        transform.position += (Vector3)(targetDirection * chaseSpeed * Time.deltaTime);
    }

    private Vector2 GetObstacleAvoidanceDirection(Vector2 desiredDirection)
    {
        obstacleAvoidanceCooldown -= Time.deltaTime;

        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(obstacleMask);

        // Cast a circle in the desired direction to detect obstacles
        int hitCount = Physics2D.CircleCast(
            transform.position,
            obstacleAvoidanceRadius,
            desiredDirection,
            contactFilter,
            obstacleHits,
            obstacleAvoidanceDistance
        );

        // If obstacles detected, calculate avoidance
        for (int i = 0; i < hitCount; i++)
        {
            var hit = obstacleHits[i];

            // Skip self
            if (hit.collider.gameObject == gameObject)
                continue;

            // Wall sliding: Project desired direction onto wall surface
            Vector2 wallNormal = hit.normal;
            Vector2 slideDirection = desiredDirection - Vector2.Dot(desiredDirection, wallNormal) * wallNormal;

            // If we can't slide, try perpendicular directions
            if (slideDirection.magnitude < 0.1f)
            {
                // Try both perpendicular directions and pick the one closer to target
                Vector2 perpLeft = new Vector2(-wallNormal.y, wallNormal.x);
                Vector2 perpRight = new Vector2(wallNormal.y, -wallNormal.x);

                slideDirection = Vector2.Dot(perpLeft, desiredDirection) > 0 ? perpLeft : perpRight;
            }

            return slideDirection.normalized;
        }

        // No obstacles, return original direction
        return desiredDirection;
    }

    public virtual void BeginChase() => isChasing = true;

    public virtual void EndChase()
    {
        isChasing = false;
        // Removed gameObject.SetActive(false) - enemy should persist and patrol instead
    }

    // New method for moving towards a specific target (useful for patrolling/suspicion)
    protected virtual void MoveTowards(Vector2 target, float speed = -1)
    {
        if (speed < 0) speed = chaseSpeed;

        Vector2 desiredDirection = (target - (Vector2)transform.position).normalized;

        // Apply obstacle avoidance to MoveTowards as well
        Vector2 finalDirection = GetObstacleAvoidanceDirection(desiredDirection);

        transform.position += (Vector3)(finalDirection * speed * Time.deltaTime);
    }

    // New method to check if player is within capture range
    protected virtual bool IsPlayerInCaptureRange(float captureRange)
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) < captureRange;
    }

    // New method to get distance to player
    protected virtual float GetDistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    // New method to safely get player position
    protected virtual Vector2 GetPlayerPosition()
    {
        if (player == null) return Vector2.zero;
        return player.position;
    }
}