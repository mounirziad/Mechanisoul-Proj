using UnityEngine;

public class AiWeapons : MonoBehaviour
{
    [Header("Weapon Offsets")]
    public Vector3 handOffsetPosition;
    public Vector3 handOffsetRotation;
    public Vector3 backOffsetPosition;
    public Vector3 backOffsetRotation;

    private RaycastWeapon currentWeapon;
    private Animator animator;

    private Transform rightHand;
    private Transform spine;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Cache important bones from Humanoid rig
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
    }

    public RaycastWeapon CurrentWeapon => currentWeapon;

    public bool HasWeapon()
    {
        return currentWeapon != null;
    }

    /// <summary>
    /// Assign existing weapon instance and holster it on back.
    /// </summary>
    public void Equip(RaycastWeapon weapon)
    {
        currentWeapon = weapon;

        // Parent to back by default (holstered)
        currentWeapon.transform.SetParent(spine, false);
        currentWeapon.transform.localPosition = backOffsetPosition;
        currentWeapon.transform.localRotation = Quaternion.Euler(backOffsetRotation);
    }

    public void ActivateWeapon()
    {
        if (animator) animator.SetTrigger("Equip");
    }

    public void OnAnimationEvent(string eventName)
    {
        if (currentWeapon == null) return;

        if (eventName == "equipWeapon")
        {
            currentWeapon.transform.SetParent(rightHand, false);
            currentWeapon.transform.localPosition = handOffsetPosition;
            currentWeapon.transform.localRotation = Quaternion.Euler(handOffsetRotation);
        }
        else if (eventName == "holsterWeapon")
        {
            currentWeapon.transform.SetParent(spine, false);
            currentWeapon.transform.localPosition = backOffsetPosition;
            currentWeapon.transform.localRotation = Quaternion.Euler(backOffsetRotation);
        }
    }

    public void DropWeapon()
    {
        if (currentWeapon)
        {
            currentWeapon.transform.SetParent(null);
            var rb = currentWeapon.gameObject.AddComponent<Rigidbody>();
            currentWeapon.gameObject.GetComponent<Collider>().enabled = true;
            currentWeapon = null;
        }
    }
}
