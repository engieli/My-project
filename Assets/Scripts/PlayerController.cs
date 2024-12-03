using UnityEngine;

public enum PlayerDirection
{
    left, right
}

public enum PlayerState
{
    idle, walking, jumping, dead
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    private PlayerDirection currentDirection = PlayerDirection.right;
    public PlayerState currentState = PlayerState.idle;
    public PlayerState previousState = PlayerState.idle;

    [Header("Horizontal")]
    public float maxSpeed = 5f;
    public float accelerationTime = 0.25f;
    public float decelerationTime = 0.15f;

    [Header("Vertical")]
    public float apexHeight = 3f;
    public float apexTime = 0.5f;

    [Header("Ground Checking")]
    public float groundCheckOffset = 0.5f;
    public Vector2 groundCheckSize = new(0.4f, 0.1f);
    public LayerMask groundCheckMask;

    private float accelerationRate;
    private float decelerationRate;

    private float gravity;
    private float initialJumpSpeed;

    private bool isGrounded = false;
    public bool isDead = false;

    [Header("Dash")]
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    public float dashSpeed = 10f;

    private Vector2 velocity;

    // Double Jump variables
    private bool canDoubleJump = false; // Allow for double jump
    private bool hasDoubleJumped = false; // Tracks if double jump was used

    public void Start()
    {
        body.gravityScale = 0;

        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;

        gravity = -2 * apexHeight / (apexTime * apexTime);
        initialJumpSpeed = 2 * apexHeight / apexTime;
    }

    public void Update()
    {
        previousState = currentState;

        CheckForGround();

        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");

        if (isDead)
        {
            currentState = PlayerState.dead;
        }

        switch (currentState)
        {
            case PlayerState.dead:
                // do nothing - we ded.
                break;
            case PlayerState.idle:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x != 0) currentState = PlayerState.walking;
                break;
            case PlayerState.walking:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x == 0) currentState = PlayerState.idle;
                break;
            case PlayerState.jumping:
                if (isGrounded)
                {
                    if (velocity.x != 0) currentState = PlayerState.walking;
                    else currentState = PlayerState.idle;
                }
                break;
        }

        MovementUpdate(playerInput);
        JumpUpdate();

        if (!isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else
            velocity.y = 0;

        body.velocity = velocity;

        if (!isDashing)
        {
            MovementUpdate(playerInput);
        }
        else
        {
            DashUpdate();
        }

        JumpUpdate();

        if (!isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else
            velocity.y = 0;

        body.velocity = velocity;

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x < 0)
            currentDirection = PlayerDirection.left;
        else if (playerInput.x > 0)
            currentDirection = PlayerDirection.right;

        if (!isDashing)
        {
            if (playerInput.x != 0)
            {
                velocity.x += accelerationRate * playerInput.x * Time.deltaTime;
                velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
            }
            else
            {
                if (velocity.x > 0)
                {
                    velocity.x -= decelerationRate * Time.deltaTime;
                    velocity.x = Mathf.Max(velocity.x, 0);
                }
                else if (velocity.x < 0)
                {
                    velocity.x += decelerationRate * Time.deltaTime;
                    velocity.x = Mathf.Min(velocity.x, 0);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
        {
            Debug.Log("Dash triggered!");
            Dash();
        }
    }

    private void Dash()
    {
        if (currentDirection == PlayerDirection.right)
        {
            velocity.x = dashSpeed;
        }
        else if (currentDirection == PlayerDirection.left)
        {
            velocity.x = -dashSpeed;
        }

        dashCooldownTimer = dashCooldown;
        isDashing = true;
        dashTimer = dashDuration;
    }

    private void DashUpdate()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0)
        {
            isDashing = false;
        }
    }

    private void JumpUpdate()
    {
        if (isGrounded)
        {
            // Reset double jump when grounded
            hasDoubleJumped = false;
            canDoubleJump = true;

            if (Input.GetButton("Jump"))
            {
                velocity.y = initialJumpSpeed;
                isGrounded = false;
                currentState = PlayerState.jumping;
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && canDoubleJump && !hasDoubleJumped)
            {
                // Second jump
                velocity.y = initialJumpSpeed;
                hasDoubleJumped = true;
                canDoubleJump = false; // Disable further jumps until grounded
            }
        }
    }

    private void CheckForGround()
    {
        isGrounded = Physics2D.OverlapBox(
            transform.position + Vector3.down * groundCheckOffset,
            groundCheckSize,
            0,
            groundCheckMask);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }

    public bool IsWalking()
    {
        return velocity.x != 0;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public PlayerDirection GetFacingDirection()
    {
        return currentDirection;
    }
}

