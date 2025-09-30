using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Stats Reference")]
    public PlayerStats playerStats;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool canMove = true;
    private bool isSprinting;

    [Header("Audio & Noise")]
    public AudioSource footstepSource;
    public AudioClip walkClip;
    public AudioClip sprintClip;
    public float footstepInterval = 0.5f;
    public float walkNoiseRadius = 5f;
    public float sprintNoiseRadius = 10f;

    private float footstepTimer = 0f;

    [Header("Animation")]
    public Animator animator;

    // Track last direction for idle
    private Vector2 lastMoveDirection = Vector2.down; // default facing down

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Initialize animator to face lastMoveDirection
        animator.SetFloat("MoveX", lastMoveDirection.x);
        animator.SetFloat("MoveY", lastMoveDirection.y);
        animator.SetFloat("Speed", 0f);
    }

    void Update()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0f);
            return;
        }

        // Raw input (-1, 0, 1)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Update lastMoveDirection if moving
        if (movement.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = movement;
        }

        // Animator parameters
        animator.SetFloat("MoveX", movement.sqrMagnitude > 0.01f ? movement.x : lastMoveDirection.x);
        animator.SetFloat("MoveY", movement.sqrMagnitude > 0.01f ? movement.y : lastMoveDirection.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // Footstep sounds
        if (movement.sqrMagnitude > 0.01f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstep();
                footstepTimer = isSprinting ? footstepInterval / 1.5f : footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepSource != null)
        {
            AudioClip clip = isSprinting ? sprintClip : walkClip;
            if (clip != null)
                footstepSource.PlayOneShot(clip);
        }
    }

    void FixedUpdate()
    {
        float speed = isSprinting ? playerStats.sprintSpeed : playerStats.walkSpeed;
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

        if (movement.sqrMagnitude > 0.01f)
        {
            float radius = isSprinting ? sprintNoiseRadius : walkNoiseRadius;
            NoiseSystem.EmitNoise(transform.position, radius);
        }
    }

    public void FreezeMovement()
    {
        canMove = false;
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
    }

    public void UnfreezeMovement()
    {
        canMove = true;
    }
}
