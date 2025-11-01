using UnityEngine;

[DisallowMultipleComponent]
public class UpgradeRouter : MonoBehaviour
{
    [SerializeField] UpgradeHandler upgradeHandler;   // big controller
    [SerializeField] DashAbility dashAbility;         // movement logic

    // references to all possible upgrade scripts
    JoyDashUpgrade joy;
    AngerDashUpgrade anger;
    SadnessDashUpgrade sad;
    LoveDashUpgrade love;
    FearDashUpgrade fear;

    void Awake()
    {
        if (!upgradeHandler) upgradeHandler = GetComponent<UpgradeHandler>();
        if (!dashAbility) dashAbility = GetComponent<DashAbility>();

        // Autospawn or fetch upgrade scripts if not already on prefab
        joy = Ensure<JoyDashUpgrade>();
        anger = Ensure<AngerDashUpgrade>();
        sad = Ensure<SadnessDashUpgrade>();
        love = Ensure<LoveDashUpgrade>();
        fear = Ensure<FearDashUpgrade>();

        upgradeHandler.InjectDashComponents(joy, anger, sad, love, fear);
    }

    T Ensure<T>() where T : Behaviour
    {
        var c = GetComponent<T>();
        //if (!c) c = gameObject.AddComponent<T>();
        //c.enabled = false;
        return c;
    }

    // RANGED (ui hooks)
    public void RangedJoyUp() { if (upgradeHandler) upgradeHandler.RangedJoyUp(); }
    public void RangedJoyDown() { if (upgradeHandler) upgradeHandler.RangedJoyDown(); }

    public void RangedAngerUp() { if (upgradeHandler) upgradeHandler.RangedAngerUp(); }
    public void RangedAngerDown() { if (upgradeHandler) upgradeHandler.RangedAngerDown(); }

    public void RangedSadnessUp() { if (upgradeHandler) upgradeHandler.RangedSadnessUp(); }
    public void RangedSadnessDown() { if (upgradeHandler) upgradeHandler.RangedSadnessDown(); }

    public void RangedLoveUp() { if (upgradeHandler) upgradeHandler.RangedLoveUp(); }
    public void RangedLoveDown() { if (upgradeHandler) upgradeHandler.RangedLoveDown(); }

    public void RangedFearUp() { if (upgradeHandler) upgradeHandler.RangedFearUp(); }
    public void RangedFearDown() { if (upgradeHandler) upgradeHandler.RangedFearDown(); }
}
