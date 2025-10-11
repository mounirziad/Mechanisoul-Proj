using UnityEngine;

[RequireComponent(typeof(DashAbility))]
public abstract class DashUpgradeBase : Upgrade
{
    [Header("Links")]
    [SerializeField] protected DashAbility dash;

    protected override void Awake()
    {
        base.Awake();
        if (!dash) dash = GetComponent<DashAbility>();
    }

    protected virtual void OnEnable()
    {
        if (dash)
        {
            dash.OnDashStep += HandleDashStep;
            dash.OnDashFinished += HandleDashFinished;
        }
    }

    protected virtual void OnDisable()
    {
        if (dash)
        {
            dash.OnDashStep -= HandleDashStep;
            dash.OnDashFinished -= HandleDashFinished;
        }
    }

    protected virtual void HandleDashStep(Vector3 pos) { }
    protected virtual void HandleDashFinished(Vector3 start, Vector3 end) { }

    // helper functions, pls ignore
    protected T Spawn<T>(T prefab, Vector3 pos) where T : Component
    {
        if (!prefab) return null;
        return Instantiate(prefab, pos, Quaternion.identity);
    }
}
