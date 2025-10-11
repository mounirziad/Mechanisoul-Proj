using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum RootMotionMode
{
    LungeOnly,      // Use only your existing lunge system (root motion disabled)
    RootMotionOnly, // Use only root motion from animations (no additional lunge)
    Hybrid          // Use root motion + reduced lunge for extra impact
}

public class PlayerCombat : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private bool useSpawnCorrection = true;
    [SerializeField] private float maxSpawnOffset = 0.5f;
    [Header("Debug")]
    [SerializeField] private bool showAimDebug = false;

    // Camera zoom while aiming
    [SerializeField] private CinemachineCamera cinemachineCam;
    [SerializeField] private float zoomedFOV = 30f;
    [SerializeField] private float zoomSpeed = 5f;
    private float defaultFOV;

    // Ranged
    [Header("Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileDamage = 10f;
    [Tooltip("Baseline shots per second (Joy multiplies this).")]
    public float baseFireRate = 2f;
    public bool rangedEnabled = true;
    public GameObject angerExplosionPrefab; // FireDoTZone-like prefab (optional)

    private float lastShotTime = -999f;
    private RangedModifiers rangedMods;    // set by UpgradeHandler

    [Header("Ranged VFX Prefabs")]
    public GameObject joyExplosionPrefab;   // joy vfx

    // Melee & input
    public List<AttackSO> combo;
    float lastClickedTime, lastComboEnd;
    int comboCounter;
    public bool isAttacking = false;
    private float minAnimationPlayTime = 0.4f;
    private float attackStartTime;
    private bool attackQueued = false;
    
    [Header("Input Buffer Settings")]
    public float inputBufferTime = 0.3f;    // How long to buffer attack inputs
    private float lastAttackInputTime = -999f;
    
    [Header("Animation Cancel Settings")]
    public float earlyComboWindow = 0.6f;   // When in animation you can start next combo (0.6 = 60% through)
    public float dodgeCancelWindow = 0.4f;  // When you can cancel attack with dodge (0.4 = 40% through)

    [Header("Air Attack Settings")]
    [Tooltip("Allow limited air attacks (prevents infinite air combos)")]
    public bool allowAirAttacks = false;
    [Tooltip("Maximum number of attacks allowed in air before landing")]
    public int maxAirAttacks = 1;
    private int currentAirAttackCount = 0;
    private bool wasGroundedLastFrame = true;

    public bool isAiming = false;
    private InputManager inputManager;
    private PlayerControls playerControls;
    private Animator anim;
    [SerializeField] Weapon weapon;
    
    [Header("Root Motion Settings")]
    [Tooltip("How to combine root motion with attack lunge movement")]
    public RootMotionMode rootMotionMode = RootMotionMode.Hybrid;
    
    [Tooltip("When using Additive mode, multiplier for extra lunge force")]
    [Range(0.1f, 2f)]
    public float additionalLungeMultiplier = 0.5f;

    [SerializeField] private float aimAssistRange = 20f;
    [SerializeField] private float aimAssistAngle = 30f;
    [SerializeField] private LayerMask enemyLayer;
    [HideInInspector] public Transform currentTarget;

    void Awake()
    {
        anim = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
    }

    void Start()
    {
        if (cinemachineCam != null) defaultFOV = cinemachineCam.Lens.FieldOfView;
        if (inputManager != null && inputManager.playerControls != null)
        {
            playerControls = inputManager.playerControls;
            playerControls.PlayerActions.Attack.performed += OnAttackPerformed;
        }
        if (weapon != null) weapon.DisableTriggerBox();
        
        // Ensure root motion is disabled initially
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }
    }

    void OnDisable()
    {
        if (playerControls != null)
            playerControls.PlayerActions.Attack.performed -= OnAttackPerformed;
    }

    void Update()
    {
        CheckAttackCompletion();
        ProcessQueuedAttack();
        HandleAiming();
        HandleCameraZoom();
        ClearQueuedAttacksInAir();
        TrackGroundedState();
    }
    
    void TrackGroundedState()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        
        // Reset air attack counter when landing
        if (isGrounded && !wasGroundedLastFrame)
        {
            currentAirAttackCount = 0;
        }
        
        wasGroundedLastFrame = isGrounded;
    }
    
    void ClearQueuedAttacksInAir()
    {
        if (attackQueued)
        {
            var playerLoco = GetComponent<PlayerLocomotion>();
            var playerMgr = GetComponent<PlayerManager>();
            bool isGrounded = playerLoco != null && playerLoco.isGrounded;
            bool isInteracting = playerMgr != null && playerMgr.isInteracting;
            
            // Clear queued attacks when airborne (if air attacks disabled) or when interacting
            if (isInteracting || (!isGrounded && !allowAirAttacks))
            {
                attackQueued = false;
            }
        }
    }

    void HandleCameraZoom()
    {
        if (cinemachineCam == null) return;
        float targetFOV = isAiming ? zoomedFOV : defaultFOV;
        var lens = cinemachineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        cinemachineCam.Lens = lens;
    }

    void HandleAiming()
    {
        if (inputManager.aimInput && !isAttacking && !isAiming) StartAiming();
        else if (isAiming && !inputManager.aimInput) StopAiming();

        if (isAiming && inputManager.shootInput) HandleShoot();
    }

    void StartAiming()
    {
        isAiming = true;
        anim.SetBool("IsAiming", true);
        var loco = GetComponent<PlayerLocomotion>();
        if (loco) { loco.walkingSpeed *= 0.7f; loco.runningSpeed *= 0.7f; }
    }

    void StopAiming()
    {
        isAiming = false;
        anim.SetBool("IsAiming", false);
        var loco = GetComponent<PlayerLocomotion>();
        if (loco) { loco.walkingSpeed /= 0.7f; loco.runningSpeed /= 0.7f; }
    }

    // Ranged API 
    public void SetRangedUpgrades(RangedModifiers mods)
    {
        rangedMods = mods;
        if (rangedMods.joyFireRateMultiplier <= 0f) rangedMods.joyFireRateMultiplier = 1f;
        if (rangedMods.joyDamageMultiplier <= 0f) rangedMods.joyDamageMultiplier = 1f;
    }

    public void ToggleRanged(bool on)
    {
        rangedEnabled = on;
        if (!on) { isAiming = false; anim.SetBool("IsAiming", false); }
    }

    // In your PlayerCombat.cs, modify the HandleShoot method:
    void HandleShoot()
    {
        if (!rangedEnabled) { inputManager.shootInput = false; return; }

        float effectiveRate = baseFireRate * rangedMods.joyFireRateMultiplier;
        float cooldown = 1f / Mathf.Max(0.0001f, effectiveRate);
        if (Time.time - lastShotTime < cooldown) { inputManager.shootInput = false; return; }

        if (projectilePrefab && shootPoint)
        {
            // Use corrected spawn position for better alignment
            Vector3 spawnPosition = GetCorrectedSpawnPosition();

            // DEBUG: Check what position we're actually using
            Debug.Log($"Shooting from spawn position: {spawnPosition}");
            Debug.Log($"Player position: {transform.position}");
            Debug.Log($"Distance between: {Vector3.Distance(spawnPosition, transform.position)}");

            var go = Instantiate(projectilePrefab, spawnPosition, shootPoint.rotation);
            var proj = go.GetComponent<PlayerProjectile>();
            if (proj != null)
            {
                float finalDamage = projectileDamage * rangedMods.joyDamageMultiplier;

                // Use trajectory-corrected direction for maximum accuracy
                Vector3 correctedDirection = GetCorrectedAimDirection();

                // DEBUG: Check the aim direction
                Debug.Log($"Corrected aim direction: {correctedDirection}");

                proj.Initialize(correctedDirection, finalDamage, rangedMods, this);
            }
            lastShotTime = Time.time;
        }
        else Debug.LogWarning("Projectile Prefab or Shoot Point not assigned.");

        inputManager.shootInput = false;
    }

    Vector3 GetAimTargetPosition()
    {
        var aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager != null && aimCameraManager.IsAimCameraActive())
        {
            Vector3 aimTarget = aimCameraManager.GetAimTarget();
            
            // Validate that the aim target is reasonable (not too close or at origin)
            if (aimTarget != Vector3.zero)
            {
                float distanceToTarget = Vector3.Distance(shootPoint.position, aimTarget);
                if (distanceToTarget > 1f) // Must be at least 1 meter away
                {
                    return aimTarget;
                }
            }
        }
        
        return Vector3.zero; // No valid target found
    }

    Vector3 GetAccurateAimDirection()
    {
        // Try to get aim target from camera manager
        var aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager != null)
        {
            Vector3 aimTarget = aimCameraManager.GetAimTarget();

            // If we have a valid aim target, calculate direction from shoot point to target
            if (aimTarget != Vector3.zero)
            {
                Vector3 direction = (aimTarget - shootPoint.position).normalized;
                return direction;
            }
        }

        // Fallback: use camera's forward direction if aim camera is active
        if (aimCameraManager != null && aimCameraManager.IsAimCameraActive())
        {
            Vector3 cameraDirection = aimCameraManager.GetAimDirection();
            if (cameraDirection != Vector3.zero)
            {
                return cameraDirection;
            }
        }

        // Final fallback: use shoot point forward (including vertical component)
        return shootPoint.forward;
    }

    Vector3 GetCorrectedAimDirection()
    {
        var aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager != null && aimCameraManager.IsAimCameraActive())
        {
            Vector3 aimTarget = aimCameraManager.GetAimTarget();
            
            if (aimTarget != Vector3.zero)
            {
                // Get the camera position
                Vector3 cameraPos = GetCameraPosition();
                
                // Calculate what the "ideal" direction would be from camera to target
                Vector3 idealDirection = (aimTarget - cameraPos).normalized;
                
                // Apply trajectory correction to compensate for shoot point offset
                Vector3 correctedDirection = CalculateTrajectoryCorrection(shootPoint.position, cameraPos, aimTarget, idealDirection);
                
                return correctedDirection;
            }
        }
        
        // Fallback to basic aim direction
        return GetAccurateAimDirection();
    }

    Vector3 CalculateTrajectoryCorrection(Vector3 shootOrigin, Vector3 cameraOrigin, Vector3 target, Vector3 idealDirection)
    {
        // Distance to target from camera
        float distanceToTarget = Vector3.Distance(cameraOrigin, target);
        
        // Calculate a convergence point along the ideal trajectory
        // The closer the target, the more correction we need
        float convergenceDistance = Mathf.Min(distanceToTarget * 0.3f, 10f); // Converge within 30% of distance or 10m max
        Vector3 convergencePoint = cameraOrigin + idealDirection * convergenceDistance;
        
        // Calculate direction from actual shoot point to convergence point
        Vector3 correctedDirection = (convergencePoint - shootOrigin).normalized;
        
        // Blend between corrected direction (for close targets) and direct direction (for far targets)
        float blendFactor = Mathf.Clamp01(20f / distanceToTarget); // More correction for closer targets
        Vector3 directDirection = (target - shootOrigin).normalized;
        
        return Vector3.Slerp(directDirection, correctedDirection, blendFactor).normalized;
    }

    Vector3 GetCameraPosition()
    {
        var aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager != null && aimCameraManager.thirdPersonAimCamera != null)
        {
            // Try to get the camera transform from the Cinemachine camera
            Transform cameraTransform = aimCameraManager.thirdPersonAimCamera.transform;
            if (cameraTransform != null)
            {
                return cameraTransform.position;
            }
        }
        
        // Fallback: estimate camera position
        return transform.position + Vector3.up * 1.6f;
    }

    Vector3 GetCorrectedSpawnPosition()
    {
        if (!useSpawnCorrection) return shootPoint.position;

        var aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager == null || !aimCameraManager.IsAimCameraActive()) return shootPoint.position;

        Vector3 aimTarget = aimCameraManager.GetAimTarget();
        if (aimTarget == Vector3.zero) return shootPoint.position;

        // Get camera position
        Vector3 cameraPos = GetCameraPosition();

        // Calculate the ideal trajectory from camera to target
        Vector3 idealDirection = (aimTarget - cameraPos).normalized;
        
        // Project the shoot point onto the ideal trajectory line
        Vector3 cameraToShoot = shootPoint.position - cameraPos;
        float projectionDistance = Vector3.Dot(cameraToShoot, idealDirection);
        Vector3 projectedPoint = cameraPos + idealDirection * projectionDistance;
        
        // Calculate offset from shoot point to the projected ideal point
        Vector3 offset = projectedPoint - shootPoint.position;
        
        // Limit the offset to prevent unrealistic corrections
        if (offset.magnitude > maxSpawnOffset)
        {
            offset = offset.normalized * maxSpawnOffset;
        }
        
        return shootPoint.position + offset;
    }

    // Melee (improved)
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (isAiming) { inputManager.shootInput = true; return; }
        
        lastAttackInputTime = Time.time;
        
        if (CanAttack()) 
        {
            Attack();
        }
        else if (isAttacking) 
        {
            // Buffer the attack input
            attackQueued = true;
        }
    }

    bool CanAttack() 
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;
        
        // Basic checks first (including interaction/landing check)
        if (isAttacking || isAiming || isInteracting || Time.time - lastComboEnd < 0.2f)
            return false;
        
        // If grounded and not interacting, allow attacks
        if (isGrounded)
        {
            if (!wasGroundedLastFrame) // Just landed
            {
                currentAirAttackCount = 0; // Reset air attack counter on landing
            }
            return true;
        }
        
        // If in air, check if air attacks are allowed and within limit
        if (allowAirAttacks && currentAirAttackCount < maxAirAttacks)
        {
            return true;
        }
        
        // Default: no attack allowed
        return false;
    }

    // Check for buffered attacks during animation
    void ProcessQueuedAttack()
    {
        if (!attackQueued || isAttacking) return;
        
        // Ensure player can still attack (check grounded, air limits, and interaction state)
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;
        
        // Cancel queued attack if player is interacting (landing, dodging, etc.)
        if (isInteracting)
        {
            attackQueued = false;
            return;
        }
        
        if (!isGrounded)
        {
            if (!allowAirAttacks || currentAirAttackCount >= maxAirAttacks)
            {
                attackQueued = false; // Cancel queued attack if air attacks disabled or limit reached
                return;
            }
        }
        
        // Check if we're in the combo window
        if (Time.time - attackStartTime >= minAnimationPlayTime)
        {
            float norm = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (norm >= earlyComboWindow || norm > 0.95f)
            {
                Attack();
            }
        }
        
        // Clear old buffered inputs
        if (Time.time - lastAttackInputTime > inputBufferTime)
        {
            attackQueued = false;
        }
    }

    private void SetAttackTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aimAssistRange);
        float closest = Mathf.Infinity; Transform nearest = null;
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            Vector3 d = h.transform.position - transform.position;
            float ang = Vector3.Angle(transform.forward, d);
            if (ang < aimAssistAngle)
            {
                float dist = d.sqrMagnitude;
                if (dist < closest) { closest = dist; nearest = h.transform; }
            }
        }
        currentTarget = nearest;
    }

    void Attack()
    {
        if (comboCounter < combo.Count && combo[comboCounter] != null)
        {
            CancelInvoke(nameof(EndCombo));
            anim.runtimeAnimatorController = combo[comboCounter].animatorOV;
            anim.Play("Attack", 0, 0);
            weapon.damage = combo[comboCounter].damage;
            
            // Reset weapon hit sound cooldown for new attack
            if (weapon != null)
            {
                weapon.ResetHitSoundCooldown();
            }
            
            // Play attack sound effect
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayAttackSound();
            }

            SetAttackTarget();
            
            // Track air attacks
            var playerLoco = GetComponent<PlayerLocomotion>();
            bool isGrounded = playerLoco != null && playerLoco.isGrounded;
            if (!isGrounded)
            {
                currentAirAttackCount++; // Increment air attack counter
            }
            
            // Handle different root motion modes
            switch (rootMotionMode)
            {
                case RootMotionMode.LungeOnly:
                    // Traditional system - no root motion, full lunge
                    anim.applyRootMotion = false;
                    StartAttackMovement();
                    break;
                    
                case RootMotionMode.RootMotionOnly:
                    // Pure root motion - let animation drive movement completely
                    anim.applyRootMotion = true;
                    // Don't call StartAttackMovement()
                    break;
                    
                case RootMotionMode.Hybrid:
                    // Best of both worlds - root motion + reduced lunge
                    anim.applyRootMotion = true;
                    StartAttackMovementHybrid();
                    break;
            }
            
            isAttacking = true;
            attackStartTime = Time.time;
            comboCounter++;
            lastClickedTime = Time.time;
            attackQueued = false;
            if (comboCounter >= combo.Count) comboCounter = 0;
        }
    }

    void StartAttackMovementHybrid()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        if (playerLoco != null)
        {
            Vector3 attackDirection = transform.forward;
            
            // If we have a target, move towards it
            if (currentTarget != null)
            {
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                directionToTarget.y = 0;
                attackDirection = directionToTarget;
                
                // Rotate towards target for more dynamic combat
                transform.rotation = Quaternion.LookRotation(attackDirection);
            }
            
            // Get the correct attack data for the current attack
            int currentAttackIndex = comboCounter - 1;
            if (currentAttackIndex < 0) currentAttackIndex = combo.Count - 1;
            
            AttackSO currentAttackData = combo[currentAttackIndex];
            
            // Create a modified attack data for hybrid mode (reduced movement since root motion is also active)
            AttackSO hybridAttackData = ScriptableObject.CreateInstance<AttackSO>();
            hybridAttackData.moveDistance = currentAttackData.moveDistance * additionalLungeMultiplier;
            hybridAttackData.moveSpeed = currentAttackData.moveSpeed * additionalLungeMultiplier;
            hybridAttackData.moveDuration = currentAttackData.moveDuration;
            hybridAttackData.moveCurve = currentAttackData.moveCurve;
            hybridAttackData.rotateTowardsTarget = currentAttackData.rotateTowardsTarget;
            
            // Force stop any existing attack movement and start new one
            playerLoco.ForceStopAttackLunge();
            playerLoco.StartAttackLunge(attackDirection, hybridAttackData);
            
            Debug.Log($"Starting hybrid attack {currentAttackIndex} - Root Motion: ON, Extra Lunge: {hybridAttackData.moveDistance}");
        }
    }

    void StartAttackMovement()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        if (playerLoco != null)
        {
            Vector3 attackDirection = transform.forward;
            
            // If we have a target, move towards it
            if (currentTarget != null)
            {
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                directionToTarget.y = 0;
                attackDirection = directionToTarget;
                
                // Rotate towards target for more dynamic combat
                transform.rotation = Quaternion.LookRotation(attackDirection);
            }
            
            // Get the correct attack data for the current attack
            // comboCounter has already been incremented, so we need the previous index
            int currentAttackIndex = comboCounter - 1;
            if (currentAttackIndex < 0) currentAttackIndex = combo.Count - 1;
            
            AttackSO currentAttackData = combo[currentAttackIndex];
            
            // Force stop any existing attack movement and start new one
            playerLoco.ForceStopAttackLunge();
            playerLoco.StartAttackLunge(attackDirection, currentAttackData);
            
            Debug.Log($"Starting attack {currentAttackIndex} with move distance: {currentAttackData.moveDistance}");
        }
    }

    void CheckAttackCompletion()
    {
        if (!isAttacking) return;
        float norm = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        
        // Allow early combo transitions for fluid combat
        if (Time.time - attackStartTime >= minAnimationPlayTime && norm >= earlyComboWindow)
        {
            if (attackQueued) 
            {
                isAttacking = false; // Allow next attack to start
                return;
            }
        }

        // Complete attack when animation is nearly finished
        if (norm > 0.95f && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            CompleteAttack();
    }

    void CompleteAttack()
    {
        isAttacking = false;
        
        // Disable root motion when attack completes (except for LungeOnly mode where it's already off)
        if (rootMotionMode != RootMotionMode.LungeOnly)
        {
            anim.applyRootMotion = false;
        }
        
        // Stop attack movement when attack completes (unless combo continues)
        if (!attackQueued)
        {
            var playerLoco = GetComponent<PlayerLocomotion>();
            if (playerLoco != null)
            {
                playerLoco.ForceStopAttackLunge();
            }
            Invoke(nameof(EndCombo), 0.5f);
        }
        
        currentTarget = null;
    }

    // Allow dodge canceling during attacks for anime-style mobility
    public bool CanDodgeCancel()
    {
        if (!isAttacking) return true;
        
        float norm = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        return norm >= dodgeCancelWindow;
    }

    public bool IsAttacking() => isAttacking;

    void EndCombo()
    {
        if (!isAttacking)
        {
            comboCounter = 0;
            lastComboEnd = Time.time;
            attackQueued = false;
        }
    }

    // Animation events
    public void OnAnimationEnableWeapon() { if (weapon != null) weapon.EnableTriggerBox(); }
    public void OnAnimationDisableWeapon() { if (weapon != null) weapon.DisableTriggerBox(); }
    
    public void CancelAttack() 
    { 
        isAttacking = false; 
        attackQueued = false; 
        anim.Play("Idle"); 
        currentTarget = null;
        
        // Disable root motion when canceling attack (except for LungeOnly mode where it's already off)
        if (rootMotionMode != RootMotionMode.LungeOnly)
        {
            anim.applyRootMotion = false;
        }
        
        // Stop any attack movement
        var playerLoco = GetComponent<PlayerLocomotion>();
        if (playerLoco != null)
        {
            playerLoco.ForceStopAttackLunge();
        }
    }
    
    // Public method to reset air attack counter (useful for abilities, special moves, etc.)
    public void ResetAirAttackCounter()
    {
        currentAirAttackCount = 0;
    }
    
    // Public method to check current air attack status (useful for UI or other systems)
    public bool CanPerformAirAttack()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;
        
        // Can't attack while interacting (landing, dodging, etc.)
        if (isInteracting) return false;
        
        if (isGrounded) return true; // Always can attack when grounded and not interacting
        
        return allowAirAttacks && currentAirAttackCount < maxAirAttacks;
    }
    
    // Debug method to get air attack info
    public string GetAirAttackDebugInfo()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;
        
        return $"Grounded: {isGrounded}, Interacting: {isInteracting}, Air Attacks: {currentAirAttackCount}/{maxAirAttacks}, Allow Air: {allowAirAttacks}";
    }

    void OnDrawGizmos()
    {
        if (!showAimDebug || !isAiming || shootPoint == null) return;

        var aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager == null) return;

        Vector3 aimTarget = aimCameraManager.GetAimTarget();
        if (aimTarget == Vector3.zero) return;

        // Draw crosshair target
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimTarget, 0.2f);

        // Draw original shoot point
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(shootPoint.position, 0.1f);

        // Draw corrected spawn position
        Vector3 correctedSpawn = GetCorrectedSpawnPosition();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(correctedSpawn, 0.08f);

        // Draw connection between original and corrected spawn
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(shootPoint.position, correctedSpawn);

        // Draw corrected trajectory
        Gizmos.color = Color.green;
        Vector3 correctedDirection = GetCorrectedAimDirection();
        Gizmos.DrawLine(correctedSpawn, correctedSpawn + correctedDirection * 10f);

        // Draw camera position
        Vector3 cameraPos = GetCameraPosition();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(cameraPos, Vector3.one * 0.1f);
        
        // Draw ideal camera-to-target line
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(cameraPos, aimTarget);
    }
}
