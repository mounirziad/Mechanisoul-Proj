using UnityEngine;
using UnityEngine.ProBuilder;

public class RaycastWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float fireRate = 0.25f;
    public float damage = 10f;
    public float range = 100f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 50f;

    private float lastFireTime;

    [Header("Visuals")]
    public ParticleSystem muzzleFlash;
    public LineRenderer bulletLine;

    public Transform firePoint;

    private int raycastLayerMask;

    private void Awake()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int characterLayer = LayerMask.NameToLayer("Character");
        int pickUpLayer = LayerMask.NameToLayer("PickUp");
        raycastLayerMask = ~((1 << enemyLayer) | (1 << characterLayer) | (1 << pickUpLayer));
    }

    public void Fire()
    {
        if (Time.time - lastFireTime < fireRate) return;
        lastFireTime = Time.time;

        if (muzzleFlash != null) muzzleFlash.Play();

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

        if (bulletLine != null && firePoint != null)
        {
            Vector3 startPos = firePoint.position;
            Vector3 endPos;
            
            RaycastHit hit;
            if (Physics.Raycast(startPos, firePoint.forward, out hit, range, raycastLayerMask))
            {
                endPos = hit.point;
            }
            else
            {
                endPos = startPos + firePoint.forward * range;
            }
            
            StartCoroutine(DrawBulletLine(startPos, endPos));
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
