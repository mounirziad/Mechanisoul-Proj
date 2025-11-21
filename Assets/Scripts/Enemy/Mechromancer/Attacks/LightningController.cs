using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.VFX;

public class LightningController : MonoBehaviour
{
    [Header("Lightning")]
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private float lightningDuration = 1.5f;
    [SerializeField] private Transform lightningOrigin;

    private GameObject activeLightning;
    private bool isAttacking;

    public BlackboardReference blackboard;

    public void CastLightning()
    {
        Vector3 playerPos = GameObject.FindWithTag("Player").transform.position;
        CastLightningAtGround(playerPos, 3f, 15f);
    }

    public void CastLightningAtGround(Vector3 centerPosition, float radius, float damage)
    {
        if (isAttacking) return;
        isAttacking = true;

        Vector3 targetPosition = centerPosition + new Vector3(
            Random.Range(-radius, radius),
            0f,
            Random.Range(-radius, radius)
        );

        if (Physics.Raycast(targetPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            targetPosition = hit.point;
        }

        activeLightning = Instantiate(lightningPrefab, lightningOrigin.position, Quaternion.identity);
        LightningStrike strike = activeLightning.GetComponent<LightningStrike>();

        if (strike != null)
        {
            strike.Initialize(targetPosition, this);
        }

        Debug.Log($"Lightning cast toward ground at {targetPosition}");

        StartCoroutine(ApplyAoEDamage(targetPosition, radius, damage));

        Invoke(nameof(StopLightning), lightningDuration);
    }

    private IEnumerator ApplyAoEDamage(Vector3 center, float radius, float damage)
    {
        yield return new WaitForSeconds(0.25f);

        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (Collider hit in hits)
        {
            IDamage damageable = hit.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Lightning hit {hit.name} for {damage} damage");
            }
        }

        blackboard.SetVariableValue("LightningFinished", true);
    }

    public void OnStrike(Vector3 hitPos)
    {
        Debug.Log("Lightning reached ground at " + hitPos);

        StartCoroutine(ApplyAoEDamage(hitPos, 3f, 15f));
    }

    public void StopLightning()
    {
        if (activeLightning != null)
            Destroy(activeLightning);

        //isAttacking = false;
        Debug.Log("Lightning stopped");
    }
}
