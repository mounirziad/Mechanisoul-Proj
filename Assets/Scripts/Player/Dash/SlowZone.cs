using UnityEngine;

public interface ISlowable { void AddSlow(float percent, float seconds); }

[RequireComponent(typeof(SphereCollider))]
public class SlowZone : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 3f;

    [Header("Area")]
    public float radius = 1.25f;
    public LayerMask enemyMask;

    float slowPercent = 0.25f;
    float slowSeconds = 2f;

    SphereCollider col;
    float t;

    public void Configure(float percent, float seconds)
    {
        slowPercent = Mathf.Clamp01(percent);
        slowSeconds = Mathf.Max(0f, seconds);
    }

    void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        var slowable = other.GetComponentInParent<ISlowable>();
        if (slowable != null)
        {
            slowable.AddSlow(slowPercent, slowSeconds);
            return;
        }
        other.SendMessage("ApplySlow", new Vector2(slowPercent, slowSeconds), SendMessageOptions.DontRequireReceiver);
    }
}
