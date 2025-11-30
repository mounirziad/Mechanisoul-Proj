using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button creditsExitButton;
    [SerializeField] private CanvasGroup mainMenuButtonsCanvasGroup;
    [SerializeField] private GameObject creditsButton;
    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }
    
    private void Start()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Credits Panel is not assigned in the inspector.");
        }
    }

    void OnEnable()
    {
        playerControls.InteractableUI.Enable();
        playerControls.InteractableUI.Cancel.performed += OnCreditsPerformed;
    }
    void OnDisable()
    {
        playerControls.InteractableUI.Cancel.performed -= OnCreditsPerformed;
        playerControls.InteractableUI.Disable();
    }

    public void OnPlay()
    {
        SceneManager.LoadScene("TutorialCutscene");
    }

    void OnCreditsPerformed(InputAction.CallbackContext context)
    {
        if (creditsPanel != null && creditsPanel.activeSelf)
        {
            OnCredits();
        }
    }

    public void OnCredits()
    {
        if (creditsPanel != null)
        {
            bool isActive = !creditsPanel.activeSelf;
            creditsPanel.SetActive(isActive);

            if (mainMenuButtonsCanvasGroup != null)
            {
                mainMenuButtonsCanvasGroup.interactable = !isActive;
            }

            if (isActive)
            {
                if (creditsExitButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(creditsExitButton.gameObject);
                }
            }
            else
            {
                if (creditsButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(creditsButton);
                }
            }
        }
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
