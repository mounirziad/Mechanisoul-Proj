using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public PlayerControls playerControls;
    PlayerLocomotion playerLocomotion;
    AnimatorManager animatorManager;

    public Vector2 movementInput;
    public Vector2 cameraInput;

    public float cameraInputX;
    public float cameraInputY;

    public float moveAmount;
    public float verticalInput;
    public float horizontalInput;

    public bool jumpInput;
    public bool advanceDialogueInput; // NEW: Separate input for dialogue

    public bool b_Input;

    public bool dodgeInput;

    public bool aimInput; // Right click hold
    public bool shootInput; // Left click while aiming

    private float jumpInputBuffer = 0f;
    private const float jumpBufferTime = 0.2f; // Buffer for 0.2 seconds

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void OnEnable()
    {
        if (playerControls == null) // FIX: initialize only once if null
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed +=
            i => movementInput = i.ReadValue<Vector2>();

            playerControls.PlayerMovement.Movement.canceled +=
                i => movementInput = Vector2.zero; // reset when stick released

            playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();

            playerControls.PlayerActions.B.performed += i => b_Input = true;
            playerControls.PlayerActions.B.canceled += i => b_Input = false;
            playerControls.PlayerActions.Jump.performed += i => jumpInput = true;
            playerControls.PlayerActions.AdvanceDialogue.performed += i => advanceDialogueInput = true; // NEW
            playerControls.PlayerActions.Dodge.performed += i => dodgeInput = true;
            playerControls.PlayerActions.RangedAim.performed += i => aimInput = true;
            playerControls.PlayerActions.RangedAim.canceled += i => aimInput = false;
            playerControls.PlayerActions.Shoot.performed += i => shootInput = true;
            playerControls.PlayerActions.EnemyLockOn.performed += i =>
            {
                if (GetComponent<LockOnSystem>() != null)
                    GetComponent<LockOnSystem>().ToggleLock();
            };
        }
        playerControls.Enable();
    }

    private void LateUpdate()
    {
        shootInput = false; // Reset shoot input each frame
        advanceDialogueInput = false; // NEW: Reset dialogue input each frame
    }

    private void OnDisable()
    {
        if (playerControls != null) // avoid disabling null reference
            playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        bool isDialogueActive = DialogueSystem.Instance != null && DialogueSystem.Instance.IsDisplaying;

        if (!isDialogueActive)
        {
            HandleMovementInput();
            HandleSprintingInput();
            HandleJumpingInput();
            HandleDodgeInput();
        }
        else
        {
            movementInput = Vector2.zero;
            verticalInput = 0f;
            horizontalInput = 0f;
            moveAmount = 0f;
            dodgeInput = false;
            jumpInput = false; // Reset jump input during dialogue
            aimInput = false;
            shootInput = false;
            animatorManager.UpdateAnimatorValues(0, 0, false);
        }
    }

    private void HandleDodgeInput()
    {
        if (dodgeInput)
        {
            dodgeInput = false;
            playerLocomotion.HandleDodge();
        }
    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputX = cameraInput.x;
        cameraInputY = cameraInput.y;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmount, playerLocomotion.isSprinting);
    }

    private void HandleSprintingInput()
    {
        if (b_Input && moveAmount > 0.5f)
        {
            playerLocomotion.isSprinting = true;
        }
        else
        {
            playerLocomotion.isSprinting = false;
        }
    }

    private void HandleJumpingInput()
    {
        // Set buffer when jump is pressed
        if (jumpInput)
        {
            jumpInput = false;

            // Only set buffer if we're actually able to jump
            if (playerLocomotion.isGrounded && playerLocomotion.canJump && !playerLocomotion.isJumping)
            {
                jumpInputBuffer = jumpBufferTime;
            }
            else
            {
                // Clear any existing buffer if we can't jump
                jumpInputBuffer = 0f;
            }
        }

        // Decrease buffer over time
        if (jumpInputBuffer > 0)
        {
            jumpInputBuffer -= Time.deltaTime;

            // Try to jump while buffer is active
            if (playerLocomotion.isGrounded && playerLocomotion.canJump && !playerLocomotion.isJumping)
            {
                jumpInputBuffer = 0f; // Consume the buffer
                playerLocomotion.HandleJumping();
            }
        }
    }

    // NEW: Public method to check for dialogue input
    public bool GetAdvanceDialogueInput()
    {
        return advanceDialogueInput;
    }

    // Inside InputManager.cs
    public void SetMovementInputActive(bool isActive)
    {
        if (playerControls == null) return;

        if (isActive)
        {
            playerControls.PlayerMovement.Enable();
            playerControls.PlayerActions.Enable();
        }
        else
        {
            playerControls.PlayerMovement.Disable();
            playerControls.PlayerActions.Disable();
            jumpInput = false;
        }
    }
}