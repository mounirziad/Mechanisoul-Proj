using UnityEngine;
using Unity.Cinemachine;

public class CombatCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The FreeLook camera to control")]
    public CinemachineCamera freeLookCamera;
    
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
        if (freeLookCamera == null)
        {
            freeLookCamera = GetComponent<CinemachineCamera>();
        }
        
        if (freeLookCamera != null)
        {
            defaultFOV = freeLookCamera.Lens.FieldOfView;
            targetFOV = defaultFOV;
        }
    }

    private void LateUpdate()
    {
        if (freeLookCamera != null && isInCombatZone)
        {
            LensSettings lens = freeLookCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
            freeLookCamera.Lens = lens;
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
