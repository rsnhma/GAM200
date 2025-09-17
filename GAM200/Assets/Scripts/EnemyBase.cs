using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Detection Settings")]
    public float lineOfSightRange = 6f;   // How far the enemy can "see"
    public LayerMask obstacleMask;        // Which layers block line of sight (e.g., walls, lockers, hedges)

    [Header("Chase Settings")]
    public float moveSpeed = 2f;          // Default chase speed
    protected Transform player;           // Reference to the player’s position
    protected bool isChasing = false;     // Track whether the enemy is currently chasing

    protected virtual void Start()
    {
        // Get reference to player using Unity's tag system
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
        }
    }

    /// <summary>
    /// Check if player is visible (line of sight).
    /// </summary>
    protected bool HasLineOfSight()
    {
        Vector2 direction = player.position - transform.position;   // Get direction to player
        float distance = direction.magnitude;                      // Distance between enemy and player

        if (distance > lineOfSightRange) return false;              // Too far away

        // Raycast to see if something blocks the line of sight
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleMask);

        if (hit.collider == null) return true;                      // Nothing blocking = player visible
        return false;                                               // Something in the way = no LOS
    }

    /// <summary>
    /// Enemy moves toward player.
    /// </summary>
    protected virtual void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Start chasing.
    /// </summary>
    public virtual void BeginChase()
    {
        isChasing = true;
    }

    /// <summary>
    /// Stop chasing and despawn (for disappearing behaviour).
    /// </summary>
    public virtual void EndChase()
    {
        isChasing = false;
        gameObject.SetActive(false); // Hide enemy when chase ends
    }
}
