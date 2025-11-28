using UnityEngine;

public class CustomCameraController : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private FreeLookCamera freeLookCamera;
    [SerializeField] private AimCamera aimCamera;
    [SerializeField] private LockOnCamera lockOnCamera;
    [SerializeField] private ZTargetingCamera zTargetingCamera;

    [Header("Camera Priorities")]
    [SerializeField] private int freeLookPriority = 10;
    [SerializeField] private int aimPriority = 20;
    [SerializeField] private int lockOnPriority = 30;
    [SerializeField] private int inactivePriority = 0;

    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 8f;

    private InputManager inputManager;
    private LockOnSystem lockOnSystem;
    private ZTargetingSystem zTargetingSystem;
    
    private Camera freeLookCameraComponent;
    private Camera aimCameraComponent;
    private Camera lockOnCameraComponent;
    private Camera zTargetingCameraComponent;
    
    private bool isAimMode = false;
    private bool isLockOnMode = false;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        lockOnSystem = GetComponent<LockOnSystem>();
        zTargetingSystem = GetComponent<ZTargetingSystem>();

        FindCamerasIfNeeded();
        CacheCameraComponents();
    }
    
    private void FindCamerasIfNeeded()
    {
        if (freeLookCamera == null)
        {
            freeLookCamera = FindAnyObjectByType<FreeLookCamera>();
        }

        if (aimCamera == null)
        {
            aimCamera = FindAnyObjectByType<AimCamera>();
        }

        if (lockOnCamera == null)
        {
            lockOnCamera = FindAnyObjectByType<LockOnCamera>();
        }

        if (zTargetingCamera == null)
        {
            zTargetingCamera = FindAnyObjectByType<ZTargetingCamera>();
        }
    }
    
    private void CacheCameraComponents()
    {
        if (freeLookCamera != null)
        {
            freeLookCameraComponent = freeLookCamera.GetComponent<Camera>();
        }

        if (aimCamera != null)
        {
            aimCameraComponent = aimCamera.GetComponent<Camera>();
        }

        if (lockOnCamera != null)
        {
            lockOnCameraComponent = lockOnCamera.GetComponent<Camera>();
        }

        if (zTargetingCamera != null)
        {
            zTargetingCameraComponent = zTargetingCamera.GetComponent<Camera>();
        }
    }
    
    public void RefreshCameraReferences()
    {
        FindCamerasIfNeeded();
        CacheCameraComponents();
        
        Transform playerTransform = transform;
        
        if (freeLookCamera != null)
        {
            freeLookCamera.SetTarget(playerTransform);
        }
        
        if (aimCamera != null)
        {
            aimCamera.SetTarget(playerTransform);
        }
        
        if (lockOnCamera != null)
        {
            lockOnCamera.SetTarget(playerTransform);
        }
        
        if (zTargetingCamera != null)
        {
            zTargetingCamera.SetTarget(playerTransform);
        }
        
        Debug.Log("CustomCameraController: Camera references refreshed.");
    }

    private void Start()
    {
        SetCameraMode(CameraMode.FreeLook);
    }

    private void Update()
    {
        if (inputManager == null)
        {
            return;
        }

        bool isDialogueActive = DialogueSystem.Instance != null && DialogueSystem.Instance.IsDisplaying;
        
        if (isDialogueActive)
        {
            if (isAimMode || isLockOnMode)
            {
                SetCameraMode(CameraMode.FreeLook);
            }
            return;
        }

        // Check new Z-Targeting system first, fallback to old system
        bool lockActive = (zTargetingSystem != null && zTargetingSystem.IsLocked) ||
                         (lockOnSystem != null && lockOnSystem.IsLockedOn());
        bool shouldAim = inputManager.aimInput;

        if (lockActive)
        {
            if (!isLockOnMode)
            {
                SetCameraMode(CameraMode.LockOn);
            }
        }
        else if (shouldAim)
        {
            if (!isAimMode)
            {
                SetCameraMode(CameraMode.Aim);
            }
        }
        else
        {
            if (isAimMode || isLockOnMode)
            {
                SetCameraMode(CameraMode.FreeLook);
            }
        }
    }

    private enum CameraMode
    {
        FreeLook,
        Aim,
        LockOn
    }

    private void SetCameraMode(CameraMode mode)
    {
        isAimMode = (mode == CameraMode.Aim);
        isLockOnMode = (mode == CameraMode.LockOn);

        if (freeLookCameraComponent != null)
        {
            freeLookCameraComponent.depth = (mode == CameraMode.FreeLook) ? freeLookPriority : inactivePriority;
        }

        if (aimCameraComponent != null)
        {
            aimCameraComponent.depth = (mode == CameraMode.Aim) ? aimPriority : inactivePriority;
        }

        // Prefer ZTargetingCamera if it exists, otherwise use old LockOnCamera
        if (mode == CameraMode.LockOn)
        {
            if (zTargetingCameraComponent != null)
            {
                zTargetingCameraComponent.depth = lockOnPriority;
                if (lockOnCameraComponent != null && lockOnCameraComponent != zTargetingCameraComponent)
                {
                    lockOnCameraComponent.depth = inactivePriority;
                }
            }
            else if (lockOnCameraComponent != null)
            {
                lockOnCameraComponent.depth = lockOnPriority;
            }
        }
        else
        {
            if (zTargetingCameraComponent != null)
            {
                zTargetingCameraComponent.depth = inactivePriority;
            }
            if (lockOnCameraComponent != null && (zTargetingCameraComponent == null || lockOnCameraComponent != zTargetingCameraComponent))
            {
                lockOnCameraComponent.depth = inactivePriority;
            }
        }
    }

    public Vector3 GetAimTarget()
    {
        if (isLockOnMode)
        {
            // Try ZTargetingSystem first
            if (zTargetingSystem != null && zTargetingSystem.CurrentTarget != null)
            {
                return zTargetingSystem.CurrentTarget.position + Vector3.up * 0.5f;
            }
            
            // Fallback to old LockOnCamera
            if (lockOnCamera != null)
            {
                Transform lockTarget = lockOnCamera.GetCurrentLockTarget();
                if (lockTarget != null)
                {
                    return lockTarget.position + Vector3.up * 0.5f;
                }
            }
        }

        if (aimCamera != null && isAimMode)
        {
            return aimCamera.GetAimTarget();
        }

        return Vector3.zero;
    }

    public Vector3 GetAimDirection()
    {
        if (isLockOnMode)
        {
            // Use ZTargetingCamera if available
            if (zTargetingCamera != null)
            {
                return zTargetingCamera.transform.forward;
            }
            
            // Fallback to old LockOnCamera
            if (lockOnCamera != null)
            {
                return lockOnCamera.GetCameraForward();
            }
        }

        if (aimCamera != null && isAimMode)
        {
            return aimCamera.GetAimDirection();
        }
        else if (freeLookCamera != null)
        {
            return freeLookCamera.transform.forward;
        }

        return Vector3.forward;
    }

    public bool IsAimCameraActive()
    {
        return isAimMode;
    }

    public bool IsLockOnCameraActive()
    {
        return isLockOnMode;
    }

    public Transform GetAimTargetTransform()
    {
        if (isLockOnMode)
        {
            // Try ZTargetingSystem first
            if (zTargetingSystem != null && zTargetingSystem.CurrentTarget != null)
            {
                return zTargetingSystem.CurrentTarget;
            }
            
            // Fallback to old LockOnCamera
            if (lockOnCamera != null)
            {
                return lockOnCamera.GetCurrentLockTarget();
            }
        }

        if (aimCamera != null)
        {
            return aimCamera.GetAimTargetTransform();
        }

        return null;
    }

    public LockOnCamera GetLockOnCamera()
    {
        return lockOnCamera;
    }

    public ZTargetingCamera GetZTargetingCamera()
    {
        return zTargetingCamera;
    }

    public FreeLookCamera GetFreeLookCamera()
    {
        return freeLookCamera;
    }

    public AimCamera GetAimCamera()
    {
        return aimCamera;
    }
}

