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
    public SpacebarQTESystem spacebarQTESystem;
    public float pushbackDistance = 3f; // How far to push enemy back on successful escape

    [Header("Roam/Patrol Settings")]
    public float roamSpeed = 1.5f;
    public float minRoamTime = 1f;
    public float maxRoamTime = 3f;
    public float obstacleCheckDistance = 1f;
    public LayerMask obstacleLayers;

    private TheEntityData entityData;
    private bool isCapturing = false;
    private float pauseTimer = 0f;
    private float captureCooldown = 0f;

    [Header("Animation")]
    public Animator animator;
    public float rotationSpeed = 10f;
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

    private GameObject currentRoom;
    private bool isInDeactivatedRoom = false;

    protected override void Start()
    {
        base.Start();

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
        lastMovement = new Vector2(0f, 1f);
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    private void FindCurrentRoom()
    {
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

            if (SoundIndicatorUI.Instance != null)
            {
                SoundIndicatorUI.Instance.ShowIndicator();
            }
        }
    }
    protected override void Update()
    {
        // Handle pause timer (enemy stunned after player escapes)
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        // Handle capture cooldown (prevents immediate re-capture)
        if (captureCooldown > 0)
        {
            captureCooldown -= Time.deltaTime;
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

            float targetAngle = Mathf.Atan2(lastMovement.x, lastMovement.y) * Mathf.Rad2Deg;
            targetAngle += 180f;

            Quaternion targetRotation = Quaternion.Euler(0, 0, -targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            animator.SetFloat("Speed", movement.magnitude);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }

        lastPosition = currentPosition;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyManager.EnemyState.Chasing && !isCapturing)
        {
            // Check cooldown before allowing capture
            if (captureCooldown > 0)
            {
                return; // Silent cooldown
            }

            float distance = Vector2.Distance(transform.position, other.transform.position);
            if (distance < entityData.captureRange)
            {
                TryCapturePlayer();
            }
        }
    }

    private void UpdateChasing()
    {
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
        }
        else
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < lineOfSightRange * 0.5f)
            {
                lastKnownPosition = player.position;
                lingeringTimer = Mathf.Max(lingeringTimer, 1f);
            }
        }

        ChasePlayer();

        float distanceToLastKnown = Vector2.Distance(transform.position, lastKnownPosition);

        if (!HasLineOfSight() && distanceToLastKnown < 0.5f)
        {
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

        if (HasLineOfSight())
        {
            TransitionToState(EnemyManager.EnemyState.Chasing);
            return;
        }

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

        transform.position += (Vector3)(roamDirection * roamSpeed * Time.deltaTime);

        if (HasLineOfSight())
        {
            TransitionToState(EnemyManager.EnemyState.Chasing);
        }
    }

    private void PickNewRoamDirection()
    {
        Vector2 newDirection = Vector2.zero;
        int attempts = 0;

        while (attempts < 10)
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

                if (SoundIndicatorUI.Instance != null)
                {
                    SoundIndicatorUI.Instance.ResetDetection();
                }
                break;

            case EnemyManager.EnemyState.Inactive:
                Debug.Log("Enemy state: Inactive");
                isChasing = false;

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

        Debug.Log("=== CAPTURING PLAYER ===");

        isCapturing = true;
        playerMovement.FreezeMovement();

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        if (SpacebarQTESystem.Instance != null)
        {
            Debug.Log("Starting SpacebarQTE with callbacks...");
            SpacebarQTESystem.Instance.BeginQTE(
                successCallback: OnEscapeSuccess,
                failCallback: OnEscapeFail
            );
        }
        else
        {
            Debug.LogError("SpacebarQTESystem.Instance is null!");
            OnEscapeFail();
        }
    }

    private void OnEscapeSuccess()
    {
        Debug.Log("=== OnEscapeSuccess() CALLED ===");

        if (this == null)
        {
            Debug.LogError("Enemy is null in OnEscapeSuccess!");
            return;
        }

        Debug.Log("Player escaped via spacebar QTE!");

        isCapturing = false;

        // Unfreeze player FIRST
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
            Debug.Log("Player input re-enabled");
        }

        if (playerMovement != null)
        {
            playerMovement.UnfreezeMovement();
            Debug.Log("Player movement unfrozen");
        }

        // PUSHBACK: Enemy gets knocked back away from player
        Vector2 pushDirection = (transform.position - player.position).normalized;
        Vector2 pushbackPosition = (Vector2)transform.position + (pushDirection * pushbackDistance);

        // Check if pushback position hits a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, pushDirection, pushbackDistance, obstacleLayers);
        if (hit.collider != null)
        {
            // Hit a wall, push back less
            pushbackPosition = hit.point - (pushDirection * 0.5f);
        }

        transform.position = pushbackPosition;
        Debug.Log($"Enemy pushed back to {pushbackPosition}");

        // Set stun timer and capture cooldown
        pauseTimer = entityData.successPauseTime;
        captureCooldown = entityData.successPauseTime + 2f;
        Debug.Log($"Enemy stunned for {pauseTimer}s, capture cooldown: {captureCooldown}s");

        // Apply sanity loss
        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.OnQTESuccess();
        }

        TransitionToState(EnemyManager.EnemyState.Patrolling);

        if (SoundIndicatorUI.Instance != null)
        {
            SoundIndicatorUI.Instance.ResetDetection();
        }

        Debug.Log("=== OnEscapeSuccess() COMPLETE ===");
    }

    private void OnEscapeFail()
    {
        if (this == null) return;

        Debug.Log("Player failed spacebar QTE! Losing sanity and retrying...");

        // Apply larger sanity loss for failure
        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.OnQTEFailed();

            // Check for game over condition
            if (PlayerSanity.Instance.GetSanityPercent() <= 0.2f)
            {
                Debug.Log("Player sanity too low - triggering game over");

                // Release player before game over
                isCapturing = false;
                PlayerInput playerInput = player.GetComponent<PlayerInput>();
                if (playerInput != null) playerInput.enabled = true;
                if (playerMovement != null) playerMovement.UnfreezeMovement();

                GameOverUI gameOver = FindObjectOfType<GameOverUI>();
                if (gameOver != null)
                {
                    gameOver.ShowGameOver();
                    return;
                }
            }
        }

        // Player stays caught - retry QTE after brief delay
        StartCoroutine(RetryQTEAfterDelay());
    }

    private IEnumerator RetryQTEAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (isCapturing && PlayerSanity.Instance != null &&
            PlayerSanity.Instance.GetSanityPercent() > 0.2f)
        {
            Debug.Log("Restarting QTE - player must try again!");

            if (SpacebarQTESystem.Instance != null)
            {
                SpacebarQTESystem.Instance.BeginQTE(
                    successCallback: OnEscapeSuccess,
                    failCallback: OnEscapeFail
                );
            }
        }
        else
        {
            isCapturing = false;
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null) playerInput.enabled = true;
            if (playerMovement != null) playerMovement.UnfreezeMovement();
        }
    }

    public void OnTeleportedToHallway()
    {
        Debug.Log("Enemy teleported to hallway");
        TransitionToState(EnemyManager.EnemyState.Chasing);
    }

    public GameObject GetCurrentRoom()
    {
        return currentRoom;
    }

    public bool IsInRoom(GameObject room)
    {
        if (room == null) return false;

        if (transform.IsChildOf(room.transform))
            return true;

        if (currentRoom == room)
            return true;

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