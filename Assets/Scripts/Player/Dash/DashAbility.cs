using UnityEngine;

public class DashAbility : MonoBehaviour
{
    [Header("Dash Core")]
    public float dashDistance = 8f;
    public float dashDuration = 0.15f; // seconds
    public LayerMask collisionMask;

    [Header("VFX/Gameplay Zones")]
    public GameObject fireDoTPrefab;   // FireDoTZone (ANGER)
    public GameObject slowZonePrefab;  // SlowZone (SADNESS)

    // Internal dash state
    private bool isDashing;
    private Vector3 dashDir;
    private float dashTime;

    // Upgrades (set by UpgradeHandler or legacy PlayerManager)
    private DashUpgrades mods;

    /// Sets all dash upgrade values at once
    public void SetDashUpgrades(DashUpgrades u)
    {
        mods = u;
    }

    /// Perform the dash toward a world-space direction (normalized internally)
    public void PerformDash(Vector3 worldDirection)
    {
        if (isDashing) return;

        isDashing = true;
        dashTime = 0f;
        dashDir = worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : transform.forward;

        // Spawn anger DoT zone (if enabled)
        if (mods.angerDoTOnDash && fireDoTPrefab != null)
        {
            var go = Instantiate(fireDoTPrefab, transform.position, Quaternion.identity);
            var dot = go.GetComponent<FireDoTZone>();
            if (dot != null) dot.Configure(Mathf.Max(0f, mods.angerAOEPercent)); // percent (e.g., 0.15 = 15%)
        }

        // Spawn sadness slow zone
        if (mods.sadnessSlowPercent > 0f && mods.sadnessSlowSeconds > 0f && slowZonePrefab != null)
        {
            var slow = Instantiate(slowZonePrefab, transform.position, Quaternion.identity);
            var zone = slow.GetComponent<SlowZone>();
            if (zone != null) zone.Configure(mods.sadnessSlowPercent, mods.sadnessSlowSeconds);
        }
    }

    private void Update()
    {
        if (!isDashing) return;

        dashTime += Time.deltaTime;
        float t = Mathf.Clamp01(dashTime / Mathf.Max(0.0001f, dashDuration));
        float moveStep = (dashDistance / dashDuration) * Time.deltaTime;

        // Simple straight-line dash; add collision handling as needed
        transform.position += dashDir * moveStep;

        if (t >= 1f) EndDash();
    }

    private void EndDash()
    {
        isDashing = false;
    }

    // compatibility patches below

    public bool TryDash()
    {
        if (isDashing) return false;
        PerformDash(transform.forward);
        return true;
    }

    public bool TryDash(Vector3 worldDirection)
    {
        if (isDashing) return false;
        PerformDash(worldDirection);
        return true;
    }

    public void SetUpgrades(float angerAOEPercent, float sadnessSlowPercent, float sadnessSlowSeconds, float lifeStealUnused, float stunUnused)
    {
        mods.angerAOEPercent = Mathf.Max(0f, angerAOEPercent);
        mods.angerDoTOnDash = angerAOEPercent > 0f;
        mods.sadnessSlowPercent = Mathf.Clamp01(sadnessSlowPercent);
        mods.sadnessSlowSeconds = Mathf.Max(0f, sadnessSlowSeconds);
    }
}
