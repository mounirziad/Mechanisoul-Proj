using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;

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

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            sensitivitySlider.minValue = 0.5f;
            sensitivitySlider.maxValue = 1.5f;
        }
    }

    private void OnEnable()
    {
        if (pauseMenu != null && pauseMenu.activeSelf && firstSelectedPauseButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedPauseButton.gameObject);
        }

        InitializeSliders();
    }

    private void InitializeSliders()
    {
        if (GameSettingsManager.Instance != null)
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = GameSettingsManager.Instance.masterVolume;
            }

            if (sensitivitySlider != null)
            {
                sensitivitySlider.value = GameSettingsManager.Instance.Sensitivity;
            }
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetVolume(value);
        }
    }

    private void OnSensitivityChanged(float value)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetSensitivity(value);
        }
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

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
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
