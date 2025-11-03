using UnityEngine;
using Unity.Cinemachine;

public class LockOnCameraActivator : MonoBehaviour
{
    public LockOnSystem lockOnSystem;
    private CinemachineCamera cinemachineCamera;


    [Header("Camera Priorities")]
    public int activePriority = 20;
    public int inactivePriority = 5;
    private void Start()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();

        if (lockOnSystem == null)
        {
            Debug.LogError("LockOnSystem reference not assigned!");
            return;
        }

        // Start with Cinemachine camera component disabled but GameObject active
        if (cinemachineCamera != null)
        {
        }
        else
        {
            Debug.LogError("No CinemachineCamera component found!");
        }
    }

    private void Update()
    {
        if (lockOnSystem != null && cinemachineCamera != null)
        {
            int targetPriority = lockOnSystem.IsLocked() ? activePriority : inactivePriority;

            if (cinemachineCamera.Priority != targetPriority)
            {
                // Debug.Log($"Setting lock-on cam priority to: {targetPriority}");
                cinemachineCamera.Priority = targetPriority;
            }
        }
    }
}