using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RaycastWeapon weaponFab; // prefab reference

    private void OnTriggerEnter(Collider other)
    {
        HitBox hitBox = other.gameObject.GetComponent<HitBox>();
        if (hitBox)
        {
            AiWeapons weapons = hitBox.health.GetComponent<AiWeapons>();
            if (weapons != null)
            {
                // Spawn a weapon instance
                RaycastWeapon newWeapon = Instantiate(weaponFab);

                // Equip it on the AI
                weapons.Equip(newWeapon);

                // Destroy the pickup
                Destroy(gameObject);
            }
        }
    }
}
