using UnityEngine;

public class ZTargetingCamera : MonoBehaviour
{
    [Header("Target References")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private ZTargetingSystem zTargeting;
    [SerializeField] private Vector3 playerOffset = new Vector3(0f, 1.5f, 0f);
    
    [Header("Camera Position")]
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.6f, 0.3f, 0f);
    [SerializeField] private float defaultDistance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    
    [Header("Locked Mode Settings")]
    [SerializeField] private float lockedDistance = 6f;
    [SerializeField] private float targetVerticalOffset = 1f;
    [SerializeField] private float targetFramingOffset = 0.25f;
    [SerializeField] private bool adjustDistanceByTargetDistance = true;
    [SerializeField] private float distanceAdjustmentFactor = 0.4f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float freeRotationSpeed = 8f;
    [SerializeField] private float lockedRotationSpeed = 12f;
    [SerializeField] private float manualRotationSensitivity = 2f;
    [SerializeField] private bool allowManualRotation = true;
    
    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 10f;
    [SerializeField] private float rotationSmoothing = 8f;
    [SerializeField] private float lockTransitionSmoothing = 6f;
    [SerializeField] private float distanceSmoothing = 5f;
    
    [Header("Input")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private bool useMouseInput = true;
    [SerializeField] private float mouseSensitivity = 0.15f;
    
    [Header("Collision")]
    [SerializeField] private CameraCollisionHandler collisionHandler;
    [SerializeField] private bool enableCollisionHandling = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool showGizmos = true;
    
    private Vector3 currentPosition;
    private Quaternion currentRotation;
    private float currentDistance;
    private Vector2 lookInput;
    private Quaternion manualRotationOffset = Quaternion.identity;
    private bool wasLocked;
    
    private void Awake()
    {
        FindPlayerIfNeeded();
        
        if (zTargeting == null && playerTarget != null)
        {
            zTargeting = playerTarget.GetComponent<ZTargetingSystem>();
        }
        
        if (inputManager == null && playerTarget != null)
        {
            inputManager = playerTarget.GetComponent<InputManager>();
        }
        
        currentDistance = defaultDistance;
        currentPosition = transform.position;
        currentRotation = transform.rotation;
    }
    
    private void FindPlayerIfNeeded()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                Debug.Log($"ZTargetingCamera automatically found player: {player.name}");
            }
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;
        
        if (playerTarget != null && zTargeting == null)
        {
            zTargeting = playerTarget.GetComponent<ZTargetingSystem>();
        }
        
        if (playerTarget != null && inputManager == null)
        {
            inputManager = playerTarget.GetComponent<InputManager>();
        }
    }
    
    private void Start()
    {
        if (collisionHandler != null)
        {
            collisionHandler.InitializeDistance(currentDistance);
        }
    }
    
    private void LateUpdate()
    {
        if (playerTarget == null)
            return;
        
        ReadInput();
        
        UpdateCameraDistance();
        
        Vector3 desiredPosition;
        Quaternion desiredRotation;
        
        if (zTargeting != null && zTargeting.IsLocked)
        {
            CalculateLockedCamera(out desiredPosition, out desiredRotation);
        }
        else
        {
            CalculateFreeCamera(out desiredPosition, out desiredRotation);
        }
        
        if (enableCollisionHandling && collisionHandler != null)
        {
            desiredPosition = HandleCollision(desiredPosition);
        }
        
        ApplySmoothing(desiredPosition, desiredRotation);
        
        wasLocked = zTargeting != null && zTargeting.IsLocked;
    }
    
    private void ReadInput()
    {
        lookInput = Vector2.zero;
        
        if (!allowManualRotation || inputManager == null || inputManager.playerControls == null)
            return;
        
        if (useMouseInput)
        {
            lookInput = inputManager.playerControls.PlayerMovement.Look.ReadValue<Vector2>();
        }
        else
        {
            lookInput = inputManager.cameraInput;
        }
    }
    
    private void UpdateCameraDistance()
    {
        float targetDistance = defaultDistance;
        
        if (zTargeting != null && zTargeting.IsLocked)
        {
            targetDistance = lockedDistance;
            
            if (adjustDistanceByTargetDistance && zTargeting.CurrentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(
                    playerTarget.position,
                    zTargeting.CurrentTarget.position
                );
                
                targetDistance += distanceToTarget * distanceAdjustmentFactor;
                targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            }
        }
        
        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            distanceSmoothing * Time.deltaTime
        );
    }
    
    private void CalculateFreeCamera(out Vector3 position, out Quaternion rotation)
    {
        Vector3 targetPoint = playerTarget.position + playerOffset;
        
        if (lookInput.sqrMagnitude > 0.01f)
        {
            float sensitivity = useMouseInput ? mouseSensitivity : manualRotationSensitivity;
            float yawInput = lookInput.x * sensitivity;
            
            Quaternion yawRotation = Quaternion.Euler(0f, yawInput, 0f);
            manualRotationOffset = yawRotation * manualRotationOffset;
        }
        
        rotation = manualRotationOffset;
        
        Vector3 localOffset = rotation * shoulderOffset;
        Vector3 offsetPoint = targetPoint + localOffset;
        
        Vector3 backDirection = rotation * Vector3.back;
        position = offsetPoint + backDirection * currentDistance;
    }
    
    private void CalculateLockedCamera(out Vector3 position, out Quaternion rotation)
    {
        if (zTargeting.CurrentTarget == null)
        {
            CalculateFreeCamera(out position, out rotation);
            return;
        }
        
        Vector3 playerPoint = playerTarget.position + playerOffset;
        Vector3 targetPoint = zTargeting.CurrentTarget.position + Vector3.up * targetVerticalOffset;
        
        Vector3 midPoint = Vector3.Lerp(playerPoint, targetPoint, 0.4f);
        
        Vector3 playerToTarget = (targetPoint - playerPoint).normalized;
        Vector3 frameOffset = Vector3.Cross(playerToTarget, Vector3.up) * targetFramingOffset;
        Vector3 framedTargetPoint = targetPoint + frameOffset;
        
        Vector3 idealCameraPosition = midPoint + Vector3.back * currentDistance;
        
        Vector3 directionToFramedTarget = (framedTargetPoint - idealCameraPosition).normalized;
        
        if (directionToFramedTarget.sqrMagnitude < 0.01f)
        {
            directionToFramedTarget = (targetPoint - playerPoint).normalized;
        }
        
        rotation = Quaternion.LookRotation(directionToFramedTarget);
        
        if (lookInput.sqrMagnitude > 0.01f)
        {
            float sensitivity = useMouseInput ? mouseSensitivity * 0.5f : manualRotationSensitivity * 0.5f;
            float yawInput = lookInput.x * sensitivity;
            
            Quaternion manualRotation = Quaternion.Euler(0f, yawInput, 0f);
            rotation = manualRotation * rotation;
        }
        
        Vector3 localOffset = rotation * shoulderOffset;
        Vector3 offsetPoint = playerPoint + localOffset;
        
        Vector3 backDirection = rotation * Vector3.back;
        position = offsetPoint + backDirection * currentDistance;
    }
    
    private Vector3 HandleCollision(Vector3 desiredPosition)
    {
        Vector3 targetPoint = playerTarget.position + playerOffset;
        float checkDistance = Vector3.Distance(targetPoint, desiredPosition);
        
        return collisionHandler.HandleCollision(targetPoint, desiredPosition, checkDistance);
    }
    
    private void ApplySmoothing(Vector3 desiredPosition, Quaternion desiredRotation)
    {
        bool isLocked = zTargeting != null && zTargeting.IsLocked;
        
        float posSmooth = isLocked ? lockTransitionSmoothing : positionSmoothing;
        float rotSmooth = isLocked ? lockTransitionSmoothing : rotationSmoothing;
        
        currentPosition = Vector3.Lerp(currentPosition, desiredPosition, posSmooth * Time.deltaTime);
        currentRotation = Quaternion.Slerp(currentRotation, desiredRotation, rotSmooth * Time.deltaTime);
        
        transform.position = currentPosition;
        transform.rotation = currentRotation;
        
        if (showDebugInfo)
        {
            Debug.Log($"Camera - Locked: {isLocked} | Distance: {currentDistance:F2} | Pos: {currentPosition}");
        }
    }
    
    public bool IsLocked()
    {
        return zTargeting != null && zTargeting.IsLocked;
    }
    
    public Vector3 GetCameraForward()
    {
        return transform.forward;
    }
    
    public Transform GetCurrentTarget()
    {
        return zTargeting != null ? zTargeting.CurrentTarget : null;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying || playerTarget == null)
            return;
        
        Gizmos.color = Color.cyan;
        Vector3 playerPoint = playerTarget.position + playerOffset;
        Gizmos.DrawWireSphere(playerPoint, 0.2f);
        
        if (zTargeting != null && zTargeting.IsLocked && zTargeting.CurrentTarget != null)
        {
            Gizmos.color = Color.red;
            Vector3 targetPoint = zTargeting.CurrentTarget.position + Vector3.up * targetVerticalOffset;
            Gizmos.DrawWireSphere(targetPoint, 0.3f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPoint);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerPoint, targetPoint);
        }
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
