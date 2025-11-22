using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [Header("Collision Detection")]
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float cameraRadius = 0.2f;
    [SerializeField] private float minimumDistanceFromTarget = 0.5f;

    [Header("Advanced Settings")]
    [SerializeField] private int sphereCastResolution = 3;
    [SerializeField] private float collisionPadding = 0.2f;
    [SerializeField] private float recoverySpeed = 3f;
    [SerializeField] private float nearCameraExtraPadding = 0.15f;

    [Header("Tight Space Handling")]
    [SerializeField] private bool useAdaptiveRadius = true;
    [SerializeField] private float minAdaptiveRadius = 0.05f;
    [SerializeField] private bool useMultipleRaycasts = true;
    [SerializeField] private int raycastCount = 5;

    private float currentCollisionDistance;
    private float targetCollisionDistance;

    public void InitializeDistance(float initialDistance)
    {
        currentCollisionDistance = initialDistance;
        targetCollisionDistance = initialDistance;
    }

    public Vector3 HandleCollision(Vector3 targetPoint, Vector3 desiredPosition, float desiredDistance)
    {
        Vector3 direction = desiredPosition - targetPoint;
        float distance = direction.magnitude;
        
        if (distance < 0.001f)
        {
            return targetPoint + Vector3.back * minimumDistanceFromTarget;
        }

        direction.Normalize();

        float safeDistance = FindSafeDistance(targetPoint, direction, desiredDistance);
        
        targetCollisionDistance = Mathf.Max(safeDistance, minimumDistanceFromTarget);
        
        float speedMultiplier = recoverySpeed;
        if (safeDistance < currentCollisionDistance)
        {
            speedMultiplier = recoverySpeed * 3f;
        }
        
        currentCollisionDistance = Mathf.Lerp(currentCollisionDistance, targetCollisionDistance, speedMultiplier * Time.deltaTime);

        return targetPoint + direction * currentCollisionDistance;
    }

    public float FindSafeDistance(Vector3 origin, Vector3 direction, float maxDistance)
    {
        // Minimum safe distance to prevent camera from getting too close to player
        float minSafeDistance = 1.0f; // Adjust this value based on your game's needs

        float closestHit = maxDistance;
        float currentRadius = cameraRadius;

        if (useAdaptiveRadius)
        {
            if (CheckForTightSpace(origin, direction, maxDistance))
            {
                currentRadius = Mathf.Lerp(cameraRadius, minAdaptiveRadius, 0.7f);
            }
        }

        if (useMultipleRaycasts)
        {
            closestHit = PerformMultipleRaycasts(origin, direction, maxDistance, currentRadius);
        }
        else
        {
            closestHit = PerformSphereCast(origin, direction, maxDistance, currentRadius);
        }

        // ENSURE minimum safe distance - this is the critical fix
        float finalDistance = Mathf.Max(closestHit, minSafeDistance);

        // Also ensure we don't exceed the maximum desired distance
        finalDistance = Mathf.Min(finalDistance, maxDistance);

        return finalDistance;
    }

    private float PerformSphereCast(Vector3 origin, Vector3 direction, float maxDistance, float radius)
    {
        float closestHit = maxDistance;
        float totalPadding = collisionPadding + nearCameraExtraPadding;

        for (int i = 0; i < sphereCastResolution; i++)
        {
            float t = i / Mathf.Max(1f, sphereCastResolution - 1f);
            float currentRadius = Mathf.Lerp(radius * 0.5f, radius, t);
            
            if (Physics.SphereCast(origin, currentRadius, direction, out RaycastHit hit, maxDistance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                float hitDistance = Mathf.Max(0f, hit.distance - totalPadding);
                closestHit = Mathf.Min(closestHit, hitDistance);
            }
        }

        return closestHit;
    }

    private float PerformMultipleRaycasts(Vector3 origin, Vector3 direction, float maxDistance, float radius)
    {
        float closestHit = maxDistance;

        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right.magnitude < 0.001f)
        {
            right = Vector3.Cross(direction, Vector3.forward).normalized;
        }
        Vector3 up = Vector3.Cross(right, direction).normalized;

        closestHit = Mathf.Min(closestHit, PerformSingleRaycast(origin, direction, maxDistance));

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (i / (float)raycastCount) * 360f;
            float radians = angle * Mathf.Deg2Rad;
            
            Vector3 offset = (right * Mathf.Cos(radians) + up * Mathf.Sin(radians)) * radius;
            Vector3 rayOrigin = origin + offset;
            
            closestHit = Mathf.Min(closestHit, PerformSingleRaycast(rayOrigin, direction, maxDistance));
        }

        if (Physics.SphereCast(origin, radius * 0.5f, direction, out RaycastHit sphereHit, maxDistance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            float totalPadding = collisionPadding + nearCameraExtraPadding;
            float hitDistance = Mathf.Max(0f, sphereHit.distance - totalPadding);
            closestHit = Mathf.Min(closestHit, hitDistance);
        }

        return closestHit;
    }

    private float PerformSingleRaycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        float totalPadding = collisionPadding + nearCameraExtraPadding;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(0f, hit.distance - totalPadding);
        }
        return maxDistance;
    }

    private bool CheckForTightSpace(Vector3 origin, Vector3 direction, float maxDistance)
    {
        int hitCount = 0;
        float checkRadius = cameraRadius * 2f;

        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right.magnitude < 0.001f)
        {
            right = Vector3.Cross(direction, Vector3.forward).normalized;
        }
        Vector3 up = Vector3.Cross(right, direction).normalized;

        Vector3[] checkDirections = new Vector3[]
        {
            right, -right, up, -up,
            (right + up).normalized, (-right + up).normalized,
            (right - up).normalized, (-right - up).normalized
        };

        foreach (Vector3 checkDir in checkDirections)
        {
            if (Physics.Raycast(origin, checkDir, checkRadius, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                hitCount++;
            }
        }

        return hitCount >= 3;
    }

    public LayerMask GetCollisionLayers()
    {
        return collisionLayers;
    }

    public float GetCameraRadius()
    {
        return cameraRadius;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, cameraRadius);
    }


}
