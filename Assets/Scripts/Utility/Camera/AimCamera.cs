using UnityEngine;
using UnityEngine.InputSystem;

public class AimCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Transform aimTarget;

    [Header("Input Settings")]
    [SerializeField] private InputManager playerInputManager;
    [SerializeField] private bool useMouseDelta = true;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gamepadSensitivity = 2500f;

    [Header("Camera Position")]
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.6f, 0f, 0f);
    [SerializeField] private float cameraDistance = 3f;
    [SerializeField] private float minDistance = 1f;

    [Header("Rotation Settings")]
    [SerializeField] private float horizontalSensitivity = 2f;
    [SerializeField] private float verticalSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -60f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private bool invertY = false;

    [Header("Aim Detection")]
    [SerializeField] private LayerMask aimLayers = -1;
    [SerializeField] private float aimDistance = 100f;
    [SerializeField] private bool ignorePlayerLayer = true;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 15f;
    [SerializeField] private float rotationSmoothing = 15f;
    [SerializeField] private float aimTargetSmoothing = 15f;

    [Header("Collision")]
    [SerializeField] private CameraCollisionHandler collisionHandler;
    [SerializeField] private bool enableFinalSafetyCheck = false;

    private float horizontalAngle;
    private float verticalAngle;

    private Vector2 lookInput;

    private Vector3 desiredPosition;
    private Quaternion desiredRotation;
    private Vector3 currentAimPoint;
    private Vector3 instantAimPoint;
    private float currentShoulderOffsetScale = 1f;

    private void Awake()
    {
        if (collisionHandler == null)
        {
            collisionHandler = GetComponent<CameraCollisionHandler>();
        }

        if (playerInputManager == null && target != null)
        {
            playerInputManager = target.GetComponent<InputManager>();
        }

        if (aimTarget == null)
        {
            GameObject aimObj = new GameObject("AimTarget");
            aimTarget = aimObj.transform;
            aimTarget.SetParent(transform);
        }
    }

    private void Start()
    {
        FindPlayerIfNeeded();

        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            horizontalAngle = angles.y;
            verticalAngle = angles.x;
        }

        if (playerInputManager == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInputManager = player.GetComponent<InputManager>();
            }
        }

        currentAimPoint = transform.position + transform.forward * aimDistance;
        currentShoulderOffsetScale = 1f;

        if (ignorePlayerLayer)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                aimLayers &= ~(1 << playerLayer);
            }
        }

        if (collisionHandler != null)
        {
            collisionHandler.InitializeDistance(cameraDistance);
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"AimCamera automatically found player: {player.name}");
            }
        }
    }

    private void ReadInput()
    {
        if (playerInputManager == null || playerInputManager.playerControls == null)
        {
            return;
        }

        if (useMouseDelta)
        {
            lookInput = playerInputManager.playerControls.PlayerMovement.Look.ReadValue<Vector2>();

            if (Gamepad.current != null && playerInputManager.playerControls.PlayerMovement.Look.activeControl?.device is Gamepad)
            {
                lookInput *= gamepadSensitivity * Time.deltaTime;
            }
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
        UpdateRotation();
        CalculateDesiredPosition();
        HandleCollision();
        UpdateAimTarget();
        ApplySmoothing();
    }

    private void UpdateRotation()
    {
        float hSensitivity = useMouseDelta ? mouseSensitivity : horizontalSensitivity;
        float vSensitivity = useMouseDelta ? mouseSensitivity : verticalSensitivity;

        horizontalAngle += lookInput.x * hSensitivity;

        float verticalChange = lookInput.y * vSensitivity;
        if (invertY)
        {
            verticalChange = -verticalChange;
        }

        verticalAngle -= verticalChange;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

        desiredRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
    }

    private void CalculateDesiredPosition()
    {
        Vector3 targetPoint = target.position + targetOffset;

        Vector3 localOffset = desiredRotation * shoulderOffset;
        float targetOffsetScale = CheckShoulderOffsetScale(targetPoint, localOffset);

        currentShoulderOffsetScale = Mathf.Lerp(currentShoulderOffsetScale, targetOffsetScale, 10f * Time.deltaTime);

        localOffset *= currentShoulderOffsetScale;
        Vector3 offsetTargetPoint = targetPoint + localOffset;

        Vector3 direction = desiredRotation * Vector3.back;
        desiredPosition = offsetTargetPoint + direction * cameraDistance;
    }

    private void HandleCollision()
    {
        if (collisionHandler != null)
        {
            Vector3 targetPoint = target.position + targetOffset;
            Vector3 localOffset = desiredRotation * shoulderOffset * currentShoulderOffsetScale;
            Vector3 offsetTargetPoint = targetPoint + localOffset;

            float adjustedDistance = Mathf.Max(cameraDistance, minDistance);
            desiredPosition = collisionHandler.HandleCollision(offsetTargetPoint, desiredPosition, adjustedDistance);
        }
    }

    private float CheckShoulderOffsetScale(Vector3 targetPoint, Vector3 localOffset)
    {
        float offsetDistance = localOffset.magnitude;
        if (offsetDistance < 0.001f)
        {
            return 1f;
        }

        Vector3 offsetDirection = localOffset.normalized;

        if (collisionHandler != null)
        {
            LayerMask layers = collisionHandler.GetCollisionLayers();
            float checkRadius = 0.15f;

            if (Physics.SphereCast(targetPoint, checkRadius, offsetDirection, out RaycastHit hit, offsetDistance, layers, QueryTriggerInteraction.Ignore))
            {
                float safeDistance = Mathf.Max(0f, hit.distance - 0.1f);
                return Mathf.Clamp01(safeDistance / offsetDistance);
            }
        }

        return 1f;
    }

    private void UpdateAimTarget()
    {
        Vector3 aimPoint;

        // Use desiredRotation instead of transform.forward for consistency
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = desiredRotation * Vector3.forward;

        Ray aimRay = new Ray(rayOrigin, rayDirection);

        if (Physics.Raycast(aimRay, out RaycastHit hit, aimDistance, aimLayers, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }
        else
        {
            aimPoint = aimRay.origin + aimRay.direction * aimDistance;
        }

        instantAimPoint = aimPoint;
        currentAimPoint = Vector3.Lerp(currentAimPoint, aimPoint, aimTargetSmoothing * Time.deltaTime);

        if (aimTarget != null)
        {
            aimTarget.position = currentAimPoint;
        }
    }

    private void ApplySmoothing()
    {
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, positionSmoothing * Time.deltaTime);

        if (enableFinalSafetyCheck)
        {
            smoothedPosition = FinalPositionCheck(smoothedPosition);
        }

        transform.position = smoothedPosition;
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothing * Time.deltaTime);
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
            position = position + directionToTarget * 0.1f;
        }

        return position;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (playerInputManager == null && target != null)
        {
            playerInputManager = target.GetComponent<InputManager>();
        }
    }

    public Vector3 GetAimTarget()
    {
        return instantAimPoint;
    }

    public Vector3 GetSmoothedAimTarget()
    {
        return currentAimPoint;
    }

    public Vector3 GetAimDirection()
    {
        return transform.forward;
    }

    public Transform GetAimTargetTransform()
    {
        return aimTarget;
    }

    public void SetSensitivity(float multiplier)
    {
        mouseSensitivity = 0.1f * multiplier;
        gamepadSensitivity = 2500f * multiplier;
    }


    private void OnDrawGizmosSelected()
    {
        if (target == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Vector3 targetPoint = target.position + targetOffset;
        Gizmos.DrawWireSphere(targetPoint, 0.2f);

        if (Application.isPlaying && desiredRotation != Quaternion.identity)
        {
            Vector3 localOffset = desiredRotation * shoulderOffset;
            Vector3 offsetTargetPoint = targetPoint + localOffset;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(targetPoint, offsetTargetPoint);
            Gizmos.DrawWireSphere(offsetTargetPoint, 0.15f);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(targetPoint, transform.position);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(targetPoint, currentAimPoint);
        Gizmos.DrawWireSphere(currentAimPoint, 0.3f);
    }
}
