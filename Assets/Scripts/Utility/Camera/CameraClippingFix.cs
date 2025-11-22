using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraClippingFix : MonoBehaviour
{
    [Header("Near Plane Adjustment")]
    [SerializeField] private float defaultNearPlane = 0.3f;
    [SerializeField] private float minNearPlane = 0.05f;
    [SerializeField] private bool adjustNearPlane = true;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayers = -1;
    [SerializeField] private float wallCheckDistance = 0.5f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (!adjustNearPlane)
        {
            return;
        }

        if (IsNearWall())
        {
            cam.nearClipPlane = minNearPlane;
        }
        else
        {
            cam.nearClipPlane = Mathf.Lerp(cam.nearClipPlane, defaultNearPlane, 5f * Time.deltaTime);
        }
    }

    private bool IsNearWall()
    {
        return Physics.Raycast(transform.position, transform.forward, wallCheckDistance, wallLayers, QueryTriggerInteraction.Ignore) ||
               Physics.Raycast(transform.position, -transform.forward, wallCheckDistance, wallLayers, QueryTriggerInteraction.Ignore);
    }
}
