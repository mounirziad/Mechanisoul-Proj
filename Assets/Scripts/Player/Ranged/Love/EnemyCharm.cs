using System.Collections;
using UnityEngine;

public class EnemyCharm : MonoBehaviour
{
    public bool IsCharmed { get; private set; }
    public Transform CharmFocus { get; private set; }
    [SerializeField] float defaultCharmSeekRadius = 12f;
    [SerializeField] LayerMask enemyLayer = ~0;

    public bool ShouldIgnorePlayerAndFightEnemies() => IsCharmed;
    public Transform GetCharmAttackTarget(Transform self) => GetOrPickFocus(self);

    BasicEnemyHealth health;
    Coroutine c;

    void Awake()
    {
        health = GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }

    void HandleDeath()
    {
        if (c != null)
        {
            StopCoroutine(c);
            c = null;
        }
        IsCharmed = false;
        CharmFocus = null;
    }

    public void ApplyCharm(float seconds, Transform focus = null)
    {
        if (seconds <= 0f) return;
        if (health != null && health.currentHealth <= 0) return;
        CharmFocus = focus;
        if (c != null) StopCoroutine(c);
        c = StartCoroutine(CharmCR(seconds));
    }

    IEnumerator CharmCR(float t)
    {
        IsCharmed = true;
        yield return new WaitForSeconds(t);
        IsCharmed = false;
        CharmFocus = null;
        c = null;
    }

    public Transform GetOrPickFocus(Transform self, float radiusOverride = -1f)
    {
        if (CharmFocus != null) return CharmFocus;

        float r = (radiusOverride > 0f) ? radiusOverride : defaultCharmSeekRadius;
        var hits = Physics.OverlapSphere(self.position, r, enemyLayer, QueryTriggerInteraction.Ignore);

        Transform best = null;
        float bestSqr = float.MaxValue;

        foreach (var h in hits)
        {
            if (h.transform == self) continue;
            if (h.GetComponent<EnemyCharm>() == null) continue; // ensure it's an enemy
            float sq = (h.transform.position - self.position).sqrMagnitude;
            if (sq < bestSqr) { bestSqr = sq; best = h.transform; }
        }
        CharmFocus = best;
        return CharmFocus;
    }
}
