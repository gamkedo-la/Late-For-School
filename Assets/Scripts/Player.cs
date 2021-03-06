﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Skill
    {
        Jump,
        Dash,
        Slide,
        WallClimb,
        WallJump
    }

    public LayerMask platformsLayerMask; // what counts as a platform (will trigger isGrounded)
    public SpriteRenderer slideSprite;
    public Vector2 startPos;
    public int maxHealth = 3;
    public float jumpVelocity = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float dashVelocity = 25f;
    public float forceFallVelocity = 50f;
    public float speed = 10f;
    public float airAcceleration = 20f;
    public float slideSpeed = 1f;
    public float wallJumpVelocity = 10f;
    public float wallJumpResetTime = 1f; // Prevents player from getting back to the wall
    public float wallJumpStopMultiplier = 2f; // higher amount = player is able to move back towards a wall they just jumped away from more easily
    public float nearWallDistance = 0.5f; // Allows wall jumping even if not exactly touching the wall
    public float onWallDistance = 0.1f;
    public float groundedDistance = 0.25f;
    public float slideTime = 1f;
    public float timeInvincibleAfterHurt = 2.5f;
    public float stuckSlowdownFactor = 5.0f;
    public float jumpSlowdownFactor = 1.75f;
    public float dashSlowdownFactor = 2.0f;
    public Color stuckColour = Color.grey;
    public bool ledgeAutoStop = true; // stops the player when they reach a ledge and arent moving
    public bool upToJump = false; // this allows the up key to allow jumping as well as jump key CURRENTLY BROKEN AS HOLDING IT CHANGES HOW WE AFFECT GRAVITY SO CHANGES GAME
    public float groundedRecentlyTime = 0.15f; // amount of time we decide it's okay for the player to jump instead of dash after leaving the ground
    public List<GameObject> healthContainers;
    public GameObject dashContainer;
    public GameObject runParticles; // looping, turned off and on
    public GameObject jumpFxPrefab;
    public GameObject landFxPrefab;
    public GameObject slideFxPrefab;
    public GameObject walljumpFxPrefab;
    public GameObject dashFxPrefab;
    public PopupTooltip wallClimbTooltip;
    public PopupTooltip wallDetachTooltip;
    public float zPos = -2;

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
    [FMODUnity.EventRef]
    public string lifeLostSound;
    [FMODUnity.EventRef]
    public string dashSound;

    private SpriteRenderer uprightSprite;
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
    private bool isGrabbingWall = false;
    private bool isStuck = false;
    private bool isInvincible = false;
    private float prevVelocityX = 0;
    private bool jumpedWhileAgainstRightWall = false;
    private float timeSinceLeftGround = 0;
    private bool leftGroundFromJump = false;
    private bool wallClimbTooltipShown = false;
    private bool wallDetachTooltipShown = false;
    private bool wasGrounded = false;

    private const KeyCode GrabWallKey = KeyCode.LeftShift;
    private const KeyCode JumpAndDashKey = KeyCode.Space;

    private float inputVerticalAxis = 0f;
    private float inputHorizontalAxis = 0f;
    private float inputHorizontalAxisDown = 0f;
    private float inputVerticalAxisDown = 0f;
    private bool inputHorizontalAxisInUse = false;
    private bool inputVerticalAxisInUse = false;
    private bool jumpAndDashKeyDown = false;
    private bool jumpAndDashKey = false;
    private bool grabKeyDown = false;

    public enum HorizontalDirection
    {
        Left,
        Right
    }
    private HorizontalDirection lookDirection = HorizontalDirection.Right;
    private HorizontalDirection slideDirection;

    private bool useMobileInput = false;

    private FMOD.Studio.EventInstance wallSlideSoundInstance;
    private FMOD.Studio.EventInstance wallClimbSoundInstance;
    private FMOD.Studio.EventInstance runSoundInstance;

    private Animator anim;

    private static Player instance;

    public static Player GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        uprightSprite = GetComponent<SpriteRenderer>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        rigidBodyGravityScale = rigidbody2d.gravityScale;
        anim = GetComponent<Animator>();
        ResetPos();
    }

    private void Update() {
        // fixes bug where player stops being in the set z position
        var pos = transform.position;
        pos.z = zPos;
        transform.position = pos;

        if (GameManager.GetInstance().GetState() != GameManager.GameState.Pause)
        {
            GetPlayerInput();

            // reset variables associated with when and how we left the ground
            if (IsGrounded())
            {
                timeSinceLeftGround = 0;

                if (!wasGrounded)
                { // we *just* landed!
                    if (landFxPrefab) Instantiate(landFxPrefab, new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z), Quaternion.identity);
                }

            }
            else
            {
                timeSinceLeftGround += Time.deltaTime;
            }

            if (IsGrounded() && !wasGrounded)
            {
                leftGroundFromJump = false;
            }

            HandleGravity();
            HandleHorizontalMovement();
            HandleDash(); // Dash has to be before jump, for jumping at edge to work
            HandleJump();
            HandleWallClimb();
            HandleWallJump();
            HandleSlide();
            HandleLookDirection();
            HandlePushPlayerOverWall();

            if (slideSprite.enabled != isSliding && !isInvincible)
            {
                slideSprite.enabled = isSliding;
                uprightSprite.enabled = !isSliding;
            }

            DisplayHealth();
            DisplayDashAvailability();

            // reset level if dead
            if (health <= 0 && GameManager.GetInstance().GetState() == GameManager.GameState.Play)
            {
                Debug.Log("Player Lost");
                ScoreManager.GetInstance().SaveScore();
                StopAllLoopingSounds();
                GameManager.GetInstance().TransitionToRunSummaryState();
                ResetPos();
                FreezePos();
                rigidbody2d.velocity = Vector3.zero;
            }

            if (useMobileInput) // Reset wall and grab button to not pressed
            {
                grabKeyDown = false;
                jumpAndDashKeyDown = false;
            }

            wasGrounded = IsGrounded();
        }
        else
        {
            StopAllLoopingSounds();
        }
    }

    void FixedUpdate()
    {
        HandleWallGrab();
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
            SetHorizontalAxisInput(Input.GetAxisRaw("Horizontal"));
            SetVerticalAxisInput(Input.GetAxisRaw("Vertical"));
            grabKeyDown = Input.GetKeyDown(GrabWallKey);
            jumpAndDashKeyDown = Input.GetKeyDown(JumpAndDashKey);
            jumpAndDashKey = Input.GetKey(JumpAndDashKey);
        }
    }

    private void HandleJump()
    {
        if (!isDashing)
        {
            // jump
            bool canJump = (IsGrounded() || (timeSinceLeftGround <= groundedRecentlyTime && !leftGroundFromJump)) && !(isSliding && ObstructionAbove(0.5f));
            if (canJump && (jumpAndDashKeyDown || (upToJump && inputVerticalAxisDown > 0)))
            {
                if (isSliding)
                {
                    StopSlide();
                }

                Vector2 jumpVector = Vector2.up * jumpVelocity;
                if (isStuck) { jumpVector /= jumpSlowdownFactor; }
                rigidbody2d.velocity = jumpVector;
                FMODUnity.RuntimeManager.PlayOneShot(jumpSound);

                if (jumpFxPrefab) Instantiate(jumpFxPrefab, transform.position, Quaternion.identity);

                // This variable is used to prevent the player from auto grabbing if jumping too near the wall (see in HandleWallGrab)
                if (IsNearRightWall()) { jumpedWhileAgainstRightWall = true; }

                leftGroundFromJump = true;
            }
        }

        if (!IsNearRightWall()) { jumpedWhileAgainstRightWall = false; }
    }

    private void HandleHorizontalMovement()
    {
        if (!isDashing)
        {
            bool justStartedWallJumping = wallJumpTimeLeft > wallJumpResetTime - 0.1;
            // move sidewards
            if (!isGrabbingWall && !justStartedWallJumping)
            {
                // sliding while background not moving - slow down amount that horizontal movement has one speed
                if (GameManager.GetInstance().GetState() != GameManager.GameState.Play && isSliding)
                {
                    rigidbody2d.velocity = new Vector2(inputHorizontalAxis * speed / 2, rigidbody2d.velocity.y);
                }
                else if (GameManager.GetInstance().GetState() == GameManager.GameState.Play && isStuck)
                {
                    float velX = -1 * stuckSlowdownFactor;
                    velX += (inputHorizontalAxis * speed) / stuckSlowdownFactor;
                    rigidbody2d.velocity = new Vector2(velX, rigidbody2d.velocity.y);
                }
                // ground movement
                else if (!isWallJumpingLeft && !isWallJumpingRight && IsGrounded())
                {
                    if (isSliding && inputHorizontalAxis < 0)
                    {
                        inputHorizontalAxis /= 2;
                    }
                    rigidbody2d.velocity = new Vector2(inputHorizontalAxis * speed, rigidbody2d.velocity.y);
                } // normal air movement
                else if (!isWallJumpingLeft && !isWallJumpingRight && !IsGrounded())
                {
                    rigidbody2d.velocity = Vector2.Lerp(rigidbody2d.velocity, new Vector2(inputHorizontalAxis * speed, rigidbody2d.velocity.y), airAcceleration * Time.deltaTime);
                }
                // prevent moving back to the wall if jumping away from it
                else
                {
                    rigidbody2d.velocity = Vector2.Lerp(rigidbody2d.velocity, new Vector2(inputHorizontalAxis * speed, rigidbody2d.velocity.y), wallJumpStopMultiplier * Time.deltaTime);
                }
            }


            bool isOnWall = IsOnLeftWall() || IsOnRightWall();
            // Running sound
            bool isRunning = (GameManager.GetInstance().GetState() == GameManager.GameState.Play && IsGrounded() && !isSliding && !isOnWall) ||
                     (GameManager.GetInstance().GetState() != GameManager.GameState.Play && IsGrounded() && inputHorizontalAxis != 0 && !isSliding && !isOnWall);

            runParticles.gameObject.SetActive(isRunning);

            // Stop at ledge
            if (GameManager.GetInstance().GetState() == GameManager.GameState.Play && AtRightLedge() && inputHorizontalAxis == 0 && ledgeAutoStop)
            {
                Vector2 pos = transform.position;
                pos.x -= ChunkSpawner.GetInstance().levelConfig.speed * Time.deltaTime;
                transform.position = pos;
                isRunning = false;
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
        }
    }

    private void HandleDash()
    {

        // reset dash
        if (IsGrounded())
        {
            dashAvailable = true;
        }

        // reset isDashing, and drag (after dash)
        if (IsOnLeftWall() || IsOnRightWall() || rigidbody2d.drag <= 0)
        {
            isDashing = false;
            rigidbody2d.drag = 0;
        }
        else if (rigidbody2d.drag > 0)
        {
            rigidbody2d.drag -= 40f * Time.deltaTime; // TODO: make this value configurable in inspector
        }

        // dash
        bool justStartedWallJumping = wallJumpTimeLeft > wallJumpResetTime - 0.1;
        bool isNearWall = IsNearLeftWall() || IsNearRightWall();
        bool canJump = IsGrounded() || (timeSinceLeftGround <= groundedRecentlyTime && !leftGroundFromJump); // this line causes handledash to have to be before handlejump (otherwise jump will set leftGroundFromJump)
        if (dashAvailable && !canJump && !isNearWall && !justStartedWallJumping && jumpAndDashKeyDown)
        {
            Dash(inputHorizontalAxis, inputVerticalAxis);
            
            if (dashFxPrefab) {
                GameObject go = Instantiate(dashFxPrefab, transform.position, Quaternion.identity);
                go.transform.SetParent(transform); // stay inside the player so the trail follows you as you move
            }
            
            // if (dashParticles) dashParticles.Play();//Stop(); IDEA: maybe turn on anf off a looping ps instead
            
            wallJumpTimeLeft = 0; // if we dash mid wall jump, we don't want to still be in the wall jump state
        }
    }

    private void HandleWallGrab()
    {
        Collider2D isOnLeftWall = IsOnLeftWall();
        Collider2D isOnRightWall = IsOnRightWall();
        bool isOnWall = isOnLeftWall || isOnRightWall;
        bool isGrounded = IsGrounded();

        bool backgroundMoving = GameManager.GetInstance().GetState() == GameManager.GameState.Play;

        bool attachToWall = ((isOnLeftWall && !isGrounded && prevVelocityX < 0 && !isWallJumpingRight) ||
                             (isOnRightWall && !isGrounded && prevVelocityX > 0 && !isWallJumpingLeft) ||
                             (isOnWall && inputVerticalAxis > 0) ||
                             // want to attach to wall if background moving and velocity is 0, since this still means we're moving right
                             // except if we jump when we're close to the wall, we probably don't want to attach since we'll attach too low
                             (backgroundMoving && isOnRightWall && !isGrounded && prevVelocityX >= 0 && !isWallJumpingLeft && !jumpedWhileAgainstRightWall));
        bool detatchFromWall = (!isOnLeftWall && !isOnRightWall) || 
                               ((isOnLeftWall && inputHorizontalAxis > 0) || (isOnRightWall && inputHorizontalAxis < 0) || 
                               (isGrounded && inputVerticalAxis <= 0));
        if (IsOnDangerousObjectOnWall()) { 
            attachToWall = false;

            if (isOnRightWall && rigidbody2d.velocity.x > 0 || isOnLeftWall && rigidbody2d.velocity.x < 0)
            {
                rigidbody2d.velocity = new Vector2(0, rigidbody2d.velocity.y);
            }
        }

        if (attachToWall) { isGrabbingWall = true; }
        if (detatchFromWall) { isGrabbingWall = false; }

        // set gravity scale to 0 if we're grabbing wall to stay stuck to it
        if (isGrabbingWall)
        {
            if (rigidbody2d.gravityScale != 0)
            {
                rigidBodyGravityScale = rigidbody2d.gravityScale;
                rigidbody2d.gravityScale = 0;
                
                // For some reason this is needed to get the player to stick to the walls consistently from a dash or wall jump
                if (isOnLeftWall) { rigidbody2d.velocity = new Vector2(-100, 0); }
                else if (isOnRightWall) { rigidbody2d.velocity = new Vector2(100, 0); }
            }
        }
        else
        {
            rigidbody2d.gravityScale = rigidBodyGravityScale;
        }

        // set wall to parent
        if (isGrabbingWall && isOnLeftWall && gameObject.transform.parent != isOnLeftWall.transform)
        {
            gameObject.transform.parent = isOnLeftWall.transform;
        }
        else if (isGrabbingWall && isOnRightWall && gameObject.transform.parent != isOnRightWall.transform)
        {
            gameObject.transform.parent = isOnRightWall.transform;
        }
        else if (!isOnLeftWall && !isOnRightWall && gameObject.transform.parent != null)
        {
            gameObject.transform.parent = null;
        }

        prevVelocityX = rigidbody2d.velocity.x;
    }

    private void HandleWallJump()
    {
        bool justStartedWallJumping = wallJumpTimeLeft > wallJumpResetTime - 0.1;
        bool isNearWall = IsNearLeftWall() || IsNearRightWall();

        if (wallJumpTimeLeft > 0)
        {
            wallJumpTimeLeft -= Time.deltaTime;
        }

        // reset wall jump
        if (IsGrounded() || isGrabbingWall || wallJumpTimeLeft <= 0)
        {
            isWallJumpingLeft = false;
            isWallJumpingRight = false;
            //isWallJumping = false;
            wallJumpTimeLeft = 0;
        }

        // wall jump
        if (isNearWall && !IsGrounded() && jumpAndDashKeyDown && !justStartedWallJumping && !IsOnDangerousObjectOnWall())
        {
            isGrabbingWall = false; // let go of wall
            float velocityX;
            if (IsNearLeftWall())
            {
                velocityX = wallJumpVelocity;
                isWallJumpingRight = true;
            }
            else
            {
                velocityX = -wallJumpVelocity;
                isWallJumpingLeft = true;
            }
            Vector2 wallJumpVector = new Vector2(velocityX, wallJumpVelocity);
            if (isStuck) { wallJumpVector /= jumpSlowdownFactor; }
            rigidbody2d.velocity = wallJumpVector;
            wallJumpTimeLeft = wallJumpResetTime;
            FMODUnity.RuntimeManager.PlayOneShot(wallJumpSound);
            if (walljumpFxPrefab) Instantiate(walljumpFxPrefab, transform.position, Quaternion.identity);
        }
    }

    private void HandlePushPlayerOverWall()
    {
        float pushUpAmount = 10f;
        if (GameManager.GetInstance().GetState() != GameManager.GameState.Play) { pushUpAmount = 12.5f; }

        if (!IsOnRightWall() && FeetAreOnRightWall() && inputHorizontalAxis > 0 && inputVerticalAxis > 0)
        {
            rigidbody2d.velocity = Vector2.up * pushUpAmount;
        }
        if (!IsOnLeftWall() && FeetAreOnLeftWall() && inputHorizontalAxis < 0 && inputVerticalAxis > 0)
        {
            rigidbody2d.velocity = Vector2.up * pushUpAmount;
        }
    }

    private void HandleLookDirection()
    {
        // Set isLookingRight
        if (isGrabbingWall)
        {
            if (IsOnLeftWall())
            {
                lookDirection = HorizontalDirection.Left;
            }
            else if (IsOnRightWall())
            {
                lookDirection = HorizontalDirection.Right;
            }
        }
        else if (isWallJumpingLeft)
        {
            lookDirection = HorizontalDirection.Left;
        }
        else if (isWallJumpingRight)
        {
            lookDirection = HorizontalDirection.Right;
        }
        else if (GameManager.GetInstance().GetState() != GameManager.GameState.Play)
        {
            if (!isSliding) // Don't allow changing direction mid slide
            {
                if (inputHorizontalAxis > 0)
                {
                    lookDirection = HorizontalDirection.Right;
                }
                else if (inputHorizontalAxis < 0)
                {
                    lookDirection = HorizontalDirection.Left;
                }
            }
        }
        else
        {
            lookDirection = HorizontalDirection.Right;
        }

        // Change transform to look left or right
        if (lookDirection == HorizontalDirection.Right)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
        }
        else
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
        }
    }

    private void HandleWallClimb()
    {
        // wall climb
        if (isGrabbingWall)
        {
            Vector2 climbVector = new Vector2(0, inputVerticalAxis * speed);
            if (isStuck) { climbVector /= stuckSlowdownFactor; }
            rigidbody2d.velocity = climbVector;

            if (inputVerticalAxis != 0 && !ObstructionAboveOverlapCircle(0.1f, 0.1f))
            {
                StartWallClimbSound();
                anim.SetBool("isClimbing", true);
            }
            else
            {
                StopWallClimbSound();
                anim.SetBool("isClimbing", false);
            }
            anim.SetBool("isWallGrabbing", true);


            // Set off climb tooltip if player hasn't climbed in level yet
            if (GameManager.GetInstance().GetState() == GameManager.GameState.Play &&
                GameManager.GetInstance().levelInputConfig.includeTutorialChunks &&
                IsOnRightWall() &&
                !ChunkSpawner.GetInstance().IsKnownSkill(Skill.WallClimb) && 
                !wallClimbTooltipShown)
            {
                wallClimbTooltip.parent = ChunkSpawner.GetInstance().LastChunk().transform;
                wallClimbTooltip.Activate(transform.position + wallClimbTooltip.position);
                wallClimbTooltipShown = true;
            }

            // Set off detach tooltip if player hasn't attached to left side yet
            if (GameManager.GetInstance().GetState() == GameManager.GameState.Play &&
                GameManager.GetInstance().levelInputConfig.includeTutorialChunks &&
                IsOnLeftWall() &&
                !ChunkSpawner.GetInstance().IsKnownSkill(Skill.WallJump) && 
                !wallDetachTooltipShown)
            {
                wallDetachTooltip.parent = ChunkSpawner.GetInstance().LastChunk().transform;
                wallDetachTooltip.Activate(transform.position + wallDetachTooltip.position);
                wallDetachTooltipShown = true;
            }
        }
        else
        {
            anim.SetBool("isWallGrabbing", false);
            anim.SetBool("isClimbing", false);
            StopWallClimbSound();
        }
    }

    private void HandleGravity()
    {
        if (!isDashing)
        {
            // falling - takes into account whether we're mid jump
            if (rigidbody2d.velocity.y < 0)
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rigidbody2d.velocity.y > 0 && !jumpAndDashKey && !(upToJump && inputVerticalAxis > 0))
            {
                rigidbody2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
    }

    private void HandleSlide()
    {
        bool canSlide = CanSlideRight(boxCollider2d.bounds.size.y / 2); // TODO: replace this parameter with the slide collider height when it is known
        if (GameManager.GetInstance().GetState() != GameManager.GameState.Play)
        {
            canSlide = canSlide && CanSlideLeft(boxCollider2d.bounds.size.y / 2);
        }

        if (inputVerticalAxisDown < 0 && IsGrounded() && !isSliding && canSlide)
        {
            StartSlide();
        }
        if (isSliding && slideTimeLeft > 0)
        {
            if (slideFxPrefab) {
                var slideParticles = Instantiate(slideFxPrefab);
                Vector3 particlesPosition = slideParticles.transform.position;
                particlesPosition.x = transform.position.x;
                particlesPosition.y = transform.position.y - 0.5f;
                slideParticles.transform.position = particlesPosition;
                if ((GameManager.GetInstance().GetState() == GameManager.GameState.Play ||
                    GameManager.GetInstance().GetState() == GameManager.GameState.RunSummary)
                    && ChunkSpawner.GetInstance().LastChunk() != null)
                {
                    // attach particles to a chunk so that they move left if the game is being played
                    slideParticles.transform.parent = ChunkSpawner.GetInstance().LastChunk().transform;
                }
            };
            // If we're not playing the game, then the game is not moving - get player slide to move
            if (GameManager.GetInstance().GetState() != GameManager.GameState.Play)
            {
                float speed;
                if (slideDirection == HorizontalDirection.Left) { speed = -slideSpeed; }
                else { speed = slideSpeed; }
                Vector2 velocity = rigidbody2d.velocity;
                velocity.x += speed;
                rigidbody2d.velocity = velocity;
            }
            slideTimeLeft -= Time.deltaTime;
        }
        else if (isSliding && slideTimeLeft <= 0 && !ObstructionAbove(0.5f))
        {
            StopSlide();
        }


        // Stop sliding if we hit a wall or for some reason we're no longer on the ground
        if (isSliding && !canSlide)
        {
            StopSlide();
        }
        if (isSliding && !IsGrounded())
        {
            StopSlide();
        }
    }

    private bool IsGrounded()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x, boxCollider2d.bounds.center.y - boxCollider2d.bounds.size.y / 2);
        float radius = groundedDistance;
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private Collider2D IsOnLeftWall()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x - boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y);
        float radius = 0.03f;
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private Collider2D FeetAreOnLeftWall()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x - boxCollider2d.bounds.size.x / 2, 
                                       boxCollider2d.bounds.center.y - boxCollider2d.bounds.extents.y * 0.8f);
        float radius = 0.01f;
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private Collider2D IsOnRightWall()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x + boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y);
        float radius = 0.03f;
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private bool IsOnDangerousObjectOnWall()
    {
        var colliders = Physics2D.OverlapBoxAll(boxCollider2d.bounds.center, boxCollider2d.bounds.extents * 2f, 0);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<StationaryDangerousObstacle>() && collider.gameObject.GetComponent<StationaryDangerousObstacle>().isOnWall) {
                return true;
            }
        }

        return false;
    }

    private Collider2D FeetAreOnRightWall()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x + boxCollider2d.bounds.size.x / 2,
                                       boxCollider2d.bounds.center.y - boxCollider2d.bounds.extents.y * 0.8f);
        float radius = 0.01f;
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private bool IsNearLeftWall()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x - boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y);
        float radius = nearWallDistance * 2;
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private bool IsNearRightWall()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x + boxCollider2d.bounds.size.x / 2, boxCollider2d.bounds.center.y);
        float radius = nearWallDistance * 2;
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private bool ObstructionAbove(float amountAbove)
    {
        Bounds bounds = boxCollider2d.bounds;
        Vector2 topLeft = new Vector2(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y + amountAbove);
        Vector2 bottomRight = new Vector2(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y);
        return Physics2D.OverlapArea(topLeft, bottomRight, platformsLayerMask);
    }

    private bool ObstructionAboveOverlapCircle(float amountAbove, float radius)
    {
        Bounds bounds = boxCollider2d.bounds;
        Vector2 position = new Vector2(bounds.center.x, bounds.center.y + bounds.extents.y + amountAbove);
        return Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    private bool CanSlideRight(float slideColliderHeight)
    {
        Bounds bounds = boxCollider2d.bounds;
        Vector2 topLeft = new Vector2(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y + slideColliderHeight);
        Vector2 bottomRight = new Vector2(bounds.center.x + bounds.extents.x + 0.01f, bounds.center.y - bounds.extents.y + 0.01f);
        return !Physics2D.OverlapArea(topLeft, bottomRight, platformsLayerMask);
    }

    private bool CanSlideLeft(float slideColliderHeight)
    {
        Bounds bounds = boxCollider2d.bounds;
        Vector2 topLeft = new Vector2(bounds.center.x - bounds.extents.x - 0.01f, bounds.center.y - bounds.extents.y + slideColliderHeight);
        Vector2 bottomRight = new Vector2(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y + 0.01f);
        return !Physics2D.OverlapArea(topLeft, bottomRight, platformsLayerMask);
    }

    private bool AtRightLedge()
    {
        Vector2 position = new Vector2(boxCollider2d.bounds.center.x + boxCollider2d.bounds.extents.x,
                                       boxCollider2d.bounds.center.y - boxCollider2d.bounds.extents.y);
        float radius = 0.03f;
        return IsGrounded() && !Physics2D.OverlapCircle(position, radius, platformsLayerMask);
    }

    public void StartSlide()
    {
        isSliding = true;

        slideTimeLeft = slideTime;
        slideDirection = lookDirection;

        // Set sprite to half size
        float crouchSize = 0.5f;
        float verticalOffset = -0.25f;

        boxCollider2d.size = new Vector2(boxCollider2d.size.x, crouchSize);
        boxCollider2d.offset = new Vector2(boxCollider2d.offset.x, verticalOffset);

        /*Vector3 spriteMaskScale = crouchSpriteMask.transform.localScale;
        spriteMaskScale.y = crouchSize;
        crouchSpriteMask.transform.localScale = spriteMaskScale;

        Vector3 spriteMaskPos = crouchSpriteMask.transform.localPosition;
        spriteMaskPos.y = verticalOffset;
        crouchSpriteMask.transform.localPosition = spriteMaskPos;*/

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

        /*Vector3 spriteMaskScale = crouchSpriteMask.transform.localScale;
        spriteMaskScale.y = standSize;
        crouchSpriteMask.transform.localScale = spriteMaskScale;

        Vector3 spriteMaskPos = crouchSpriteMask.transform.localPosition;
        spriteMaskPos.y = -verticalOffset;
        crouchSpriteMask.transform.localPosition = spriteMaskPos;*/
    }

    public void Dash(float x, float y)
    {
        rigidbody2d.velocity = Vector2.zero;

        if (x == 0 && y == 0) { y = 1; } // default dash is up
        Vector2 dashVector = new Vector2(x, y).normalized * dashVelocity;
        if (isStuck) { dashVector /= dashSlowdownFactor; }

        rigidbody2d.velocity += dashVector;
        dashAvailable = false;
        isDashing = true;
        rigidbody2d.drag = 10; // TODO: make this value configurable in inspector

        if (rigidbody2d.gravityScale != 0)
        {
            rigidBodyGravityScale = rigidbody2d.gravityScale;
            rigidbody2d.gravityScale = 0;
        }

        FMODUnity.RuntimeManager.PlayOneShot(dashSound);
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

        // This allows us to use inputHorizontalAxisDown similarly to key down
        inputHorizontalAxisDown = inputHorizontalAxis;
        if (inputHorizontalAxis != 0)
        {
            if (inputHorizontalAxisInUse) { inputHorizontalAxisDown = 0f; }
            inputHorizontalAxisInUse = true;
        }
        else
        {
            inputHorizontalAxisInUse = false;
        }
    }

    public void SetVerticalAxisInput(float value)
    {
        inputVerticalAxis = value;

        // This allows us to use inputVerticalAxisDown similarly to key down
        inputVerticalAxisDown = inputVerticalAxis;
        if (inputVerticalAxis != 0)
        {
            if (inputVerticalAxisInUse) { inputVerticalAxisDown = 0f; }
            inputVerticalAxisInUse = true;
        }
        else
        {
            inputVerticalAxisInUse = false;
        }
    }

    public void SetJumpAndDashInput(bool value)
    {
        if(value && !jumpAndDashKey)
        {
            jumpAndDashKeyDown = value;
        }
        jumpAndDashKey = value;
    }

    public void SetGrabWallInput(bool value)
    {
        grabKeyDown = value;
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
        if (!isInvincible && healthToRemove > 0 && health > 0)
        {
            health -= healthToRemove;
            if (health < 0) { health = 0; }
            SetInvincible(timeInvincibleAfterHurt);
            FMODUnity.RuntimeManager.PlayOneShot(lifeLostSound);
        }
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
        if (timeSecs > 0 && !isStuck)
        {
            isStuck = true;
            uprightSprite.color = stuckColour;
            slideSprite.color = stuckColour;
            Invoke("ResumePlayerMovement", timeSecs);
        }
    }

    private void ResumePlayerMovement()
    {
        isStuck = false;
        uprightSprite.color = Color.white;
        slideSprite.color = Color.white;
    }

    public void SetInvincible(float timeSecs)
    {
        if (!isInvincible)
        {
            isInvincible = true;
            Invoke("EndInvincibility", timeSecs);
            StartCoroutine(Flash(timeSecs, 0.05f));
        }
    }

    private void EndInvincibility()
    {
        isInvincible = false;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    IEnumerator Flash(float time, float intervalTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            SpriteRenderer renderer;
            if (isSliding)
            {
                renderer = slideSprite;
                uprightSprite.enabled = false;
            }
            else
            {
                renderer = uprightSprite;
                slideSprite.enabled = false;
            }
            renderer.enabled = !renderer.enabled;

            elapsedTime += Time.deltaTime + intervalTime;
            yield return new WaitForSeconds(intervalTime);
        }
        if (isSliding)
        {
            slideSprite.enabled = true;
            uprightSprite.enabled = false;
        }
        else
        {
            uprightSprite.enabled = true;
            slideSprite.enabled = false;
        }
    }

    public void ResetPos()
    {
        Vector3 position = transform.position;
        position.x = startPos.x;
        position.y = startPos.y;
        transform.position = position;
    }

    public void FreezePos()
    {
        rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }

    public void UnFreezePos()
    {
        rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void ResetHealth()
    {
        health = maxHealth;
    }

    public void ResetTooltips()
    {
        wallClimbTooltipShown = false;
        wallDetachTooltipShown = false;
    }
}
