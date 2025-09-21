using UnityEngine;

public class MainEnemy : EnemyBase
{
    [Header("QTE System")]
    public QTESystem qteSystem;

    private TheEntityData entityData; // typed reference to SO
    private bool isCapturing = false;
    private float pauseTimer = 0f;

    // Suspicion / lingering behaviour
    private Vector2 lastKnownPosition;
    private bool isSuspicious = false;
    private float lingeringTimer = 0f;

    protected override void Start()
    {
        base.Start();

        // Explicit cast from baseData to TheEntityData
        entityData = baseData as TheEntityData;
        if (entityData == null)
        {
            Debug.LogError("MainEnemy requires a TheEntityData ScriptableObject assigned!");
            return;
        }
        
        moveSpeed = entityData.chaseSpeed;
        lingeringTimer = entityData.chaseBreakTime;
        BeginChase();
    }

    protected override void Update()
    {
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        if (isCapturing) return;

        if (isChasing)
        {
            if (HasLineOfSight())
            {
                lastKnownPosition = player.position;
                isSuspicious = false;
                lingeringTimer = entityData.chaseBreakTime;

                base.Update();

                float distance = Vector2.Distance(transform.position, player.position);
                if (distance < entityData.captureRange)
                    TryCapturePlayer();
            }
            else
                HandleSuspicion();
        }
    }

    private void HandleSuspicion()
    {
        if (!isSuspicious)
        {
            isSuspicious = true;
            lingeringTimer = entityData.chaseBreakTime;
        }

        lingeringTimer -= Time.deltaTime;
        MoveTowards(lastKnownPosition);

        if (Vector2.Distance(transform.position, lastKnownPosition) < 0.5f || lingeringTimer <= 0)
            EndChase();
    }

    private void MoveTowards(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    private void TryCapturePlayer()
    {
        isCapturing = true;
        player.GetComponent<CharacterMovement>().FreezeMovement();

        qteSystem.BeginQTE((int)entityData.qteDuration, OnEscapeSuccess, OnEscapeFail);
    }

    private void OnEscapeSuccess()
    {
        isCapturing = false;
        pauseTimer = entityData.successPauseTime;
        player.GetComponent<CharacterMovement>().UnfreezeMovement();

        player.GetComponent<PlayerSanity>().LoseSanity(entityData.sanityLossOnSuccess);

        EndChase();
    }

    private void OnEscapeFail()
    {
        isCapturing = false;
        player.GetComponent<CharacterMovement>().UnfreezeMovement();

        player.GetComponent<PlayerSanity>().LoseSanity(entityData.sanityLossOnFail);

        EndChase();
    }
}
