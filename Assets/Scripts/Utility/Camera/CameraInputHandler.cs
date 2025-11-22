using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FreeLookCamera))]
public class CameraInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool enableMouseControl = true;
    [SerializeField] private bool enableGamepadControl = true;
    [SerializeField] private float gamepadSensitivity = 100f;

    private FreeLookCamera freeLookCamera;
    private PlayerInput playerInput;
    
    private InputAction lookAction;
    private InputAction zoomAction;

    private void Awake()
    {
        freeLookCamera = GetComponent<FreeLookCamera>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            lookAction = playerInput.actions["Look"];
            zoomAction = playerInput.actions["Zoom"];
        }
    }

    private void OnEnable()
    {
        if (lookAction != null)
        {
            lookAction.performed += OnLook;
            lookAction.canceled += OnLook;
            lookAction.Enable();
        }

        if (zoomAction != null)
        {
            zoomAction.performed += OnZoom;
            zoomAction.canceled += OnZoom;
            zoomAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (lookAction != null)
        {
            lookAction.performed -= OnLook;
            lookAction.canceled -= OnLook;
            lookAction.Disable();
        }

        if (zoomAction != null)
        {
            zoomAction.performed -= OnZoom;
            zoomAction.canceled -= OnZoom;
            zoomAction.Disable();
        }
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        if (context.control.device is Mouse && !enableMouseControl)
        {
            input = Vector2.zero;
        }
        else if (context.control.device is Gamepad && !enableGamepadControl)
        {
            input = Vector2.zero;
        }

        if (context.control.device is Gamepad)
        {
            input *= gamepadSensitivity * Time.deltaTime;
        }

        freeLookCamera.OnLook(context);
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        freeLookCamera.OnZoom(context);
    }
}
