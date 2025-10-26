using UnityEngine;

public enum FootstepMode
{
    AnimationEvents,
    AutomaticTiming
}

public class FootstepController : MonoBehaviour
{
    [Header("Footstep Mode")]
    [SerializeField] private FootstepMode footstepMode = FootstepMode.AnimationEvents;
    [SerializeField] private bool enableDebugLogs = false;
    
    [Header("References")]
    private PlayerLocomotion playerLocomotion;
    private Rigidbody playerRigidbody;
    
    [Header("Automatic Timing Settings")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.35f;
    [SerializeField] private float sprintStepInterval = 0.25f;
    
    [Header("Movement Threshold")]
    [SerializeField] private float minimumMovementSpeed = 0.5f;
    
    private float stepTimer = 0f;
    private bool wasGroundedLastFrame = false;

    private void Awake()
    {
        playerLocomotion = GetComponent<PlayerLocomotion>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (playerLocomotion == null || playerRigidbody == null)
            return;
        
        if (footstepMode == FootstepMode.AutomaticTiming)
        {
            HandleFootsteps();
        }
        
        HandleLanding();
    }

    private void HandleFootsteps()
    {
        if (!playerLocomotion.isGrounded)
        {
            stepTimer = 0f;
            return;
        }
        
        Vector3 horizontalVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        
        if (currentSpeed < minimumMovementSpeed)
        {
            stepTimer = 0f;
            return;
        }
        
        float currentStepInterval = GetStepInterval(currentSpeed);
        
        stepTimer += Time.deltaTime;
        
        if (stepTimer >= currentStepInterval)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayFootstep(currentSpeed, playerLocomotion.isGrounded);
            }
            stepTimer = 0f;
        }
    }

    private void HandleLanding()
    {
        if (playerLocomotion.isGrounded && !wasGroundedLastFrame)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLandSound();
            }
        }
        
        wasGroundedLastFrame = playerLocomotion.isGrounded;
    }

    private float GetStepInterval(float speed)
    {
        if (speed >= 6.5f)
        {
            return sprintStepInterval;
        }
        else if (speed >= 4.5f)
        {
            return runStepInterval;
        }
        else
        {
            return walkStepInterval;
        }
    }
    
    public void OnFootstep()
    {
        if (playerLocomotion == null || playerRigidbody == null)
            return;
            
        Vector3 horizontalVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        
        if (enableDebugLogs)
        {
            Debug.Log($"Footstep triggered! Speed: {currentSpeed:F2}, Grounded: {playerLocomotion.isGrounded}");
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayFootstep(currentSpeed, playerLocomotion.isGrounded);
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("SoundManager.Instance is null!");
        }
    }
    
    public void OnFootLeft()
    {
        if (enableDebugLogs)
        {
            Debug.Log("Left foot animation event triggered");
        }
        OnFootstep();
    }
    
    public void OnFootRight()
    {
        if (enableDebugLogs)
        {
            Debug.Log("Right foot animation event triggered");
        }
        OnFootstep();
    }
}
