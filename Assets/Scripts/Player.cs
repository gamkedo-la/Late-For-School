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
    public float sideSpeed = 1f;
    public float slideSpeed = 1f;
    public Text healthDisplay;

    private Rigidbody2D rigidbody2d;
    private BoxCollider2D boxCollider2d;
    private bool canDash = false;
    private bool isDashing = false;

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider2d = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Get horizontal and vertical input
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // dispay health
        healthDisplay.text = "Health: " + health.ToString();

        // reset level if dead
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        bool isGrounded = IsGrounded();

        // reset dash
        if (isGrounded)
        {
            canDash = true;
        }

        // prevent adding velocity into the walls - this stops the character from getting stuck on a wall if you are moving towards it while you're on it
        bool isOnLeftWall = IsOnLeftWall();
        bool isOnRightWall = IsOnRightWall();
        if ((isOnLeftWall && x < 0) || (isOnRightWall && x > 0)) // on left wall and moving left or on right wall and moving right
        {
            x = 0; // deaden x velocity
        }

        // reset drag (after dash)
        if (rigidbody2d.drag > 0)
        {
            rigidbody2d.drag -= 0.2f;
        }
        else
        {
            rigidbody2d.gravityScale = 3; // Todo: store old gravity, and reset it to that value
            isDashing = false;
        }

        if (!isDashing)
        {
            // jump
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                rigidbody2d.velocity = Vector2.up * jumpVelocity;
            }

            // force fall
            if (!isGrounded && y == -1)
            {
                rigidbody2d.AddForce(Vector2.down * forceFallVelocity);
            }

            // crouch
            Crouch(y == -1);


            // move sidewards
            rigidbody2d.velocity = new Vector2(x * sideSpeed, rigidbody2d.velocity.y);

            // falling
            if (rigidbody2d.velocity.y < 0)
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rigidbody2d.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }

            if ((isOnLeftWall || isOnRightWall) && !isGrounded)
            {
                WallSlide();
            }
        }

        // dash
        if (canDash && !isGrounded && Input.GetKeyDown(KeyCode.J) && (x != 0 || y != 0))
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
        rigidbody2d.gravityScale = 0;
    }

    public void WallSlide()
    {
        rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, -slideSpeed);
    }
}
