using UnityEngine;

[DisallowMultipleComponent]
public class UpgradeRouter : MonoBehaviour
{
    [SerializeField] UpgradeHandler upgradeHandler;
    [SerializeField] DashAbility dashAbility;

    void Awake()
    {
        if (!upgradeHandler) upgradeHandler = GetComponent<UpgradeHandler>();
        if (!dashAbility) dashAbility = GetComponent<DashAbility>();

        // nothing else needed: the handler centralizes both ranged push and dash spawning
        // your UI buttons can keep calling the Router, which delegates to the handler:
    }

    // RANGED UI hooks (unchanged)
    public void RangedJoyUp() { upgradeHandler?.RangedJoyUp(); }
    public void RangedJoyDown() { upgradeHandler?.RangedJoyDown(); }
    public void RangedAngerUp() { upgradeHandler?.RangedAngerUp(); }
    public void RangedAngerDown() { upgradeHandler?.RangedAngerDown(); }
    public void RangedSadnessUp() { upgradeHandler?.RangedSadnessUp(); }
    public void RangedSadnessDown() { upgradeHandler?.RangedSadnessDown(); }
    public void RangedLoveUp() { upgradeHandler?.RangedLoveUp(); }
    public void RangedLoveDown() { upgradeHandler?.RangedLoveDown(); }
    public void RangedFearUp() { upgradeHandler?.RangedFearUp(); }
    public void RangedFearDown() { upgradeHandler?.RangedFearDown(); }

    // DASH UI hooks (mutual exclusivity handled inside handler)
    public void DashAngerUp() { upgradeHandler?.DashAngerUp(); }
    public void DashAngerDown() { upgradeHandler?.DashAngerDown(); }
    public void DashSadnessUp() { upgradeHandler?.DashSadnessUp(); }
    public void DashSadnessDown() { upgradeHandler?.DashSadnessDown(); }
    public void DashJoyUp() { upgradeHandler?.DashJoyUp(); }
    public void DashJoyDown() { upgradeHandler?.DashJoyDown(); }
    public void DashLoveUp() { upgradeHandler?.DashLoveUp(); }
    public void DashLoveDown() { upgradeHandler?.DashLoveDown(); }
    public void DashFearUp() { upgradeHandler?.DashFearUp(); }
    public void DashFearDown() { upgradeHandler?.DashFearDown(); }
    public void ClearDash() { upgradeHandler?.ClearDash(); }
}
