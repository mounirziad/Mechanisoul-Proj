using UnityEngine;

[RequireComponent(typeof(FreeLookCamera))]
public class FreeLookCameraBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager playerInputManager;

    [Header("Input Settings")]
    [SerializeField] private bool useLookAction = true;
    [SerializeField] private bool useCameraAction = false;

    private FreeLookCamera freeLookCamera;
    private Vector2 lastLookInput;

    private void Awake()
    {
        freeLookCamera = GetComponent<FreeLookCamera>();
    }

    private void Start()
    {
        if (playerInputManager == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInputManager = player.GetComponent<InputManager>();
            }
        }
    }

    private void Update()
    {
        if (playerInputManager == null || freeLookCamera == null)
        {
            return;
        }

        Vector2 lookInput = Vector2.zero;

        if (useLookAction && playerInputManager.playerControls != null)
        {
            lookInput = playerInputManager.playerControls.PlayerMovement.Look.ReadValue<Vector2>();
        }
        else if (useCameraAction)
        {
            lookInput = playerInputManager.cameraInput;
        }

        if (lookInput != lastLookInput)
        {
            lastLookInput = lookInput;
            freeLookCamera.OnLook(new UnityEngine.InputSystem.InputAction.CallbackContext());
        }

        UpdateLookInput(lookInput);
    }

    private void UpdateLookInput(Vector2 input)
    {
        var context = new UnityEngine.InputSystem.InputAction.CallbackContext();
        freeLookCamera.OnLook(context);
    }
}
