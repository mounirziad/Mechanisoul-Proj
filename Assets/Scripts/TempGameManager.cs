using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TempGameManager : MonoBehaviour
{
    [SerializeField] private GameObject UI;
    public void OnArtScene(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SceneManager.LoadScene("Art Scene");
        }
    }
    public void OnCombatScene(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SceneManager.LoadScene("CombatScene");
        }
    }

    public void OnWarehouseScene(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SceneManager.LoadScene("WareHouseLevel");
        }
    }

    public void OnExitGame(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Application.Quit();
        }
    }

    public void OnUIActivate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!UI.activeSelf)
            {
                UI.SetActive(true);
            }
            else
            {
                UI.SetActive(false);
            }
        }
    }
}
