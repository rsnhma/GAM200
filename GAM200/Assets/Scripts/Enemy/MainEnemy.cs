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
    public float obstacleCheckDistance = 1f;
    public LayerMask obstacleLayers;

    private TheEntityData entityData;
    private bool isCapturing = false;
    private float pauseTimer = 0f;

    [Header("Animation")]
    public Animator animator;
    public float rotationSpeed = 10f; // How fast enemy turns
    private SpriteRenderer spriteRenderer;
    private Vector2 lastPosition;
    private Vector2 lastMovement;


    // Enemy states
    private EnemyManager.EnemyState currentState = EnemyManager.EnemyState.Inactive;

    // Suspicion variables
    private Vector2 lastKnownPosition;
    private float lingeringTimer = 3f;
    private bool hasExtendedLingering = false;

    // Roam variables
    private Vector2 roamDirection;
    private float roamCooldown = 0f;
    private Vector2 currentRoamTarget;

    // Player components
    private CharacterMovement playerMovement;
    private PlayerSanity playerSanity;

    private GameObject currentRoom; // Track which room the enemy is in
    private bool isInDeactivatedRoom = false;


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

        chaseSpeed = 5f;

        // Find which room the enemy is currently in
        FindCurrentRoom();

        // Start chasing immediately
        StartCoroutine(StartChaseWithBuffer());

        // Initialize animation
        lastPosition = transform.position;
        lastMovement = new Vector2(0f, 1f); // Default facing up
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }
    private void FindCurrentRoom()
    {
        // Find the room this enemy is currently in by checking parent objects
        Transform currentTransform = transform;
        while (currentTransform != null)
        {
            if (currentTransform.CompareTag("Room"))
            {
                currentRoom = currentTransform.gameObject;
                Debug.Log($"Enemy spawned in room: {currentRoom.name}");
                break;
            }
            currentTransform = currentTransform.parent;
        }

        // If not found as child of a room, check by position
        if (currentRoom == null)
        {
            currentRoom = FindRoomByPosition(transform.position);
        }
    }

    private GameObject FindRoomByPosition(Vector3 position)
    {
        GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");
        foreach (GameObject room in rooms)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();
            if (roomCollider != null && roomCollider.bounds.Contains(position))
            {
                return room;
            }
        }
        return null;
    }

    private IEnumerator StartChaseWithBuffer()
    {
        // Buffer time for player to react
        yield return new WaitForSeconds(1f);
        Debug.Log("Buffer time ended - starting chase");
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
            lastKnownPosition = position;
            lingeringTimer = entityData.chaseBreakTime;
            TransitionToState(EnemyManager.EnemyState.Suspicious);
            Debug.Log($"MainEnemy alerted by noise (radius {radius}) at {position}");

            // Show sound indicator (only if not already detected)
            if (SoundIndicatorUI.Instance != null)
            {
                SoundIndicatorUI.Instance.ShowIndicator();
            }
        }
    }

    protected override void Update()
    {
        // Check if the current room is deactivated
        /*if (currentRoom != null && !currentRoom.activeInHierarchy && !isInDeactivatedRoom)
        {
            Debug.Log($"Enemy detected room deactivation: {currentRoom.name}");
            EmergencyTeleportToHallway();
            return;
        }*/
      
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

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        Vector2 currentPosition = transform.position;
        Vector2 movement = (currentPosition - lastPosition) / Time.deltaTime;

        if (movement.sqrMagnitude > 0.01f)
        {
            lastMovement = movement.normalized;

            // Calculate angle from movement direction
            float targetAngle = Mathf.Atan2(lastMovement.x, lastMovement.y) * Mathf.Rad2Deg;

            // Add 180 degrees to face the correct direction (front of sprite toward movement)
            targetAngle += 180f;

            // Smoothly rotate toward movement direction
            Quaternion targetRotation = Quaternion.Euler(0, 0, -targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Set walking animation (only use Speed, no directional blending needed)
            animator.SetFloat("Speed", movement.magnitude);
        }
        else
        {
            // Idle - stop animation but keep rotation
            animator.SetFloat("Speed", 0f);
        }

        lastPosition = currentPosition;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyManager.EnemyState.Chasing && !isCapturing)
        {
            float distance = Vector2.Distance(transform.position, other.transform.position);
            if (distance < entityData.captureRange)
            {
                TryCapturePlayer();
            }
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

        // Always track player's last known position when we can see them
        if (HasLineOfSight())
        {
            lastKnownPosition = player.position;
            lingeringTimer = entityData.chaseBreakTime;
        }
        else
        {
            // Even without line of sight, keep updating last known position if we're close
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < lineOfSightRange * 0.5f) // Within half of sight range
            {
                lastKnownPosition = player.position;
                lingeringTimer = Mathf.Max(lingeringTimer, 1f); // Keep at least 1 second
            }
        }

        // Continue chasing towards last known position
        ChasePlayer();

        // Only go to suspicious if we're stuck at last known position for a while
        float distanceToLastKnown = Vector2.Distance(transform.position, lastKnownPosition);

        // More lenient conditions - don't give up so easily
        if (!HasLineOfSight() && distanceToLastKnown < 0.5f)
        {
            // Reduce timer when stuck at location
            lingeringTimer -= Time.deltaTime * 2f;

            if (lingeringTimer <= 0f)
            {
                TransitionToState(EnemyManager.EnemyState.Suspicious);
            }
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

        // Check for line of sight more frequently in suspicious mode
        if (HasLineOfSight())
        {
            TransitionToState(EnemyManager.EnemyState.Chasing);
            return;
        }

        // When stuck, try to look around a bit
        if (distanceToLastPosition < 0.5f)
        {
            if (lingeringTimer <= 0f)
            {
                TransitionToState(EnemyManager.EnemyState.Patrolling);
            }
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
        Debug.Log($"Enemy state changing from {currentState} to {newState}");
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
                PickNewRoamDirection();

                // IMPORTANT: Reset detection when enemy loses player
                if (SoundIndicatorUI.Instance != null)
                {
                    SoundIndicatorUI.Instance.ResetDetection();
                }
                break;

            case EnemyManager.EnemyState.Inactive:
                Debug.Log("Enemy state: Inactive");
                isChasing = false;

                // Also reset on inactive
                if (SoundIndicatorUI.Instance != null)
                {
                    SoundIndicatorUI.Instance.ResetDetection();
                }
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

        // Reset detection after successful escape
        if (SoundIndicatorUI.Instance != null)
        {
            SoundIndicatorUI.Instance.ResetDetection();
        }
    }

    private void OnEscapeFail()
    {
        if (this == null) return;

        isCapturing = false;

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;

        if (playerMovement != null) playerMovement.UnfreezeMovement();

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.LoseSanity(entityData.sanityLossOnFail);

            if (PlayerSanity.Instance.GetSanityPercent() <= 0.2f)
            {
                GameOverUI gameOver = FindObjectOfType<GameOverUI>();
                if (gameOver != null)
                {
                    gameOver.ShowGameOver();
                    return;
                }
            }
        }

        TransitionToState(EnemyManager.EnemyState.Patrolling);

        // Reset detection after failed escape
        if (SoundIndicatorUI.Instance != null)
        {
            SoundIndicatorUI.Instance.ResetDetection();
        }
    }

    public void OnTeleportedToHallway()
    {
        Debug.Log("Enemy teleported to hallway");

        // Reset enemy state for hallway pursuit
        TransitionToState(EnemyManager.EnemyState.Chasing);

    }
    public GameObject GetCurrentRoom()
    {
        return currentRoom;
    }

    public bool IsInRoom(GameObject room)
    {
        if (room == null) return false;

        // Check if we're a child of the room
        if (transform.IsChildOf(room.transform))
            return true;

        // Check if our current room reference matches
        if (currentRoom == room)
            return true;

        // Check if we're physically in the room
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        if (roomCollider != null && roomCollider.bounds.Contains(transform.position))
            return true;

        return false;
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