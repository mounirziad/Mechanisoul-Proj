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
            
            if (currentLockTarget == null || !IsTargetValid(currentLockTarget))
            {
                Debug.Log("Target invalid - attempting retarget or unlock");
                // Try retarget; if fail, clear lock with smooth transition
                Transform ret = FindBestTarget();
                if (ret == null) 
                {
                    Debug.Log("No new target found - unlocking");
                    ClearLock();  // This will now handle smooth unlocking
                }
                else 
                {
                    Debug.Log($"Retargeting to: {ret.name}");
                    SetLockTarget(ret);  // Smooth transition to new target
                }
            }
            else
            {
                // Update desired position for smooth following
                desiredPosition = currentLockTarget.position + Vector3.up * targetHeightOffset;
                
                if (useSmoothing)
                {
                    // Detect if player is attacking/moving rapidly
                    float playerMovementSpeed = (transform.position - lastPlayerPosition).magnitude / Time.deltaTime;
                    bool isPlayerAttacking = playerCombat != null && playerCombat.IsAttacking();
                    bool isRapidMovement = playerMovementSpeed > attackDetectionThreshold;
                    
                    // Use faster following during attacks to reduce jitter
                    float currentFollowSpeed = (isPlayerAttacking || isRapidMovement) ? attackFollowSpeed : targetFollowSpeed;
                    
                    // Smooth interpolation towards target
                    targetPosition = Vector3.Lerp(targetPosition, desiredPosition, currentFollowSpeed * Time.deltaTime);
                    lockTargetPoint.position = targetPosition;
                }
                else
                {
                    // Direct assignment (original behavior)
                    lockTargetPoint.position = desiredPosition;
                }
                
                // Update player position tracking
                lastPlayerPosition = transform.position;
            }
        }
        
        // Handle smooth transitions when locking/unlocking
        if (isTransitioning && useSmoothing)
        {
            targetPosition = Vector3.Lerp(targetPosition, desiredPosition, lockTransitionSpeed * Time.deltaTime);
            lockTargetPoint.position = targetPosition;
            
            // Check if transition is complete
            if (Vector3.Distance(targetPosition, desiredPosition) < 0.1f)
            {
                isTransitioning = false;
                targetPosition = desiredPosition;
                lockTargetPoint.position = targetPosition;
                
                // If this was an unlock transition, now switch the camera target
                if (isUnlockingSmooth)
                {
                    isUnlockingSmooth = false;
                    if (freeLookCam != null) freeLookCam.LookAt = transform;
                }
            }
        }
    }

    public void ToggleLock()
    {
        if (isLocked) ClearLock();
        else
        {
            Transform best = FindBestTarget();
            if (best != null) SetLockTarget(best);
        }
    }

    public void SetLockTarget(Transform t)
    {
        if (currentLockTarget != null)
        {
            // Unsubscribe from old target's death event
            var oldHealth = currentLockTarget.GetComponent<BasicEnemyHealth>();
            if (oldHealth != null) oldHealth.OnDeath -= OnTargetDeath;
        }

        currentLockTarget = t;
        isLocked = t != null;

        if (currentLockTarget != null)
        {
            var health = currentLockTarget.GetComponent<BasicEnemyHealth>();
            if (health != null) health.OnDeath += OnTargetDeath;
        }


        if (freeLookCam != null)
        {
            freeLookCam.LookAt = lockTargetPoint;  // << fixed
        }

        

        if (isLocked)
        {
            desiredPosition = currentLockTarget.position + Vector3.up * targetHeightOffset;
            
            if (useSmoothing)
            {
                // Start smooth transition to new target
                if (lockTargetPoint.position == Vector3.zero || Vector3.Distance(lockTargetPoint.position, desiredPosition) > 0.5f)
                {
                    // If this is the first lock or target is far, start transitioning
                    isTransitioning = true;
                    if (lockTargetPoint.position == Vector3.zero)
                    {
                        // Initialize position if this is the first lock
                        targetPosition = transform.position + transform.forward * 2f + Vector3.up * targetHeightOffset;
                    }
                    else
                    {
                        targetPosition = lockTargetPoint.position;
                    }
                }
                else
                {
                    // Target is close, update immediately
                    targetPosition = desiredPosition;
                    lockTargetPoint.position = targetPosition;
                }
            }
            else
            {
                // Direct assignment (original behavior)
                lockTargetPoint.position = desiredPosition;
            }
        }
        else
        {
            if (freeLookCam != null) freeLookCam.LookAt = transform;  // << fixed
        }

        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.currentTarget = isLocked ? currentLockTarget : null;
    }

    private void OnTargetDeath()
    {
        Debug.Log("Locked target died — clearing lock");
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
        
        // Check if enemy is dead
        AiAgent aiAgent = t.GetComponent<AiAgent>();
        if (aiAgent != null && aiAgent.isDead)
        {
            Debug.Log($"Target validation failed: {t.name} is dead");
            return false;
        }
        
        Vector3 d = t.position - transform.position;
        if (d.sqrMagnitude > lockRange * lockRange) 
        {
            Debug.Log($"Target validation failed: {t.name} out of range ({d.magnitude} > {lockRange})");
            return false;
        }

        // Skip occlusion check for trigger colliders since raycast won't hit them
        Collider targetCollider = t.GetComponent<Collider>();
        if (targetCollider != null && targetCollider.isTrigger)
        {
            Debug.Log($"Target validation passed: {t.name} (trigger collider, skipping occlusion check)");
            return true;
        }

        // Occlusion check for non-trigger colliders
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 dir = (t.position + Vector3.up * targetHeightOffset) - origin;
        RaycastHit hit;
        if (Physics.Raycast(origin, dir.normalized, out hit, lockRange))
        {
            if (hit.transform != t) 
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

        int validEnemies = 0;
        foreach (var c in hits)
        {
            if (c.transform == transform) continue;
            
            Debug.Log($"Checking object: {c.name}, Tag: {c.tag}, Layer: {LayerMask.LayerToName(c.gameObject.layer)}");
            
            if (!c.CompareTag(enemyTag)) continue;
            
            validEnemies++;
            Debug.Log($"Found valid enemy: {c.name}");

            Vector3 toTarget = c.transform.position - transform.position;
            float ang = Vector3.Angle(transform.forward, toTarget);
            if (ang > maxAngle) 
            {
                Debug.Log($"Enemy {c.name} outside angle range: {ang} > {maxAngle}");
                continue;
            }

            // occlusion check - skip for trigger colliders
            if (!c.isTrigger)
            {
                RaycastHit hit;
                Vector3 origin = transform.position + Vector3.up * 1.2f;
                Vector3 dir = (c.transform.position + Vector3.up * targetHeightOffset) - origin;
                if (Physics.Raycast(origin, dir.normalized, out hit, lockRange))
                {
                    if (hit.transform != c.transform) 
                    {
                        Debug.Log($"Enemy {c.name} occluded by {hit.transform.name}");
                        continue;
                    }
                }
            }
            else
            {
                Debug.Log($"Skipping occlusion check for trigger collider: {c.name}");
            }

            float score = toTarget.sqrMagnitude; // closer is better
            if (score < bestScore)
            {
                bestScore = score;
                best = c.transform;
                Debug.Log($"New best target: {c.name} (score: {score})");
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
