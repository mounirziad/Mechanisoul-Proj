using UnityEngine;

public class HitBox : MonoBehaviour
{
    public BasicEnemyHealth health;

    public void OnRayCastHit(Weapon weapon, Vector3 direction)
    {
        health.TakeDamage(weapon.damage, direction);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
