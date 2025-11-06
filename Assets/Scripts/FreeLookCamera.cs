using UnityEngine;
using UnityEngine.InputSystem;

public class FreeLookCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Input Settings")]
    [SerializeField] private InputManager playerInputManager;
    [SerializeField] private bool useMouseDelta = true;
    [SerializeField] private float mouseSensitivity = 0.1f;

    [Header("Orbit Settings")]
    [SerializeField] private float orbitSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private bool invertY = false;

    [Header("Distance Settings")]
    [SerializeField] private float defaultDistance = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float zoomSpeed = 2f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 10f;
    [SerializeField] private float rotationSmoothing = 10f;

    [Header("Collision")]
    [SerializeField] private CameraCollisionHandler collisionHandler;

    private float currentDistance;
    private float horizontalAngle;
    private float verticalAngle;
    
    private Vector2 lookInput;
    private float zoomInput;

    private Vector3 desiredPosition;
    private Quaternion desiredRotation;

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
    }

    private void Start()
    {
        currentDistance = defaultDistance;
        
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
        
        if (collisionHandler != null)
        {
            collisionHandler.InitializeDistance(currentDistance);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        zoomInput = context.ReadValue<float>();
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
        }
        else
        {
            lookInput = playerInputManager.cameraInput;
        }

        zoomInput = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        ReadInput();
        UpdateAngles();
        UpdateDistance();
        CalculateDesiredPosition();
        HandleCollision();
        ApplySmoothing();
    }

    private void UpdateAngles()
    {
        float sensitivity = useMouseDelta ? mouseSensitivity : orbitSensitivity;
        
        horizontalAngle += lookInput.x * sensitivity;
        
        float verticalChange = lookInput.y * sensitivity;
        if (invertY)
        {
            verticalChange = -verticalChange;
        }
        
        verticalAngle -= verticalChange;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateDistance()
    {
        currentDistance -= zoomInput * zoomSpeed * 0.01f;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    private void CalculateDesiredPosition()
    {
        desiredRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
        
        Vector3 targetPoint = target.position + targetOffset;
        Vector3 direction = desiredRotation * Vector3.back;
        
        desiredPosition = targetPoint + direction * currentDistance;
    }

    private void HandleCollision()
    {
        if (collisionHandler != null)
        {
            Vector3 targetPoint = target.position + targetOffset;
            desiredPosition = collisionHandler.HandleCollision(targetPoint, desiredPosition, currentDistance);
        }
    }

    private void ApplySmoothing()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothing * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDistance(float distance)
    {
        currentDistance = Mathf.Clamp(distance, minDistance, maxDistance);
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
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(targetPoint, transform.position);
    }
}
