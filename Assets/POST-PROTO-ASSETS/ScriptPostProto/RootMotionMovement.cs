using UnityEngine;
using UnityEngine.InputSystem;

public class RootMotionMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;
    public float rotationSmoothness = 0.1f; // How smooth the rotation is
    
    [Header("Root Motion Settings")]
    public bool useRootMotionWhenAvailable = true;
    public float rootMotionThreshold = 0.1f; // Minimum movement to consider root motion active
    
    [Header("Camera Settings")]
    public Transform cameraTransform; // Reference to main camera
    public bool useCameraRelativeMovement = true; // Move relative to camera direction
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    Animator animator;
    CharacterController characterController;

    int isWalkingHash;
    int isRunningHash;

    PlayerControls input;

    Vector2 currentMovement;
    bool movementPressed;
    bool runPressed;
    
    // Root motion tracking
    Vector3 rootMotionPositionDelta;
    Quaternion rootMotionRotationDelta;
    bool hasRootMotion;
    
    // Smooth movement tracking
    Vector3 currentVelocity;
    Vector3 targetDirection;

    void HandleRotation()
    {
        if (currentMovement == Vector2.zero) return;

        Vector3 inputDirection = GetMovementDirection();
        
        if (inputDirection.magnitude > 0.1f)
        {
            // Calculate target rotation based on movement direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            
            // Smooth rotation towards target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    Vector3 GetMovementDirection()
    {
        Vector3 inputDirection = Vector3.zero;
        
        if (useCameraRelativeMovement && cameraTransform != null)
        {
            // Get camera forward and right vectors (removing Y component for ground movement)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Calculate movement direction relative to camera
            inputDirection = cameraForward * currentMovement.y + cameraRight * currentMovement.x;
        }
        else
        {
            // World-space movement (Z = forward, X = right)
            inputDirection = new Vector3(currentMovement.x, 0f, currentMovement.y);
        }
        
        return inputDirection.normalized;
    }

    void HandleScriptBasedMovement()
    {
        if (currentMovement == Vector2.zero)
        {
            currentVelocity = Vector3.zero;
            return;
        }

        Vector3 moveDirection = GetMovementDirection();
        float currentSpeed = runPressed ? runSpeed : walkSpeed;
        
        // Smooth acceleration/deceleration
        Vector3 targetVelocity = moveDirection * currentSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * 10f);

        Vector3 movement = currentVelocity * Time.deltaTime;

        if (characterController != null)
        {
            // Add gravity if using CharacterController
            if (!characterController.isGrounded)
            {
                currentVelocity.y += Physics.gravity.y * Time.deltaTime;
            }
            else
            {
                currentVelocity.y = 0f;
            }
            
            Vector3 finalMovement = new Vector3(movement.x, currentVelocity.y * Time.deltaTime, movement.z);
            characterController.Move(finalMovement);
        }
        else
        {
            transform.Translate(movement, Space.World);
        }
    }

    private void Awake()
    {
        input = new PlayerControls();
        input.PlayerMovement.Movement.performed += ctx =>
        {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.magnitude > 0.1f;
        };
        input.PlayerMovement.Movement.canceled += ctx =>
        {
            currentMovement = Vector2.zero;
            movementPressed = false;
        };
        input.PlayerMovement.Run.performed += ctx => runPressed = true;
        input.PlayerMovement.Run.canceled += ctx => runPressed = false;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        isWalkingHash = Animator.StringToHash("IsWalking");
        isRunningHash = Animator.StringToHash("IsRunning");
        
        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }
        
        // Ensure root motion is enabled on the animator
        if (animator != null)
        {
            animator.applyRootMotion = useRootMotionWhenAvailable;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        
        if (showDebugInfo)
        {
            Debug.Log($"Input: {currentMovement} | Has Root Motion: {hasRootMotion} | Direction: {GetMovementDirection()}");
        }
    }

    void HandleMovement()
    {
        bool isRunning = animator.GetBool(isRunningHash);
        bool isWalking = animator.GetBool(isWalkingHash);

        if(movementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }

        if (!movementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((movementPressed && runPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }

        if((!movementPressed || !runPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
        
        // Apply movement only if root motion is not providing sufficient movement
        if (!hasRootMotion && movementPressed)
        {
            HandleScriptBasedMovement();
        }
    }

    // Called by Unity's animation system
    void OnAnimatorMove()
    {
        if (animator == null) return;

        // Get root motion data from animator
        rootMotionPositionDelta = animator.deltaPosition;
        rootMotionRotationDelta = animator.deltaRotation;
        
        // Check if the animation provides meaningful root motion
        hasRootMotion = rootMotionPositionDelta.magnitude > rootMotionThreshold;
        
        if (useRootMotionWhenAvailable && hasRootMotion)
        {
            // Use root motion for movement
            if (characterController != null)
            {
                characterController.Move(rootMotionPositionDelta);
            }
            else
            {
                transform.position += rootMotionPositionDelta;
            }
            
            // Apply root motion rotation if desired
            transform.rotation *= rootMotionRotationDelta;
        }
        // If no significant root motion, let the script handle movement in Update()
    }

    private void OnEnable()
    {
        input.PlayerMovement.Enable();
    }

    private void OnDisable()
    {
        input.PlayerMovement.Disable();
    }
    
    // Helper method to visualize movement direction in Scene view
    void OnDrawGizmos()
    {
        if (Application.isPlaying && currentMovement != Vector2.zero)
        {
            Vector3 direction = GetMovementDirection();
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, direction * 2f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
    }
}
