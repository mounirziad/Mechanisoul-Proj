using Unity.Cinemachine;
using UnityEngine;

public class CameraPriorityFixer : MonoBehaviour
{
    [Header("Camera References")]
    public CinemachineCamera freeLookCamera;
    public CinemachineCamera lockOnCamera;
    public CinemachineCamera aimCamera;

    [Header("Priority Settings")]
    public int freeLookDefaultPriority = 10;
    public int aimCameraPriority = 15;
    public int lockOnActivePriority = 20;
    public int inactivePriority = 0;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private LockOnSystem lockOnSystem;
    private CameraManagerAdapter cameraAdapter;
    private InputManager inputManager;
    private bool usingCustomCameraSystem = false;

    private void Awake()
    {
        lockOnSystem = GetComponent<LockOnSystem>();
        cameraAdapter = GetComponent<CameraManagerAdapter>();
        inputManager = GetComponent<InputManager>();

        if (cameraAdapter != null)
        {
            usingCustomCameraSystem = cameraAdapter.UsingCustomCameraSystem();
        }

        if (!usingCustomCameraSystem)
        {
            if (freeLookCamera == null)
            {
                GameObject freeLookObj = GameObject.Find("FreeLook Camera");
                if (freeLookObj != null)
                    freeLookCamera = freeLookObj.GetComponent<CinemachineCamera>();
            }

            if (lockOnCamera == null)
            {
                GameObject lockOnObj = GameObject.Find("LockOnCamera");
                if (lockOnObj != null)
                    lockOnCamera = lockOnObj.GetComponent<CinemachineCamera>();
            }

            if (aimCamera == null)
            {
                GameObject aimObj = GameObject.Find("AimCamera");
                if (aimObj != null)
                    aimCamera = aimObj.GetComponent<CinemachineCamera>();
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log("Custom camera system detected. CameraPriorityFixer will not manage Cinemachine priorities.");
            }
        }
    }

    private void LateUpdate()
    {
        if (usingCustomCameraSystem)
        {
            return;
        }

        bool isLockedOn = lockOnSystem != null && lockOnSystem.IsLocked();
        bool isAiming = inputManager != null && inputManager.aimInput;

        if (isLockedOn)
        {
            SetCameraPriorities(lockOnCamera, lockOnActivePriority, "LockOn");
        }
        else if (isAiming)
        {
            SetCameraPriorities(aimCamera, aimCameraPriority, "Aim");
        }
        else
        {
            SetCameraPriorities(freeLookCamera, freeLookDefaultPriority, "FreeLook");
        }
    }

    private void SetCameraPriorities(CinemachineCamera activeCamera, int activePriority, string activeCameraName)
    {
        if (freeLookCamera != null)
        {
            int targetPriority = (activeCamera == freeLookCamera) ? activePriority : inactivePriority;
            if (freeLookCamera.Priority != targetPriority)
            {
                freeLookCamera.Priority = targetPriority;
                if (showDebugLogs) Debug.Log($"FreeLook Camera priority set to: {targetPriority}");
            }
        }

        if (lockOnCamera != null)
        {
            int targetPriority = (activeCamera == lockOnCamera) ? activePriority : inactivePriority;
            if (lockOnCamera.Priority != targetPriority)
            {
                lockOnCamera.Priority = targetPriority;
                if (showDebugLogs) Debug.Log($"LockOn Camera priority set to: {targetPriority}");
            }
        }

        if (aimCamera != null)
        {
            int targetPriority = (activeCamera == aimCamera) ? activePriority : inactivePriority;
            if (aimCamera.Priority != targetPriority)
            {
                aimCamera.Priority = targetPriority;
                if (showDebugLogs) Debug.Log($"Aim Camera priority set to: {targetPriority}");
            }
        }

        if (showDebugLogs && activeCamera != null)
        {
            Debug.Log($"Active camera: {activeCameraName} (Priority: {activePriority})");
        }
    }
}
