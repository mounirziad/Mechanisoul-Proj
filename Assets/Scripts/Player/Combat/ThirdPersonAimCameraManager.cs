using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonAimCameraManager : MonoBehaviour
{
    [Header("Camera References")]
    public CinemachineCamera thirdPersonAimCamera;
    public CinemachineCamera freeLookCamera;
    
    [Header("Camera Priorities")]
    public int aimCameraPriority = 15;
    public int freeLookCameraPriority = 10;
    public int inactivePriority = 0;
    
    [Header("Settings")]
    public float cameraTransitionSpeed = 2f;
    
    private PlayerCombat playerCombat;
    private InputManager inputManager;
    private CinemachineThirdPersonFollow thirdPersonFollow;
    private CinemachineThirdPersonAim thirdPersonAim;
    
    // Store original settings to restore them
    private Vector3 originalShoulderOffset;
    private float originalCameraDistance;
    
    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        inputManager = GetComponent<InputManager>();
        
        // Find cameras if not assigned
        if (thirdPersonAimCamera == null)
        {
            GameObject aimCamObj = GameObject.Find("Third Person Aim Camera");
            if (aimCamObj != null)
                thirdPersonAimCamera = aimCamObj.GetComponent<CinemachineCamera>();
        }
        
        if (freeLookCamera == null)
        {
            GameObject freeLookCamObj = GameObject.Find("FreeLook Camera");
            if (freeLookCamObj != null)
                freeLookCamera = freeLookCamObj.GetComponent<CinemachineCamera>();
        }
        
        // Get the Third Person Follow and Aim components
        if (thirdPersonAimCamera != null)
        {
            thirdPersonFollow = thirdPersonAimCamera.GetComponent<CinemachineThirdPersonFollow>();
            thirdPersonAim = thirdPersonAimCamera.GetComponent<CinemachineThirdPersonAim>();
            
            // Store original settings
            if (thirdPersonFollow != null)
            {
                originalShoulderOffset = thirdPersonFollow.ShoulderOffset;
                originalCameraDistance = thirdPersonFollow.CameraDistance;
            }
        }
    }
    
    private void Start()
    {
        // Configure the Third Person Follow component for stable aiming
        if (thirdPersonFollow != null)
        {
            // Set recommended damping values for smooth movement
            thirdPersonFollow.Damping = new Vector3(0.1f, 0.5f, 0.3f);
            
            // Configure collision avoidance settings
            if (thirdPersonFollow.AvoidObstacles.Enabled)
            {
                // Set collision damping for smooth collision handling
                thirdPersonFollow.AvoidObstacles.DampingIntoCollision = 0.5f;
                thirdPersonFollow.AvoidObstacles.DampingFromCollision = 2f;
                thirdPersonFollow.AvoidObstacles.CameraRadius = 0.2f;
            }
        }
        
        // Configure the CinemachineCamera tracking target
        if (thirdPersonAimCamera != null)
        {
            thirdPersonAimCamera.Follow = transform;
            // Optional: Set look at target if needed
            // thirdPersonAimCamera.LookAt = transform;
            
            // Ensure the camera has input and rotation components for mouse control
            SetupCameraInputComponents();
        }
        
        // Configure the Third Person Aim component
        if (thirdPersonAim != null)
        {
            // Configure aim detection
            thirdPersonAim.AimCollisionFilter = ~LayerMask.GetMask("Player"); // Ignore player layer
            thirdPersonAim.IgnoreTag = "Player";
            thirdPersonAim.AimDistance = 100f;
            thirdPersonAim.NoiseCancellation = true;
        }
        
        // Start with FreeLook camera active
        SetCameraMode(false);
    }
    
    private void SetupCameraInputComponents()
    {
        if (thirdPersonAimCamera == null) return;
        
        // Add CinemachineInputAxisController if it doesn't exist
        var inputController = thirdPersonAimCamera.GetComponent<Unity.Cinemachine.CinemachineInputAxisController>();
        if (inputController == null)
        {
            inputController = thirdPersonAimCamera.gameObject.AddComponent<Unity.Cinemachine.CinemachineInputAxisController>();
            
            // Configure for Input System auto-enabling
            inputController.AutoEnableInputs = true;
            inputController.PlayerIndex = -1; // Single player
        }
        
        // Add CinemachinePanTilt if it doesn't exist
        var panTilt = thirdPersonAimCamera.GetComponent<Unity.Cinemachine.CinemachinePanTilt>();
        if (panTilt == null)
        {
            panTilt = thirdPersonAimCamera.gameObject.AddComponent<Unity.Cinemachine.CinemachinePanTilt>();
            
            // Configure pan/tilt settings for aiming
            panTilt.PanAxis = new Unity.Cinemachine.InputAxis()
            {
                Range = new Vector2(-180f, 180f),
                Wrap = true,
                Center = 0f,
                Value = 0f
            };
            
            panTilt.TiltAxis = new Unity.Cinemachine.InputAxis()
            {
                Range = new Vector2(-60f, 60f),
                Wrap = false,
                Center = 0f,
                Value = 0f
            };
            
            // Set reference frame to tracking target for third person
            panTilt.ReferenceFrame = Unity.Cinemachine.CinemachinePanTilt.ReferenceFrames.TrackingTarget;
        }
        
        Debug.Log("Third Person Aim Camera input components configured");
        Debug.Log("MANUAL SETUP REQUIRED: You need to assign Input Action References to the PanTilt component in the Inspector.");
        Debug.Log("Create Input Action References for your Camera action and assign them to Pan Axis and Tilt Axis in CinemachinePanTilt component.");
    }
    
    private void Update()
    {
        if (playerCombat == null || inputManager == null) return;
        
        // Check if lock-on system is active - it should take priority over aiming camera
        LockOnSystem lockOnSystem = GetComponent<LockOnSystem>();
        if (lockOnSystem != null && lockOnSystem.IsLocked())
        {
            // Let lock-on camera handle everything, disable aim camera
            SetCameraMode(false);
            return;
        }
        
        // Check if player is aiming
        bool isAiming = inputManager.aimInput;
        
        // Switch camera based on aiming state
        SetCameraMode(isAiming);
    }
    
    private void SetCameraMode(bool isAiming)
    {
        if (thirdPersonAimCamera == null || freeLookCamera == null) return;
        
        if (isAiming)
        {
            // Activate Third Person Aim Camera
            thirdPersonAimCamera.Priority = aimCameraPriority;
            freeLookCamera.Priority = inactivePriority;
        }
        else
        {
            // Activate FreeLook Camera
            thirdPersonAimCamera.Priority = inactivePriority;
            freeLookCamera.Priority = freeLookCameraPriority;
        }
    }
    
    // Method to adjust camera settings during runtime if needed
    public void SetAimCameraDistance(float distance)
    {
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.CameraDistance = distance;
        }
    }
    
    public void SetAimCameraOffset(Vector3 offset)
    {
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.ShoulderOffset = offset;
        }
    }
    
    // Reset to original settings
    public void ResetToOriginalSettings()
    {
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.ShoulderOffset = originalShoulderOffset;
            thirdPersonFollow.CameraDistance = originalCameraDistance;
        }
    }
    
    // Get the current aim target position for use in other scripts
    public Vector3 GetAimTarget()
    {
        if (thirdPersonAim != null)
        {
            return thirdPersonAim.AimTarget;
        }
        return Vector3.zero;
    }

    // Get the current camera aim direction (where the camera is looking)
    public Vector3 GetAimDirection()
    {
        if (thirdPersonAimCamera != null && thirdPersonAimCamera.Priority > 10)
        {
            // Return the full camera forward direction including vertical component
            return thirdPersonAimCamera.transform.forward;
        }
        return Vector3.zero;
    }

    // Check if the aim camera is currently active
    public bool IsAimCameraActive()
    {
        return thirdPersonAimCamera != null && thirdPersonAimCamera.Priority > 10;
    }


}