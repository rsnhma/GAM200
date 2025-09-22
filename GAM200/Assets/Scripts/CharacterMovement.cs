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
    public AudioClip walkClip;      // footstep sound for walking
    public AudioClip sprintClip;    // footstep sound for sprinting
    public float footstepInterval = 0.5f;
    public float walkNoiseRadius = 5f;
    public float sprintNoiseRadius = 10f;

    private float footstepTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Footstep timer
        if (movement.magnitude > 0f)
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
            // Choose clip based on walking or sprinting
            AudioClip clip = isSprinting ? sprintClip : walkClip;
            if (clip != null)
                footstepSource.PlayOneShot(clip);
        }
    }

    void FixedUpdate()
    {
        float speed = isSprinting ? playerStats.sprintSpeed : playerStats.walkSpeed;
        rb.linearVelocity = movement * speed;

        // Emit noise if moving
        if (movement.magnitude > 0f)
        {
            float radius = isSprinting ? sprintNoiseRadius : walkNoiseRadius;
            NoiseSystem.EmitNoise(transform.position, radius);
        }
    }

    public void FreezeMovement()
    {
        canMove = false;
        movement = Vector2.zero;
    }

    public void UnfreezeMovement()
    {
        canMove = true;
    }
}