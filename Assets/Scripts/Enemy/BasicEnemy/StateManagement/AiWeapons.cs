using UnityEngine;

public class AiWeapons : MonoBehaviour
{
    RaycastWeapon currentWeapon;

    // Assign the weapon and parent it to the AI
    public void Equip(RaycastWeapon weapon)
    {
        currentWeapon = weapon;

        currentWeapon.transform.SetParent(transform, false);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }
}
