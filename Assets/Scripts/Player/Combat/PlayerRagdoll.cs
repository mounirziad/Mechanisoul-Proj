using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Animator mainAnimator;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Collider playerCollider;
    
    [Header("Weapon Settings")]
    [SerializeField] private Weapon playerWeapon;
    [SerializeField] private Rigidbody weaponRigidbody;
    
    [Header("Death Camera Settings")]
    [SerializeField] private Camera deathCamera;
    [SerializeField] private float deathCameraDepth = 100f;
    
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool weaponInitialGravity;
    private float initialDeathCameraDepth;

    private void Start()
    {
        if (ragdollRoot != null)
        {
            ragdollRigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
            ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
        }
        
        if (weaponRigidbody != null)
        {
            weaponInitialGravity = weaponRigidbody.useGravity;
        }
        
        if (deathCamera != null)
        {
            initialDeathCameraDepth = deathCamera.depth;
        }
        
        DeactivateRagdoll();
    }

    public void DeactivateRagdoll()
    {
        if (ragdollRigidbodies != null)
        {
            foreach (var rb in ragdollRigidbodies)
            {
                rb.isKinematic = true;
            }
        }

        if (ragdollColliders != null)
        {
            foreach (var col in ragdollColliders)
            {
                col.enabled = false;
            }
        }

        if (mainAnimator != null)
        {
            mainAnimator.enabled = true;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        if (weaponRigidbody != null)
        {
            weaponRigidbody.useGravity = weaponInitialGravity;
        }

        if (playerWeapon != null)
        {
            playerWeapon.EnableTriggerBox();
        }

        if (deathCamera != null)
        {
            deathCamera.depth = initialDeathCameraDepth;
        }
    }

    public void ActivateRagdoll()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        if (ragdollRigidbodies != null)
        {
            foreach (var rb in ragdollRigidbodies)
            {
                rb.isKinematic = false;
            }
        }

        if (ragdollColliders != null)
        {
            foreach (var col in ragdollColliders)
            {
                col.enabled = true;
            }
        }

        if (mainAnimator != null)
        {
            mainAnimator.enabled = false;
        }

        if (weaponRigidbody != null)
        {
            weaponRigidbody.useGravity = true;
        }

        if (playerWeapon != null)
        {
            playerWeapon.DisableTriggerBox();
        }

        if (deathCamera != null)
        {
            deathCamera.depth = deathCameraDepth;
        }
    }

    public void ApplyForce(Vector3 force)
    {
        if (mainAnimator != null && ragdollRigidbodies != null && ragdollRigidbodies.Length > 0)
        {
            var hipsTransform = mainAnimator.GetBoneTransform(HumanBodyBones.Hips);
            if (hipsTransform != null)
            {
                var hipsRigidbody = hipsTransform.GetComponent<Rigidbody>();
                if (hipsRigidbody != null)
                {
                    hipsRigidbody.AddForce(force, ForceMode.VelocityChange);
                }
            }
        }
    }
}
