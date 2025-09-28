using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RaycastWeapon weaponFab; // prefab reference

    private void OnTriggerEnter(Collider other)
    {
        AiWeapons aiWeapons = other.GetComponentInParent<AiWeapons>();
        if (aiWeapons && !aiWeapons.HasWeapon())
        {
            // Instantiate once here
            RaycastWeapon newWeapon = Instantiate(weaponFab);

            // Equip the same instance
            aiWeapons.Equip(newWeapon);

            // Destroy pickup
            Destroy(gameObject);
        }
    }
}
