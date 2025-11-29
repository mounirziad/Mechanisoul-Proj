using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider))]

public class Terminal : MonoBehaviour
{
    [SerializeField] bool inRange;
    [SerializeField] InteractableUI interactableUI;
    private PlayerControls playerControls;

    void Awake()
    {
        if (interactableUI == null)
        {
            interactableUI = FindObjectOfType<InteractableUI>();
        }
        
        inRange = false;
    }

    void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.InteractableUI.Interact.performed += OnInteract;
        }
        
        playerControls.Enable();
    }

    void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.InteractableUI.Interact.performed -= OnInteract;
            playerControls.Disable();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (inRange && interactableUI != null)
        {
            interactableUI.ToggleUpgradeUI();
        }
    }
    private void OnTriggerEnter(Collider other) => inRange = other.CompareTag("Player") ? true : false;
    private void OnTriggerExit(Collider other) => inRange = other.CompareTag("Player") ? false : true;
}
