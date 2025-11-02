using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class LightningController : MonoBehaviour
{
    [Header("Lightning")]
    [SerializeField] private GameObject lightning;
    [SerializeField] private float lightningDuration = 1.5f;
    [SerializeField] private Transform lightningOrigin;

    private GameObject activeLightning;
    private bool isAttacking;

    public void CastLightning(Vector3 targetPosition)
    {
        if (isAttacking) return;

        isAttacking = true;

        activeLightning = Instantiate(lightning, lightningOrigin.position, Quaternion.identity);

        LightningStrike strike = activeLightning.GetComponent<LightningStrike>();
        if (strike != null)
        {
            strike.Initialize(targetPosition);
        }

        Debug.Log("Lightning cast toward player last known position: {targetPosition}");

        Invoke(nameof(StopLightning), lightningDuration);
    }

    public void StopLightning()
    {
        if (!isAttacking) return;

        if (activeLightning != null)
        {
            Destroy(activeLightning);
        }

        isAttacking = false;
        Debug.Log("Lightning stopped");
    }
}
