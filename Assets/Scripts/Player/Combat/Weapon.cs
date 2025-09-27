using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public float attackRange;

    [SerializeField] private GameObject hitVFX;
    private BoxCollider triggerBox;

    public Camera cam;

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
            //Added if statement - Alyssa
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackRange))
            {
                Mechromancer enemy = hit.collider.GetComponent<Mechromancer>();
                if (enemy!= null)
                {
                    Debug.Log("Hit enemy");
                    enemy.TakeDamage(8f);
                }
            }
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            // spawn hit effect
            if (hitVFX != null)
            {
                GameObject vfxInstance = Instantiate(hitVFX, contactPoint, Quaternion.identity);
                vfxInstance.GetComponent<VFXCameraLookAt>().distanceFromCamera =
                    Vector3.Distance(contactPoint, Camera.main.transform.position) - 0.1f;
            }

            // choose your direction
            Vector3 direction = (other.transform.position - transform.position).normalized;

            // damage enemy through HitBox
            HitBox hitBox = other.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.OnRayCastHit(this, direction);
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
