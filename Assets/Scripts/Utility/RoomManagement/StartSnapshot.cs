using UnityEngine;

public class StartSnapshot : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UpgradeSnapshotManager.Instance != null)
            {
                UpgradeSnapshotManager.Instance.CaptureFloorEntryState();
            }
        }
    }
}
