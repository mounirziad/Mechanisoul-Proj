using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZTargetingSystem : MonoBehaviour
{
    [Header("Target Detection")]
    [SerializeField] private float lockOnRange = 20f;
    [SerializeField] private float maxAngleFromCamera = 70f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private string targetTag = "Enemy";
    
    [Header("Target Validation")]
    [SerializeField] private float targetLostDistance = 25f;
    [SerializeField] private float targetValidationInterval = 0.2f;
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask obstacleLayers;
    
    [Header("Target Switching")]
    [SerializeField] private bool allowTargetSwitching = true;
    [SerializeField] private float switchTargetCooldown = 0.3f;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputManager inputManager;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool showGizmos = true;
    
    public Transform CurrentTarget { get; private set; }
    public bool IsLocked => CurrentTarget != null;
    
    private float lastValidationTime;
    private float lastSwitchTime;
    private float lastToggleTime;
    private const float TOGGLE_COOLDOWN = 0.2f;
    private HashSet<Transform> deadTargets = new HashSet<Transform>();
    
    public delegate void TargetChangedHandler(Transform newTarget, Transform oldTarget);
    public event TargetChangedHandler OnTargetChanged;
    
    public delegate void LockStateChangedHandler(bool isLocked);
    public event LockStateChangedHandler OnLockStateChanged;
    
    private void Awake()
    {
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }
        
        if (inputManager == null)
        {
            inputManager = GetComponent<InputManager>();
        }
    }
    
    private void OnEnable()
    {
        if (inputManager != null && inputManager.playerControls != null)
        {
            try
            {
                inputManager.playerControls.PlayerActions.EnemyLockOn.performed += OnToggleLockInput;
            }
            catch
            {
                Debug.LogWarning("EnemyLockOn input action not found. Please bind it manually.");
            }
        }
    }
    
    private void OnDisable()
    {
        if (inputManager != null && inputManager.playerControls != null)
        {
            try
            {
                inputManager.playerControls.PlayerActions.EnemyLockOn.performed -= OnToggleLockInput;
            }
            catch { }
        }
    }
    
    private void Update()
    {
        if (IsLocked)
        {
            ValidateCurrentTarget();
        }
        
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    private void OnToggleLockInput(InputAction.CallbackContext context)
    {
        if (showDebugInfo)
            Debug.Log($"🎮 Input received: phase={context.phase}, time={Time.time:F3}");
        ToggleLock();
    }
    
    public void ToggleLock()
    {
        if (Time.time - lastToggleTime < TOGGLE_COOLDOWN)
        {
            if (showDebugInfo)
                Debug.Log($"Toggle ignored - cooldown active ({Time.time - lastToggleTime:F2}s since last toggle)");
            return;
        }
        
        lastToggleTime = Time.time;
        
        if (IsLocked)
        {
            if (showDebugInfo)
                Debug.Log("Toggle: Clearing lock");
            ClearTarget();
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("Toggle: Acquiring target");
            AcquireTarget();
        }
    }
    
    public bool AcquireTarget()
    {
        Transform bestTarget = FindBestTarget();
        
        if (bestTarget != null)
        {
            SetTarget(bestTarget);
            return true;
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("No valid target found for lock-on");
            return false;
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == CurrentTarget)
        {
            if (showDebugInfo)
                Debug.Log("SetTarget: Target unchanged");
            return;
        }
        
        Transform oldTarget = CurrentTarget;
        
        if (CurrentTarget != null)
        {
            UnsubscribeFromTarget(CurrentTarget);
        }
        
        CurrentTarget = newTarget;
        lastValidationTime = Time.time;
        
        if (CurrentTarget != null)
        {
            SubscribeToTarget(CurrentTarget);
            
            if (showDebugInfo)
                Debug.Log($"✓ Locked onto: {CurrentTarget.name} at {Time.time:F2}");
        }
        
        OnTargetChanged?.Invoke(CurrentTarget, oldTarget);
        
        bool wasLocked = oldTarget != null;
        bool isNowLocked = CurrentTarget != null;
        
        if (wasLocked != isNowLocked)
        {
            if (showDebugInfo)
                Debug.Log($"Lock state changed: {wasLocked} → {isNowLocked}");
            OnLockStateChanged?.Invoke(isNowLocked);
        }
    }
    
    public void ClearTarget()
    {
        if (CurrentTarget != null)
        {
            if (showDebugInfo)
                Debug.Log($"✗ Cleared lock from: {CurrentTarget.name} at {Time.time:F2}");
            
            SetTarget(null);
        }
    }
    
    public bool SwitchToNextTarget(Vector2 direction)
    {
        if (!allowTargetSwitching)
            return false;
        
        if (Time.time - lastSwitchTime < switchTargetCooldown)
            return false;
        
        List<Transform> allTargets = GetAllValidTargets();
        
        if (allTargets.Count <= 1)
            return false;
        
        Transform bestSwitch = FindBestSwitchTarget(allTargets, direction);
        
        if (bestSwitch != null && bestSwitch != CurrentTarget)
        {
            SetTarget(bestSwitch);
            lastSwitchTime = Time.time;
            return true;
        }
        
        return false;
    }
    
    private Transform FindBestTarget()
    {
        List<Transform> validTargets = GetAllValidTargets();
        
        if (validTargets.Count == 0)
            return null;
        
        Transform bestTarget = null;
        float bestScore = float.MaxValue;
        Vector3 playerPos = transform.position;
        Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        
        foreach (Transform target in validTargets)
        {
            Vector3 toTarget = target.position - playerPos;
            float distance = toTarget.magnitude;
            
            Vector3 directionToTarget = toTarget.normalized;
            float angleFromCamera = Vector3.Angle(cameraForward, directionToTarget);
            
            if (angleFromCamera > maxAngleFromCamera)
                continue;
            
            float distanceScore = distance / lockOnRange;
            float angleScore = angleFromCamera / maxAngleFromCamera;
            
            float combinedScore = (distanceScore * 0.6f) + (angleScore * 0.4f);
            
            if (combinedScore < bestScore)
            {
                bestScore = combinedScore;
                bestTarget = target;
            }
        }
        
        return bestTarget;
    }
    
    private Transform FindBestSwitchTarget(List<Transform> targets, Vector2 screenDirection)
    {
        if (cameraTransform == null || CurrentTarget == null)
            return null;
        
        Vector3 switchDirection = cameraTransform.right * screenDirection.x + cameraTransform.up * screenDirection.y;
        switchDirection.Normalize();
        
        Transform bestTarget = null;
        float bestScore = float.MaxValue;
        Vector3 currentTargetPos = CurrentTarget.position;
        
        foreach (Transform target in targets)
        {
            if (target == CurrentTarget)
                continue;
            
            Vector3 toTarget = (target.position - currentTargetPos).normalized;
            float alignment = Vector3.Dot(toTarget, switchDirection);
            
            if (alignment < 0.3f)
                continue;
            
            float distance = Vector3.Distance(target.position, currentTargetPos);
            float score = distance / alignment;
            
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }
        
        return bestTarget;
    }
    
    private List<Transform> GetAllValidTargets()
    {
        List<Transform> validTargets = new List<Transform>();
        
        Collider[] potentialTargets = Physics.OverlapSphere(
            transform.position,
            lockOnRange,
            targetLayers,
            QueryTriggerInteraction.Collide
        );
        
        HashSet<Transform> processedRoots = new HashSet<Transform>();
        
        foreach (Collider col in potentialTargets)
        {
            if (col.transform == transform)
                continue;
            
            if (!string.IsNullOrEmpty(targetTag) && !col.CompareTag(targetTag))
                continue;
            
            Transform root = GetTargetRoot(col.transform);
            
            if (root == null || processedRoots.Contains(root))
                continue;
            
            processedRoots.Add(root);
            
            if (IsTargetValid(root))
            {
                validTargets.Add(root);
            }
        }
        
        return validTargets;
    }
    
    private Transform GetTargetRoot(Transform t)
    {
        Transform current = t;
        
        while (current != null)
        {
            if (current.GetComponent<AiAgent>() != null ||
                current.GetComponent<BasicEnemyHealth>() != null)
            {
                return current;
            }
            
            current = current.parent;
        }
        
        return t;
    }
    
    private bool IsTargetValid(Transform target, bool useExtendedRange = false)
    {
        if (target == null)
            return false;
        
        if (deadTargets.Contains(target))
            return false;
        
        AiAgent aiAgent = target.GetComponent<AiAgent>();
        if (aiAgent != null && aiAgent.isDead)
        {
            deadTargets.Add(target);
            return false;
        }
        
        BasicEnemyHealth health = target.GetComponent<BasicEnemyHealth>();
        if (health != null && health.currentHealth <= 0)
        {
            deadTargets.Add(target);
            return false;
        }
        
        float distance = Vector3.Distance(transform.position, target.position);
        float maxRange = useExtendedRange ? targetLostDistance : lockOnRange;
        if (distance > maxRange)
            return false;
        
        if (requireLineOfSight)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 targetPoint = target.position + Vector3.up * 1f;
            Vector3 direction = targetPoint - origin;
            
            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, obstacleLayers))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target) && !target.IsChildOf(hit.transform))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private void ValidateCurrentTarget()
    {
        if (Time.time - lastValidationTime < targetValidationInterval)
            return;
        
        lastValidationTime = Time.time;
        
        if (CurrentTarget == null)
        {
            ClearTarget();
            return;
        }
        
        float distance = Vector3.Distance(transform.position, CurrentTarget.position);
        if (distance > targetLostDistance)
        {
            if (showDebugInfo)
                Debug.Log($"⚠ Target lost: too far ({distance:F1}m > {targetLostDistance}m)");
            
            Transform retarget = FindBestTarget();
            if (retarget != null)
                SetTarget(retarget);
            else
                ClearTarget();
            
            return;
        }
        
        if (!IsTargetValid(CurrentTarget, useExtendedRange: true))
        {
            if (showDebugInfo)
                Debug.Log("⚠ Target lost: no longer valid");
            
            Transform retarget = FindBestTarget();
            if (retarget != null)
                SetTarget(retarget);
            else
                ClearTarget();
        }
    }
    
    private void SubscribeToTarget(Transform target)
    {
        BasicEnemyHealth health = target.GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            health.OnDeath += OnTargetDeath;
        }
    }
    
    private void UnsubscribeFromTarget(Transform target)
    {
        BasicEnemyHealth health = target.GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            health.OnDeath -= OnTargetDeath;
        }
    }
    
    private void OnTargetDeath()
    {
        if (showDebugInfo)
            Debug.Log("Target died, searching for new target");
        
        Transform retarget = FindBestTarget();
        if (retarget != null)
            SetTarget(retarget);
        else
            ClearTarget();
    }
    
    public Vector3 GetTargetDirection()
    {
        if (!IsLocked || CurrentTarget == null)
            return transform.forward;
        
        Vector3 direction = CurrentTarget.position - transform.position;
        direction.y = 0;
        
        if (direction.sqrMagnitude < 0.01f)
            return transform.forward;
        
        return direction.normalized;
    }
    
    public Vector3 GetTargetPosition()
    {
        return CurrentTarget != null ? CurrentTarget.position : transform.position;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockOnRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetLostDistance);
        
        if (IsLocked && CurrentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, CurrentTarget.position + Vector3.up);
            Gizmos.DrawWireSphere(CurrentTarget.position + Vector3.up, 0.5f);
        }
    }
}
