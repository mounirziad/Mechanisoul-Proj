using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public float attackRange;

    public GameObject hitVFX;
    private BoxCollider triggerBox;

    public Camera cam;

    PlayerManager playerManager;

    private void Awake()
    {
        playerManager = transform.root.gameObject.GetComponent<PlayerManager>();
    }

    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
        triggerBox.isTrigger = true; // make sure it's set as a trigger
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
                // Still keep Mechromancer (if that’s another type of enemy)
                Mechromancer enemy = other.GetComponent<Mechromancer>();
                if (enemy != null)
                {
                    Debug.Log("Hit mechromancer");
                    enemy.TakeDamage(damage);
                }

                // spawn hit VFX
                Vector3 contactPoint = other.ClosestPoint(transform.position);
            if (hitVFX != null)
            {
                GameObject vfxInstance = Instantiate(hitVFX, contactPoint, Quaternion.identity);
                vfxInstance.GetComponent<VFXCameraLookAt>().distanceFromCamera =
                    Vector3.Distance(contactPoint, Camera.main.transform.position) - 0.1f;
            }

            // hitbox damage forwarding
            Vector3 dir = (other.transform.position - transform.position).normalized;
            HitBox hitBox = other.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.OnRayCastHit(this, dir);
            }
           
            BasicEnemyHealth basicEEnemy = other.GetComponent<BasicEnemyHealth>();
            if (basicEEnemy != null)
            {
                Debug.Log("Hit basic enemy");
                Vector3 direction = (other.transform.position - transform.position).normalized;
                basicEEnemy.TakeDamage(damage, direction);

                basicEEnemy.ApplyModifiers(
                     playerManager.GetSlowAmount(),
                     playerManager.GetSlowLength(),
                     playerManager.GetStunLength()
                 );
                playerManager.ApplyLifesteal(damage);
            }
        }
    }

    public void EnableTriggerBox()
    {
        triggerBox.enabled = true;
    }

    public void DisableTriggerBox()
    {
        triggerBox.enabled = false;
    }
}
