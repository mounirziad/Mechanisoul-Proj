using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class RootMotionMovement : MonoBehaviour
{

    Animator animator;

    int isWalkingHash;
    int isRunningHash;

    PlayerControls input;

    Vector2 currentMovement;
    bool movementPressed;
    bool runPressed;

    void HandleRotation()
    {
        Vector3 currentPosition = transform.position;

        Vector3 newPosition = new Vector3(currentMovement.x, 0, currentMovement.y);

        Vector3 positionToLookAt = currentPosition + newPosition;

        transform.LookAt(positionToLookAt);
    }

    private void Awake()
    {
        input = new PlayerControls();
        input.PlayerMovement.Movement.performed += ctx =>
        {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
        };
        input.PlayerMovement.Run.performed += ctx => ctx.ReadValueAsButton();

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("IsWalking");
        isRunningHash = Animator.StringToHash("IsRunning");
    }

    // Update is called once per frame
    void Update()
    {
        handleMovement();
        HandleRotation();
    }

    void handleMovement()
    {
        bool isRunning = animator.GetBool(isRunningHash);
        bool isWalking = animator.GetBool(isWalkingHash);

        if(movementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }

        if (!movementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((movementPressed && runPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }

        if((!movementPressed || !runPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }

    private void OnEnable()
    {
        input.PlayerMovement.Enable();
    }

    private void OnDisable()
    {
        input.PlayerMovement.Disable();
    }
}
