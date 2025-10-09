using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [System.Obsolete]
    public CinemachineFreeLook freeLookCamera;

    [Header("Zoom Settings")]
    public float normalRadius = 5f;
    public float aimRadius = 2f;
    public float zoomSpeed = 5f;

    private bool isAiming = false;
    private PlayerCombat playerCombat;

    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();

        // If freeLookCamera is not assigned, try to find it
        if (freeLookCamera == null)
        {
            freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        }

        // Store the original radius if not set
        if (freeLookCamera != null && normalRadius <= 0)
        {
            normalRadius = freeLookCamera.m_Orbits[0].m_Radius;
        }
    }

    private void Update()
    {
        if (freeLookCamera == null) return;

        // Check if ThirdPersonAimCameraManager is handling camera switching
        ThirdPersonAimCameraManager aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager != null)
        {
            // Let the ThirdPersonAimCameraManager handle camera switching
            // Only apply zoom when FreeLook camera is active
            if (freeLookCamera.Priority > 5) // Only when FreeLook is the active camera
            {
                ApplyFreeLookZoom();
            }
        }
        else
        {
            // Fallback to original behavior if no ThirdPersonAimCameraManager
            ApplyFreeLookZoom();
        }
    }

    private void ApplyFreeLookZoom()
    {
        // Get aiming state from PlayerCombat
        if (playerCombat != null)
        {
            isAiming = playerCombat.isAiming;
        }

        // Smoothly transition between zoom levels
        float targetRadius = isAiming ? aimRadius : normalRadius;

        // Apply zoom to all three rigs (top, middle, bottom)
        for (int i = 0; i < 3; i++)
        {
            float currentRadius = freeLookCamera.m_Orbits[i].m_Radius;
            float newRadius = Mathf.Lerp(currentRadius, targetRadius, zoomSpeed * Time.deltaTime);

            // Update the orbit radius
            var orbit = freeLookCamera.m_Orbits[i];
            orbit.m_Radius = newRadius;
            freeLookCamera.m_Orbits[i] = orbit;
        }
    }

    // Public method to set zoom directly (optional)
    public void SetZoom(float radius, float speed = 5f)
    {
        aimRadius = radius;
        zoomSpeed = speed;
    }
}