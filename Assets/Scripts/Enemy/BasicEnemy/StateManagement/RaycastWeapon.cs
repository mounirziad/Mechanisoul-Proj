using UnityEngine;
using UnityEngine.ProBuilder;

public class RaycastWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float fireRate = 0.25f;       // shots per second
    public float damage = 10f;           // damage per hit
    public float range = 100f;           // raycast range

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 50f;

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

        // Spawn projectile
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                projScript.damage = damage;
                projScript.speed = projectileSpeed;
            }
        }

        // Still keep bullet line for optional visual
        if (bulletLine != null)
        {
            StartCoroutine(DrawBulletLine(firePoint.position, firePoint.position + firePoint.forward * range));
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
