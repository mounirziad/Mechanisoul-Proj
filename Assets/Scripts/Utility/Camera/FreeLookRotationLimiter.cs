using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineCamera))]
public class FreeLookRotationLimiter : MonoBehaviour
{
    public Transform followTarget;
    public LayerMask obstacleLayers;
    public float rotationSpeed = 100f;
    public float smoothTime = 0.05f;

    private CinemachineCamera cam;
    private PlayerControls controls;
    private Vector2 cameraInput;

    private float targetYaw;
    private float targetPitch;
    private float yawVelocity;
    private float pitchVelocity;

    void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.PlayerMovement.Camera.performed += ctx => cameraInput = ctx.ReadValue<Vector2>();
        controls.PlayerMovement.Camera.canceled += ctx => cameraInput = Vector2.zero;
        controls.Enable();

        Vector3 angles = cam.transform.eulerAngles;
        targetYaw = angles.y;
        targetPitch = angles.x;
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void LateUpdate()
    {
        if (!followTarget) return;

        // Apply input to target rotation
        targetYaw += cameraInput.x * rotationSpeed * Time.deltaTime;
        targetPitch -= cameraInput.y * rotationSpeed * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, -80f, 80f);

        // Smoothly rotate camera
        float smoothYaw = Mathf.SmoothDampAngle(cam.transform.eulerAngles.y, targetYaw, ref yawVelocity, smoothTime);
        float smoothPitch = Mathf.SmoothDampAngle(cam.transform.eulerAngles.x, targetPitch, ref pitchVelocity, smoothTime);
        cam.transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);

        // Collision prevention handled by Deoccluder
        // Make sure Deoccluder is added to the camera and 'Collide Against' layers include all walls
    }
}
