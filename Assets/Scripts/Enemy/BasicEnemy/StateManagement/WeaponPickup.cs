using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RaycastWeapon weaponFab; // prefab reference

    private void OnTriggerEnter(Collider other)
    {
        AiWeapons aiWeapons = other.GetComponentInParent<AiWeapons>();
        if (aiWeapons && !aiWeapons.HasWeapon())
        {
            RaycastWeapon newWeapon = Instantiate(weaponFab);
            aiWeapons.Equip(newWeapon);
            Destroy(gameObject);
        }
    }
}
