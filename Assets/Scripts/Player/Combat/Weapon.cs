using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;

    BoxCollider triggerBox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.CompareTag("Enemy");
        if(enemy != null)
        {
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
