using UnityEngine;

public class CustomCameraController : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private FreeLookCamera freeLookCamera;
    [SerializeField] private AimCamera aimCamera;
    [SerializeField] private LockOnCamera lockOnCamera;
    [SerializeField] private ZTargetingCamera zTargetingCamera;

    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 8f;

    private InputManager inputManager;
    private LockOnSystem lockOnSystem;
    private ZTargetingSystem zTargetingSystem;
    
    private bool isAimMode = false;
    private bool isLockOnMode = false;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        lockOnSystem = GetComponent<LockOnSystem>();
        zTargetingSystem = GetComponent<ZTargetingSystem>();

        if (freeLookCamera == null)
        {
            freeLookCamera = FindObjectOfType<FreeLookCamera>();
        }

        if (aimCamera == null)
        {
            aimCamera = FindObjectOfType<AimCamera>();
        }

        if (lockOnCamera == null)
        {
            lockOnCamera = FindObjectOfType<LockOnCamera>();
        }

        if (zTargetingCamera == null)
        {
            zTargetingCamera = FindObjectOfType<ZTargetingCamera>();
        }
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

        if (freeLookCamera != null)
        {
            freeLookCamera.gameObject.SetActive(mode == CameraMode.FreeLook);
        }

        if (aimCamera != null)
        {
            aimCamera.gameObject.SetActive(mode == CameraMode.Aim);
        }

        // Activate the appropriate lock-on camera
        // Prefer ZTargetingCamera if it exists, otherwise use old LockOnCamera
        if (mode == CameraMode.LockOn)
        {
            if (zTargetingCamera != null)
            {
                zTargetingCamera.gameObject.SetActive(true);
                if (lockOnCamera != null && lockOnCamera.gameObject != zTargetingCamera.gameObject)
                {
                    lockOnCamera.gameObject.SetActive(false);
                }
            }
            else if (lockOnCamera != null)
            {
                lockOnCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            // Disable both lock-on cameras when not in lock mode
            if (zTargetingCamera != null)
            {
                zTargetingCamera.gameObject.SetActive(false);
            }
            if (lockOnCamera != null && (zTargetingCamera == null || lockOnCamera.gameObject != zTargetingCamera.gameObject))
            {
                lockOnCamera.gameObject.SetActive(false);
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

