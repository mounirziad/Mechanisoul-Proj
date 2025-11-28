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
    [SerializeField] private Transform weaponParent;
    
    [Header("Death Camera Settings")]
    [SerializeField] private Camera deathCamera;
    [SerializeField] private float deathCameraDepth = 100f;
    
    [Header("Death UI Settings")]
    [SerializeField] private GameObject deathUI;
    
    [Header("Player Control Settings")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PlayerHealth playerHealth;
    
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool weaponInitialGravity;
    private float initialDeathCameraDepth;
    private bool weaponColliderInitialState;
    private Vector3 weaponInitialLocalPosition;
    private Quaternion weaponInitialLocalRotation;
    private bool weaponInitialKinematic;

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
            weaponInitialKinematic = weaponRigidbody.isKinematic;
            
            if (weaponParent == null)
            {
                weaponParent = weaponRigidbody.transform.parent;
            }
            
            weaponInitialLocalPosition = weaponRigidbody.transform.localPosition;
            weaponInitialLocalRotation = weaponRigidbody.transform.localRotation;
        }
        
        if (deathCamera != null)
        {
            initialDeathCameraDepth = deathCamera.depth;
        }
        
        if (playerWeapon != null)
        {
            BoxCollider weaponCollider = playerWeapon.GetComponent<BoxCollider>();
            if (weaponCollider != null)
            {
                weaponColliderInitialState = weaponCollider.enabled;
            }
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
            weaponRigidbody.isKinematic = weaponInitialKinematic;
            weaponRigidbody.useGravity = weaponInitialGravity;
            weaponRigidbody.linearVelocity = Vector3.zero;
            weaponRigidbody.angularVelocity = Vector3.zero;
            
            if (weaponParent != null)
            {
                weaponRigidbody.transform.SetParent(weaponParent);
            }
            
            weaponRigidbody.transform.localPosition = weaponInitialLocalPosition;
            weaponRigidbody.transform.localRotation = weaponInitialLocalRotation;
        }

        if (playerWeapon != null)
        {
            BoxCollider weaponCollider = playerWeapon.GetComponent<BoxCollider>();
            if (weaponCollider != null)
            {
                weaponCollider.enabled = weaponColliderInitialState;
            }
        }

        if (deathCamera != null)
        {
            deathCamera.depth = initialDeathCameraDepth;
        }

        if (deathUI != null)
        {
            deathUI.SetActive(false);
        }

        if (inputManager != null)
        {
            inputManager.SetMovementInputActive(true);
        }

        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = true;
        }

        if (playerManager != null)
        {
            playerManager.enabled = true;
        }

        if (playerHealth != null)
        {
            playerHealth.Revive();
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
            weaponRigidbody.isKinematic = false;
        }

        if (playerWeapon != null)
        {
            playerWeapon.DisableTriggerBox();
        }

        if (deathCamera != null)
        {
            deathCamera.depth = deathCameraDepth;
        }

        if (deathUI != null)
        {
            deathUI.SetActive(true);
        }

        if (inputManager != null)
        {
            inputManager.SetMovementInputActive(false);
        }

        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = false;
        }

        if (playerManager != null)
        {
            playerManager.enabled = false;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
