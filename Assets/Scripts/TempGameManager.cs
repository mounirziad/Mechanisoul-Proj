using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TempGameManager : MonoBehaviour
{
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
}
