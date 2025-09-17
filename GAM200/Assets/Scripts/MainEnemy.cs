using UnityEngine;

public class MainEnemy : EnemyBase
{
    [Header("Enemy Capture Settings")]
    public QTESystem qteSystem;     
    public int maxEscapeAttempts = 3;
    private int remainingAttempts;

    private bool isCapturing = false;   // Prevent chasing while capture is active
    private float pauseTimer = 0f;      // Time left to wait after failed capture


    // Additional behaviour: Tailing and Lingering
    private Vector2 lastKnownPosition;
    private bool isSuspicious = false;  // True when LOS is lost but still searching
    private float lingeringDuration = 3f; // How long Sadako keeps lingering around player's LKP
    private float lingeringTimer = 0f;


    protected override void Start()
    {
        base.Start();
        remainingAttempts = maxEscapeAttempts;

        // For now, Sadako always starts chasing right away (no cutscene yet)
        BeginChase();
    }

    protected override void Update()
    {
        // If Sadako is paused (after failed QTE), count down the timer
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return; // Don’t do anything else while paused
        }

        // If currently capturing, don’t move
        if (isCapturing) return;

        // If chasing, check LOS
        if (isChasing)
        {

            if (HasLineOfSight())
            {
                // Update last known position whenever Sadako can see the player
                lastKnownPosition = player.position;
                isSuspicious = false; // Reset suspicion
                lingeringTimer = lingeringDuration;

                // Move toward the player directly
                base.Update();

                // Check capture range
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance < 1.5f)
                {
                    TryCapturePlayer();
                }
            }
            else
            {
                // LOS lost: enter suspicion mode
                if (!isSuspicious)
                {
                    isSuspicious = true;
                    lingeringTimer = lingeringDuration;
                    Debug.Log("Sadako lost sight. Moving to last known position.");
                }

                // While suspicious, move toward last known position
                if (isSuspicious)
                {
                    lingeringTimer -= Time.deltaTime;
                    MoveTowards(lastKnownPosition);

                    float distToLKP = Vector2.Distance(transform.position, lastKnownPosition);

                    // If she reached last known pos OR suspicion time runs out
                    if (distToLKP < 0.5f || lingeringTimer <= 0)
                    {
                        Debug.Log("Sadako gives up and disappears.");
                        EndChase();
                    }
                }
            }
        }
    }
          
    private void MoveTowards(Vector2 target)
    {
        // Simple movement toward a target (without needing LOS)
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    private void TryCapturePlayer()
    {
        isCapturing = true;
        Debug.Log("Sadako grabs the player");
        player.GetComponent<CharacterMovement>().FreezeMovement();
        qteSystem.BeginQTE(5, OnEscapeSuccess, OnEscapeFail);
    }

    private void OnEscapeSuccess()
    {
        Debug.Log("Player escaped Sadako’s grasp");
        isCapturing = false;

        pauseTimer = 3f; // Sadako pauses for 3 seconds

        player.GetComponent<CharacterMovement>().UnfreezeMovement();
    }

    private void OnEscapeFail()
    {
        remainingAttempts--;

        if (remainingAttempts <= 0)
        {
            Debug.Log("GAME OVER: Sadako fully captured the player");
            // TODO: trigger game over system
        }
        else
        {
            Debug.Log("Player failed, Sadako grabs again");
            TryCapturePlayer(); // Retry until no attempts left
        }
    }
}
