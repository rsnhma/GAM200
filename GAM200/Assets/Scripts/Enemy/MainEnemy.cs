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
    public float roamSpeed = 2.5f; // Increased from 1.5f
    public float minRoamTime = 3f; // Increased from 1f
    public float maxRoamTime = 6f; // Increased from 3f
    public float obstacleCheckDistance = 1f;
    public LayerMask obstacleLayers;

    [Header("Camera Visibility")]
    [SerializeField] private float outOfViewDisableTime = 5f;
    private float outOfViewTimer = 0f;
    private bool isVisible = false;
    private bool cameraDespawnEnabled = false;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 1.5f; // Reduced from 2.5s
    [SerializeField] private float stunPushbackForce = 5f;
    [SerializeField] private float stunRecoveryCooldown = 1f; // Cooldown before enemy can capture again
    private bool isStunned = false;

    [Header("Spawn Point Settings")]
    [SerializeField] private float spawnPointSearchRadius = 3f;

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

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("Enemy needs a Collider2D component!");
        }
        else
        {
            col.isTrigger = false;
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
            Debug.Log("Enemy spawned chasing - marked as detected");
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
        if (isCapturing || isStunned) return;

        bool isValidNoise = Mathf.Approximately(radius, NoiseTypes.SprintRadius) ||
                            Mathf.Approximately(radius, NoiseTypes.LockerRadius) ||
                            Mathf.Approximately(radius, NoiseTypes.PuzzleFailRadius);

        if (isValidNoise)
        {
            lastKnownPosition = position;
            lingeringTimer = entityData.chaseBreakTime;

            CheckAndTeleportToNoiseLocation(position);

            TransitionToState(EnemyManager.EnemyState.Suspicious);
            Debug.Log($"MainEnemy alerted by noise (radius {radius}) at {position}");

            if (SoundIndicatorUI.Instance != null)
            {
                SoundIndicatorUI.Instance.ShowIndicator();
            }
        }
    }

    private void CheckAndTeleportToNoiseLocation(Vector2 noisePosition)
    {
        GameObject noiseRoom = FindRoomByPosition(noisePosition);
        GameObject enemyRoom = FindRoomByPosition(transform.position);

        if (noiseRoom != null && enemyRoom != null && noiseRoom != enemyRoom)
        {
            Debug.Log($"Noise in different room ({noiseRoom.name}), enemy in {enemyRoom.name} - teleporting to spawn point");

            if (EnemySpawnPointManager.Instance != null)
            {
                Transform nearestSpawnPoint = EnemySpawnPointManager.Instance.GetNearestSpawnPoint(noisePosition);

                if (nearestSpawnPoint != null)
                {
                    OnTeleportedThroughDoor(nearestSpawnPoint.position);
                    Debug.Log($"Enemy teleported to spawn point {nearestSpawnPoint.name}");
                }
                else
                {
                    Debug.LogWarning("No spawn point found near noise location!");
                }
            }
        }
    }

    protected override void Update()
    {
        if (isStunned)
        {
            UpdateAnimation();
            return;
        }

        if (cameraDespawnEnabled && !isVisible)
        {
            outOfViewTimer += Time.deltaTime;

            if (outOfViewTimer >= outOfViewDisableTime)
            {
                Debug.Log("Enemy out of view for 5+ seconds - patrolling elsewhere");
                TransitionToState(EnemyManager.EnemyState.Patrolling);
                outOfViewTimer = 0f;
            }
        }
        else
        {
            outOfViewTimer = 0f;
        }

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
        if (other.CompareTag("Player") && currentState == EnemyManager.EnemyState.Chasing && !isCapturing && !isStunned)
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

        CheckPlayerRoomTransition();

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

    private void CheckPlayerRoomTransition()
    {
        if (player == null) return;

        GameObject playerRoom = FindRoomByPosition(player.position);
        GameObject enemyRoom = FindRoomByPosition(transform.position);

        // Log for debugging
        Debug.Log($"[Room Check] Player room: {playerRoom?.name ?? "None"}, Enemy room: {enemyRoom?.name ?? "None"}");

        // Player is in a different room than enemy
        if (playerRoom != null && enemyRoom != null && playerRoom != enemyRoom)
        {
            Debug.Log($"<color=yellow>ROOM TRANSITION DETECTED! Player entered {playerRoom.name}, Enemy still in {enemyRoom.name}</color>");

            // Use the spawn point manager to find nearest spawn point
            if (EnemySpawnPointManager.Instance != null)
            {
                Transform nearestSpawnPoint = EnemySpawnPointManager.Instance.GetNearestSpawnPoint(player.position);

                if (nearestSpawnPoint != null)
                {
                    // Verify the spawn point is actually in the player's room
                    GameObject spawnPointRoom = FindRoomByPosition(nearestSpawnPoint.position);

                    if (spawnPointRoom == playerRoom)
                    {
                        Debug.Log($"<color=green>Teleporting enemy to spawn point: {nearestSpawnPoint.name} at position {nearestSpawnPoint.position}</color>");

                        // STOP chasing temporarily
                        isChasing = false;

                        // Teleport immediately
                        OnTeleportedThroughDoor(nearestSpawnPoint.position);

                        // Resume chase in new room
                        isChasing = true;
                        lastKnownPosition = player.position;

                        Debug.Log($"<color=cyan>Enemy successfully teleported and resuming chase!</color>");
                    }
                    else
                    {
                        Debug.LogWarning($"Nearest spawn point is in wrong room ({spawnPointRoom?.name}), looking for spawn point in {playerRoom.name}");

                        // Fallback: teleport near player
                        Vector2 fallbackPos = (Vector2)player.position + UnityEngine.Random.insideUnitCircle * 5f;
                        transform.position = fallbackPos;
                        currentRoom = playerRoom;
                    }
                }
                else
                {
                    Debug.LogError($"<color=red>NO SPAWN POINT FOUND! Check EnemySpawnPointManager!</color>");
                }
            }
            else
            {
                Debug.LogError("<color=red>EnemySpawnPointManager.Instance is NULL! Make sure it exists in the scene!</color>");
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
                StopChaseMusic();
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
            Debug.Log("Patrolling: New direction picked");
        }

        // Use transform movement for animation tracking
        transform.position += (Vector3)(roamDirection * roamSpeed * Time.deltaTime);

        // Check for line of sight while patrolling
        if (HasLineOfSight())
        {
            Debug.Log("Patrolling: Spotted player, switching to chase!");
            TransitionToState(EnemyManager.EnemyState.Chasing);
        }

        Debug.Log($"Patrolling: Moving in direction {roamDirection}, cooldown: {roamCooldown}");
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
                Debug.Log("Enemy state: Suspicious - searching for player");
                isChasing = true;
                PlayChaseMusic();
                break;

            case EnemyManager.EnemyState.Patrolling:
                Debug.Log("Enemy state: Patrolling - lost player");
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

    private void OnBecameVisible()
    {
        isVisible = true;
        outOfViewTimer = 0f;
        Debug.Log("Enemy is visible to camera");
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
        Debug.Log("Enemy is invisible to camera - starting timer");
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

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.OnQTESuccess();
        }

        if (SoundIndicatorUI.Instance != null)
        {
            SoundIndicatorUI.Instance.ResetDetection();
        }

        StartCoroutine(StunEnemy());

        Debug.Log("=== OnEscapeSuccess() COMPLETE ===");
    }

    private IEnumerator StunEnemy()
    {
        Debug.Log("Enemy stunned!");
        isStunned = true;

        // DON'T stop chase music - keep the tension!
        // Chase music will continue playing

        // Push enemy back from player
        if (player != null)
        {
            Vector2 pushDirection = (transform.position - player.position).normalized;
            Vector2 pushbackTarget = (Vector2)transform.position + (pushDirection * stunPushbackForce);

            float pushTimer = 0f;
            float pushDuration = 0.3f;
            Vector2 startPos = transform.position;

            while (pushTimer < pushDuration)
            {
                pushTimer += Time.deltaTime;
                transform.position = Vector2.Lerp(startPos, pushbackTarget, pushTimer / pushDuration);
                yield return null;
            }
        }

        // Visual feedback
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        // Wait for stun duration
        yield return new WaitForSeconds(stunDuration);

        // Recover from stun
        isStunned = false;

        // Set capture cooldown to prevent immediate re-capture
        captureCooldown = stunRecoveryCooldown;

        Debug.Log("Enemy recovered from stun, returning to patrol");

        // Transition to patrol state after stun
        TransitionToState(EnemyManager.EnemyState.Patrolling);
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

        StartCoroutine(RetryQTE());
    }

    private IEnumerator RetryQTE()
    {
        yield return new WaitForSeconds(0.0f);

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

    public void OnTeleportedThroughDoor(Vector2 newPosition)
    {
        Debug.Log($"Enemy teleported through door to {newPosition}");

        transform.position = newPosition;

        currentRoom = FindRoomByPosition(newPosition);

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