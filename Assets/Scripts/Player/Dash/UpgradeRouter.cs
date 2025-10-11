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

        // Register these in the handler
        upgradeHandler.InjectDashComponents(joy, anger, sad, love, fear);
    }

    T Ensure<T>() where T : Behaviour
    {
        var c = GetComponent<T>();
        if (!c) c = gameObject.AddComponent<T>();
        c.enabled = false;
        return c;
    }
}
