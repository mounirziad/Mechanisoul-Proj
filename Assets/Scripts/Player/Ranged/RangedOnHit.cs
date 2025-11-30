using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RangedOnHit : MonoBehaviour
{
    [Header("damage")]
    [SerializeField] float baseDamage = 10f;

    [Header("state")]
    [SerializeField] LayerMask enemyLayer = ~0;
    [SerializeField] float lifetime = 3f;
    RangedModifiers mods;

    void Start() { Destroy(gameObject, lifetime); }

    public void SetModifiers(RangedModifiers m) { mods = m; }

    void OnTriggerEnter(Collider other)
    {
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        var hp = other.GetComponent<BasicEnemyHealth>();
        if (!hp) return;

        // damage + joy crit
        float damage = baseDamage * Mathf.Max(0.01f, mods.joyDamageMultiplier);
        bool crit = mods.joyCritChance > 0f && Random.value < mods.joyCritChance;
        if (crit) damage *= Mathf.Max(1f, mods.joyCritMultiplier);

        hp.TakeDamage(damage, (hp.transform.position - transform.position).normalized);
        if (crit) RangedDebug.Log($"[ranged] joy crit -> {damage:0.0}");

        // anger splash
        if (mods.angerExplosionOnHit && mods.angerAOEPercent > 0f)
            DoAngerExplosion(other.transform.position);

        // sadness stacks
        if (mods.sadnessSlowPerStack > 0f && mods.sadnessMaxStacks > 0)
        {
            var slow = other.GetComponent<EnemySlowStacks>();
            if (slow)
            {
                slow.ApplyStackingSlow(mods.sadnessSlowPerStack, mods.sadnessMaxStacks);
                RangedDebug.Log($"[ranged] sadness stack -> {slow.CurrentStacks} at {(slow.CurrentSpeedMultiplier * 100f):0}% speed");

                // Use whatever duration matches slow decay
                StatusEffectUtility.ApplyStatus(other, StatusEffectType.Sadness, 3f);
            }
        }

        // love charm
        if (mods.loveCharmChance > 0f && Random.value < mods.loveCharmChance)
        {
            var charm = other.GetComponent<EnemyCharm>();
            if (charm)
            {
                charm.ApplyCharm(mods.loveCharmDuration);
                RangedDebug.Log($"[ranged] love charm for {mods.loveCharmDuration:0.0}s");

                StatusEffectUtility.ApplyStatus(other, StatusEffectType.Love, mods.loveCharmDuration);
            }
            else
            {
                RangedDebug.Log("[ranged] love triggered but EnemyCharm missing");
            }
        }

        // fear stun (single target)
        if (mods.fearStunDuration > 0f)
        {
            var stun = other.GetComponent<EnemyStun>();
            if (stun)
            {
                stun.ApplyStun(mods.fearStunDuration);
                RangedDebug.Log($"[ranged] fear stun for {mods.fearStunDuration:0.00}s");

                StatusEffectUtility.ApplyStatus(other, StatusEffectType.Fear, mods.fearStunDuration);
            }
            else
            {
                RangedDebug.Log("[ranged] fear triggered but EnemyStun missing");
            }
        }

        Destroy(gameObject);
    }


    void DoAngerExplosion(Vector3 pos)
    {
        int hitCount = 0;
        var hits = Physics.OverlapSphere(pos, mods.angerAOEPercent, enemyLayer, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            var hp = h.GetComponent<BasicEnemyHealth>();
            if (!hp) continue;
            hp.TakeDamage(baseDamage * 0.5f, (hp.transform.position - pos).normalized);
            hitCount++;
        }
        RangedDebug.Log($"[ranged] anger splash hit {hitCount} enemies (r={mods.angerAOEPercent:0.0})");
    }
}
