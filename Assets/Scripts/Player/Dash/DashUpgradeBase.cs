using UnityEngine;

[DisallowMultipleComponent]
public class DashUpgradeBase : Upgrade   // <- inherit from Upgrade so assignments work
{
    [Header("Legacy Spawner (optional)")]
    [Tooltip("When ON, this component subscribes to Dash events and spawns its own effect (legacy behavior). " +
             "Leave OFF when using DashRingSpawner as the single source of truth.")]
    public bool autoSubscribeToDash = false;

    protected DashAbility dash;

    protected override void Awake()
    {
        base.Awake();
        
        // Find DashAbility on self or parent
        dash = GetComponent<DashAbility>();
        if (!dash) dash = GetComponentInParent<DashAbility>();
    }

    protected virtual void OnEnable()
    {
        if (autoSubscribeToDash && dash != null)
            dash.OnDashFinished += HandleDashFinished;
    }

    protected virtual void OnDisable()
    {
        if (dash != null)
            dash.OnDashFinished -= HandleDashFinished;
    }

    protected virtual void HandleDashFinished(Vector3 start, Vector3 end) { }
    protected T Spawn<T>(T prefab, Vector3 pos) where T : Component
    {
        if (!prefab) return null;
        return Instantiate(prefab, pos, Quaternion.identity);
    }
}
