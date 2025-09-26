using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;

    [SerializeField] private GameObject hitVFX;
    private BoxCollider triggerBox;

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
            Debug.Log("Hit enemy");
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
