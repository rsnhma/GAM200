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
    public float pushbackDistance = 3f;

    [Header("Audio")]
    public AudioSource chaseAudioSource;
    public AudioClip chaseMusic;
    [Range(0f, 1f)] public float chaseMusicVolume = 0.7f;
    public float musicFadeTime = 1f;
    private bool isChaseMusicPlaying = false;

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

    private Rigidbody2D rb;
    private EnemyManager.EnemyState currentState = EnemyManager.EnemyState.Inactive;

    private Vector2 lastKnownPosition;
    private float lingeringTimer = 3f;
    private bool hasExtendedLingering = false;

    private Vector2 roamDirection;
    private float roamCooldown = 0f;
    private Vector2 currentRoamTarget;

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

        NoiseSystem.OnNoiseEmitted += OnNoiseHeard;

        entityData = baseData as TheEntityData;
        if (entityData == null)
        {
            Debug.LogError("MainEnemy requires a TheEntityData ScriptableObject assigned!");
            return;
        }

        chaseSpeed = 5f;

        // Setup audio source if not assigned
        if (chaseAudioSource == null)
        {
            chaseAudioSource = gameObject.AddComponent<AudioSource>();
        }

        chaseAudioSource.loop = true;
        chaseAudioSource.playOnAwake = false;
        chaseAudioSource.clip = chaseMusic;
        chaseAudioSource.volume = 0f;

        FindCurrentRoom();
        StartCoroutine(StartChaseWithBuffer());

        lastPosition = transform.position;
        lastMovement = new Vector2(0f, 1f);
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        // Setup Rigidbody2D properly for collision detection
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // CRITICAL: Set these properties for proper collision
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // No gravity for top-down
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent physics rotation
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth movement

        // Ensure collider exists
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("Enemy needs a Collider2D component! Add CircleCollider2D or CapsuleCollider2D");
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
        if (SoundIndicatorUI.Instance != null)
        {
            SoundIndicatorUI.Instance.MarkAsDetected();
            Debug.Log("Enemy spawned chasing - marked as detected, UI won't show");
        }

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

        StopChaseMusic();
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
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

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
            if (captureCooldown > 0)
            {
                return;
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

        Vector2 targetPosition = (Vector2)transform.position + (roamDirection * roamSpeed * Time.deltaTime);
        rb.MovePosition(targetPosition);

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
                PlayChaseMusic();
                break;

            case EnemyManager.EnemyState.Suspicious:
                Debug.Log("Enemy state: Suspicious");
                isChasing = true;
                PlayChaseMusic();
                break;

            case EnemyManager.EnemyState.Patrolling:
                Debug.Log("Enemy state: Patrolling");
                isChasing = false;
                PickNewRoamDirection();
                StopChaseMusic();

                if (SoundIndicatorUI.Instance != null)
                {
                    SoundIndicatorUI.Instance.ResetDetection();
                }
                break;

            case EnemyManager.EnemyState.Inactive:
                Debug.Log("Enemy state: Inactive");
                isChasing = false;
                StopChaseMusic();

                if (SoundIndicatorUI.Instance != null)
                {
                    SoundIndicatorUI.Instance.ResetDetection();
                }
                break;
        }
    }

    private void PlayChaseMusic()
    {
        if (chaseMusic == null || chaseAudioSource == null)
        {
            Debug.LogWarning("Chase music or audio source not assigned!");
            return;
        }

        if (!isChaseMusicPlaying)
        {
            isChaseMusicPlaying = true;

            if (!chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Play();
            }

            StopAllCoroutines();
            StartCoroutine(FadeMusic(0f, chaseMusicVolume, musicFadeTime));

            Debug.Log("Chase music started!");
        }
    }

    private void StopChaseMusic()
    {
        if (isChaseMusicPlaying)
        {
            isChaseMusicPlaying = false;
            StopAllCoroutines();
            StartCoroutine(FadeMusicAndStop(chaseMusicVolume, 0f, musicFadeTime));

            Debug.Log("Chase music stopping...");
        }
    }

    private IEnumerator FadeMusic(float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            chaseAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        chaseAudioSource.volume = targetVolume;
    }

    private IEnumerator FadeMusicAndStop(float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            chaseAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        chaseAudioSource.volume = 0f;
        chaseAudioSource.Stop();
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

        Vector2 pushDirection = (transform.position - player.position).normalized;
        Vector2 pushbackPosition = (Vector2)transform.position + (pushDirection * pushbackDistance);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, pushDirection, pushbackDistance, obstacleLayers);
        if (hit.collider != null)
        {
            pushbackPosition = hit.point - (pushDirection * 0.5f);
        }

        transform.position = pushbackPosition;
        Debug.Log($"Enemy pushed back to {pushbackPosition}");

        pauseTimer = entityData.successPauseTime;
        captureCooldown = entityData.successPauseTime + 2f;
        Debug.Log($"Enemy stunned for {pauseTimer}s, capture cooldown: {captureCooldown}s");

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

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.OnQTEFailed();

            if (PlayerSanity.Instance.GetSanityPercent() <= 0.2f)
            {
                Debug.Log("Player sanity too low - triggering game over");

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

    // Called when enemy is teleported through a door
    public void OnTeleportedThroughDoor(Vector2 newPosition)
    {
        Debug.Log($"Enemy teleported through door to {newPosition}");
        transform.position = newPosition;

        // Update room tracking
        currentRoom = FindRoomByPosition(newPosition);

        // If chasing, update last known position to player's current position
        if (currentState == EnemyManager.EnemyState.Chasing && player != null)
        {
            lastKnownPosition = player.position;
            Debug.Log("Continuing chase in new room");
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