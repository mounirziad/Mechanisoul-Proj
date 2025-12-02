using UnityEngine;
using System.Collections;

public class BreakRoomInitializer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float healAmount = 100f;
    [SerializeField] private float initializationDelay = 0.5f;

    private void Start()
    {
        StartCoroutine(InitializeBreakRoom());
    }

    private IEnumerator InitializeBreakRoom()
    {
        yield return new WaitForSeconds(initializationDelay);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Revive();
                playerHealth.Heal(healAmount);
            }

            PlayerRagdoll playerRagdoll = player.GetComponent<PlayerRagdoll>();
            if (playerRagdoll != null)
            {
                playerRagdoll.DeactivateRagdoll();
            }

            PlayerLocomotion playerLocomotion = player.GetComponent<PlayerLocomotion>();
            if (playerLocomotion != null)
            {
                playerLocomotion.enabled = true;
            }

            if (PersistentDeathUI.Instance != null)
            {
                PersistentDeathUI.Instance.gameObject.SetActive(false);
            }

            if (UpgradeSnapshotManager.Instance != null)
            {
                Debug.Log("BreakRoomInitializer: Calling RestoreFloorEntryState");
                UpgradeSnapshotManager.Instance.RestoreFloorEntryState();
            }
            else
            {
                Debug.LogError("BreakRoomInitializer: UpgradeSnapshotManager.Instance is null!");
            }

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Debug.Log("BreakRoomInitializer: Player state fully reset in break room");
        }
        else
        {
            Debug.LogWarning("BreakRoomInitializer: Player not found!");
        }
    }
}
