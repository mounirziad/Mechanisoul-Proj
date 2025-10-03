using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManager))]
public class LockOnSystem : MonoBehaviour
{
    [Header("Lock-On Settings")]
    public float lockRange = 15f;                // max distance to candidate
    public float maxAngle = 60f;                 // max angle from camera forward
    public LayerMask enemyLayer;                 // enemies should be on this layer (or use tag check)
    public Transform lockTargetPoint;            // an empty transform used by Cinemachine.LookAt (assign in inspector or created at runtime)
    public CinemachineCamera freeLookCam;      // assign your FreeLook cam here
    public string enemyTag = "Enemy";

    [Header("Smoothing Settings")]
    public float targetFollowSpeed = 8f;         // how fast the lock point follows the target
    public float lockTransitionSpeed = 12f;     // how fast camera transitions when locking/unlocking
    public bool useSmoothing = true;             // toggle smoothing on/off
    public float targetHeightOffset = 0.5f;     // height offset above target (reduce this to minimize camera height changes)
    
    [Header("Attack Smoothing")]
    public float attackFollowSpeed = 15f;       // faster following during attacks to reduce jitter
    public float attackDetectionThreshold = 5f; // player velocity threshold to detect attack movement

    [Header("Search Settings")]
    public int maxTargetsToCheck = 32;           // cap for OverlapSphere

    [HideInInspector] public Transform currentLockTarget;

    private float lastToggleTime = 0f;
    private float toggleCooldown = 0.5f; // Prevent rapid toggling

    PlayerManager playerManager;
    InputManager inputManager;
    PlayerControls playerControls;               // optional if you want to subscribe directly
    bool isLocked = false;
    
    // Smoothing variables
    private Vector3 targetPosition;
    private Vector3 desiredPosition;
    private bool isTransitioning = false;
    private bool isUnlockingSmooth = false;  // Track smooth unlock transitions
    
    // Attack movement detection
    private Vector3 lastPlayerPosition;
    private PlayerCombat playerCombat;

    private float lockGracePeriod = 0.3f;
    private float lockTime;


    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        playerCombat = GetComponent<PlayerCombat>();

        // if lockTargetPoint not assigned, create one
        if (lockTargetPoint == null)
        {
            GameObject go = new GameObject("LockOnLookAt");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            lockTargetPoint = go.transform;
        }
        
        // Initialize player position tracking
        lastPlayerPosition = transform.position;
    }

    private void Start()
    {
        if (freeLookCam != null && freeLookCam.LookAt == null)
            freeLookCam.LookAt = lockTargetPoint;
    }

    private void OnEnable()
    {
        // Try to subscribe to PlayerControls if available on InputManager
        if (inputManager != null && inputManager.playerControls != null)
        {
            playerControls = inputManager.playerControls;
            // make sure action exists
            try
            {
                playerControls.PlayerActions.EnemyLockOn.performed += ctx => ToggleLock();
            }
            catch { /* ignore if not present; user can bind in inspector */ }
        }
    }

    private void OnDisable()
    {
        if (playerControls != null)
        {
            try
            {
                playerControls.PlayerActions.EnemyLockOn.performed -= ctx => ToggleLock();
            }
            catch { }
        }
    }

    void Update()
    {
        // If locked and target becomes invalid (dead/out of range/occluded) auto unlock or retarget
        if (isLocked)
        {
            Debug.Log($"Lock-on Update: Checking target {(currentLockTarget ? currentLockTarget.name : "null")}");

            // Always update target position regardless of grace period
            if (currentLockTarget != null)
            {
                desiredPosition = currentLockTarget.position + Vector3.up * targetHeightOffset;

                if (useSmoothing)
                {
                    // Detect if player is attacking/moving rapidly
                    float playerMovementSpeed = (transform.position - lastPlayerPosition).magnitude / Time.deltaTime;
                    bool isPlayerAttacking = playerCombat != null && playerCombat.IsAttacking();
                    bool isRapidMovement = playerMovementSpeed > attackDetectionThreshold;

                    float currentFollowSpeed = (isPlayerAttacking || isRapidMovement) ? attackFollowSpeed : targetFollowSpeed;

                    targetPosition = Vector3.Lerp(targetPosition, desiredPosition, currentFollowSpeed * Time.deltaTime);
                    lockTargetPoint.position = targetPosition;
                }
                else
                {
                    lockTargetPoint.position = desiredPosition;
                }

                lastPlayerPosition = transform.position;
            }

            // Only validate target after grace period
            if (Time.time - lockTime >= lockGracePeriod)
            {
                if (currentLockTarget == null || !IsTargetValid(currentLockTarget))
                {
                    Debug.Log("Target invalid - attempting retarget or unlock");
                    Transform ret = FindBestTarget();
                    if (ret == null)
                    {
                        Debug.Log("No new target found - unlocking");
                        ClearLock(); // This will now handle smooth unlocking
                    }
                    else
                    {
                        Debug.Log($"Retargeting to: {ret.name}");
                        SetLockTarget(ret); // Smooth transition to new target
                    }
                }
            }
            else
            {
                Debug.Log("Within grace period - skipping validation");
            }
        }

        // Handle smooth transitions when locking/unlocking
        if (isTransitioning && useSmoothing)
        {
            targetPosition = Vector3.Lerp(targetPosition, desiredPosition, lockTransitionSpeed * Time.deltaTime);
            lockTargetPoint.position = targetPosition;

            // Smooth camera rotation during unlock
            if (isUnlockingSmooth && freeLookCam != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(
                    transform.position - freeLookCam.transform.position,
                    Vector3.up
                );

                freeLookCam.transform.rotation = Quaternion.Slerp(
                    freeLookCam.transform.rotation,
                    targetRotation,
                    lockTransitionSpeed * Time.deltaTime
                );
            }

            // Check if transition is complete
            if (Vector3.Distance(targetPosition, desiredPosition) < 0.1f)
            {
                isTransitioning = false;
                targetPosition = desiredPosition;
                lockTargetPoint.position = targetPosition;

                if (isUnlockingSmooth)
                {
                    isUnlockingSmooth = false;
                    if (freeLookCam != null)
                        freeLookCam.LookAt = transform; // now safe to snap back to player
                }
            }
        }
    }


    public void ToggleLock()
    {
        // Prevent rapid toggling
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            Debug.Log("ToggleLock cooldown - ignoring input");
            return;
        }

        lastToggleTime = Time.time;

        Debug.Log($"ToggleLock called - Current lock state: {isLocked}, Current target: {(currentLockTarget ? currentLockTarget.name : "null")}");

        if (isLocked && currentLockTarget != null)
        {
            Debug.Log("Already locked - clearing lock");
            ClearLock();
        }
        else
        {
            Debug.Log("Not locked - searching for target");
            Transform best = FindBestTarget();
            if (best != null)
            {
                Debug.Log($"Found target: {best.name} - setting lock");
                SetLockTarget(best);
            }
            else
            {
                Debug.Log("No valid target found");
            }
        }
    }

    public void SetLockTarget(Transform t)
    {
        Debug.Log($"SetLockTarget called with: {(t ? t.name : "null")}");

        lockTime = Time.time;

        if (currentLockTarget != null)
        {
            // Unsubscribe from old target's death event
            var oldHealth = currentLockTarget.GetComponent<BasicEnemyHealth>();
            if (oldHealth != null)
            {
                oldHealth.OnDeath -= OnTargetDeath;
                Debug.Log($"Unsubscribed from old target: {currentLockTarget.name}");
            }
        }

        currentLockTarget = t;
        isLocked = t != null;

        Debug.Log($"After SetLockTarget - isLocked: {isLocked}, currentLockTarget: {(currentLockTarget ? currentLockTarget.name : "null")}");

        if (currentLockTarget != null)
        {
            var health = currentLockTarget.GetComponent<BasicEnemyHealth>();
            if (health != null)
            {
                health.OnDeath += OnTargetDeath;
                Debug.Log($"Subscribed to new target: {currentLockTarget.name}");
            }
        }

        // Rest of your existing SetLockTarget code...
        if (freeLookCam != null)
        {
            freeLookCam.LookAt = lockTargetPoint;
        }

        if (isLocked)
        {
            desiredPosition = currentLockTarget.position + Vector3.up * targetHeightOffset;

            if (useSmoothing)
            {
                if (lockTargetPoint.position == Vector3.zero || Vector3.Distance(lockTargetPoint.position, desiredPosition) > 0.5f)
                {
                    isTransitioning = true;
                    if (lockTargetPoint.position == Vector3.zero)
                    {
                        targetPosition = transform.position + transform.forward * 2f + Vector3.up * targetHeightOffset;
                    }
                    else
                    {
                        targetPosition = lockTargetPoint.position;
                    }
                }
                else
                {
                    targetPosition = desiredPosition;
                    lockTargetPoint.position = targetPosition;
                }
            }
            else
            {
                lockTargetPoint.position = desiredPosition;
            }
        }
        else
        {
            if (freeLookCam != null) freeLookCam.LookAt = transform;
        }

        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.currentTarget = isLocked ? currentLockTarget : null;

        Debug.Log($"SetLockTarget completed - isLocked: {isLocked}");
    }

    private void OnTargetDeath()
    {
        Debug.Log("Locked target died � clearing lock");
        ClearLock();
    }

    public void ClearLock()
    {
        Debug.Log("ClearLock() called - unlocking target");

        // Unsubscribe from target's death event
        if (currentLockTarget != null)
        {
            var health = currentLockTarget.GetComponent<BasicEnemyHealth>();
            if (health != null)
            {
                health.OnDeath -= OnTargetDeath;
            }
        }

        currentLockTarget = null;
        isLocked = false;

        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.currentTarget = null;

        if (useSmoothing && lockTargetPoint.position != Vector3.zero)
        {
            Debug.Log("Starting smooth unlock transition");

            // Start smooth transition away from target
            isTransitioning = true;
            isUnlockingSmooth = true;
            targetPosition = lockTargetPoint.position;

            // Move the lock point to a position slightly in front of the player
            desiredPosition = transform.position + transform.forward * 3f + Vector3.up * targetHeightOffset;

            // Keep the camera looking at lockTargetPoint during transition
        }
        else
        {
            Debug.Log("Immediate unlock (no smoothing)");

            // No smoothing or no previous lock position, switch immediately
            if (freeLookCam != null) freeLookCam.LookAt = transform;
        }
    }

    bool IsTargetValid(Transform t)
    {
        if (t == null) 
        {
            Debug.Log("Target validation failed: target is null");
            return false;
        }
        
        // Check if enemy is dead - be more careful with null checks
        AiAgent aiAgent = t.GetComponent<AiAgent>();
        if (aiAgent != null && aiAgent.isDead)
        {
            Debug.Log($"Target validation failed: {t.name} is dead");
            return false;
        }
        
        // Also check BasicEnemyHealth for consistency
        BasicEnemyHealth enemyHealth = t.GetComponent<BasicEnemyHealth>();
        if (enemyHealth != null && enemyHealth.currentHealth <= 0)
        {
            Debug.Log($"Target validation failed: {t.name} has no health");
            return false;
        }
        
        Vector3 d = t.position - transform.position;
        if (d.sqrMagnitude > lockRange * lockRange) 
        {
            Debug.Log($"Target validation failed: {t.name} out of range ({d.magnitude} > {lockRange})");
            return false;
        }

        // For trigger colliders, skip occlusion check as raycasts won't hit them reliably
        Collider targetCollider = t.GetComponent<Collider>();
        if (targetCollider != null && targetCollider.isTrigger)
        {
            Debug.Log($"Target validation passed: {t.name} (trigger collider, skipping occlusion check)");
            return true;
        }

        // For non-trigger colliders, do a more comprehensive occlusion check
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 targetPos = t.position + Vector3.up * targetHeightOffset;
        Vector3 dir = targetPos - origin;
        
        // Use SphereCast instead of Raycast for better detection
        RaycastHit hit;
        float sphereRadius = 0.1f;
        if (Physics.SphereCast(origin, sphereRadius, dir.normalized, out hit, lockRange))
        {
            // Check if we hit the target or something on the target (like child colliders)
            if (hit.transform != t && !hit.transform.IsChildOf(t) && !t.IsChildOf(hit.transform)) 
            {
                Debug.Log($"Target validation failed: {t.name} occluded by {hit.transform.name}");
                return false;
            }
        }
        
        Debug.Log($"Target validation passed: {t.name}");
        return true;
    }

    Transform FindBestTarget()
    {
        // Sphere overlap to find candidates - include triggers since some enemies use trigger colliders
        Collider[] hits = Physics.OverlapSphere(transform.position, lockRange, enemyLayer, QueryTriggerInteraction.Collide);
        float bestScore = Mathf.Infinity;
        Transform best = null;

        Debug.Log($"Lock-on search: Layer mask returned {hits.Length} hits");

        // If layer mask didn't return any but you tag enemies, also try generic OverlapSphere
        if (hits.Length == 0)
        {
            hits = Physics.OverlapSphere(transform.position, lockRange, ~0, QueryTriggerInteraction.Collide);
            Debug.Log($"Lock-on search: Generic search returned {hits.Length} hits");
        }

        // First pass: collect all valid root enemy GameObjects
        System.Collections.Generic.HashSet<Transform> rootEnemies = new System.Collections.Generic.HashSet<Transform>();
        
        foreach (var c in hits)
        {
            if (c.transform == transform) continue;
            if (!c.CompareTag(enemyTag)) continue;
            
            // Find the root enemy GameObject (one with AiAgent component)
            Transform rootEnemy = c.transform;
            while (rootEnemy != null)
            {
                if (rootEnemy.GetComponent<AiAgent>() != null)
                {
                    rootEnemies.Add(rootEnemy);
                    break;
                }
                rootEnemy = rootEnemy.parent;
            }
        }

        Debug.Log($"Found {rootEnemies.Count} unique root enemies from {hits.Length} collider hits");

        int validEnemies = 0;
        foreach (var enemyTransform in rootEnemies)
        {
            Debug.Log($"Checking root enemy: {enemyTransform.name}");
            
            // Skip dead enemies immediately
            AiAgent aiAgent = enemyTransform.GetComponent<AiAgent>();
            if (aiAgent != null && aiAgent.isDead) 
            {
                Debug.Log($"Skipping dead enemy: {enemyTransform.name}");
                continue;
            }
            
            // Also check health component
            BasicEnemyHealth enemyHealth = enemyTransform.GetComponent<BasicEnemyHealth>();
            if (enemyHealth != null && enemyHealth.currentHealth <= 0)
            {
                Debug.Log($"Skipping enemy with no health: {enemyTransform.name}");
                continue;
            }
            
            validEnemies++;
            Debug.Log($"Found valid root enemy: {enemyTransform.name}");

            Vector3 toTarget = enemyTransform.position - transform.position;
            float ang = Vector3.Angle(transform.forward, toTarget);
            if (ang > maxAngle) 
            {
                Debug.Log($"Enemy {enemyTransform.name} outside angle range: {ang} > {maxAngle}");
                continue;
            }

            // Check occlusion using the enemy's main collider
            Collider enemyCollider = enemyTransform.GetComponent<Collider>();
            if (enemyCollider != null && !enemyCollider.isTrigger)
            {
                RaycastHit hit;
                Vector3 origin = transform.position + Vector3.up * 1.2f;
                Vector3 dir = (enemyTransform.position + Vector3.up * targetHeightOffset) - origin;
                if (Physics.Raycast(origin, dir.normalized, out hit, lockRange))
                {
                    if (hit.transform != enemyTransform && !hit.transform.IsChildOf(enemyTransform) && !enemyTransform.IsChildOf(hit.transform)) 
                    {
                        Debug.Log($"Enemy {enemyTransform.name} occluded by {hit.transform.name}");
                        continue;
                    }
                }
            }
            else
            {
                Debug.Log($"Skipping occlusion check for {enemyTransform.name} (trigger or no collider)");
            }

            float score = toTarget.sqrMagnitude; // closer is better
            if (score < bestScore)
            {
                bestScore = score;
                best = enemyTransform;
                Debug.Log($"New best target: {enemyTransform.name} (score: {score})");
            }
        }

        Debug.Log($"Lock-on result: Found {validEnemies} valid enemies, best: {(best ? best.name : "none")}");
        return best;
    }

    // Expose lock state
    public bool IsLocked() 
    { 
        Debug.Log($"IsLocked() called: {isLocked} (target: {(currentLockTarget ? currentLockTarget.name : "null")})");
        return isLocked;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockRange);
        if (currentLockTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.2f, currentLockTarget.position + Vector3.up * 1f);
        }
    }
}
