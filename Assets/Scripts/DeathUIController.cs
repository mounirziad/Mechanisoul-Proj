using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class DeathUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathUIPanel;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Controller Support")]
    [SerializeField] private Button firstSelectedButton;

    private bool isDeathUIActive = false;

    private void Awake()
    {
        if (deathUIPanel == null)
        {
            deathUIPanel = gameObject;
        }

        if (respawnButton == null)
        {
            respawnButton = transform.Find("Respawn Button")?.GetComponent<Button>();
        }

        if (mainMenuButton == null)
        {
            mainMenuButton = transform.Find("Main Menu Button")?.GetComponent<Button>();
        }

        if (firstSelectedButton == null)
        {
            firstSelectedButton = respawnButton;
        }

        SetupButtonNavigation();

        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(OnRespawnButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }

        HideDeathUI();
    }

    private void SetupButtonNavigation()
    {
        if (respawnButton != null && mainMenuButton != null)
        {
            Navigation respawnNav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnDown = mainMenuButton,
                selectOnUp = mainMenuButton
            };
            respawnButton.navigation = respawnNav;

            Navigation mainMenuNav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = respawnButton,
                selectOnDown = respawnButton
            };
            mainMenuButton.navigation = mainMenuNav;
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

    public void ShowDeathUI()
    {
        if (isDeathUIActive) return;

        isDeathUIActive = true;
        
        if (deathUIPanel != null)
        {
            deathUIPanel.SetActive(true);
        }

        SetCursorState(true);
        
        StartCoroutine(SelectButtonDelayed());

        Debug.Log("Death UI shown");
    }

    private IEnumerator SelectButtonDelayed()
    {
        yield return null;
        
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("DeathUIController: EventSystem not found!");
            yield break;
        }

        eventSystem.SetSelectedGameObject(null);
        
        yield return null;

        if (firstSelectedButton != null)
        {
            eventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
            Debug.Log($"DeathUIController: Selected button: {firstSelectedButton.name}");
        }
    }

    public void HideDeathUI()
    {
        if (!isDeathUIActive) return;

        isDeathUIActive = false;
        
        if (deathUIPanel != null)
        {
            deathUIPanel.SetActive(false);
        }

        SetCursorState(false);

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }

        Debug.Log("Death UI hidden");
    }

    public void OnRespawnButtonClicked()
    {
        Debug.Log("Respawn button clicked!");
        
        HideDeathUI();

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
        Debug.Log("Main Menu button clicked!");
        
        SceneManager.LoadScene("Main Menu");
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!isDeathUIActive) return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        if (eventSystem.currentSelectedGameObject == null && firstSelectedButton != null)
        {
            eventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }
}
