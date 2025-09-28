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

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (baseData != null)
            lineOfSightRange = baseData.lineOfSightRange;
    }

    protected virtual void Update()
    {
        // Base Update is now empty - let derived classes handle their own update logic
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
        // Use the static property from your Locker class
        return Locker.IsPlayerInsideLocker;
    }

    protected virtual void ChasePlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * chaseSpeed * Time.deltaTime);
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

        Vector2 direction = (target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
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