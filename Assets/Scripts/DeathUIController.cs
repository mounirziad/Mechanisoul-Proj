using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeathUIController : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Controller Support")]
    [SerializeField] private Button firstSelectedButton;

    private void Awake()
    {
        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(OnRespawnButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
    }

    private void OnEnable()
    {
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (respawnButton != null)
        {
            respawnButton.onClick.RemoveListener(OnRespawnButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        }
    }

    public void OnRespawnButtonClicked()
    {
        if (PlayerRespawnManager.Instance != null)
        {
            PlayerRespawnManager.Instance.RespawnPlayer();
        }
        else
        {
            Debug.LogError("DeathUIController: PlayerRespawnManager.Instance is null!");
        }
    }

    public void OnMainMenuButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
