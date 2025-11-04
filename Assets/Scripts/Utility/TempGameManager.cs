using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TempGameManager : MonoBehaviour
{
    [SerializeField] private GameObject UI;
    [SerializeField] UpgradeUIScript upgradeUIScript;
    private bool pauseActive = false;
    private bool upgradeUIActive = false;

    void Awake()
    {
        if (upgradeUIScript != null && upgradeUIScript.pauseMenu != null)
        {
            upgradeUIScript.pauseMenu.style.display = DisplayStyle.None;
        }

        pauseActive = false;
    }

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
        if (context.performed && pauseActive == false)
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.pauseMenu.style.display = DisplayStyle.Flex;
                upgradeUIScript.skillMenu.style.display = DisplayStyle.None;
                pauseActive = true;
                
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
            }
        }
        else if (context.performed && pauseActive == true)
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.pauseMenu.style.display = DisplayStyle.None;
                pauseActive = false;
                
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void OnUIActivate(InputAction.CallbackContext context)
    {
        if (context.performed && upgradeUIActive == false)
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.skillMenu.style.display = DisplayStyle.Flex;
                upgradeUIScript.pauseMenu.style.display = DisplayStyle.None;
                upgradeUIActive = true;
                pauseActive = false;
                
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
            }
        }
        else if (context.performed && upgradeUIActive == true)
        {
            if (upgradeUIScript != null)
            {
                upgradeUIScript.skillMenu.style.display = DisplayStyle.None;
                upgradeUIActive = false;
                
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
