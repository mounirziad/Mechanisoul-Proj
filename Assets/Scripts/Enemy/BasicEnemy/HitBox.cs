using UnityEngine;

public class HitBox : MonoBehaviour
{
    public BasicEnemyHealth health;

    public void OnRayCastHit(Weapon weapon, Vector3 direction)
    {
        health.TakeDamage(weapon.damage, direction);
    }

    public void OnRayCastHit(Weapon weapon, Vector3 direction, Vector3 hitPoint)
    {
        health.TakeDamageAtPosition(weapon.damage, direction, hitPoint);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
