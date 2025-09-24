using UnityEngine;
using System;
using static NoiseSystem;
using UnityEngine.InputSystem;
using System.Collections;

public class MainEnemy : EnemyBase
{
    [Header("References")]
    public EnemyManager enemyManager;

    [Header("QTE System")]
    public QTESystem qteSystem;

    [Header("Roam/Patrol Settings")]
    public float roamSpeed = 1.5f;
    public float minRoamTime = 1f;
    public float maxRoamTime = 3f;
    public float obstacleCheckDistance = 0.5f;
    public LayerMask obstacleLayers; // Assign walls, borders, decor

    private TheEntityData entityData;
    private bool isCapturing = false;
    private float pauseTimer = 0f;

    // Enemy states
    private EnemyManager.EnemyState currentState = EnemyManager.EnemyState.Inactive;

    // Suspicion variables
    private Vector2 lastKnownPosition;
    private float lingeringTimer = 0f;
    private bool hasExtendedLingering = false;

    // Roam variables
    private Vector2 roamDirection;
    private float roamCooldown = 0f;
    private Vector2 currentRoamTarget;

    // Cache player components
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

        chaseSpeed = 3f;

        // If we have an enemy manager, apply saved state
        if (enemyManager != null && enemyManager.isEnemyActive)
        {
            ApplySavedState(enemyManager.currentEnemyState, enemyManager.lastKnownPosition, enemyManager.lingerTimer);
        }
        else
        {
            // Start chasing immediately with buffer time
            StartCoroutine(StartChaseWithBuffer());
        }
    }

    private IEnumerator StartChaseWithBuffer()
    {
        // 2 second buffer time for player to react
        yield return new WaitForSeconds(2f);
        TransitionToState(EnemyManager.EnemyState.Chasing);
    }

    public void ApplySavedState(EnemyManager.EnemyState savedState, Vector2 savedPosition, float savedTimer)
    {
        lastKnownPosition = savedPosition;
        lingeringTimer = savedTimer;
        TransitionToState(savedState);
    }

    private void OnDestroy()
    {
        NoiseSystem.OnNoiseEmitted -= OnNoiseHeard;

        // Save state to manager when destroyed (scene change)
        if (enemyManager != null && currentState != EnemyManager.EnemyState.Inactive)
        {
            enemyManager.UpdateEnemyState(currentState, lastKnownPosition, lingeringTimer);
        }
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
                lingeringTimer = entityData.chaseBreakTime;
                TransitionToState(EnemyManager.EnemyState.Suspicious);
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

        switch (currentState)
        {
            case EnemyManager.EnemyState.Chasing:
                UpdateChasing();
                break;
            case EnemyManager.EnemyState.Suspicious:
                UpdateSuspicious();
                break;
            case EnemyManager.EnemyState.Patrolling:
                UpdatePatrolling();
                break;
        }

        // Update manager with current state
        if (enemyManager != null)
        {
            enemyManager.UpdateEnemyState(currentState, lastKnownPosition, lingeringTimer);
        }
    }

    private void UpdateChasing()
    {
        // Check if player is hiding first (this should override line of sight)
        if (Locker.IsPlayerInsideLocker)
        {
            Debug.Log("Player is hiding in locker, switching to suspicious state");
            TransitionToState(EnemyManager.EnemyState.Suspicious);
            return;
        }

        if (HasLineOfSight())
        {
            lastKnownPosition = player.position;
            lingeringTimer = entityData.chaseBreakTime;

            ChasePlayer();

            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < entityData.captureRange)
                TryCapturePlayer();
        }
        else
        {
            TransitionToState(EnemyManager.EnemyState.Suspicious);
        }
    }

    private void UpdateSuspicious()
    {
        lingeringTimer -= Time.deltaTime;

        if (Locker.IsPlayerInsideLocker && !hasExtendedLingering)
        {
            lingeringTimer += 2f;
            hasExtendedLingering = true;
        }

        MoveTowards(lastKnownPosition);

        float distanceToLastPosition = Vector2.Distance(transform.position, lastKnownPosition);
        if (distanceToLastPosition < 0.5f || lingeringTimer <= 0f)
        {
            TransitionToState(EnemyManager.EnemyState.Patrolling);
        }

        if (HasLineOfSight())
        {
            TransitionToState(EnemyManager.EnemyState.Chasing);
        }
    }

    private void UpdatePatrolling()
    {
        roamCooldown -= Time.deltaTime;

        if (roamCooldown <= 0f || IsPathBlocked(roamDirection))
        {
            PickNewRoamDirection();
        }

        // Move in the current roam direction
        transform.position += (Vector3)(roamDirection * roamSpeed * Time.deltaTime);

        if (HasLineOfSight())
        {
            TransitionToState(EnemyManager.EnemyState.Chasing);
        }
    }

    private void PickNewRoamDirection()
    {
        // Try to find a clear direction
        Vector2 newDirection = Vector2.zero;
        int attempts = 0;

        while (attempts < 10) // Prevent infinite loop
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            newDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            if (!IsPathBlocked(newDirection))
            {
                roamDirection = newDirection;
                break;
            }
            attempts++;
        }

        // If all attempts failed, pick a random direction anyway
        if (attempts >= 10)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            roamDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        }

        roamCooldown = UnityEngine.Random.Range(minRoamTime, maxRoamTime);
    }

    private bool IsPathBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleCheckDistance, obstacleLayers);
        return hit.collider != null;
    }


    private void TransitionToState(EnemyManager.EnemyState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case EnemyManager.EnemyState.Chasing:
                Debug.Log("Enemy state: Chasing");
                isChasing = true;
                hasExtendedLingering = false;
                break;

            case EnemyManager.EnemyState.Suspicious:
                Debug.Log("Enemy state: Suspicious");
                isChasing = true;
                break;

            case EnemyManager.EnemyState.Patrolling:
                Debug.Log("Enemy state: Patrolling");
                isChasing = false;
                PickNewRoamDirection(); // Start roaming immediately
                break;

            case EnemyManager.EnemyState.Inactive:
                Debug.Log("Enemy state: Inactive");
                isChasing = false;
                break;
        }
    }

    private void TryCapturePlayer()
    {
        if (Locker.IsPlayerInsideLocker) return;

        if (player == null || playerMovement == null) return;

        isCapturing = true;
        playerMovement.FreezeMovement();

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        QTESystem.Instance.BeginQTE((int)entityData.qteDuration, OnEscapeSuccess, OnEscapeFail);
    }

    private void OnEscapeSuccess()
    {
        if (this == null) return;

        isCapturing = false;
        pauseTimer = entityData.successPauseTime;

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;

        if (playerMovement != null) playerMovement.UnfreezeMovement();
        if (PlayerSanity.Instance != null) PlayerSanity.Instance.LoseSanity(entityData.sanityLossOnSuccess);

        TransitionToState(EnemyManager.EnemyState.Patrolling);
    }

    private void OnEscapeFail()
    {
        if (this == null) return;

        isCapturing = false;

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;

        if (playerMovement != null) playerMovement.UnfreezeMovement();
        if (PlayerSanity.Instance != null) PlayerSanity.Instance.LoseSanity(entityData.sanityLossOnFail);

        TransitionToState(EnemyManager.EnemyState.Patrolling);
    }

    public override void BeginChase()
    {
        TransitionToState(EnemyManager.EnemyState.Chasing);
    }

    public override void EndChase()
    {
        TransitionToState(EnemyManager.EnemyState.Patrolling);
    }
}