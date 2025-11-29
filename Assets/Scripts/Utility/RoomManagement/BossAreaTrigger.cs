using UnityEngine;

public class BossAreaTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (GameProgressionManager.Instance != null)
            {
                GameProgressionManager.Instance.SetReachedBoss(true);
                hasTriggered = true;
                Debug.Log("BossAreaTrigger: Player has entered boss area!");
            }
            else
            {
                Debug.LogError("BossAreaTrigger: GameProgressionManager.Instance is null!");
            }
        }
    }
}
