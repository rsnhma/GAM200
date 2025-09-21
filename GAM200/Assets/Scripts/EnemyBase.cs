using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Scriptable Object Data")]
    public EnemyData baseData; 

    [Header("Detection Settings")]
    protected float lineOfSightRange;
    public LayerMask obstacleMask;

    [Header("Chase Settings")]
    protected float moveSpeed;
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
        if (isChasing)
        {
            ChasePlayer();
        }
    }

    protected bool HasLineOfSight()
    {
        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > lineOfSightRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleMask);

        return hit.collider == null; // true = player visible
    }

    protected virtual void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    public virtual void BeginChase() => isChasing = true;
    public virtual void EndChase()
    {
        isChasing = false;
        gameObject.SetActive(false);
    }
}
