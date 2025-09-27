using UnityEngine;

public class AiWeapons : MonoBehaviour
{
    RaycastWeapon currentWeapon;
    Animator animator;
    MeshSockets sockets; 
    private void Start()
    {
        animator = GetComponent<Animator>();
        sockets = GetComponent<MeshSockets>();
    }
    // Assign the weapon and parent it to the AI
    public void Equip(RaycastWeapon weapon)
    {
        currentWeapon = weapon;

        sockets.Attach(weapon.transform, MeshSockets.SocketId.Spine);
    }

    public void ActivateWeapon()
    {
        animator.SetBool("Equip", true);
    }

    public bool HasWeapon()
    {
        return currentWeapon != null;
    }

    public void DropWeapon()
    {
        if (currentWeapon)
        {
            currentWeapon.transform.SetParent(null);
            currentWeapon.gameObject.GetComponent<BoxCollider>().enabled = true;
            currentWeapon.gameObject.AddComponent<Rigidbody>();
            currentWeapon = null;
        }
    }

    public void OnAnimationEvent(string eventName)
    {
        if(eventName == "equipWeapon")
        {
            sockets.Attach(currentWeapon.transform, MeshSockets.SocketId.RightHand);
        }
    }
}
