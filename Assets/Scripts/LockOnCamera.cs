using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private LockOnSystem lockOnSystem;

    [Header("Camera Position")]
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.6f, 0.2f, 0f);
    [SerializeField] private float cameraDistance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float playerInfluence = 0.3f;
    [SerializeField] private bool allowManualRotation = true;
    [SerializeField] private float manualRotationSpeed = 2f;

    [Header("Target Framing")]
    [SerializeField] private float targetFrameOffset = 0.3f;
    [SerializeField] private float verticalOffset = 0.5f;

    [Header("Input Settings")]
    [SerializeField] private InputManager playerInputManager;
    [SerializeField] private bool useMouseDelta = true;
    [SerializeField] private float mouseSensitivity = 0.1f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 12f;
    [SerializeField] private float rotationSmoothing = 10f;
    [SerializeField] private float lockTransitionSmoothing = 8f;

    [Header("Collision")]
    [SerializeField] private CameraCollisionHandler collisionHandler;
    [SerializeField] private bool enableFinalSafetyCheck = false;
    [SerializeField] private float maxRotationDuringCollision = 30f;

    [Header("Distance Adjustment")]
    [SerializeField] private bool adjustDistanceByTargetDistance = true;
    [SerializeField] private float distanceAdjustmentSpeed = 5f;
    [SerializeField] private float targetDistanceMultiplier = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Vector3 desiredPosition;
    private Vector3 idealPosition;
    private Quaternion desiredRotation;
    private Vector2 lookInput;
    private float currentShoulderOffsetScale = 1f;
    private float currentDistance;
    private bool wasLocked = false;
    private Quaternion unlockRotation;
    private bool isInCollision = false;
    private Quaternion stableHorizontalRotation;

    public bool IsInCollision()
    {
        return isInCollision;
    }

    public float GetCurrentCollisionInfluence()
    {
        if (!isInCollision) return 1f;

        // Return a value between 0-1 indicating how much collision is affecting the camera
        LockOnSystem lockSys = target?.GetComponent<LockOnSystem>();
        if (lockSys != null && lockSys.IsLockedOn() && lockSys.currentLockTarget != null)
        {
            float distanceToTarget = Vector3.Distance(target.position, lockSys.currentLockTarget.position);

            // Use the lockRange from the LockOnSystem component
            float systemLockRange = lockSys.lockRange; // Access the public field from LockOnSystem
            return Mathf.Clamp01(distanceToTarget / systemLockRange);
        }

        return 0.5f;
    }
    private void Start()
    {
        FindPlayerIfNeeded();

        if (lockOnSystem == null && target != null)
        {
            lockOnSystem = target.GetComponent<LockOnSystem>();
        }

        currentDistance = cameraDistance;

        if (collisionHandler != null)
        {
            collisionHandler.InitializeDistance(currentDistance);
        }

        unlockRotation = transform.rotation;
    }
    
    private void FindPlayerIfNeeded()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"LockOnCamera automatically found player: {player.name}");
            }
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target != null && lockOnSystem == null)
        {
            lockOnSystem = target.GetComponent<LockOnSystem>();
        }
        
        if (target != null && playerInputManager == null)
        {
            playerInputManager = target.GetComponent<InputManager>();
        }
    }

    private void ReadInput()
    {
        if (!allowManualRotation || playerInputManager == null || playerInputManager.playerControls == null)
        {
            lookInput = Vector2.zero;
            return;
        }

        if (useMouseDelta)
        {
            lookInput = playerInputManager.playerControls.PlayerMovement.Look.ReadValue<Vector2>();
        }
        else
        {
            lookInput = playerInputManager.cameraInput;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        ReadInput();
        UpdateCameraDistance();
        CalculateDesiredPosition();
        CalculateDesiredRotation();
        HandleCollision();
        ApplySmoothing();
    }

    private void UpdateCameraDistance()
    {
        if (!adjustDistanceByTargetDistance || lockOnSystem == null || !lockOnSystem.IsLockedOn())
        {
            currentDistance = Mathf.Lerp(currentDistance, cameraDistance, distanceAdjustmentSpeed * Time.deltaTime);
            return;
        }

        Transform lockTarget = lockOnSystem.currentLockTarget;
        if (lockTarget != null)
        {
            float distanceToTarget = Vector3.Distance(target.position, lockTarget.position);
            float adjustedDistance = Mathf.Clamp(
                cameraDistance + (distanceToTarget * targetDistanceMultiplier),
                minDistance,
                maxDistance
            );
            currentDistance = Mathf.Lerp(currentDistance, adjustedDistance, distanceAdjustmentSpeed * Time.deltaTime);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, cameraDistance, distanceAdjustmentSpeed * Time.deltaTime);
        }
    }

    private void CalculateDesiredRotation()
    {
        if (lockOnSystem == null || !lockOnSystem.IsLockedOn())
        {
            HandleUnlockedRotation();
            return;
        }

        Transform lockTarget = lockOnSystem.currentLockTarget;
        if (lockTarget == null)
        {
            HandleUnlockedRotation();
            return;
        }

        Vector3 targetPoint = target.position + targetOffset;
        Vector3 lockTargetPoint = lockTarget.position + Vector3.up * verticalOffset;
        
        Vector3 frameOffset = Vector3.Cross((lockTargetPoint - targetPoint).normalized, Vector3.up) * targetFrameOffset;
        Vector3 framedPoint = lockTargetPoint + frameOffset;

        Vector3 directionToTarget = (framedPoint - idealPosition).normalized;

        if (directionToTarget.sqrMagnitude < 0.01f)
        {
            directionToTarget = (framedPoint - targetPoint).normalized;
        }

        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0f, directionToTarget.z);
        
        if (horizontalDirection.sqrMagnitude < 0.01f)
        {
            horizontalDirection = transform.forward;
            horizontalDirection.y = 0f;
        }
        
        horizontalDirection.Normalize();

        Quaternion horizontalRotation = Quaternion.LookRotation(horizontalDirection);
        stableHorizontalRotation = horizontalRotation;

        float pitch = Mathf.Asin(Mathf.Clamp(directionToTarget.y, -0.99f, 0.99f)) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        Quaternion targetRotation = horizontalRotation * Quaternion.Euler(pitch, 0f, 0f);

        if (allowManualRotation && lookInput.sqrMagnitude > 0.01f)
        {
            float hSensitivity = useMouseDelta ? mouseSensitivity : manualRotationSpeed;
            Quaternion manualRotation = Quaternion.Euler(0f, lookInput.x * hSensitivity, 0f);
            targetRotation = manualRotation * targetRotation;
            stableHorizontalRotation = manualRotation * stableHorizontalRotation;
        }
        else if (playerInfluence > 0f)
        {
            Quaternion playerRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            targetRotation = Quaternion.Slerp(targetRotation, playerRotation, playerInfluence);
        }

        desiredRotation = targetRotation;
        wasLocked = true;
    }

    private void HandleUnlockedRotation()
    {
        if (wasLocked)
        {
            unlockRotation = transform.rotation;
            wasLocked = false;
        }

        Vector3 targetPoint = target.position + targetOffset;
        Vector3 cameraForward = unlockRotation * Vector3.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        if (allowManualRotation && lookInput.sqrMagnitude > 0.01f)
        {
            float hSensitivity = useMouseDelta ? mouseSensitivity : manualRotationSpeed;
            Quaternion manualRotation = Quaternion.Euler(0f, lookInput.x * hSensitivity, 0f);
            unlockRotation = manualRotation * unlockRotation;
        }
        else if (playerInfluence > 0f)
        {
            Quaternion playerRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            unlockRotation = Quaternion.Slerp(unlockRotation, playerRotation, playerInfluence * Time.deltaTime);
        }

        stableHorizontalRotation = Quaternion.LookRotation(cameraForward);
        desiredRotation = unlockRotation;
    }

    private void CalculateDesiredPosition()
    {
        Vector3 targetPoint = target.position + targetOffset;

        Quaternion localRotation = desiredRotation;
        Vector3 localOffset = shoulderOffset;
        localOffset *= currentShoulderOffsetScale;
        
        Vector3 rotatedOffset = localRotation * localOffset;
        Vector3 offsetTargetPoint = targetPoint + rotatedOffset;

        Vector3 direction = (localRotation * Vector3.back).normalized;
        
        idealPosition = offsetTargetPoint + direction * currentDistance;
        desiredPosition = idealPosition;
    }

    private void HandleCollision()
    {
        if (collisionHandler == null)
        {
            isInCollision = false;
            return;
        }

        Vector3 targetPoint = target.position + targetOffset;
        Quaternion localRotation = desiredRotation;
        Vector3 localOffset = shoulderOffset * currentShoulderOffsetScale;
        Vector3 rotatedOffset = localRotation * localOffset;
        Vector3 offsetTargetPoint = targetPoint + rotatedOffset;

        float adjustedDistance = Vector3.Distance(offsetTargetPoint, desiredPosition);
        Vector3 collisionAdjustedPosition = collisionHandler.HandleCollision(offsetTargetPoint, desiredPosition, adjustedDistance);
        
        float distanceAfterCollision = Vector3.Distance(offsetTargetPoint, collisionAdjustedPosition);
        isInCollision = distanceAfterCollision < (adjustedDistance * 0.95f);
        
        desiredPosition = collisionAdjustedPosition;
    }

    private void ApplySmoothing()
    {
        float smoothSpeed = (lockOnSystem != null && lockOnSystem.IsLockedOn()) 
            ? lockTransitionSmoothing 
            : positionSmoothing;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (enableFinalSafetyCheck && collisionHandler != null)
        {
            smoothedPosition = FinalPositionCheck(smoothedPosition);
        }

        transform.position = smoothedPosition;

        float rotSmooth = (lockOnSystem != null && lockOnSystem.IsLockedOn())
            ? lockTransitionSmoothing
            : rotationSmoothing;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotSmooth * Time.deltaTime);
        
        if (showDebugInfo)
        {
            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            Debug.Log($"[LockOnCamera] InCollision: {isInCollision} | Yaw: {transform.rotation.eulerAngles.y:F1}° | Pitch: {transform.rotation.eulerAngles.x:F1}° | HFwd: {flatForward.normalized}");
        }
    }

    private Vector3 FinalPositionCheck(Vector3 position)
    {
        if (collisionHandler == null || target == null)
        {
            return position;
        }

        LayerMask layers = collisionHandler.GetCollisionLayers();
        float radius = collisionHandler.GetCameraRadius() * 0.5f;

        if (Physics.CheckSphere(position, radius, layers, QueryTriggerInteraction.Ignore))
        {
            Vector3 targetPoint = target.position + targetOffset;
            Vector3 directionToTarget = (targetPoint - position).normalized;
            float maxPushDistance = 1f;

            for (float distance = 0.1f; distance < maxPushDistance; distance += 0.1f)
            {
                Vector3 testPosition = position + directionToTarget * distance;
                if (!Physics.CheckSphere(testPosition, radius, layers, QueryTriggerInteraction.Ignore))
                {
                    return testPosition;
                }
            }
        }

        return position;
    }

    public Vector3 GetCameraForward()
    {
        return transform.forward;
    }

    public Vector3 GetCameraPosition()
    {
        return transform.position;
    }

    public bool IsLocked()
    {
        return lockOnSystem != null && lockOnSystem.IsLockedOn();
    }

    public Transform GetCurrentLockTarget()
    {
        if (lockOnSystem != null)
        {
            return lockOnSystem.currentLockTarget;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || target == null)
        {
            return;
        }

        if (lockOnSystem != null && lockOnSystem.IsLockedOn() && lockOnSystem.currentLockTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lockOnSystem.currentLockTarget.position + Vector3.up * verticalOffset, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, lockOnSystem.currentLockTarget.position + Vector3.up * verticalOffset);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position + targetOffset, 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);
    }

    public void EmergencyRecovery()
    {
        if (!isInCollision) return;

        // Force camera to a safer position
        Vector3 targetPoint = target.position + targetOffset;
        Vector3 safeDirection = (targetPoint - transform.position).normalized;

        // Find a position that's not colliding
        if (collisionHandler != null)
        {
            float safeDistance = collisionHandler.FindSafeDistance(targetPoint, safeDirection, currentDistance);
            transform.position = targetPoint + safeDirection * safeDistance;
        }

        isInCollision = false;
    }
}
