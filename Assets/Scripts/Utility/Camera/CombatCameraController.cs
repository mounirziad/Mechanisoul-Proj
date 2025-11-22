using UnityEngine;
using Unity.Cinemachine;

public class CombatCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The Cinemachine FreeLook camera to control (for Cinemachine setup)")]
    public CinemachineCamera cinemachineFreeLookCamera;
    
    [Tooltip("The custom FreeLook camera to control (for custom setup)")]
    public Camera customFreeLookCamera;
    
    [Header("FOV Settings")]
    [Tooltip("The default FOV value when not in combat")]
    public float defaultFOV = 60f;
    
    [Tooltip("The combat FOV value when inside the room trigger")]
    public float combatFOV = 73.9f;
    
    [Header("Transition Settings")]
    [Tooltip("How fast the FOV transitions (higher = faster)")]
    public float fovTransitionSpeed = 2f;
    
    private float targetFOV;
    private bool isInCombatZone = false;
    public bool IsInCombatZone => isInCombatZone;

    private void Awake()
    {
        if (cinemachineFreeLookCamera == null && customFreeLookCamera == null)
        {
            cinemachineFreeLookCamera = GetComponent<CinemachineCamera>();
            if (cinemachineFreeLookCamera == null)
            {
                customFreeLookCamera = GetComponent<Camera>();
            }
        }
        
        if (cinemachineFreeLookCamera != null)
        {
            defaultFOV = cinemachineFreeLookCamera.Lens.FieldOfView;
            targetFOV = defaultFOV;
        }
        else if (customFreeLookCamera != null)
        {
            defaultFOV = customFreeLookCamera.fieldOfView;
            targetFOV = defaultFOV;
        }
    }

    private void LateUpdate()
    {
        if (cinemachineFreeLookCamera != null)
        {
            LensSettings lens = cinemachineFreeLookCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
            cinemachineFreeLookCamera.Lens = lens;
        }
        else if (customFreeLookCamera != null)
        {
            customFreeLookCamera.fieldOfView = Mathf.Lerp(customFreeLookCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
    }

    public void EnterCombatZone()
    {
        isInCombatZone = true;
        targetFOV = combatFOV;
    }

    public void ExitCombatZone()
    {
        isInCombatZone = false;
        targetFOV = defaultFOV;
    }

    public void SetCombatMode(bool inCombat)
    {
        if (inCombat)
        {
            EnterCombatZone();
        }
        else
        {
            ExitCombatZone();
        }
    }

    public float GetCurrentTargetFOV()
    {
        return targetFOV;
    }
}
