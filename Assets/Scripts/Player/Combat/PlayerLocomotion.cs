using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    PlayerManager playerManager;
    InputManager inputManager;
    AnimatorManager animatorManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidbody;

    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public float rayCastHeightOffset = 0.5f;
    public LayerMask groundLayer;

    public float detectionradius = 0.2f;
    public float detectionheight = 4f;
    public float capsuleHeight = 1.8f;

    public bool isJumping;
    public bool isSprinting;
    public bool isGrounded;

    public float walkingSpeed;
    public float runningSpeed;
    public float sprintingSpeed;
    public float rotationSpeed;

    public float baseWalkingSpeed = 1.5f;
    public float baseRunningSpeed = 5;
    public float baseSprintingSpeed = 7;
    public float baseRotationSpeed = 15;

    public float jumpHeight = 3;
    public float gravityIntensity = -15;
    public Transform groundCheck;

    [SerializeField] private float stepHeight = 0.3f;  // max height you can step up
    [SerializeField] private float stepSmooth = 0.1f;  // smooth movement up

    [Header("Dodge Settings")]
    public float dodgeSpeed = 10f;
    public float dodgeDuration = 0.5f;
    public float dodgeCooldown = 1f;
    public bool isDodging;
    public bool canDodge = true;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private Vector3 dodgeDirection;
    private Vector3 lastMovementDirection;
    PlayerCombat playerCombat;


    [Header("Attack Movement Settings")]
    public float attackMoveDistance = 0.5f; // How far the attack pushes forward
    public float attackMoveSpeed = 5f;      // How fast the lunge is
    private bool isAttackingWithLunge = false;
    private float attackMoveTimer = 0f;
    private Vector3 attackMoveDirection;
    private float attackMoveStartTime;
    private AttackSO currentAttackData;
    private float originalMoveDistance;

    LockOnSystem lockOnSystem;

    // Add these new variables for jump cooldown
    public float jumpCooldown = 1f; // 1 second cooldown after landing
    private float jumpCooldownTimer = 0f;
    public bool canJump = true;

    [Header("Ground Detection Settings")]
    public float groundDetectionBufferTime = 0.2f;
    private float lastTimeGrounded = 0f;
    private bool wasGroundedLastFrame = true;
    
    [Header("Improved Stair Detection")]
    public bool useMultiPointDetection = true;
    public int detectionPoints = 5;
    public float detectionSpread = 0.3f;
    public float maxStairAngle = 50f;
    public float coyoteTime = 0.2f;
    private float timeSinceGrounded = 0f;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    public bool isOnSlope = false;
    private Vector3 slopeNormal = Vector3.up;

    [Header("Ground Check Reparenting")]
    private Transform originalGroundCheckParent;
    private Vector3 originalGroundCheckLocalPosition;
    public Transform footBone;
    private bool isGroundCheckReparented = false;
    public float footGroundCheckOffset = 0.3f;
    public bool useFootTrackingDuringJump = true;
    private CapsuleCollider playerCapsule;
    
    private bool isPlayingFallingAnimation = false;


    [Header("Camera Collision Sync")]
    public float maxPlayerRotationDuringCollision = 30f;
    
    [Header("Camera Direction Cache")]
    private Vector3 cachedCameraForward;
    private Vector3 cachedCameraRight;
    private float cachedCameraYaw;

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main != null ? Camera.main.transform : null;
        playerCombat = GetComponent<PlayerCombat>();
        lockOnSystem = GetComponent<LockOnSystem>();
        playerCapsule = GetComponent<CapsuleCollider>();

        walkingSpeed = baseWalkingSpeed;
        runningSpeed = baseRunningSpeed;
        sprintingSpeed = baseSprintingSpeed;
        rotationSpeed = baseRotationSpeed;

        if (groundCheck != null)
        {
            originalGroundCheckParent = groundCheck.parent;
            originalGroundCheckLocalPosition = groundCheck.localPosition;
        }
    }
    public void RefreshReferences()
    {
        cameraObject = Camera.main != null ? Camera.main.transform : null;
    }
    
    public void CacheCameraDirection()
    {
        if (cameraObject == null)
        {
            RefreshReferences();
            if (cameraObject == null) return;
        }
        
        cachedCameraYaw = cameraObject.eulerAngles.y;
        cachedCameraForward = Quaternion.Euler(0, cachedCameraYaw, 0) * Vector3.forward;
        cachedCameraRight = Quaternion.Euler(0, cachedCameraYaw, 0) * Vector3.right;
    }
    public void HandleAllMovement()
    {
        if (cameraObject == null) RefreshReferences();

        HandleFallingAndLanding();
        HandleDodgeCooldown();
        HandleJumpCooldown();
        UpdateGroundCheckPosition();

        if (playerManager.isInteracting && !isDodging)
        {
            return;
        }

        if (playerCombat != null && playerCombat.IsAttacking())
        {
            HandleAttackMovementLock();
        }

        HandleDodgeMovement(); 

        if (isDodging)
        {
            return;
        }

        HandleMovement();
        HandleRotation();
        HandleSteps();
    }

    private void HandleAttackMovementLock()
    {
        // Apply smooth attack movement with proper curves
        if (isAttackingWithLunge)
        {
            attackMoveTimer -= Time.deltaTime;
            
            if (attackMoveTimer > 0 && currentAttackData != null)
            {
                // Calculate progress through the attack movement
                float elapsedTime = Time.time - attackMoveStartTime;
                float progress = Mathf.Clamp01(elapsedTime / currentAttackData.moveDuration);
                
                // Use animation curve for smooth movement falloff
                float curveValue = currentAttackData.moveCurve.Evaluate(progress);
                float currentSpeed = currentAttackData.moveSpeed * curveValue;
                
                // Apply movement
                Vector3 movement = attackMoveDirection * currentSpeed * Time.deltaTime;
                playerRigidbody.MovePosition(transform.position + movement);
            }
            else
            {
                isAttackingWithLunge = false;
                currentAttackData = null;
            }
        }
    }

    public void StartAttackLunge(Vector3 direction, AttackSO attackData)
    {
        if (attackData == null) return;
        
        // Always reset the attack lunge state for new attacks
        isAttackingWithLunge = true;
        attackMoveDirection = direction.normalized;
        attackMoveTimer = attackData.moveDuration;
        attackMoveStartTime = Time.time;
        currentAttackData = attackData;
        originalMoveDistance = attackData.moveDistance;
        
        // Instantly snap to face attack direction for responsive feel
        if (attackData.rotateTowardsTarget && direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        Debug.Log($"Started attack lunge: Distance={attackData.moveDistance}, Speed={attackData.moveSpeed}, Duration={attackData.moveDuration}");
    }

    public void ForceStopAttackLunge()
    {
        isAttackingWithLunge = false;
        attackMoveTimer = 0f;
        currentAttackData = null;
    }




    private void HandleMovement()
    {
        if (cameraObject == null) return;

        if (isJumping) { return; }

        Vector3 currentVelocity = playerRigidbody.linearVelocity;

        // Use cached camera directions instead of reading directly from camera
        moveDirection = cachedCameraForward * inputManager.verticalInput;
        moveDirection += cachedCameraRight * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (playerCombat != null && playerCombat.IsAttacking())
        {
            Vector3 limitedMovement = Vector3.zero;
            
            if (!isAttackingWithLunge)
            {
                limitedMovement = cachedCameraForward * inputManager.verticalInput * 0.3f;
                limitedMovement += cachedCameraRight * inputManager.horizontalInput * 0.3f;
                limitedMovement.Normalize();
                limitedMovement.y = 0;
                limitedMovement *= walkingSpeed * 0.5f;
            }
            
            Vector3 targetVelocity = new Vector3(limitedMovement.x, currentVelocity.y, limitedMovement.z);
            playerRigidbody.linearVelocity = targetVelocity;
            return;
        }

        // Calculate input-based direction (already calculated above with cameraYaw)
        // moveDirection is already set!

        if (moveDirection != Vector3.zero)
        {
            lastMovementDirection = moveDirection;
        }

        // Apply speed depending on state
        if (isSprinting)
        {
            moveDirection *= sprintingSpeed;
        }
        else if (inputManager.moveAmount >= 0.5f)
        {
            moveDirection *= runningSpeed;
        }
        else
        {
            moveDirection *= walkingSpeed;
        }

        Vector3 finalTargetVelocity = new Vector3(moveDirection.x, currentVelocity.y, moveDirection.z);
        playerRigidbody.linearVelocity = finalTargetVelocity;
    }


    private void HandleRotation()
    {
        TargetedPlayerRotation targetedRotation = GetComponent<TargetedPlayerRotation>();

        if (targetedRotation != null)
        {
            // TargetedPlayerRotation handles all rotation in LateUpdate
            // Only handle special cases here (aiming, etc) if NOT locked
            ZTargetingSystem zTarget = GetComponent<ZTargetingSystem>();
            if (zTarget != null && zTarget.IsLocked)
            {
                return; // Let TargetedPlayerRotation handle it
            }
        }

        if (cameraObject == null) return;
        if (isJumping) { return; }

        Vector3 targetDirection = Vector3.zero;
        float currentRotationSpeed = rotationSpeed;

        // Check if camera is in collision and limit player rotation accordingly
        LockOnCamera lockOnCam = cameraObject.GetComponent<LockOnCamera>();
        bool cameraInCollision = lockOnCam != null && lockOnCam.IsInCollision();

        // Priority 1: Lock-on system (highest priority)
        if (lockOnSystem != null && lockOnSystem.IsLocked() && lockOnSystem.currentLockTarget != null)
        {
            targetDirection = lockOnSystem.currentLockTarget.position - transform.position;
            targetDirection.y = 0;

            if (targetDirection.magnitude < 1f)
            {
                targetDirection = transform.forward;
            }
            else
            {
                targetDirection.Normalize();
            }

            // RESPONSIVE FIX: Limit rotation speed when camera is colliding
            if (cameraInCollision)
            {
                currentRotationSpeed = rotationSpeed * 0.5f; // Slower rotation during collision

                // Additional: Clamp the rotation angle if needed
                float angleToTarget = Vector3.Angle(transform.forward, targetDirection);
                if (angleToTarget > maxPlayerRotationDuringCollision)
                {
                    // Use a more conservative approach during collision
                    targetDirection = Vector3.RotateTowards(transform.forward, targetDirection,
                        maxPlayerRotationDuringCollision * Mathf.Deg2Rad, 0f);
                }
            }
            else
            {
                currentRotationSpeed = rotationSpeed * 2f;
            }

            if (targetDirection != Vector3.zero)
            {
                Quaternion lockOnRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, lockOnRotation, currentRotationSpeed * Time.deltaTime);
            }
            return;
        }

        // Only execute the following if NOT locked on

        // Priority 2: Aiming system (medium priority)
        else if (inputManager.aimInput)
        {
            // Try to get custom camera controller first
            CustomCameraController customCameraController = GetComponent<CustomCameraController>();
            ThirdPersonAimCameraManager aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
            
            // Use custom camera controller if available
            if (customCameraController != null && customCameraController.IsAimCameraActive())
            {
                // Get the aim direction from the custom camera system
                targetDirection = customCameraController.GetAimDirection();
                targetDirection.y = 0;
                
                if (targetDirection.sqrMagnitude > 0.01f)
                {
                    targetDirection.Normalize();
                }
                else
                {
                    // Fallback to camera forward if aim direction is invalid
                    targetDirection = cameraObject.forward;
                    targetDirection.y = 0;
                    targetDirection.Normalize();
                }
            }
            else if (aimCameraManager != null && aimCameraManager.IsAimCameraActive())
            {
                // Use the aim camera manager's direction for player rotation
                targetDirection = aimCameraManager.GetAimDirection();
                
                if (targetDirection == Vector3.zero)
                {
                    // Fallback to camera forward if aim direction is not available
                    targetDirection = cameraObject.forward;
                    targetDirection.y = 0;
                    targetDirection.Normalize();
                }
            }
            else
            {
                // Fallback to camera forward for non-aim camera aiming
                targetDirection = cameraObject.forward;
                targetDirection.y = 0;
                targetDirection.Normalize();
            }

            // Don't blend with movement direction when aiming - just face where you're aiming
            // This ensures the player rotates to match the reticle position
            
            // Slightly increased rotation speed when aiming for responsive aiming
            currentRotationSpeed = rotationSpeed * 1.2f;
        }
        // Priority 3: Normal free movement (lowest priority)
        else
        {
            // Normal free movement rotation - use cached camera directions
            targetDirection = cachedCameraForward * inputManager.verticalInput;
            targetDirection += cachedCameraRight * inputManager.horizontalInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
                targetDirection = transform.forward;

            // Use normal rotation speed for free movement
            currentRotationSpeed = rotationSpeed;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.fixedDeltaTime);
        transform.rotation = playerRotation;
    }

    private void HandleFallingAndLanding()
    {
        Vector3 groundCheckPosition = groundCheck.position;
        bool wasGroundedThisFrame = isGrounded;
        bool groundDetected = false;

        if (useMultiPointDetection)
        {
            groundDetected = MultiPointGroundCheck(groundCheckPosition);
        }
        else
        {
            groundDetected = Physics.SphereCast(groundCheckPosition, detectionradius, Vector3.down, out RaycastHit hit, detectionheight, groundLayer);
        }

        if (groundDetected)
        {
            bool isValidLanding = !isGrounded &&
                                 !isJumping &&
                                 playerRigidbody.linearVelocity.y <= 0f &&
                                 inAirTimer > 0.3f;

            if (isValidLanding)
            {
                Debug.Log($"LANDING - isGrounded:{isGrounded}, isJumping:{isJumping}, velocity.y:{playerRigidbody.linearVelocity.y}, inAirTimer:{inAirTimer}");
                
                // Check if player is moving when landing
                bool isMovingOnLanding = inputManager.moveAmount > 0.1f;
                
                // Always reset the jumping state in the animator
                animatorManager.animator.SetBool("isJumping", false);
                
                // Reset falling animation flag
                isPlayingFallingAnimation = false;
                
                if (isMovingOnLanding)
                {
                    // Skip landing animation and go directly to locomotion
                    // CrossFade to Empty state with very short transition
                    animatorManager.animator.CrossFade("Empty", 0.05f, 1);
                    playerManager.isInteracting = false;
                }
                else
                {
                    // Play landing animation normally when not moving (don't lock movement)
                    animatorManager.PlayTargetAnimation("Land", false);
                }

                if (isGroundCheckReparented)
                {
                    groundCheck.SetParent(originalGroundCheckParent);
                    groundCheck.localPosition = originalGroundCheckLocalPosition;
                    isGroundCheckReparented = false;
                }
            }
            
            // CRITICAL FIX: Force exit falling state when grounded
            if (isPlayingFallingAnimation && groundDetected)
            {
                Debug.Log($"FORCING EXIT FROM FALLING - velocity.y:{playerRigidbody.linearVelocity.y}");
                isPlayingFallingAnimation = false;
                
                // Force animator to exit falling state
                animatorManager.animator.SetBool("isJumping", false);
                playerManager.isInteracting = false;
                
                // Crossfade to empty to reset animation state
                animatorManager.animator.CrossFade("Empty", 0.1f, 1);
            }

            inAirTimer = 0;
            isGrounded = true;
            isJumping = false;
            isPlayingFallingAnimation = false;
            lastTimeGrounded = Time.time;
            timeSinceGrounded = 0f;
        }
        else
        {
            timeSinceGrounded += Time.deltaTime;
            
            if (timeSinceGrounded > coyoteTime)
            {
                isGrounded = false;
            }
            
            inAirTimer += Time.deltaTime;

            // Only trigger falling animation ONCE, not every frame
            if (!isGrounded && !isJumping && ShouldPlayFallingAnimation())
            {
                if (!playerManager.isInteracting && !isPlayingFallingAnimation)
                {
                    Debug.Log($"TRIGGERING FALLING - inAirTimer:{inAirTimer}, velocity.y:{playerRigidbody.linearVelocity.y}");
                    animatorManager.PlayTargetAnimation("Falling", false);
                    isPlayingFallingAnimation = true;
                }
            }
        }

        wasGroundedLastFrame = wasGroundedThisFrame;
    }

    private bool MultiPointGroundCheck(Vector3 centerPosition)
    {
        int groundHits = 0;
        int totalPoints = detectionPoints;
        bool foundValidSlope = false;
        Vector3 averageNormal = Vector3.zero;
        int normalCount = 0;
        
        RaycastHit hit;
        if (Physics.SphereCast(centerPosition, detectionradius, Vector3.down, out hit, detectionheight, groundLayer))
        {
            groundHits++;
            averageNormal += hit.normal;
            normalCount++;
            
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle <= maxSlopeAngle)
            {
                foundValidSlope = true;
            }
        }
        
        float angleStep = 360f / (totalPoints - 1);
        for (int i = 0; i < totalPoints - 1; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * detectionSpread,
                0f,
                Mathf.Sin(angle) * detectionSpread
            );
            
            Vector3 checkPosition = centerPosition + offset;
            
            if (Physics.SphereCast(checkPosition, detectionradius * 0.8f, Vector3.down, out hit, detectionheight, groundLayer))
            {
                groundHits++;
                averageNormal += hit.normal;
                normalCount++;
                
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle <= maxSlopeAngle)
                {
                    foundValidSlope = true;
                }
            }
        }
        
        if (normalCount > 0)
        {
            slopeNormal = (averageNormal / normalCount).normalized;
            float currentSlopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
            isOnSlope = currentSlopeAngle > 0.1f && currentSlopeAngle <= maxSlopeAngle;
        }
        else
        {
            slopeNormal = Vector3.up;
            isOnSlope = false;
        }
        
        return foundValidSlope || groundHits >= Mathf.Max(1, totalPoints / 3);
    }

    private bool ShouldPlayFallingAnimation()
    {
        if (isGrounded) return false;

        if (isJumping && playerRigidbody.linearVelocity.y > 0) return false;

        return playerRigidbody.linearVelocity.y < -2f && inAirTimer > 0.4f;
    }

    private void HandleJumpCooldown()
    {
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer <= 0)
            {
                canJump = true;
                jumpCooldownTimer = 0f;
            }
        }

        if (isGrounded && !playerManager.isInteracting && !isDodging && !isJumping)
        {
            if (!canJump && jumpCooldownTimer <= 0)
            {
                canJump = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && groundCheck != null)
        {
            Vector3 capsuleBottom = groundCheck.position;
            Vector3 capsuleTop = capsuleBottom + Vector3.up * capsuleHeight;
            float capsuleRadius = detectionradius;

            Gizmos.color = isGrounded ? (isOnSlope ? Color.cyan : Color.green) : Color.red;

            DrawCapsule(capsuleBottom, capsuleTop, capsuleRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(capsuleBottom, capsuleBottom + Vector3.down * detectionheight);
            Gizmos.DrawLine(capsuleTop, capsuleTop + Vector3.down * detectionheight);
            
            if (useMultiPointDetection)
            {
                Gizmos.color = Color.cyan;
                float angleStep = 360f / (detectionPoints - 1);
                for (int i = 0; i < detectionPoints - 1; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(
                        Mathf.Cos(angle) * detectionSpread,
                        0f,
                        Mathf.Sin(angle) * detectionSpread
                    );
                    
                    Vector3 checkPosition = capsuleBottom + offset;
                    Gizmos.DrawWireSphere(checkPosition, detectionradius * 0.8f);
                    Gizmos.DrawLine(checkPosition, checkPosition + Vector3.down * detectionheight);
                }
            }
            
            if (isOnSlope && isGrounded)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(transform.position, slopeNormal * 2f);
            }
        }
    }

    // Helper method to draw a capsule
    private void DrawCapsule(Vector3 bottom, Vector3 top, float radius)
    {
        // Draw side lines
        Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
        Gizmos.DrawLine(bottom - Vector3.right * radius, top - Vector3.right * radius);
        Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
        Gizmos.DrawLine(bottom - Vector3.forward * radius, top - Vector3.forward * radius);

        // Draw bottom hemisphere
        DrawHemisphere(bottom, Vector3.up, radius);
        DrawHemisphere(bottom, Vector3.down, radius);

        // Draw top hemisphere
        DrawHemisphere(top, Vector3.up, radius);
        DrawHemisphere(top, Vector3.down, radius);
    }

    // Helper method to draw a hemisphere
    private void DrawHemisphere(Vector3 center, Vector3 direction, float radius)
    {
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * radius;
        if (perpendicular.magnitude == 0) perpendicular = Vector3.Cross(direction, Vector3.up).normalized * radius;

        int segments = 12;
        float angleIncrement = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(i * angleIncrement, direction);
            Vector3 start = center + rotation * perpendicular;

            Quaternion nextRotation = Quaternion.AngleAxis((i + 1) * angleIncrement, direction);
            Vector3 end = center + nextRotation * perpendicular;

            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(start, center + direction * radius);
        }
    }

    private void UpdateGroundCheckPosition()
    {
        if (groundCheck == null || playerCapsule == null) return;

        if (!isGroundCheckReparented && !isGrounded && useFootTrackingDuringJump)
        {
            if (footBone != null)
            {
                Vector3 capsuleBottom = transform.position + Vector3.down * (playerCapsule.height * 0.5f - playerCapsule.radius);
                groundCheck.position = capsuleBottom;
            }
        }
    }

    public void HandleJumping()
    {
        if (isGrounded && canJump && !isJumping)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);
            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidbody.linearVelocity = playerVelocity;

            isJumping = true;
            canJump = false;
            isPlayingFallingAnimation = false;
            jumpCooldownTimer = jumpCooldown;

            if (groundCheck != null && footBone != null)
            {
                groundCheck.SetParent(footBone);
                groundCheck.localPosition = new Vector3(0, -footGroundCheckOffset, 0);
                isGroundCheckReparented = true;
            }
        }
    }

    private void HandleSteps()
    {
        // Only try to climb steps if the player is moving
        Vector3 horizontalVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        if (horizontalVelocity.magnitude < 0.1f)
            return;

        RaycastHit hitLower;
        Vector3 originLower = transform.position + Vector3.up * 0.1f; // just above ground
        if (Physics.Raycast(originLower, transform.forward, out hitLower, 0.5f, groundLayer))
        {
            // Now check above, at step height
            RaycastHit hitUpper;
            Vector3 originUpper = transform.position + Vector3.up * stepHeight;
            if (!Physics.Raycast(originUpper, transform.forward, out hitUpper, 0.5f, groundLayer))
            {
                // Smoothly move up
                playerRigidbody.position += new Vector3(0, stepSmooth, 0);
            }
        }
    }


    public void HandleDodge()
    {
        if (isDodging || !canDodge || !isGrounded || playerManager.isInteracting)
            return;

        if (dodgeCooldownTimer > 0)
            return;
        
        // Check if we can cancel attack with dodge
        if (playerCombat != null && playerCombat.IsAttacking())
        {
            if (!playerCombat.CanDodgeCancel())
                return;
            else
                playerCombat.CancelAttack(); // Cancel the current attack
        }

        // Determine dodge direction based on input - use cached camera directions
        if (inputManager.moveAmount > 0.1f)
        {
            // Dodge in current movement direction
            dodgeDirection = cachedCameraForward * inputManager.verticalInput;
            dodgeDirection += cachedCameraRight * inputManager.horizontalInput;
            dodgeDirection.y = 0;
            dodgeDirection.Normalize();
        }
        else
        {
            // Use last movement direction if available, otherwise dodge forward
            dodgeDirection = (lastMovementDirection != Vector3.zero) ? lastMovementDirection : transform.forward;
        }

        StartDodge();
    }

    private void StartDodge()
    {
        isDodging = true;
        canDodge = false;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;

        // Play dodge animation
        animatorManager.PlayTargetAnimation("DodgeRoll", true);

        // Set interacting flag
        playerManager.isInteracting = true;
    }

    private void HandleDodgeMovement()
    {
        if (!isDodging)
            return;

        dodgeTimer -= Time.deltaTime;

        if (dodgeTimer <= 0)
        {
            EndDodge();
            return;
        }

        // Apply dodge movement (preserve some Y velocity for gravity)
        Vector3 dodgeVelocity = dodgeDirection * dodgeSpeed;
        dodgeVelocity.y = playerRigidbody.linearVelocity.y;
        playerRigidbody.linearVelocity = dodgeVelocity;
    }

    private void EndDodge()
    {
        isDodging = false;
        playerManager.isInteracting = false;
    }

    private void HandleDodgeCooldown()
    {
        if (dodgeCooldownTimer > 0)
        {
            dodgeCooldownTimer -= Time.deltaTime;
            if (dodgeCooldownTimer <= 0)
            {
                canDodge = true;
            }
        }
    }

    public void ChangeSpeed(float speed)
    {
        walkingSpeed = baseWalkingSpeed *speed;
        runningSpeed = baseRunningSpeed * speed;
        sprintingSpeed = baseSprintingSpeed * speed;
        rotationSpeed = baseRotationSpeed * speed;
    }


}
