using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InteractableUI : MonoBehaviour
{
    [SerializeField] UpgradeUIScript upgradeUIScript;
    [SerializeField] InputManager playerInputManager;

    private bool pauseActive = false;
    private bool upgradeUIActive = false;
    private PlayerControls playerControls;

    void Awake()
    {
        if (upgradeUIScript == null)
        {
            upgradeUIScript = GetComponent<UpgradeUIScript>();
        }

        if (playerInputManager == null)
        {
            playerInputManager = FindObjectOfType<InputManager>();
        }

        if (upgradeUIScript != null && upgradeUIScript.pauseMenu != null)
        {
            upgradeUIScript.pauseMenu.style.display = DisplayStyle.None;
        }

        pauseActive = false;
    }

    void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.InteractableUI.PauseMenu.performed += OnPauseMenuToggle;
            playerControls.InteractableUI.UpgradeUI.performed += OnUpgradeUIToggle;
        }

        playerControls.Enable();
    }

    void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.InteractableUI.PauseMenu.performed -= OnPauseMenuToggle;
            playerControls.InteractableUI.UpgradeUI.performed -= OnUpgradeUIToggle;
            playerControls.Disable();
        }
    }

    private void OnPauseMenuToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePauseMenu();
        }
    }

    private void OnUpgradeUIToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleUpgradeUI();
        }
    }

    public void TogglePauseMenu()
    {
        if (!pauseActive)
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.pauseMenu.style.display = DisplayStyle.Flex;
                upgradeUIScript.skillMenu.style.display = DisplayStyle.None;
                pauseActive = true;
                upgradeUIActive = false;

                SetCursorState(true);
                SetPlayerInputActive(false);
                Time.timeScale = 0f;
            }
        }
        else
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.pauseMenu.style.display = DisplayStyle.None;
                pauseActive = false;

                SetCursorState(false);
                SetPlayerInputActive(true);
                Time.timeScale = 1f;
            }
        }
    }

    public void ToggleUpgradeUI()
    {
        if (!upgradeUIActive)
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.skillMenu.style.display = DisplayStyle.Flex;
                upgradeUIScript.pauseMenu.style.display = DisplayStyle.None;
                upgradeUIActive = true;
                pauseActive = false;

                SetCursorState(true);
                SetPlayerInputActive(false);
            }
        }
        else
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.skillMenu.style.display = DisplayStyle.None;
                upgradeUIActive = false;

                SetCursorState(false);
                SetPlayerInputActive(true);
            }
        }
    }

    private void SetCursorState(bool visible)
    {
        UnityEngine.Cursor.visible = visible;
        UnityEngine.Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void SetPlayerInputActive(bool isActive)
    {
        if (playerInputManager != null && playerInputManager.playerControls != null)
        {
            playerInputManager.SetMovementInputActive(isActive);
        }
    }
}
