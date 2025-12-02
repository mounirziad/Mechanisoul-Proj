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

    private XPManager cachedXPManager;
    private UpgradeUIScript cachedUpgradeUI;

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

    private XPManager GetXPManager()
    {
        if (cachedXPManager == null)
        {
            cachedXPManager = XPManager.Instance;
        }
        if (cachedXPManager == null)
        {
            cachedXPManager = FindObjectOfType<XPManager>();
        }
        return cachedXPManager;
    }

    private UpgradeUIScript GetUpgradeUI()
    {
        if (cachedUpgradeUI == null)
        {
            if (PersistentUIManager.Instance != null)
            {
                cachedUpgradeUI = PersistentUIManager.Instance.FindUIComponent<UpgradeUIScript>();
            }
        }
        if (cachedUpgradeUI == null)
        {
            cachedUpgradeUI = FindObjectOfType<UpgradeUIScript>();
        }
        return cachedUpgradeUI;
    }

    public void CaptureFloorEntryState()
    {
        XPManager xpManager = GetXPManager();
        UpgradeUIScript upgradeUI = GetUpgradeUI();

        if (xpManager == null)
        {
            Debug.LogError("UpgradeSnapshotManager: XPManager not found!");
            return;
        }

        if (upgradeUI == null)
        {
            Debug.LogError("UpgradeSnapshotManager: UpgradeUIScript not found!");
            return;
        }

        currentSnapshot = new FloorSnapshot
        {
            treeState = upgradeUI.GetPurchasedState(),
            availablePoints = xpManager.CurrentSkillPoints
        };

        Debug.Log($"UpgradeSnapshotManager: Captured snapshot - Points: {currentSnapshot.availablePoints}, " +
                  $"Melee: {currentSnapshot.treeState.meleeEmotion} L{currentSnapshot.treeState.meleeLevel}, " +
                  $"Ranged: {currentSnapshot.treeState.rangedEmotion} L{currentSnapshot.treeState.rangedLevel}, " +
                  $"Dash: {currentSnapshot.treeState.dashEmotion} L{currentSnapshot.treeState.dashLevel}");
    }

    public void RestoreFloorEntryState()
    {
        if (currentSnapshot == null)
        {
            Debug.LogWarning("UpgradeSnapshotManager: No snapshot exists to restore!");
            return;
        }

        Debug.Log($"UpgradeSnapshotManager: Attempting to restore snapshot - Points: {currentSnapshot.availablePoints}");

        XPManager xpManager = GetXPManager();
        UpgradeUIScript upgradeUI = GetUpgradeUI();

        if (xpManager == null)
        {
            Debug.LogError("UpgradeSnapshotManager: XPManager not found during restore!");
            return;
        }

        if (upgradeUI == null)
        {
            Debug.LogError("UpgradeSnapshotManager: UpgradeUIScript not found during restore!");
            return;
        }

        xpManager.SetSkillPoints(currentSnapshot.availablePoints);
        upgradeUI.ApplyPurchasedState(currentSnapshot.treeState);

        Debug.Log($"UpgradeSnapshotManager: Restore complete. Current points: {xpManager.CurrentSkillPoints}");
    }
}

