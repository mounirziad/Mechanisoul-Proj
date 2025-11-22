using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider))]

public class Terminal : MonoBehaviour
{
    [SerializeField] bool inRange;
    [SerializeField] UpgradeUIScript upgradeUIScript;
    private bool upgradeUIActive = false;
    private bool pauseActive = false;

    void Awake()
    {
        if (upgradeUIScript != null && upgradeUIScript.pauseMenu != null)
        {
            upgradeUIScript.pauseMenu.style.display = DisplayStyle.None;
        }

        inRange = false;
    }

    public void OnUIActivate(InputAction.CallbackContext context)
    {
        if (context.performed && upgradeUIActive == false && inRange)
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
    private void OnTriggerEnter(Collider other) => inRange = other.CompareTag("Player") ? true : false;
    private void OnTriggerExit(Collider other) => inRange = other.CompareTag("Player") ? false : true;
}
