using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class JoyCritZone : MonoBehaviour
{
    [SerializeField, Min(0f)] float critChance = 0.40f; // +40% by default (set by JoyDashUpgrade tables), cant go lower unless off
    [SerializeField, Min(0.1f)] float radius = 3.0f;
    [SerializeField, Min(0.1f)] float lifetime = 2.0f;
    [SerializeField] string playerTag = "Player";

    SphereCollider col;
    PlayerCritBuff cachedBuff; // last player in zone (dash use-case usually 1)

    public void Configure(float chance, float size, float life = 2.0f)
    {
        critChance = Mathf.Max(0f, chance);
        radius = Mathf.Max(0.1f, size);
        lifetime = Mathf.Max(0.1f, life);
        ApplyCollider();
    }

    void Awake()
    {
        col = GetComponent<SphereCollider>();
        ApplyCollider();
    }

    void OnEnable()
    {
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    void ApplyCollider()
    {
        if (!col) col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.center = Vector3.zero;
        col.radius = radius;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var buff = other.GetComponentInParent<PlayerCritBuff>();
        if (!buff) buff = other.gameObject.AddComponent<PlayerCritBuff>();

        cachedBuff = buff;
        buff.AddOrUpdate(this, critChance);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var buff = other.GetComponentInParent<PlayerCritBuff>();
        if (buff) buff.Remove(this);
        if (cachedBuff == buff) cachedBuff = null;
    }

    void OnDestroy()
    {
        if (cachedBuff) cachedBuff.Remove(this);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, .92f, .16f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
