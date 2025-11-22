using UnityEngine;
using Unity.Cinemachine;

public class SmoothCameraCollision : CinemachineExtension
{
    [Header("Collision Settings")]
    [Tooltip("Layers that block the camera view")]
    public LayerMask collisionLayers = -1;
    
    [Tooltip("How close can the camera get to obstacles")]
    public float cameraRadius = 0.2f;
    
    [Tooltip("Minimum distance from follow target")]
    public float minDistanceFromTarget = 1f;
    
    [Header("Corner Detection")]
    [Tooltip("Additional sphere check at camera position to prevent clipping through corners")]
    public bool checkCameraPosition = true;
    
    [Tooltip("Extra padding when checking camera position")]
    public float cameraPadding = 0.3f;
    
    [Tooltip("Maximum attempts to find a safe position around corners")]
    public int maxRepositionAttempts = 3;
    
    [Header("Smoothing")]
    [Tooltip("How quickly camera moves when hitting obstacle (higher = faster)")]
    public float collisionDamping = 10f;
    
    [Tooltip("How quickly camera returns to normal position (higher = faster)")]
    public float returnDamping = 4f;
    
    [Header("Debug")]
    public bool showDebug = false;

    private Vector3 previousCameraPosition;

    protected override void Awake()
    {
        base.Awake();
        previousCameraPosition = Vector3.zero;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
            return;

        if (vcam.Follow == null)
            return;

        Vector3 followPosition = vcam.Follow.position;
        Vector3 desiredCameraPosition = state.RawPosition;
        Vector3 direction = desiredCameraPosition - followPosition;
        float desiredDistance = direction.magnitude;

        if (desiredDistance < 0.01f)
            return;

        direction.Normalize();

        Vector3 finalPosition = desiredCameraPosition;
        bool isColliding = false;

        RaycastHit hit;
        if (Physics.SphereCast(
            followPosition,
            cameraRadius,
            direction,
            out hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            float safeDistance = Mathf.Max(hit.distance - cameraRadius, minDistanceFromTarget);
            finalPosition = followPosition + direction * safeDistance;
            isColliding = true;

            if (showDebug)
            {
                Debug.DrawLine(followPosition, hit.point, Color.red);
                Debug.DrawLine(hit.point, finalPosition, Color.yellow);
            }
        }
        else
        {
            if (showDebug)
            {
                Debug.DrawLine(followPosition, desiredCameraPosition, Color.green);
            }
        }

        if (checkCameraPosition)
        {
            finalPosition = FindSafeCameraPosition(followPosition, finalPosition, direction, ref isColliding);
        }

        float dampingSpeed = isColliding ? collisionDamping : returnDamping;
        
        if (previousCameraPosition == Vector3.zero)
        {
            previousCameraPosition = finalPosition;
        }

        Vector3 smoothedPosition = Vector3.Lerp(
            previousCameraPosition,
            finalPosition,
            deltaTime * dampingSpeed
        );

        previousCameraPosition = smoothedPosition;
        state.RawPosition = smoothedPosition;
    }

    Vector3 FindSafeCameraPosition(Vector3 followPosition, Vector3 targetPosition, Vector3 direction, ref bool isColliding)
    {
        Vector3 safePosition = targetPosition;
        
        for (int attempt = 0; attempt <= maxRepositionAttempts; attempt++)
        {
            if (Physics.CheckSphere(safePosition, cameraRadius + cameraPadding, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                isColliding = true;
                
                float currentDistance = Vector3.Distance(followPosition, safePosition);
                float pullbackAmount = cameraRadius + cameraPadding;
                float newDistance = Mathf.Max(currentDistance - pullbackAmount, minDistanceFromTarget);
                
                safePosition = followPosition + direction * newDistance;
                
                if (showDebug)
                {
                    Debug.DrawLine(targetPosition, safePosition, Color.magenta);
                }
            }
            else
            {
                break;
            }
        }
        
        return safePosition;
    }

    void OnDrawGizmosSelected()
    {
        CinemachineVirtualCameraBase vcam = ComponentOwner;
        if (vcam == null || vcam.Follow == null)
            return;

        Gizmos.color = Color.cyan;
        Vector3 followPos = vcam.Follow.position;
        Vector3 cameraPos = transform.position;
        Vector3 dir = (cameraPos - followPos).normalized;
        float dist = Vector3.Distance(cameraPos, followPos);
        
        Gizmos.DrawWireSphere(followPos + dir * dist, cameraRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cameraPos, cameraRadius + cameraPadding);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(followPos, cameraPos);
    }
}
