using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public LayerMask platformsLayerMask; // what counts as a platform (will trigger isGrounded)
    public LayerMask wallsLayerMask; // what counts as a wall (will trigger isOnWall)
    public SpriteMask crouchSpriteMask;
    public int maxHealth = 3;
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
    public float timeInvincibleAfterHurt = 2.5f;
    public float stuckSlowdownFactor = 2.0f;
    public List<GameObject> healthContainers;
    public GameObject dashContainer;
    public GameObject runParticles; // looping, turned off and on
    public GameObject jumpFxPrefab;
    public GameObject landFxPrefab;
    public GameObject wallgrabFxPrefab;
    public GameObject walljumpFxPrefab;
    public GameObject dashFxPrefab;

    [FMODUnity.EventRef]
    public string runSound;
    [FMODUnity.EventRef]
    public string jumpSound;
    [FMODUnity.EventRef]
    public string wallJumpSound;
    [FMODUnity.EventRef]
    public string slideSound;
    [FMODUnity.EventRef]
    public string wallSlideSound;
    [FMODUnity.EventRef]
    public string wallClimbSound;

    private int health = 3;
    private Rigidbody2D rigidbody2d;
    private BoxCollider2D boxCollider2d;
    private bool dashAvailable = true;
    private bool isDashing = false;
    private float rigidBodyGravityScale;
    private bool isWallJumpingLeft = false;
    private bool isWallJumpingRight = false;
    private float wallJumpTimeLeft = 0;
    private bool isSliding = false;
    private float slideTimeLeft = 0;
    private bool grabWallToggle = false;
    private bool isStuck = false;
    private bool isInvincible = false;

    private const KeyCode GrabWallKey = KeyCode.LeftShift;
    private const KeyCode JumpAndDashKey = KeyCode.Space;

    private float inputVerticalAxis = 0f;
    private float inputHorizontalAxis = 0f;
    private bool isJumpAndDashStarted = false;
    private bool isJumpAndDashMaintained = false;
    private bool isGrabWallStarted = false;
    private bool lookingRight = true;

    private bool useMobileInput = false;

    private FMOD.Studio.EventInstance wallSlideSoundInstance;
    private FMOD.Studio.EventInstance wallClimbSoundInstance;
    private FMOD.Studio.EventInstance runSoundInstance;

    private Animator anim;


    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        rigidBodyGravityScale = rigidbody2d.gravityScale;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        GetPlayerInput();
        HandlePlayerControl();

        DisplayHealth();
        DisplayDashAvailability();

        // reset level if dead
        if (health <= 0)
        {
            StopAllLoopingSounds();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (useMobileInput) // Reset wall and grab button to not pressed
        {
            isGrabWallStarted = false;
            isJumpAndDashStarted = false;
        }
    }

    private void DisplayHealth()
    {
        for (int i = 0; i < healthContainers.Count; i++)
        {
            if (i < health)
            {
                healthContainers[i].SetActive(true);
            }
            else
            {
                healthContainers[i].SetActive(false);
            }
        }
    }

    private void DisplayDashAvailability()
    {
        if (dashAvailable)
        {
            dashContainer.SetActive(true);
        }
        else
        {
            dashContainer.SetActive(false);
        }
    }

    private void GetPlayerInput()
    {
        if (!useMobileInput)
        {
            inputHorizontalAxis = Input.GetAxisRaw("Horizontal");
            inputVerticalAxis = Input.GetAxisRaw("Vertical");
            isGrabWallStarted = Input.GetKeyDown(GrabWallKey);
            isJumpAndDashStarted = Input.GetKeyDown(JumpAndDashKey);
            isJumpAndDashMaintained = Input.GetKey(JumpAndDashKey);
        }
    }

    // Handle all player control, forces, and sprite changes
    // TODO: refactor into smaller & neater functions
    private void HandlePlayerControl()
    {
        // Slow down movement & prevent player jump if stuck
        if (isStuck && GameManager.GetInstance().GetState() == GameManager.GameState.Play)
        {
            isJumpAndDashStarted = false;
            isJumpAndDashMaintained = false;

            // Move player left as staying still is actually moving forward
            Vector2 newPos = transform.position;
            newPos.x -= ChunkSpawner.GetInstance().speed / stuckSlowdownFactor * Time.deltaTime;
            transform.position = newPos;
        }

        bool isGrounded = IsGrounded();

        Collider2D isOnLeftWall = IsOnLeftWall();
        Collider2D isOnRightWall = IsOnRightWall();
        bool isOnWall = isOnLeftWall || isOnRightWall;

        // attach player to the wall - this is necessary for wall sliding, grabbing, and climbing to work correctly on moving walls
        if (isOnLeftWall && gameObject.transform.parent != isOnLeftWall.transform)
        {
            gameObject.transform.parent = isOnLeftWall.transform;
        }
        else if (isOnRightWall && gameObject.transform.parent != isOnRightWall.transform)
        {
            gameObject.transform.parent = isOnRightWall.transform;
        }
        else if (!isOnLeftWall && !isOnRightWall && gameObject.transform.parent != null)
        {
            gameObject.transform.parent = null;
        }

        bool isNearLeftWall = IsNearLeftWall();
        bool isNearRightWall = IsNearRightWall();
        bool isNearWall = isNearLeftWall || isNearRightWall;

        bool isWallJumping = isWallJumpingLeft || isWallJumpingRight;

        bool justStartedWallJumping = wallJumpTimeLeft > wallJumpResetTime - 0.1;

        // check if we can grab the wall
        bool canGrabWall = !isGrounded && isOnWall && !justStartedWallJumping;
        if (!canGrabWall) { grabWallToggle = false; } // Reset toggle if we're no longer in a place to use it
        if (canGrabWall && isGrabWallStarted) { grabWallToggle = !grabWallToggle; } // Toggle whether we're grabbing the wall or not

        bool isGrabbingWall = canGrabWall && grabWallToggle;

        bool isRunning = (GameManager.GetInstance().GetState() == GameManager.GameState.Play && isGrounded && !isSliding) ||
                         (GameManager.GetInstance().GetState() != GameManager.GameState.Play && isGrounded && inputHorizontalAxis != 0 && !isSliding);

        // only wall slide if player is moving towards wall
        bool attemptingWallSlide = ((isOnLeftWall && !isGrounded && inputHorizontalAxis < 0) || (isOnRightWall && !isGrounded && inputHorizontalAxis > 0));

        // Set isLookingRight
        if (attemptingWallSlide || isGrabbingWall || isWallJumping)
        {
            lookingRight = isOnRightWall || isWallJumpingRight;
        }
        else if (GameManager.GetInstance().GetState() != GameManager.GameState.Play)
        {
            if (inputHorizontalAxis != 0)
            {
                lookingRight = inputHorizontalAxis > 0;
            }
        }
        else
        {
            lookingRight = true;
        }

        if (isRunning)
        {
            StartRunSound();
            anim.SetBool("isRunning", true);
        }
        else
        {
            StopRunSound();
            anim.SetBool("isRunning", false);
        }

        if (lookingRight)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
        }
        else
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
        }

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
            isWallJumping = false;
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
            if (isGrounded && isJumpAndDashStarted)
            {
                if (isSliding)
                {
                    StopSlide();
                }

                rigidbody2d.velocity = Vector2.up * jumpVelocity;
                FMODUnity.RuntimeManager.PlayOneShot(jumpSound);

                if (jumpFxPrefab) Instantiate(jumpFxPrefab, transform.position, Quaternion.identity);
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
            else if (rigidbody2d.velocity.y > 0 && !isJumpAndDashMaintained)
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }

            // wall climb and wall slide
            if (isGrabbingWall) // wall climb
            {
                rigidbody2d.velocity = new Vector2(0, inputVerticalAxis * speed);
                StopWallSlideSound();

                if (inputVerticalAxis != 0)
                {
                    StartWallClimbSound();
                    if (wallgrabFxPrefab) Instantiate(wallgrabFxPrefab, transform.position, Quaternion.identity);

                }
                else
                {
                    StopWallClimbSound();
                }
            }
            else if (attemptingWallSlide && !justStartedWallJumping)
            {
                WallSlide();
                StopWallClimbSound();
            }
            else
            {
                StopWallSlideSound();
                StopWallClimbSound();
            }

            // wall jump
            if (isNearWall && !isGrounded && isJumpAndDashStarted && !justStartedWallJumping)
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
                FMODUnity.RuntimeManager.PlayOneShot(wallJumpSound);
                if (walljumpFxPrefab) Instantiate(walljumpFxPrefab, transform.position, Quaternion.identity);

            }
        }

        // dash
        if (dashAvailable && !isGrounded && !isNearWall && !justStartedWallJumping && isJumpAndDashStarted && (inputHorizontalAxis != 0 || inputVerticalAxis != 0))
        {
            Dash(inputHorizontalAxis, inputVerticalAxis);
            if (dashFxPrefab) Instantiate(dashFxPrefab, transform.position, Quaternion.identity);
            wallJumpTimeLeft = 0; // if we dash mid wall jump, we don't want to still be in the wall jump state
        }
    }

    private bool IsGrounded()
    {
        Bounds bounds = boxCollider2d.bounds;
        Vector2 topLeft = new Vector2(bounds.center.x - bounds.extents.x + 0.01f, bounds.center.y - bounds.extents.y + 0.01f);
        Vector2 bottomRight = new Vector2(bounds.center.x + bounds.extents.x - 0.01f, bounds.center.y - bounds.extents.y - 0.01f);
        return Physics2D.OverlapArea(topLeft, bottomRight, platformsLayerMask);
    }

    private Collider2D IsOnLeftWall()
    {
        Bounds bounds = boxCollider2d.bounds;
        Vector2 topLeft = new Vector2(bounds.center.x - bounds.extents.x - 0.01f, bounds.center.y + bounds.extents.y - 0.01f);
        Vector2 bottomRight = new Vector2(bounds.center.x - bounds.extents.x + 0.01f, bounds.center.y - bounds.extents.y + 0.01f);
        return Physics2D.OverlapArea(topLeft, bottomRight, platformsLayerMask);
    }

    private Collider2D IsOnRightWall()
    {
        Bounds bounds = boxCollider2d.bounds;
        Vector2 topLeft = new Vector2(bounds.center.x + bounds.extents.x - 0.01f, bounds.center.y + bounds.extents.y - 0.01f);
        Vector2 bottomRight = new Vector2(bounds.center.x + bounds.extents.x + 0.01f, bounds.center.y - bounds.extents.y + 0.01f);
        return Physics2D.OverlapArea(topLeft, bottomRight, platformsLayerMask);
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

        FMODUnity.RuntimeManager.PlayOneShot(slideSound); // TODO: Make slide loopable and stop sound in StopSlide()
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
        StartWallSlideSound();
    }

    public void StartRunSound()
    {
        if (!runSoundInstance.isValid())
        {
            runSoundInstance = FMODUnity.RuntimeManager.CreateInstance(runSound);
            runSoundInstance.start();
        }
    }

    public void StopRunSound()
    {
        if (runSoundInstance.isValid())
        {
            runSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            runSoundInstance.release();
        }
    }

    public void StartWallSlideSound()
    {
        if (!wallSlideSoundInstance.isValid())
        {
            wallSlideSoundInstance = FMODUnity.RuntimeManager.CreateInstance(wallSlideSound);
            wallSlideSoundInstance.start();
        }
    }

    public void StopWallSlideSound()
    {
        if (wallSlideSoundInstance.isValid())
        {
            wallSlideSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            wallSlideSoundInstance.release();
        }
    }

    public void StartWallClimbSound()
    {
        if (!wallClimbSoundInstance.isValid())
        {
            wallClimbSoundInstance = FMODUnity.RuntimeManager.CreateInstance(wallClimbSound);
            wallClimbSoundInstance.start();
        }
    }

    public void StopWallClimbSound()
    {
        if (wallClimbSoundInstance.isValid())
        {
            wallClimbSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            wallClimbSoundInstance.release();
        }
    }

    public void StopAllLoopingSounds()
    {
        StopRunSound();
        StopWallSlideSound();
        StopWallClimbSound();
    }

    public void SetHorizontalAxisInput(float value)
    {
        inputHorizontalAxis = value;
    }

    public void SetVerticalAxisInput(float value)
    {
        inputVerticalAxis = value;
    }

    public void SetJumpAndDashInput(bool value)
    {
        if(value && !isJumpAndDashMaintained)
        {
            isJumpAndDashStarted = value;
        }
        isJumpAndDashMaintained = value;
    }

    public void SetGrabWallInput(bool value)
    {
        isGrabWallStarted = value;
    }

    public void SetUseMobileInput(bool value)
    {
        useMobileInput = value;
    }

    public void AddHealth(int healthToAdd)
    {
        health += healthToAdd;
        if (health > maxHealth) { health = maxHealth; }
    }

    public void RemoveHealth(int healthToRemove)
    {
        health -= healthToRemove;
        if (health < 0) { health = 0; }
    }

    public void KillPlayer()
    {
        health = 0;
    }

    public void AddDash()
    {
        dashAvailable = true;
    }

    public void SlowPlayerMovement(float timeSecs)
    {
        if (timeSecs > 0)
        {
            isStuck = true;
            Invoke("ResumePlayerMovement", timeSecs);
        }
    }

    private void ResumePlayerMovement()
    {
        isStuck = false;
    }

    public void SetInvincible(float timeSecs)
    {
        if (!isInvincible)
        {
            isInvincible = true;
            Invoke("EndInvincibility", timeSecs);
        }
    }

    private void EndInvincibility()
    {
        isInvincible = false;
    }
}
