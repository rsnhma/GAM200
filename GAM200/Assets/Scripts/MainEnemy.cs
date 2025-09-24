using UnityEngine;
using System;
using static NoiseSystem;
using UnityEngine.InputSystem;

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

    // Cache player components to avoid repeated GetComponent calls
    private CharacterMovement playerMovement;
    private PlayerSanity playerSanity;

    protected override void Start()
    {
        base.Start();

        // Cache player components
        if (player != null)
        {
            playerMovement = player.GetComponent<CharacterMovement>();
            playerSanity = player.GetComponent<PlayerSanity>();
        }

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

        // Safety checks
        if (player == null || playerMovement == null)
        {
            Debug.LogError("Player references are null!");
            return;
        }

        isCapturing = true;
        playerMovement.FreezeMovement();

        // Disable player input component if it exists
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        QTESystem.Instance.BeginQTE((int)entityData.qteDuration, OnEscapeSuccess, OnEscapeFail);
    }

    private void OnEscapeSuccess()
    {
        if (this == null) return;

        isCapturing = false;
        pauseTimer = entityData.successPauseTime;

        // Re-enable player input
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.UnfreezeMovement();
        }

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.LoseSanity(entityData.sanityLossOnSuccess);
        }
        else
        {
            Debug.LogError("PlayerSanity.Instance is null!");
        }

        EndChase();
    }

    private void OnEscapeFail()
    {
        if (this == null) return;

        isCapturing = false;

        // Re-enable player input
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.UnfreezeMovement();
        }

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.LoseSanity(entityData.sanityLossOnFail);
        }
        else
        {
            Debug.LogError("PlayerSanity.Instance is null!");
        }

        EndChase();
    }


    public override void EndChase()
    {
        isChasing = false;
        // Don't disable immediately - wait for cleanup to complete
        StartCoroutine(DisableAfterFrame());
    }

    private System.Collections.IEnumerator DisableAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }
}