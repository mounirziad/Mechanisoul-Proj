using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Experimental.GlobalIllumination;

public class CameraManager : MonoBehaviour
{
    InputManager inputManager;
    public Transform targetTransform;
    public Transform cameraPivot;
    public Transform cameraTransform;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    public float cameraCollisionRadius = 0.2f;
    private float defaultPosition;
    public float cameraFollowSpeed = 0.2f;
    public float cameraLookSpeed = 2;
    public float cameraPivotSpeed = 2;
    public LayerMask collisionLayers; 
    public float lookAngle;
    public float pivotAngle;
    public float minimumPivotAngle = -35;
    public float maximumPivotAngle = 35;
    private Vector3 cameraVectorPosition;

    public float cameraCollisionOffset = 0.2f;
    public float minimumCollisionOffset = 0.2f;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        inputManager = FindObjectOfType<InputManager>();
        targetTransform = FindObjectOfType<PlayerManager>().transform;
        defaultPosition = cameraTransform.localPosition.z;
    }

    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, targetTransform.position, ref cameraFollowVelocity, cameraFollowSpeed);

        transform.position = targetPosition;
    }

    private void RotateCamera()
    {
        Vector3 rotation;
        lookAngle = lookAngle + (inputManager.cameraInputX * cameraLookSpeed * Time.deltaTime);
        pivotAngle = pivotAngle - (inputManager.cameraInputY * cameraPivotSpeed * Time.deltaTime);
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;
    }

    private void HandleCameraCollisions()
    {
        float targetZ = -defaultPosition;
        RaycastHit hit;

        Vector3 direction = cameraPivot.forward;

        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction, out hit, defaultPosition, collisionLayers))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetZ = -(distance - cameraCollisionOffset);
        }

        if (Mathf.Abs(targetZ) < minimumCollisionOffset)
            targetZ = -minimumCollisionOffset;

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetZ, 0.2f);
        cameraTransform.localPosition = cameraVectorPosition;
    }


}
