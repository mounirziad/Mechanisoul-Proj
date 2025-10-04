using UnityEngine;
using Unity.Cinemachine;

public class LockOnCameraActivator : MonoBehaviour
{
    public LockOnSystem lockOnSystem;
    private CinemachineCamera cinemachineCamera;

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
            cinemachineCamera.enabled = false;
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
            bool shouldBeActive = lockOnSystem.IsLocked();

            if (cinemachineCamera.enabled != shouldBeActive)
            {
                Debug.Log($"Setting Cinemachine camera enabled to: {shouldBeActive}");
                cinemachineCamera.enabled = shouldBeActive;
            }
        }
    }
}