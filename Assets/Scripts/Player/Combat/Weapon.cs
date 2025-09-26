using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;

    [SerializeField] private GameObject hitVFX;

    BoxCollider triggerBox;
 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();

       

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit something");
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy");
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            if (hitVFX != null)
            {
                GameObject vfxInstance = Instantiate(hitVFX, contactPoint, Quaternion.identity);

                vfxInstance.GetComponent<VFXCameraLookAt>().distanceFromCamera = Vector3.Distance(contactPoint, Camera.main.transform.position) - 0.1f;
            }


            //subtract damage from enemy health

            //if enemy health is <= 0 
            //destroy enemy.gameobject
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
