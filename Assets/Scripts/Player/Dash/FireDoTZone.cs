using System.Collections;
using UnityEngine;

public class FireDoTZone : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 4f;
    public float tickInterval = 0.5f;

    [Header("Area")]
    public float radius = 2.0f;
    public LayerMask enemyMask;

    // Upgrade-configured
    float aoePercentOfPlayerDamage = 0.1f;

    public void Configure(float aoePercent) => aoePercentOfPlayerDamage = Mathf.Max(0f, aoePercent);

    void OnEnable() { StartCoroutine(TickRoutine()); }

    IEnumerator TickRoutine()
    {
        float t = 0f;
        while (t < lifetime)
        {
            DealDamage();
            yield return new WaitForSeconds(tickInterval);
            t += tickInterval;
        }
        Destroy(gameObject);
    }

    void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask);
        foreach (var h in hits)
        {
            // Replace with your game's damage application
            h.SendMessage("ApplyDamageFromAOE", aoePercentOfPlayerDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
