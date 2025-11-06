using UnityEngine;

[DefaultExecutionOrder(100)]
public class TargetedPlayerRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZTargetingSystem zTargeting;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputManager inputManager;
    
    [Header("Free Rotation Settings")]
    [SerializeField] private float freeRotationSpeed = 12f;
    [SerializeField] private float freeRotationSmoothing = 0.15f;
    
    [Header("Locked Rotation Settings")]
    [SerializeField] private float lockedRotationSpeed = 100f;
    [SerializeField] private float lockedRotationSmoothing = 0.1f;
    [SerializeField] private float minDistanceForRotation = 0.5f;
    
    [Header("Camera Collision Handling")]
    [SerializeField] private bool limitRotationDuringCollision = true;
    [SerializeField] private float collisionRotationMultiplier = 0.4f;
    [SerializeField] private float maxRotationAnglePerFrame = 120f;
    
    [Header("State Blending")]
    [SerializeField] private float lockTransitionSpeed = 8f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private Quaternion targetRotation;
    private float currentRotationVelocity;
    private bool wasLockedLastFrame;
    private Quaternion lastValidRotation;
    private LockOnCamera lockOnCamera;
    
    private Rigidbody rb;
    
    private void Awake()
    {
        if (zTargeting == null)
            zTargeting = GetComponent<ZTargetingSystem>();
        
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }
        
        if (inputManager == null)
            inputManager = GetComponent<InputManager>();
        
        rb = GetComponent<Rigidbody>();
        
        targetRotation = transform.rotation;
        lastValidRotation = transform.rotation;
    }
    
    private void Start()
    {
        if (cameraTransform != null)
        {
            lockOnCamera = cameraTransform.GetComponent<LockOnCamera>();
        }
    }
    
    private void FixedUpdate()
    {
        RefreshCameraReference();
        
        UpdateTargetRotation();
        ApplyRotation();
        
        wasLockedLastFrame = zTargeting != null && zTargeting.IsLocked;
    }
    
    private void RefreshCameraReference()
    {
        if (Camera.main != null)
        {
            Transform mainCam = Camera.main.transform;
            
            if (cameraTransform != mainCam)
            {
                cameraTransform = mainCam;
                
                if (cameraTransform != null)
                {
                    lockOnCamera = cameraTransform.GetComponent<LockOnCamera>();
                }
            }
        }
    }
    
    private void UpdateTargetRotation()
    {
        if (zTargeting != null && zTargeting.IsLocked)
        {
            UpdateLockedRotation();
        }
        else
        {
            UpdateFreeRotation();
        }
    }
    
    private void UpdateLockedRotation()
    {
        if (zTargeting.CurrentTarget == null)
        {
            if (showDebugInfo)
                Debug.Log("[TargetedPlayerRotation] No current target");
            return;
        }
        
        Vector3 directionToTarget = zTargeting.CurrentTarget.position - transform.position;
        directionToTarget.y = 0;
        
        if (directionToTarget.sqrMagnitude < minDistanceForRotation * minDistanceForRotation)
        {
            if (showDebugInfo)
                Debug.Log($"[TargetedPlayerRotation] Too close to target ({directionToTarget.magnitude:F2}m < {minDistanceForRotation}m)");
            targetRotation = transform.rotation;
            return;
        }
        
        directionToTarget.Normalize();
        targetRotation = Quaternion.LookRotation(directionToTarget);
        
        if (showDebugInfo && Time.frameCount % 30 == 0)
        {
            float currentAngle = Quaternion.Angle(rb != null ? rb.rotation : transform.rotation, targetRotation);
            Debug.Log($"[TargetedPlayerRotation] Target: {zTargeting.CurrentTarget.name}, Angle diff: {currentAngle:F1}°, Current Y: {transform.eulerAngles.y:F1}°, Target Y: {targetRotation.eulerAngles.y:F1}°");
        }
    }
    
    private void UpdateFreeRotation()
    {
        if (inputManager == null || cameraTransform == null)
            return;
        
        float moveAmount = inputManager.moveAmount;
        
        if (moveAmount < 0.1f)
        {
            targetRotation = transform.rotation;
            return;
        }
        
        Vector3 moveDirection = Vector3.zero;
        
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        moveDirection = cameraForward * inputManager.verticalInput;
        moveDirection += cameraRight * inputManager.horizontalInput;
        
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            moveDirection.Normalize();
            targetRotation = Quaternion.LookRotation(moveDirection);
        }
    }
    
    private void ApplyRotation()
    {
        bool isLocked = zTargeting != null && zTargeting.IsLocked;
        
        // Only apply when locked - PlayerLocomotion handles free rotation
        if (!isLocked)
            return;
        
        // When locked, directly face the target - no smoothing to avoid conflicts
        if (!float.IsNaN(targetRotation.x) && 
            !float.IsNaN(targetRotation.y) && 
            !float.IsNaN(targetRotation.z) && 
            !float.IsNaN(targetRotation.w))
        {
            if (rb != null)
            {
                rb.MoveRotation(targetRotation);
            }
            else
            {
                transform.rotation = targetRotation;
            }
            lastValidRotation = targetRotation;
        }
        else
        {
            if (rb != null)
            {
                rb.MoveRotation(lastValidRotation);
            }
            else
            {
                transform.rotation = lastValidRotation;
            }
            Debug.LogWarning("Invalid rotation detected, using last valid rotation");
        }
    }
    
    public void SetRotationOverride(Quaternion rotation)
    {
        targetRotation = rotation;
        transform.rotation = rotation;
        lastValidRotation = rotation;
    }
    
    public void FaceDirection(Vector3 direction, bool instant = false)
    {
        direction.y = 0;
        
        if (direction.sqrMagnitude < 0.01f)
            return;
        
        direction.Normalize();
        Quaternion newRotation = Quaternion.LookRotation(direction);
        
        if (instant)
        {
            transform.rotation = newRotation;
            targetRotation = newRotation;
            lastValidRotation = newRotation;
        }
        else
        {
            targetRotation = newRotation;
        }
    }
    
    public Vector3 GetForwardDirection()
    {
        if (zTargeting != null && zTargeting.IsLocked && zTargeting.CurrentTarget != null)
        {
            return zTargeting.GetTargetDirection();
        }
        
        return transform.forward;
    }
    
    public bool IsRotatingTowardsTarget()
    {
        if (zTargeting == null || !zTargeting.IsLocked)
            return false;
        
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        return angleDifference > 5f;
    }
}
