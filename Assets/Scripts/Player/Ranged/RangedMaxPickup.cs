using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RangedMaxPickup : MonoBehaviour
{
    [SerializeField] private UpgradeHandler handler;
    [SerializeField] private bool destroyOnPickup = true;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!handler) handler = FindObjectOfType<UpgradeHandler>();
        if (!handler) { Debug.LogWarning("[RangedMaxPickup] UpgradeHandler not found."); return; }

        // zero out first
        for (int i = 0; i < 10; i++)
        {
            handler.RangedJoyDown();
            handler.RangedAngerDown();
            handler.RangedSadnessDown();
            handler.RangedLoveDown();
            handler.RangedFearDown();
        }

        // then set to level 2
        for (int i = 0; i < 2; i++)
        {
            handler.RangedJoyUp();
            handler.RangedAngerUp();
            handler.RangedSadnessUp();
            handler.RangedLoveUp();
            handler.RangedFearUp();
        }

        Debug.Log("[RangedMaxPickup] all ranged upgrades set to level 2");

        if (destroyOnPickup) Destroy(gameObject);
    }
}
