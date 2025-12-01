using UnityEngine;

public class UpgradeSnapshotManager : MonoBehaviour
{
    public static UpgradeSnapshotManager Instance { get; private set; }

    [System.Serializable]
    public class FloorSnapshot
    {
        public UpgradeUIScript.UpgradeTreeState treeState;
        public int availablePoints;
    }

    FloorSnapshot currentSnapshot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CaptureFloorEntryState()
    {
        XPManager xpManager = FindObjectOfType<XPManager>();
        UpgradeUIScript upgradeUI = FindObjectOfType<UpgradeUIScript>();

        if (xpManager == null || upgradeUI == null)
        {
            Debug.LogWarning("UpgradeSnapshotManager: missing XPManager or UpgradeUIScript, cannot capture snapshot.");
            return;
        }

        currentSnapshot = new FloorSnapshot
        {
            treeState = upgradeUI.GetPurchasedState(),
            availablePoints = xpManager.CurrentSkillPoints
        };

        Debug.Log("UpgradeSnapshotManager: captured floor entry snapshot.");
    }

    public void RestoreFloorEntryState()
    {
        if (currentSnapshot == null)
        {
            Debug.LogWarning("UpgradeSnapshotManager: no snapshot to restore.");
            return;
        }

        XPManager xpManager = FindObjectOfType<XPManager>();
        UpgradeUIScript upgradeUI = FindObjectOfType<UpgradeUIScript>();

        if (xpManager == null || upgradeUI == null)
        {
            Debug.LogWarning("UpgradeSnapshotManager: missing XPManager or UpgradeUIScript, cannot restore snapshot.");
            return;
        }

        xpManager.SetSkillPoints(currentSnapshot.availablePoints);
        upgradeUI.ApplyPurchasedState(currentSnapshot.treeState);

        Debug.Log("UpgradeSnapshotManager: restored floor entry snapshot.");
    }
}
