using UnityEngine;

public class RaycastWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float fireRate = 0.25f;       // shots per second
    public float damage = 10f;           // damage per hit
    public float range = 100f;           // raycast range

    private float lastFireTime;

    [Header("Visuals")]
    public ParticleSystem muzzleFlash;
    public LineRenderer bulletLine;

    public Transform firePoint;

    private void Awake()
    {
        
    }

    public void Fire()
    {
        if (Time.time - lastFireTime < fireRate) return; // cooldown
        lastFireTime = Time.time;

        // Play muzzle flash if set
        if (muzzleFlash != null) muzzleFlash.Play();

        // Shoot a ray forward
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log($"{name} hit {hit.collider.name}");

            // Check if the hit object has a Health component
            PlayerHealth health = hit.collider.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // Draw line if LineRenderer is set
            if (bulletLine != null)
            {
                StartCoroutine(DrawBulletLine(firePoint.position, hit.point));
            }
        }
        else
        {
            // No hit, draw line to max range
            if (bulletLine != null)
            {
                StartCoroutine(DrawBulletLine(firePoint.position, firePoint.position + firePoint.forward * range));
            }
        }
    }

    private System.Collections.IEnumerator DrawBulletLine(Vector3 start, Vector3 end)
    {
        bulletLine.positionCount = 2;
        bulletLine.SetPosition(0, start);
        bulletLine.SetPosition(1, end);

        bulletLine.enabled = true;
        yield return new WaitForSeconds(0.05f);
        bulletLine.enabled = false;
    }
}
