using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindPlayerInputManager();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"InteractableUI: Scene loaded - {scene.name}");
        FindPlayerInputManager();

        if (upgradeUIActive)
        {
            upgradeUIActive = false;
            if (upgradeUIScript != null && upgradeUIScript.skillMenu != null)
            {
                upgradeUIScript.skillMenu.SetActive(false);
            }
        }

        if (pauseActive)
        {
            pauseActive = false;
            if (pauseMenuUI != null && pauseMenuUI.pauseMenu != null)
            {
                pauseMenuUI.pauseMenu.SetActive(false);
            }
            Time.timeScale = 1f;
        }
    }

    private void FindPlayerInputManager()
    {
        if (playerInputManager == null)
        {
            playerInputManager = FindObjectOfType<InputManager>();
            if (playerInputManager != null)
            {
                Debug.Log("InteractableUI: Found InputManager");
            }
            else
            {
                Debug.LogWarning("InteractableUI: Could not find InputManager");
            }
        }
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
            if (pauseMenuUI != null && pauseMenuUI.pauseMenu != null)
            {
                pauseMenuUI.pauseMenu.SetActive(true);

                if (upgradeUIScript != null && upgradeUIScript.skillMenu != null)
                {
                    upgradeUIScript.skillMenu.SetActive(false);
                }

                pauseActive = true;
                upgradeUIActive = false;

                SetCursorState(true);
                SetPlayerInputActive(false);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PauseMusic();
                }

                if (firstPauseButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("InteractableUI: pauseMenuUI or pauseMenu is null");
            }
        }
        else
        {
            if (pauseMenuUI != null && pauseMenuUI.pauseMenu != null)
            {
                pauseMenuUI.pauseMenu.SetActive(false);
                pauseActive = false;

                SetCursorState(false);
                SetPlayerInputActive(true);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.ResumeMusic();
                }

                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }



    public void ToggleUpgradeUI()
    {
        if (!upgradeUIActive)
        {
            if (upgradeUIScript != null && upgradeUIScript.skillMenu != null)
            {
                upgradeUIScript.skillMenu.SetActive(true);

                if (pauseMenuUI != null && pauseMenuUI.pauseMenu != null)
                {
                    pauseMenuUI.pauseMenu.SetActive(false);
                }

                upgradeUIActive = true;
                pauseActive = false;

                SetCursorState(true);
                SetPlayerInputActive(false);

                if (firstUpgradeButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstUpgradeButton.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("InteractableUI: upgradeUIScript or skillMenu is null");
            }
        }
        else
        {
            if (upgradeUIScript != null && upgradeUIScript.skillMenu != null)
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
        else if (isActive)
        {
            FindPlayerInputManager();
            if (playerInputManager != null && playerInputManager.playerControls != null)
            {
                playerInputManager.SetMovementInputActive(isActive);
            }
        }
    }
}
