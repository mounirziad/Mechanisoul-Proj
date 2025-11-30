using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    [Header("Pause Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Menu Controls")]
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    [Header("Controller Support")]
    [SerializeField] private Button firstSelectedPauseButton;
    [SerializeField] private Slider firstSelectedSettingsSlider;

    private void Awake()
    {
        if (resumeButton != null) 
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (optionsButton != null) 
            optionsButton.onClick.AddListener(OnOptionsClicked);
        if (quitButton != null) 
            quitButton.onClick.AddListener(OnQuitClicked);
        if (closeSettingsButton != null) 
            closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
    }

    private void OnDestroy()
    {
        if (resumeButton != null) 
            resumeButton.onClick.RemoveListener(OnResumeClicked);
        if (optionsButton != null) 
            optionsButton.onClick.RemoveListener(OnOptionsClicked);
        if (quitButton != null) 
            quitButton.onClick.RemoveListener(OnQuitClicked);
        if (closeSettingsButton != null) 
            closeSettingsButton.onClick.RemoveListener(OnCloseSettingsClicked);
    }

    private void OnEnable()
    {
        if (pauseMenu != null && pauseMenu.activeSelf && firstSelectedPauseButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedPauseButton.gameObject);
        }
    }

    private void OnResumeClicked()
    {
        if (pauseMenu != null)
        {
            GetComponentInParent<InteractableUI>().TogglePauseMenu();
        }
        Debug.Log("Resume Button Clicked!");
    }

    private void OnOptionsClicked()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(true);
            
            if (firstSelectedSettingsSlider != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedSettingsSlider.gameObject);
            }
        }
        Debug.Log("Options Button Clicked!");
    }

    private void OnQuitClicked()
    {
        Application.Quit();
        Debug.Log("Quit Button Clicked!");
    }

    private void OnCloseSettingsClicked()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
            
            if (pauseMenu != null && pauseMenu.activeSelf && optionsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(optionsButton.gameObject);
            }
        }
        Debug.Log("Close Settings Button Clicked!");
    }
}
