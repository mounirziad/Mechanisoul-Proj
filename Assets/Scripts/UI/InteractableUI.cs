using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InteractableUI : MonoBehaviour
{
    [SerializeField] UpgradeUIScript upgradeUIScript;
    [SerializeField] PauseMenuUI pauseMenuUI;
    [SerializeField] InputManager playerInputManager;

    private bool pauseActive = false;
    private bool upgradeUIActive = false;
    private PlayerControls playerControls;

    [Header("References for Controller Navigation")]
    [SerializeField] private Button firstPauseButton;
    [SerializeField] private Button firstUpgradeButton;

    void Awake()
    {
        if (upgradeUIScript == null)
        {
            upgradeUIScript = GetComponentInChildren<UpgradeUIScript>();
        }

        if (pauseMenuUI == null)
        {
            pauseMenuUI = GetComponentInChildren<PauseMenuUI>();
        }

        if (playerInputManager == null)
        {
            playerInputManager = FindObjectOfType<InputManager>();
        }

        if (pauseMenuUI != null && pauseMenuUI.pauseMenu != null)
        {
            pauseMenuUI.pauseMenu.SetActive(false);
        }

        if (upgradeUIScript != null && upgradeUIScript.skillMenu != null)
        {
            upgradeUIScript.skillMenu.SetActive(false);
        }

        pauseActive = false;
        upgradeUIActive = false;
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
                pauseMenuUI.pauseMenu.SetActive(true);
                upgradeUIScript.skillMenu.SetActive(false);
                pauseActive = true;
                upgradeUIActive = false;

                SetCursorState(true);
                SetPlayerInputActive(false);
                Time.timeScale = 0f;

                if (firstPauseButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);
                }
            }
        }
        else
        {
            if (upgradeUIScript != null)
            {
                pauseMenuUI.pauseMenu.SetActive(false);
                pauseMenuUI.settingsMenu.SetActive(false);
                pauseActive = false;

                SetCursorState(false);
                SetPlayerInputActive(true);
                Time.timeScale = 1f;

                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void ToggleUpgradeUI()
    {
        if (!upgradeUIActive)
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.skillMenu.SetActive(true);
                pauseMenuUI.pauseMenu.SetActive(false);
                upgradeUIActive = true;
                pauseActive = false;

                SetCursorState(true);
                SetPlayerInputActive(false);

                if (firstUpgradeButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstUpgradeButton.gameObject);
                }
            }
        }
        else
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.skillMenu.SetActive(false);
                upgradeUIActive = false;

                SetCursorState(false);
                SetPlayerInputActive(true);

                EventSystem.current.SetSelectedGameObject(null);
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
