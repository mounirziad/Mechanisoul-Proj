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
    public float coyoteTime = 0.15f;
    private float timeSinceGrounded = 0f;

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main != null ? Camera.main.transform : null;
        playerCombat = GetComponent<PlayerCombat>();
        lockOnSystem = GetComponent<LockOnSystem>();

        walkingSpeed = baseWalkingSpeed;
        runningSpeed = baseRunningSpeed;
        sprintingSpeed = baseSprintingSpeed;
        rotationSpeed = baseRotationSpeed;

    }
    public void RefreshReferences()
    {
        cameraObject = Camera.main != null ? Camera.main.transform : null;
    }
    public void HandleAllMovement()
    {
        if (cameraObject == null) RefreshReferences();

        HandleFallingAndLanding();
        HandleDodgeCooldown();
        HandleJumpCooldown(); // Add this line

        if (playerManager.isInteracting && !isDodging)
        {
            return;
        }

        if (playerCombat != null && playerCombat.IsAttacking())
        {
            HandleAttackMovementLock();
            // don't return early — allow HandleRotation() below
        }

        HandleDodgeMovement(); 

        if (isDodging) // Skip normal movement during dodge
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

        // Allow limited movement during attack for more fluid combat
        if (playerCombat != null && playerCombat.IsAttacking())
        {
            // Instead of completely freezing, allow slow movement during attacks
            Vector3 limitedMovement = Vector3.zero;
            
            // Only allow movement if not in lunge phase
            if (!isAttackingWithLunge)
            {
                limitedMovement = cameraObject.forward * inputManager.verticalInput * 0.3f;
                limitedMovement += cameraObject.right * inputManager.horizontalInput * 0.3f;
                limitedMovement.Normalize();
                limitedMovement.y = 0;
                limitedMovement *= walkingSpeed * 0.5f; // Slow movement during attacks
            }
            
            Vector3 targetVelocity = new Vector3(limitedMovement.x, currentVelocity.y, limitedMovement.z);
            playerRigidbody.linearVelocity = targetVelocity;
            return;
        }

        // Calculate input-based direction
        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection += cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

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
        if (cameraObject == null) return;
        if (isJumping) { return; }

        Vector3 targetDirection = Vector3.zero;
        float currentRotationSpeed = rotationSpeed;

        // Priority 1: Lock-on system (highest priority)
        if (lockOnSystem != null && lockOnSystem.IsLocked() && lockOnSystem.currentLockTarget != null)
        {
            // When locked on, player should ONLY face the target, no camera influence
            targetDirection = lockOnSystem.currentLockTarget.position - transform.position;
            targetDirection.y = 0;

            // If target is too close or behind, use a more stable approach
            if (targetDirection.magnitude < 1f)
            {
                // If target is very close, use the direction from previous frame or maintain current forward
                targetDirection = transform.forward;
            }
            else
            {
                targetDirection.Normalize();
            }

            // Use faster rotation speed for more responsive lock-on
            currentRotationSpeed = rotationSpeed * 2f;

            // Apply rotation immediately for locked state
            if (targetDirection != Vector3.zero)
            {
                Quaternion lockOnRotation = Quaternion.LookRotation(targetDirection); // Renamed variable
                transform.rotation = Quaternion.Slerp(transform.rotation, lockOnRotation, currentRotationSpeed * Time.deltaTime);
            }
            return; // CRITICAL: Return early to prevent other rotation logic from interfering
        }

        // Only execute the following if NOT locked on

        // Priority 2: Aiming system (medium priority)
        else if (inputManager.aimInput)
        {
            // Get the aim camera manager
            ThirdPersonAimCameraManager aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
            
            if (aimCameraManager != null && aimCameraManager.IsAimCameraActive())
            {
                // Use the aim camera's direction for player rotation
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

            // Reduce movement influence on rotation when aiming for more precise control
            if (inputManager.moveAmount > 0.1f)
            {
                Vector3 movementDirection = cameraObject.forward * inputManager.verticalInput;
                movementDirection += cameraObject.right * inputManager.horizontalInput;
                movementDirection.y = 0;
                movementDirection.Normalize();

                // Reduced blend for more stable aiming (was 0.3f)
                targetDirection = Vector3.Lerp(targetDirection, movementDirection, 0.1f);
            }

            // Reduced rotation speed when aiming for more precise control
            currentRotationSpeed = rotationSpeed * 0.8f;
        }
        // Priority 3: Normal free movement (lowest priority)
        else
        {
            // Normal free movement rotation
            targetDirection = cameraObject.forward * inputManager.verticalInput;
            targetDirection += cameraObject.right * inputManager.horizontalInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
                targetDirection = transform.forward;

            // Use normal rotation speed for free movement
            currentRotationSpeed = rotationSpeed;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.deltaTime);
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
                                 !playerManager.isInteracting &&
                                 !isJumping &&
                                 playerRigidbody.linearVelocity.y <= 0f &&
                                 inAirTimer > 0.1f;

            if (isValidLanding)
            {
                animatorManager.PlayTargetAnimation("Land", true);
                jumpCooldownTimer = jumpCooldown;
                canJump = false;
            }

            inAirTimer = 0;
            isGrounded = true;
            isJumping = false;
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

            if (!isGrounded && !isJumping && ShouldPlayFallingAnimation())
            {
                if (!playerManager.isInteracting)
                {
                    animatorManager.PlayTargetAnimation("Falling", false);
                }
            }
        }

        wasGroundedLastFrame = wasGroundedThisFrame;
    }

    private bool MultiPointGroundCheck(Vector3 centerPosition)
    {
        int groundHits = 0;
        int totalPoints = detectionPoints;
        
        RaycastHit hit;
        if (Physics.SphereCast(centerPosition, detectionradius, Vector3.down, out hit, detectionheight, groundLayer))
        {
            groundHits++;
            
            if (Vector3.Angle(hit.normal, Vector3.up) <= maxStairAngle)
            {
                return true;
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
                
                if (Vector3.Angle(hit.normal, Vector3.up) <= maxStairAngle)
                {
                    return true;
                }
            }
        }
        
        return groundHits >= Mathf.Max(1, totalPoints / 3);
    }

    private bool ShouldPlayFallingAnimation()
    {
        // Don't play falling if we're grounded
        if (isGrounded) return false;

        // Don't play falling if we're jumping upwards
        if (isJumping && playerRigidbody.linearVelocity.y > 0) return false;

        // Only play falling if we have significant downward velocity AND we've been in air for a bit
        // This prevents falling animation during brief camera movements
        return playerRigidbody.linearVelocity.y < -1f && inAirTimer > 0.2f;
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

        // Failsafe: if we're grounded and not in any special state, ensure we can jump
        if (isGrounded && !playerManager.isInteracting && !isDodging && !isJumping)
        {
            // Only force enable jump if cooldown seems stuck and we're clearly grounded
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

            Gizmos.color = isGrounded ? Color.green : Color.red;

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

    public void HandleJumping()
    {
        if (isGrounded && canJump) // Added canJump check
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);
            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidbody.linearVelocity = playerVelocity;

            // Set jumping state
            isJumping = true;
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

        // Determine dodge direction based on input
        if (inputManager.moveAmount > 0.1f)
        {
            // Dodge in current movement direction
            dodgeDirection = cameraObject.forward * inputManager.verticalInput;
            dodgeDirection += cameraObject.right * inputManager.horizontalInput;
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
