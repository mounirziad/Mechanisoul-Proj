using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RaycastWeapon weaponFab; // prefab reference

    public bool isTaken = false;   

    private void OnTriggerEnter(Collider other)
    {
        AiWeapons aiWeapons = other.GetComponentInParent<AiWeapons>();
        if (aiWeapons && !aiWeapons.HasWeapon() && !isTaken)
        {
            isTaken = true; // mark as taken

            RaycastWeapon newWeapon = Instantiate(weaponFab);

            aiWeapons.Equip(newWeapon);

            Destroy(gameObject); // remove pickup after taking it
        }
    }
}
