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
    }

    private void HandleMovement()
    {
        if (isJumping) { return; }

        moveDirection = cameraObject.forward * inputManager.verticalInput; 
        moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if(isSprinting )
        {
            moveDirection = moveDirection * sprintingSpeed;
        }
        else
        {
            if (inputManager.moveAmount >= 0.5f)
            {
                moveDirection = moveDirection * runningSpeed;
            }
            else
            {
                moveDirection = moveDirection * walkingSpeed;
            }
        }

        

            Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = movementVelocity; 
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
        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;

        if(!isGrounded && !isJumping)
        {
            if(!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling", false);   
            }

            inAirTimer = inAirTimer + Time.deltaTime;
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

        // Ground check using a CapsuleCast
        Vector3 capsuleBottom = transform.position + Vector3.up * 0.2f; // just above the feet
        Vector3 capsuleTop = capsuleBottom + Vector3.up * capsuleHeight; // capsule "body"
        float capsuleRadius = detectionradius; // how wide your player is

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
            Vector3 capsuleBottom = transform.position + Vector3.up * 0.2f;
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
