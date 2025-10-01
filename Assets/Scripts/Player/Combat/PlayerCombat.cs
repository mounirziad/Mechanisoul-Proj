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

    void HandleShoot()
    {
        if (!rangedEnabled) { inputManager.shootInput = false; return; }

        float effectiveRate = baseFireRate * rangedMods.joyFireRateMultiplier;
        float cooldown = 1f / Mathf.Max(0.0001f, effectiveRate);
        if (Time.time - lastShotTime < cooldown) { inputManager.shootInput = false; return; }

        if (projectilePrefab && shootPoint)
        {
            var go = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            var proj = go.GetComponent<PlayerProjectile>();
            if (proj != null)
            {
                float finalDamage = projectileDamage * rangedMods.joyDamageMultiplier;
                proj.Initialize(shootPoint.forward, finalDamage, rangedMods, this);
            }
            lastShotTime = Time.time;
        }
        else Debug.LogWarning("Projectile Prefab or Shoot Point not assigned.");

        inputManager.shootInput = false;
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

    bool CanAttack() => !isAttacking && !isAiming && Time.time - lastComboEnd > 0.2f;

    // Check for buffered attacks during animation
    void ProcessQueuedAttack()
    {
        if (!attackQueued || isAttacking) return;
        
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

            SetAttackTarget();
            
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
}
