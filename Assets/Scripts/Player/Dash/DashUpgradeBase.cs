using UnityEngine;

[DisallowMultipleComponent]
public class DashUpgradeBase : Upgrade   // <- inherit from Upgrade so assignments work
{
    [Header("Upgrade")]
    [Range(0, 3)] public int upgradeLevel = 0;

    [Header("Legacy Spawner (optional)")]
    [Tooltip("When ON, this component subscribes to Dash events and spawns its own effect (legacy behavior). " +
             "Leave OFF when using DashRingSpawner as the single source of truth.")]
    public bool autoSubscribeToDash = false;

    protected DashAbility dash;

    protected virtual void Awake()
    {
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

    public void SetLevel(int level)
    {
        upgradeLevel = Mathf.Clamp(level, 0, 3);
#if UNITY_EDITOR
        // lightweight trace so you can see who set what
        Debug.Log($"[{GetType().Name}] SetLevel -> {upgradeLevel}", this);
#endif
    }
    protected virtual void HandleDashFinished(Vector3 start, Vector3 end) { }
    protected T Spawn<T>(T prefab, Vector3 pos) where T : Component
    {
        if (!prefab) return null;
        return Instantiate(prefab, pos, Quaternion.identity);
    }
}
