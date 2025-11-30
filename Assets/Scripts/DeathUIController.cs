using UnityEngine;

public class DeathUIController : MonoBehaviour
{
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
}
