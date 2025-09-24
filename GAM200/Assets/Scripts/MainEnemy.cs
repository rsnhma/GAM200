using UnityEngine;
using System;
using static NoiseSystem;

public class MainEnemy : EnemyBase
{
    [Header("QTE System")]
    public QTESystem qteSystem;

    private TheEntityData entityData;
    private bool isCapturing = false;
    private float pauseTimer = 0f;

    // Suspicion / lingering
    private Vector2 lastKnownPosition;
    private bool isSuspicious = false;
    private float lingeringTimer = 0f;
    private bool hasExtendedLingering = false;

    protected override void Start()
    {
        base.Start();

        // Subscribe to noise events
        NoiseSystem.OnNoiseEmitted += OnNoiseHeard;

        entityData = baseData as TheEntityData;
        if (entityData == null)
        {
            Debug.LogError("MainEnemy requires a TheEntityData ScriptableObject assigned!");
            return;
        }

        chaseSpeed = 3f; // Fixed chase speed
        lingeringTimer = entityData.chaseBreakTime;
        isChasing = false;
    }
   
    private void OnDestroy()
    {
        NoiseSystem.OnNoiseEmitted -= OnNoiseHeard; // cleanup
    }

 
    private void OnNoiseHeard(Vector2 position, float radius)
    {
        if (isCapturing) return;

        bool isValidNoise = Mathf.Approximately(radius, NoiseTypes.SprintRadius) ||
                            Mathf.Approximately(radius, NoiseTypes.LockerRadius) ||
                            Mathf.Approximately(radius, NoiseTypes.PuzzleFailRadius);

        if (isValidNoise)
        {
            float distance = Vector2.Distance(transform.position, position);
            if (distance <= radius)
            {
                lastKnownPosition = position;
                isSuspicious = true;
                lingeringTimer = entityData.chaseBreakTime;
                BeginChase(); // start moving toward noise source
                Debug.Log($"MainEnemy alerted by noise (radius {radius}) at {position}");
            }
        }
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
            hasExtendedLingering = false;
        }

        // Always tick down
        lingeringTimer -= Time.deltaTime;

        if (Locker.IsPlayerInsideLocker && !hasExtendedLingering)
        {
            // Give extra linger time once
            lingeringTimer += 2f;
            hasExtendedLingering = true;
        }

        MoveTowards(lastKnownPosition);

        if (lingeringTimer <= 0f)
        {
            EndChase(); // leave regardless of locker state
        }
    }

    private void MoveTowards(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(dir * chaseSpeed * Time.deltaTime);
    }

    private void TryCapturePlayer()
    {
        if (Locker.IsPlayerInsideLocker) return;

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
