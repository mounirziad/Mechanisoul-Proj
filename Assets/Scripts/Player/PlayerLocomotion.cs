using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    PlayerManager playerManager;
    InputManager inputManager;
    AnimatorManager animatorManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidbody;

    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public float rayCastHeightOffset = 0.5f;
    public LayerMask groundLayer;

    public float detectionradius = 0.2f;
    public float detectionheight = 4f;
    public float capsuleHeight = 1.8f;

    public bool isJumping;
    public bool isSprinting;
    public bool isGrounded;

    public float walkingSpeed = 1.5f;
    public float runningSpeed = 5;
    public float sprintingSpeed = 7;
    public float rotationSpeed = 15;

    public float jumpHeight = 3;
    public float gravityIntensity = -15;
    public Transform groundCheck;

    [SerializeField] private float stepHeight = 0.3f;  // max height you can step up
    [SerializeField] private float stepSmooth = 0.1f;  // smooth movement up
    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();

        if (playerManager.isInteracting)
        {
            return;
        }
        HandleMovement();
        HandleRotation();
        HandleSteps();
    }

    private void HandleMovement()
    {
        if (isJumping) { return; }

        // Calculate input-based direction
        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection += cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        // Apply speed depending on state
        if (isSprinting)
        {
            moveDirection *= sprintingSpeed;
        }
        else if (inputManager.moveAmount >= 0.5f)
        {
            moveDirection *= runningSpeed;
        }
        else
        {
            moveDirection *= walkingSpeed;
        }

        // Preserve current Y velocity (gravity, jumps, falls)
        Vector3 currentVelocity = playerRigidbody.linearVelocity;
        Vector3 targetVelocity = new Vector3(moveDirection.x, currentVelocity.y, moveDirection.z);

        playerRigidbody.linearVelocity = targetVelocity;
    }

    private void HandleRotation()
    {
        if(isJumping) { return; }

        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if(targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;

    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        rayCastOrigin.y += rayCastHeightOffset;

        if (!isGrounded && !isJumping)
        {
            if (!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling", false);
            }

            // Let Unity gravity do the work � no AddForce needed
        }

        // Ground check using a CapsuleCast
        Vector3 capsuleBottom = groundCheck.position;
        Vector3 capsuleTop = capsuleBottom + Vector3.up * capsuleHeight;
        float capsuleRadius = detectionradius;

        if (Physics.CapsuleCast(capsuleTop, capsuleBottom, capsuleRadius, Vector3.down, out hit, detectionheight, groundLayer))
        {
            if (!isGrounded && !playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Land", true);
            }

            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Visualize the capsule used for ground detection
            Vector3 capsuleBottom = groundCheck != null ? groundCheck.position : transform.position;
            Vector3 capsuleTop = capsuleBottom + Vector3.up * capsuleHeight;
            float capsuleRadius = detectionradius;

            Gizmos.color = isGrounded ? Color.green : Color.red;

            // Draw the capsule
            DrawCapsule(capsuleBottom, capsuleTop, capsuleRadius);

            // Draw the detection distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(capsuleBottom, capsuleBottom + Vector3.down * detectionheight);
            Gizmos.DrawLine(capsuleTop, capsuleTop + Vector3.down * detectionheight);
        }
    }

    // Helper method to draw a capsule
    private void DrawCapsule(Vector3 bottom, Vector3 top, float radius)
    {
        // Draw side lines
        Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
        Gizmos.DrawLine(bottom - Vector3.right * radius, top - Vector3.right * radius);
        Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
        Gizmos.DrawLine(bottom - Vector3.forward * radius, top - Vector3.forward * radius);

        // Draw bottom hemisphere
        DrawHemisphere(bottom, Vector3.up, radius);
        DrawHemisphere(bottom, Vector3.down, radius);

        // Draw top hemisphere
        DrawHemisphere(top, Vector3.up, radius);
        DrawHemisphere(top, Vector3.down, radius);
    }

    // Helper method to draw a hemisphere
    private void DrawHemisphere(Vector3 center, Vector3 direction, float radius)
    {
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * radius;
        if (perpendicular.magnitude == 0) perpendicular = Vector3.Cross(direction, Vector3.up).normalized * radius;

        int segments = 12;
        float angleIncrement = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(i * angleIncrement, direction);
            Vector3 start = center + rotation * perpendicular;

            Quaternion nextRotation = Quaternion.AngleAxis((i + 1) * angleIncrement, direction);
            Vector3 end = center + nextRotation * perpendicular;

            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(start, center + direction * radius);
        }
    }

    public void HandleJumping()
    {
        if(isGrounded)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);

            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidbody.linearVelocity = playerVelocity;
        }
    }

    private void HandleSteps()
    {
        // Only try to climb steps if the player is moving
        Vector3 horizontalVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        if (horizontalVelocity.magnitude < 0.1f)
            return;

        RaycastHit hitLower;
        Vector3 originLower = transform.position + Vector3.up * 0.1f; // just above ground
        if (Physics.Raycast(originLower, transform.forward, out hitLower, 0.5f, groundLayer))
        {
            // Now check above, at step height
            RaycastHit hitUpper;
            Vector3 originUpper = transform.position + Vector3.up * stepHeight;
            if (!Physics.Raycast(originUpper, transform.forward, out hitUpper, 0.5f, groundLayer))
            {
                // Smoothly move up
                playerRigidbody.position += new Vector3(0, stepSmooth, 0);
            }
        }
    }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
