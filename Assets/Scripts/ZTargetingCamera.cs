using UnityEngine;
using UnityEngine.InputSystem;

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
    
    [Header("Dynamic Framing")]
    [SerializeField] private bool keepTargetInView = true;
    [SerializeField] private Vector2 viewportSafeZone = new Vector2(0.2f, 0.15f);
    [SerializeField] private float framingDistanceMin = 3f;
    [SerializeField] private float framingDistanceMax = 12f;
    [SerializeField] private float framingAdjustmentSpeed = 3f;
    [SerializeField] private float rotationCorrectionStrength = 0.7f;
    [SerializeField] private float maxManualRotationAngle = 45f;
    
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
    [SerializeField] private float gamepadSensitivity = 100f;
    
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
    private float framingDistanceOffset = 0f;
    private Camera cachedCamera;
    private Quaternion lockedCameraIdealRotation = Quaternion.identity;
    private float desiredWorldYaw = 0f;
    private float smoothedYaw = 0f;
    private bool isFirstFrame = true;
    private bool hasInitializedYaw = false;
    
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
        
        cachedCamera = GetComponent<Camera>();
        
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
            
            if (Gamepad.current != null && inputManager.playerControls.PlayerMovement.Look.activeControl?.device is Gamepad)
            {
                lookInput *= gamepadSensitivity * Time.deltaTime;
            }
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

        Vector3 playerToTarget = (targetPoint - playerPoint).normalized;

        Quaternion autoRotation = Quaternion.LookRotation(playerToTarget);
        lockedCameraIdealRotation = autoRotation;
        float autoYaw = autoRotation.eulerAngles.y;

        if (!hasInitializedYaw)
        {
            desiredWorldYaw = autoYaw;
            smoothedYaw = autoYaw;
            hasInitializedYaw = true;
        }

        // FIX: Add input deadzone and limit rotation speed
        if (lookInput.sqrMagnitude > 0.01f)
        {
            float sensitivity = useMouseInput ? mouseSensitivity * 0.3f : manualRotationSensitivity * 0.3f; // Reduced sensitivity
            float yawInput = lookInput.x * sensitivity;

            // FIX: Limit maximum rotation per frame to prevent spinning
            yawInput = Mathf.Clamp(yawInput, -30f * Time.deltaTime, 30f * Time.deltaTime);

            desiredWorldYaw += yawInput;
        }

        // FIX: Smoother yaw tracking with player movement
        float playerYaw = playerTarget.eulerAngles.y;
        float yawDifferenceToPlayer = Mathf.DeltaAngle(desiredWorldYaw, playerYaw);

        // If player is moving significantly, bias camera toward player's facing direction
        if (Mathf.Abs(yawDifferenceToPlayer) > 60f)
        {
            float correction = Mathf.Sign(yawDifferenceToPlayer) * 30f * Time.deltaTime;
            desiredWorldYaw = Mathf.MoveTowardsAngle(desiredWorldYaw, playerYaw, Mathf.Abs(correction));
        }

        float yawDifference = Mathf.DeltaAngle(desiredWorldYaw, autoYaw);
        yawDifference = Mathf.Clamp(yawDifference, -maxManualRotationAngle, maxManualRotationAngle);
        desiredWorldYaw = autoYaw + yawDifference;

        // FIX: Additional smoothing for desired yaw
        desiredWorldYaw = Mathf.LerpAngle(smoothedYaw, desiredWorldYaw, 5f * Time.deltaTime);
        smoothedYaw = desiredWorldYaw;

        float effectiveDistance = currentDistance + framingDistanceOffset;

        Vector3 framingPoint = Vector3.Lerp(playerPoint, targetPoint, targetFramingOffset);
        Vector3 testCameraPosition = CalculateCameraPositionFromYaw(playerPoint, desiredWorldYaw, effectiveDistance);

        Vector3 cameraToFramingPoint = framingPoint - testCameraPosition;
        rotation = Quaternion.LookRotation(cameraToFramingPoint);

        if (keepTargetInView)
        {
            rotation = ApplyRotationCorrection(rotation, autoRotation, playerPoint, targetPoint, testCameraPosition);

            // FIX: Update desiredWorldYaw based on correction
            float correctedYaw = rotation.eulerAngles.y;
            float correctionDelta = Mathf.DeltaAngle(desiredWorldYaw, correctedYaw);
            if (Mathf.Abs(correctionDelta) > 5f) // Only update if significant correction
            {
                desiredWorldYaw = Mathf.LerpAngle(desiredWorldYaw, correctedYaw, 0.3f);
            }

            framingPoint = Vector3.Lerp(playerPoint, targetPoint, targetFramingOffset);
            testCameraPosition = CalculateCameraPositionFromYaw(playerPoint, rotation.eulerAngles.y, effectiveDistance);
            cameraToFramingPoint = framingPoint - testCameraPosition;
            rotation = Quaternion.LookRotation(cameraToFramingPoint);
        }

        position = testCameraPosition;

        if (keepTargetInView)
        {
            UpdateFramingDistance(position, rotation, playerPoint, targetPoint);
        }
    }

    private Vector3 CalculateCameraPosition(Vector3 playerPoint, Quaternion cameraRotation, float distance)
    {
        Vector3 localOffset = cameraRotation * shoulderOffset;
        Vector3 offsetPoint = playerPoint + localOffset;
        Vector3 backDirection = cameraRotation * Vector3.back;
        return offsetPoint + backDirection * distance;
    }
    
    private Vector3 CalculateCameraPositionFromYaw(Vector3 playerPoint, float yawAngle, float distance)
    {
        Quaternion yawRotation = Quaternion.Euler(0f, yawAngle, 0f);
        Vector3 localOffset = yawRotation * shoulderOffset;
        Vector3 offsetPoint = playerPoint + localOffset;
        Vector3 backDirection = yawRotation * Vector3.back;
        return offsetPoint + backDirection * distance;
    }
    
    private Quaternion ApplyRotationCorrection(Quaternion currentRot, Quaternion idealRot, Vector3 playerPoint, Vector3 targetPoint, Vector3 cameraPos)
    {
        if (cachedCamera == null)
            return currentRot;
        
        Matrix4x4 testViewMatrix = Matrix4x4.TRS(cameraPos, currentRot, Vector3.one).inverse;
        Matrix4x4 projectionMatrix = cachedCamera.projectionMatrix;
        Matrix4x4 vpMatrix = projectionMatrix * testViewMatrix;
        
        Vector3 playerViewport = WorldToViewportPoint(playerPoint, vpMatrix);
        Vector3 targetViewport = WorldToViewportPoint(targetPoint, vpMatrix);
        
        float minX = viewportSafeZone.x;
        float maxX = 1f - viewportSafeZone.x;
        float minY = viewportSafeZone.y;
        float maxY = 1f - viewportSafeZone.y;
        
        bool playerOutOfBounds = playerViewport.z > 0 && 
            (playerViewport.x < minX || playerViewport.x > maxX || 
             playerViewport.y < minY || playerViewport.y > maxY);
             
        bool targetOutOfBounds = targetViewport.z > 0 && 
            (targetViewport.x < minX || targetViewport.x > maxX || 
             targetViewport.y < minY || targetViewport.y > maxY);
        
        bool targetBehindCamera = targetViewport.z <= 0;
        bool playerBehindCamera = playerViewport.z <= 0;
        
        if (targetBehindCamera || playerBehindCamera || targetOutOfBounds || playerOutOfBounds)
        {
            float correctionAmount = rotationCorrectionStrength;
            
            if (targetBehindCamera || playerBehindCamera)
            {
                correctionAmount = 1f;
            }
            else if (targetOutOfBounds || playerOutOfBounds)
            {
                float distanceFromEdge = 0f;
                
                if (targetOutOfBounds)
                {
                    if (targetViewport.x < minX) distanceFromEdge = Mathf.Max(distanceFromEdge, minX - targetViewport.x);
                    if (targetViewport.x > maxX) distanceFromEdge = Mathf.Max(distanceFromEdge, targetViewport.x - maxX);
                    if (targetViewport.y < minY) distanceFromEdge = Mathf.Max(distanceFromEdge, minY - targetViewport.y);
                    if (targetViewport.y > maxY) distanceFromEdge = Mathf.Max(distanceFromEdge, targetViewport.y - maxY);
                }
                
                if (playerOutOfBounds)
                {
                    if (playerViewport.x < minX) distanceFromEdge = Mathf.Max(distanceFromEdge, minX - playerViewport.x);
                    if (playerViewport.x > maxX) distanceFromEdge = Mathf.Max(distanceFromEdge, playerViewport.x - maxX);
                    if (playerViewport.y < minY) distanceFromEdge = Mathf.Max(distanceFromEdge, minY - playerViewport.y);
                    if (playerViewport.y > maxY) distanceFromEdge = Mathf.Max(distanceFromEdge, playerViewport.y - maxY);
                }
                
                correctionAmount = Mathf.Clamp01(rotationCorrectionStrength + distanceFromEdge * 2f);
            }
            
            float yawDifference = Quaternion.Angle(currentRot, idealRot);
            float decayMultiplier = (targetBehindCamera || playerBehindCamera) ? 10f : 5f;
            float decayRate = Mathf.Clamp01(yawDifference / maxManualRotationAngle) * decayMultiplier;
            
            float idealYaw = idealRot.eulerAngles.y;
            desiredWorldYaw = Mathf.LerpAngle(desiredWorldYaw, idealYaw, decayRate * Time.deltaTime);
            
            return Quaternion.Slerp(currentRot, idealRot, correctionAmount);
        }
        
        return currentRot;
    }
    
    private void UpdateFramingDistance(Vector3 cameraPos, Quaternion cameraRot, Vector3 playerPoint, Vector3 targetPoint)
    {
        if (cachedCamera == null)
            return;
        
        Matrix4x4 viewMatrix = Matrix4x4.TRS(cameraPos, cameraRot, Vector3.one).inverse;
        Matrix4x4 projectionMatrix = cachedCamera.projectionMatrix;
        Matrix4x4 vpMatrix = projectionMatrix * viewMatrix;
        
        Vector3 playerViewport = WorldToViewportPoint(playerPoint, vpMatrix);
        Vector3 targetViewport = WorldToViewportPoint(targetPoint, vpMatrix);
        
        float minX = viewportSafeZone.x;
        float maxX = 1f - viewportSafeZone.x;
        float minY = viewportSafeZone.y;
        float maxY = 1f - viewportSafeZone.y;
        
        bool targetBehindCamera = targetViewport.z <= 0;
        bool playerBehindCamera = playerViewport.z <= 0;
        
        bool playerOutOfBounds = playerViewport.z > 0 && 
            (playerViewport.x < minX || playerViewport.x > maxX || 
             playerViewport.y < minY || playerViewport.y > maxY);
             
        bool targetOutOfBounds = targetViewport.z > 0 && 
            (targetViewport.x < minX || targetViewport.x > maxX || 
             targetViewport.y < minY || targetViewport.y > maxY);
        
        float desiredOffset = 0f;
        
        if (targetBehindCamera || playerBehindCamera)
        {
            float distanceBetween = Vector3.Distance(playerPoint, targetPoint);
            desiredOffset = Mathf.Clamp(distanceBetween * 0.8f, 2f, framingDistanceMax - currentDistance);
        }
        else if (playerOutOfBounds || targetOutOfBounds)
        {
            float distanceBetween = Vector3.Distance(playerPoint, targetPoint);
            
            float outOfBoundsAmount = 0f;
            if (targetOutOfBounds)
            {
                outOfBoundsAmount = Mathf.Max(
                    Mathf.Max(minX - targetViewport.x, targetViewport.x - maxX),
                    Mathf.Max(minY - targetViewport.y, targetViewport.y - maxY)
                );
            }
            if (playerOutOfBounds)
            {
                outOfBoundsAmount = Mathf.Max(outOfBoundsAmount, Mathf.Max(
                    Mathf.Max(minX - playerViewport.x, playerViewport.x - maxX),
                    Mathf.Max(minY - playerViewport.y, playerViewport.y - maxY)
                ));
            }
            
            outOfBoundsAmount = Mathf.Clamp01(outOfBoundsAmount);
            float baseOffset = distanceBetween * 0.4f;
            desiredOffset = baseOffset + (outOfBoundsAmount * 3f);
            desiredOffset = Mathf.Clamp(desiredOffset, 0f, framingDistanceMax - currentDistance);
        }
        else
        {
            float spreadX = Mathf.Abs(playerViewport.x - targetViewport.x);
            float spreadY = Mathf.Abs(playerViewport.y - targetViewport.y);
            
            if (spreadX > 0.6f || spreadY > 0.5f)
            {
                float excessSpread = Mathf.Max(spreadX - 0.6f, spreadY - 0.5f);
                desiredOffset = excessSpread * 2f;
            }
            else if (spreadX < 0.3f && spreadY < 0.25f && framingDistanceOffset > 0f)
            {
                desiredOffset = -0.5f;
            }
        }
        
        desiredOffset = Mathf.Clamp(desiredOffset, 
            framingDistanceMin - currentDistance, 
            framingDistanceMax - currentDistance);
        
        float adjustmentSpeed = (targetBehindCamera || playerBehindCamera || targetOutOfBounds || playerOutOfBounds) 
            ? framingAdjustmentSpeed * 2f 
            : framingAdjustmentSpeed;
        
        framingDistanceOffset = Mathf.Lerp(
            framingDistanceOffset, 
            desiredOffset, 
            adjustmentSpeed * Time.deltaTime
        );
    }
    
    private Vector3 WorldToViewportPoint(Vector3 worldPos, Matrix4x4 vpMatrix)
    {
        Vector4 clipPos = vpMatrix * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1f);
        
        if (Mathf.Approximately(clipPos.w, 0f))
            return new Vector3(0.5f, 0.5f, -1f);
        
        Vector3 ndcPos = new Vector3(clipPos.x / clipPos.w, clipPos.y / clipPos.w, clipPos.z / clipPos.w);
        
        return new Vector3(
            ndcPos.x * 0.5f + 0.5f,
            ndcPos.y * 0.5f + 0.5f,
            clipPos.w
        );
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
        
        if (!isLocked && wasLocked)
        {
            framingDistanceOffset = 0f;
            hasInitializedYaw = false;
            isFirstFrame = true;
        }
        
        desiredRotation = SmoothYawTransition(desiredRotation, isLocked);
        
        float posSmooth = isLocked ? lockTransitionSmoothing : positionSmoothing;
        float rotSmooth = isLocked ? lockTransitionSmoothing : rotationSmoothing;
        
        currentPosition = Vector3.Lerp(currentPosition, desiredPosition, posSmooth * Time.deltaTime);
        currentRotation = Quaternion.Slerp(currentRotation, desiredRotation, rotSmooth * Time.deltaTime);
        
        transform.position = currentPosition;
        transform.rotation = currentRotation;
        
        if (showDebugInfo)
        {
            float yawOffset = isLocked && zTargeting.CurrentTarget != null 
                ? Mathf.DeltaAngle(desiredWorldYaw, lockedCameraIdealRotation.eulerAngles.y) 
                : 0f;
            Debug.Log($"Camera - Locked: {isLocked} | Distance: {currentDistance:F2} | Framing Offset: {framingDistanceOffset:F2} | Yaw Offset: {yawOffset:F1}° | World Yaw: {desiredWorldYaw:F1}° | Pos: {currentPosition}");
        }
    }
    
    private Quaternion SmoothYawTransition(Quaternion desiredRotation, bool isLocked)
    {
        if (!isLocked)
        {
            smoothedYaw = desiredRotation.eulerAngles.y;
            isFirstFrame = true;
            return desiredRotation;
        }
        
        float desiredYaw = desiredRotation.eulerAngles.y;
        
        if (isFirstFrame)
        {
            smoothedYaw = desiredYaw;
            isFirstFrame = false;
            return desiredRotation;
        }
        
        float yawDelta = Mathf.DeltaAngle(smoothedYaw, desiredYaw);
        float maxYawChangePerFrame = 180f * Time.deltaTime;
        yawDelta = Mathf.Clamp(yawDelta, -maxYawChangePerFrame, maxYawChangePerFrame);
        
        smoothedYaw = Mathf.Repeat(smoothedYaw + yawDelta, 360f);
        
        Vector3 eulerAngles = desiredRotation.eulerAngles;
        eulerAngles.y = smoothedYaw;
        return Quaternion.Euler(eulerAngles);
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
    
    private void OnGUI()
    {
        if (!showDebugInfo || !Application.isPlaying)
            return;
        
        if (zTargeting != null && zTargeting.IsLocked && keepTargetInView && cachedCamera != null)
        {
            float minX = viewportSafeZone.x * Screen.width;
            float maxX = (1f - viewportSafeZone.x) * Screen.width;
            float minY = viewportSafeZone.y * Screen.height;
            float maxY = (1f - viewportSafeZone.y) * Screen.height;
            
            Color safeZoneColor = new Color(0f, 1f, 0f, 0.3f);
            DrawScreenRect(new Rect(minX, Screen.height - maxY, maxX - minX, maxY - minY), safeZoneColor);
            
            if (playerTarget != null)
            {
                Vector3 playerScreenPos = cachedCamera.WorldToScreenPoint(playerTarget.position + playerOffset);
                if (playerScreenPos.z > 0)
                {
                    DrawScreenCircle(new Vector2(playerScreenPos.x, Screen.height - playerScreenPos.y), 10f, Color.cyan);
                }
            }
            
            if (zTargeting.CurrentTarget != null)
            {
                Vector3 targetScreenPos = cachedCamera.WorldToScreenPoint(
                    zTargeting.CurrentTarget.position + Vector3.up * targetVerticalOffset);
                if (targetScreenPos.z > 0)
                {
                    DrawScreenCircle(new Vector2(targetScreenPos.x, Screen.height - targetScreenPos.y), 15f, Color.red);
                }
            }
        }
    }
    
    private void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
    
    private void DrawScreenCircle(Vector2 center, float radius, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
