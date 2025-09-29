using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public float attackRange;

    [SerializeField] private GameObject hitVFX;
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
        Debug.Log("Hit something");
        if (other.CompareTag("Enemy"))
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackRange))
            {
                BasicEnemyHealth basicEnemy = other.GetComponent<BasicEnemyHealth>();
                if (basicEnemy != null)
                {
                    Debug.Log("Hit basic enemy");
                    Vector3 direction = (other.transform.position - transform.position).normalized;
                    basicEnemy.TakeDamage(damage, direction);

                    basicEnemy.ApplyModifiers(
                         playerManager.GetSlowAmount(),
                         playerManager.GetSlowLength(),
                         playerManager.GetStunLength()
                     );
                }

                // Still keep Mechromancer (if that’s another type of enemy)
                Mechromancer enemy = hit.collider.GetComponent<Mechromancer>();
                if (enemy != null)
                {
                    Debug.Log("Hit mechromancer");
                    enemy.TakeDamage(damage);
                }
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
