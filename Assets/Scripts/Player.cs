using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public LayerMask platformsLayerMask; // what counts as a platform (will trigger isGrounded)
    public LayerMask wallsLayerMask; // what counts as a wall (will trigger isOnWall)
    public SpriteMask crouchSpriteMask;
    public float health = 3;
    public float jumpVelocity = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float dashVelocity = 25f;
    public float forceFallVelocity = 50f;
    public float speed = 10f;
    public float slideSpeed = 1f;
    public float wallJumpVelocity = 10f;
    public float wallJumpResetTime = 1f; // Prevents player from getting back to the wall
    public float wallJumpStopMultiplier = 2f; // higher amount = player is able to move back towards a wall they just jumped away from more easily
    public float nearWallDistance = 0.5f; // Allows wall jumping even if not exactly touching the wall
    public float slideTime = 1f;
    public Text healthDisplay;

    private Rigidbody2D rigidbody2d;
    private BoxCollider2D boxCollider2d;
    private bool dashAvailable = false;
    private bool isDashing = false;
    private float rigidBodyGravityScale;
    private bool isWallJumpingLeft = false;
    private bool isWallJumpingRight = false;
    private float wallJumpTimeLeft = 0;
    private bool isSliding = false;
    private float slideTimeLeft = 0;
    private bool grabWallToggle = false;

    private const KeyCode GrabWallKey = KeyCode.LeftShift;
    private const KeyCode JumpAndDashKey = KeyCode.Space;

    private float inputVerticalAxis = 0f;
    private float inputHorizontalAxis = 0f;


    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        rigidBodyGravityScale = rigidbody2d.gravityScale;
    }

    void Update()
    {
        // display health
        healthDisplay.text = "Health: " + health.ToString();

        // reset level if dead
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        GetPlayerInput();
        HandlePlayerControl();
    }

    private void GetPlayerInput()
    {
        inputHorizontalAxis = Input.GetAxisRaw("Horizontal");
        inputVerticalAxis = Input.GetAxisRaw("Vertical");
    }

    // Handle all player control, forces, and sprite changes
    // TODO: refactor into smaller & neater functions
    private void HandlePlayerControl()
    {
        // Get horizontal and vertical input
        // float x = Input.GetAxisRaw("Horizontal");
        // float y = Input.GetAxisRaw("Vertical");

        bool isGrounded = IsGrounded();

        bool isOnLeftWall = IsOnLeftWall();
        bool isOnRightWall = IsOnRightWall();
        bool isOnWall = isOnLeftWall || isOnRightWall;

        bool isNearLeftWall = IsNearLeftWall();
        bool isNearRightWall = IsNearRightWall();
        bool isNearWall = isNearLeftWall || isNearRightWall;

        bool isWallJumping = isWallJumpingLeft || isWallJumpingRight;

        bool justStartedWallJumping = wallJumpTimeLeft > wallJumpResetTime - 0.1;

        // check if we can grab the wall
        bool canGrabWall = !isGrounded && isOnWall && !justStartedWallJumping;
        if (!canGrabWall) { grabWallToggle = false; } // Reset toggle if we're no longer in a place to use it
        if (canGrabWall && Input.GetKeyDown(GrabWallKey)) { grabWallToggle = !grabWallToggle; } // Toggle whether we're grabbing the wall or not

        bool isGrabbingWall = canGrabWall && grabWallToggle;

        // reset dash
        if (isGrounded)
        {
            dashAvailable = true;
        }

        if (wallJumpTimeLeft > 0)
        {
            wallJumpTimeLeft -= Time.deltaTime;
        }

        // reset wall jump
        if (isGrounded || isGrabbingWall || wallJumpTimeLeft <= 0)
        {
            isWallJumpingLeft = false;
            isWallJumpingRight = false;
            wallJumpTimeLeft = 0;
        }

        // set gravity scale to 0 if we're grabbing wall to stay stuck to it
        if (isGrabbingWall)
        {
            if (rigidbody2d.gravityScale != 0)
            {
                rigidBodyGravityScale = rigidbody2d.gravityScale;
                rigidbody2d.gravityScale = 0;
            }
        }
        else
        {
            rigidbody2d.gravityScale = rigidBodyGravityScale;
        }

        // reset isDashing, and drag (after dash)
        if (isOnWall || rigidbody2d.drag <= 0)
        {
            isDashing = false;
            rigidbody2d.drag = 0;
        }
        else if (rigidbody2d.drag > 0)
        {
            rigidbody2d.drag -= 40f * Time.deltaTime; // TODO: make this value configurable in inspector
        }

        if (!isDashing)
        {
            // slide
            if (inputVerticalAxis == -1 && isGrounded && !isSliding)
            {
                StartSlide();
            }
            if (isSliding && slideTimeLeft > 0)
            {
                slideTimeLeft -= Time.deltaTime;
            }
            else if (isSliding && slideTimeLeft <= 0)
            {
                StopSlide();
            }

            // jump
            if (isGrounded && Input.GetKeyDown(JumpAndDashKey))
            {
                if (isSliding)
                {
                    StopSlide();
                }

                rigidbody2d.velocity = Vector2.up * jumpVelocity;
            }

            // move sidewards
            if (!isGrabbingWall && !justStartedWallJumping)
            {
                if (!isWallJumping)
                {
                    rigidbody2d.velocity = new Vector2(inputHorizontalAxis * speed, rigidbody2d.velocity.y);
                }
                else
                {
                    rigidbody2d.velocity = Vector2.Lerp(rigidbody2d.velocity, new Vector2(inputHorizontalAxis * speed, rigidbody2d.velocity.y), wallJumpStopMultiplier * Time.deltaTime);
                }
            }

            // falling - takes into account whether we're mid jump
            if (rigidbody2d.velocity.y < 0)
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rigidbody2d.velocity.y > 0 && !Input.GetKey(JumpAndDashKey))
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }

            // wall climb and wall slide
            if (isGrabbingWall) // wall climb
            {
                rigidbody2d.velocity = new Vector2(0, inputVerticalAxis * speed);
            }
            else if (((isOnLeftWall && !isGrounded && inputHorizontalAxis < 0) || (isOnRightWall && !isGrounded && inputHorizontalAxis > 0)) && !justStartedWallJumping) // only wall slide if player is moving towards wall
            {
                WallSlide();
            }

            // wall jump
            if (isNearWall && !isGrounded && Input.GetKeyDown(JumpAndDashKey) && !justStartedWallJumping)
            {
                float velocityX;
                if (isNearLeftWall)
                {
                    velocityX = wallJumpVelocity;
                    isWallJumpingRight = true;
                }
                else
                {
                    velocityX = -wallJumpVelocity;
                    isWallJumpingLeft = true;
                }

                rigidbody2d.velocity = new Vector2(velocityX, wallJumpVelocity);
                wallJumpTimeLeft = wallJumpResetTime;
            }
        }

        // dash
        if (dashAvailable && !isGrounded && !isNearWall && !isWallJumping && Input.GetKeyDown(JumpAndDashKey) && (inputHorizontalAxis != 0 || inputVerticalAxis != 0))
        {
            Dash(inputHorizontalAxis, inputVerticalAxis);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(new Vector2(boxCollider2d.bounds.center.x, boxCollider2d.bounds.center.y - boxCollider2d.bounds.size.y / 2), 0.01f, platformsLayerMask);
    }

    private bool IsOnLeftWall()
    {
        return Physics2D.OverlapCircle(new Vector2(boxCollider2d.bounds.center.x - boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y), 0.01f, wallsLayerMask);
    }

    private bool IsOnRightWall()
    {
        return Physics2D.OverlapCircle(new Vector2(boxCollider2d.bounds.center.x + boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y), 0.01f, wallsLayerMask);
    }

    private bool IsNearLeftWall()
    {
        return Physics2D.OverlapCircle(new Vector2(boxCollider2d.bounds.center.x - boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y), nearWallDistance, wallsLayerMask);
    }

    private bool IsNearRightWall()
    {
        return Physics2D.OverlapCircle(new Vector2(boxCollider2d.bounds.center.x + boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y), nearWallDistance, wallsLayerMask);
    }

    public void StartSlide()
    {
        isSliding = true;
        slideTimeLeft = slideTime;

        // Set sprite to half size
        float crouchSize = 0.5f;
        float verticalOffset = -0.25f;

        boxCollider2d.size = new Vector2(boxCollider2d.size.x, crouchSize);
        boxCollider2d.offset = new Vector2(boxCollider2d.offset.x, verticalOffset);

        Vector3 spriteMaskScale = crouchSpriteMask.transform.localScale;
        spriteMaskScale.y = crouchSize;
        crouchSpriteMask.transform.localScale = spriteMaskScale;

        Vector3 spriteMaskPos = crouchSpriteMask.transform.localPosition;
        spriteMaskPos.y = verticalOffset;
        crouchSpriteMask.transform.localPosition = spriteMaskPos;
    }

    public void StopSlide()
    {
        isSliding = false;

        // Set sprite to half size
        float standSize = 1f;
        float verticalOffset = 0f;

        boxCollider2d.size = new Vector2(boxCollider2d.size.x, standSize);
        boxCollider2d.offset = new Vector2(boxCollider2d.offset.x, verticalOffset);

        Vector3 spriteMaskScale = crouchSpriteMask.transform.localScale;
        spriteMaskScale.y = standSize;
        crouchSpriteMask.transform.localScale = spriteMaskScale;

        Vector3 spriteMaskPos = crouchSpriteMask.transform.localPosition;
        spriteMaskPos.y = -verticalOffset;
        crouchSpriteMask.transform.localPosition = spriteMaskPos;
    }

    public void Dash(float x, float y)
    {
        rigidbody2d.velocity = Vector2.zero;
        rigidbody2d.velocity += new Vector2(x, y).normalized * dashVelocity;
        dashAvailable = false;
        isDashing = true;
        rigidbody2d.drag = 10; // TODO: make this value configurable in inspector

        if (rigidbody2d.gravityScale != 0)
        {
            rigidBodyGravityScale = rigidbody2d.gravityScale;
            rigidbody2d.gravityScale = 0;
        }
    }

    public void WallSlide()
    {
        rigidbody2d.velocity = new Vector2(0, -slideSpeed);
    }
}
