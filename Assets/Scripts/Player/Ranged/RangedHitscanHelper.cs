using UnityEngine;

public static class RangedHitscanHelper
{
    // called from PlayerCombat.ShootHitscan
    public static void Apply(
        float baseDamage,
        RangedModifiers mods,
        BasicEnemyHealth hp,
        Vector3 hitPoint,
        Vector3 hitDir,
        Transform hitRoot,
        LayerMask enemyLayer)
    {
        if (!hp) return;

        // ----- damage + joy crit -----
        float dmg = baseDamage * Mathf.Max(0.01f, mods.joyDamageMultiplier);
        bool crit = mods.joyCritChance > 0f && Random.value < mods.joyCritChance;
        if (crit) dmg *= Mathf.Max(1f, mods.joyCritMultiplier);

        hp.TakeDamage(dmg, hitDir);
        if (crit) RangedDebug.Log($"[ranged/joy] crit -> {dmg:0.0}");

        // ----- anger splash -----
        if (mods.angerExplosionOnHit && mods.angerAOEPercent > 0f)
        {
            int hitCount = 0;
            var hits = Physics.OverlapSphere(hitPoint, mods.angerAOEPercent, enemyLayer, QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
            {
                var ehp = h.GetComponent<BasicEnemyHealth>();
                if (!ehp) continue;
                ehp.TakeDamage(baseDamage * 0.5f, (ehp.transform.position - hitPoint).normalized);
                hitCount++;
            }
            RangedDebug.Log($"[ranged/anger] splash hit {hitCount} (r={mods.angerAOEPercent:0.0})");
        }

        // ----- sadness slow stacks -----
        if (mods.sadnessSlowPerStack > 0f && mods.sadnessMaxStacks > 0)
        {
            var slow = hitRoot ? hitRoot.GetComponent<EnemySlowStacks>() : null;
            if (slow)
            {
                slow.ApplyStackingSlow(mods.sadnessSlowPerStack, mods.sadnessMaxStacks);
                RangedDebug.Log($"[ranged/sadness] stacks={slow.CurrentStacks} speed={(slow.CurrentSpeedMultiplier * 100f):0}%");
            }
        }

        // ----- love charm -----
        if (mods.loveCharmChance > 0f && Random.value < mods.loveCharmChance)
        {
            var charm = hitRoot ? hitRoot.GetComponent<EnemyCharm>() : null;
            if (charm)
            {
                charm.ApplyCharm(mods.loveCharmDuration);
                RangedDebug.Log($"[ranged/love] charmed {mods.loveCharmDuration:0.0}s");
            }
            else
            {
                RangedDebug.Log("[ranged/love] triggered but EnemyCharm missing");
            }
        }

        // ----- fear stun -----
        if (mods.fearStunDuration > 0f)
        {
            var stun = hitRoot ? hitRoot.GetComponent<EnemyStun>() : null;
            if (stun)
            {
                stun.ApplyStun(mods.fearStunDuration);
                RangedDebug.Log($"[ranged/fear] stunned {mods.fearStunDuration:0.00}s");
            }
            else
            {
                RangedDebug.Log("[ranged/fear] triggered but EnemyStun missing");
            }
        }
    }
}
