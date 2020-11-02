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
    public float wallJumpStopMultiplier = 2f; // amount that player using the direction against the current wall jump will stop the wall jump
    public Text healthDisplay;

    private Rigidbody2D rigidbody2d;
    private BoxCollider2D boxCollider2d;
    private bool canDash = false;
    private bool isDashing = false;
    private float rigidBodyGravityScale;
    private bool isWallJumpingLeft = false;
    private bool isWallJumpingRight = false;

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        rigidBodyGravityScale = rigidbody2d.gravityScale;
    }

    void Update()
    {
        // Get horizontal and vertical input
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        bool isGrounded = IsGrounded();

        bool isOnLeftWall = IsOnLeftWall();
        bool isOnRightWall = IsOnRightWall();
        bool isOnWall = isOnLeftWall || isOnRightWall;

        bool isGrabbingWall = !isGrounded && isOnWall && Input.GetKey(KeyCode.LeftShift);

        bool isWallJumping = isWallJumpingLeft || isWallJumpingRight;

        // dispay health
        healthDisplay.text = "Health: " + health.ToString();

        // reset level if dead
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // reset dash
        if (isGrounded)
        {
            canDash = true;
        }

        // reset isWallJumping // TODO: make this time based, so that can say the jump is ended mid-air, and allow the player to jump back onto the same side and jump again after that
        if (isGrounded || isOnLeftWall)
        {
            isWallJumpingLeft = false;
        }
        if (isGrounded || isOnRightWall)
        {
            isWallJumpingRight = false;
        }

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
       
        // reset drag (after dash)
        if (rigidbody2d.drag > 0) // drag is set to 10 after dash
        {
            rigidbody2d.drag -= 0.2f;
        }
        else if (!isGrabbingWall) // don't want to reset gravity scale if we are grabbing wall
        {
            rigidbody2d.gravityScale = rigidBodyGravityScale;
            isDashing = false;
        }

        if (!isDashing)
        {
            // jump
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                rigidbody2d.velocity = Vector2.up * jumpVelocity;
            }

            // crouch
            Crouch(y == -1 && isGrounded);

            // move sidewards
            if (!isGrabbingWall)
            {
                if (!isWallJumping)
                {
                    rigidbody2d.velocity = new Vector2(x * speed, rigidbody2d.velocity.y);
                }
                else
                {
                    rigidbody2d.velocity = Vector2.Lerp(rigidbody2d.velocity, new Vector2(x * speed, rigidbody2d.velocity.y), 2f * Time.deltaTime);
                }
            }

            // falling
            if (rigidbody2d.velocity.y < 0)
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rigidbody2d.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }

            // wall climb and wall slide
            if (!isWallJumping)
            {
                if (isGrabbingWall) // wall climb
                {
                    rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, y * speed);
                }
                else if ((isOnLeftWall && !isGrounded && x < 0) || (isOnRightWall && !isGrounded && x > 0)) // only wall slide if player is moving towards wall
                {
                    WallSlide();
                }
            }

            // wall jump
            if (isGrabbingWall && Input.GetKeyDown(KeyCode.Space) && !isWallJumping)
            {
                float velocityX;
                if (isOnLeftWall)
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
            }
        }

        // dash
        if (canDash && !isGrounded && !isGrabbingWall && Input.GetKeyDown(KeyCode.J) && (x != 0 || y != 0)) // TODO: prevent dash during middle of wall jump
        {
            Dash(x, y);
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

    public void Crouch(bool shouldCrouch)
    {
        if (shouldCrouch)
        {
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
        else
        {
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
    }

    public void Dash(float x, float y)
    {
        rigidbody2d.velocity = Vector2.zero;
        rigidbody2d.velocity += new Vector2(x, y).normalized * dashVelocity;
        canDash = false;
        isDashing = true;
        rigidbody2d.drag = 10;

        if (rigidbody2d.gravityScale != 0)
        {
            rigidBodyGravityScale = rigidbody2d.gravityScale;
            rigidbody2d.gravityScale = 0;
        }
    }

    public void WallSlide()
    {
        rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, -slideSpeed);
    }
}
