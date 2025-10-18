using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResetAllUpgradesPickup : MonoBehaviour
{
    [SerializeField] private UpgradeHandler handler;
    [SerializeField] private bool destroyOnPickup = true;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!handler) handler = FindObjectOfType<UpgradeHandler>();
        if (!handler) { Debug.LogWarning("[ResetAllUpgradesPickup] UpgradeHandler not found."); return; }

        // Drive everything to 0 using existing public Up/Down methods.
        for (int i = 0; i < 10; i++)
        {
            // Melee
            handler.MeleeAngerDown(); handler.MeleeSadnessDown(); handler.MeleeLoveDown(); handler.MeleeFearDown(); handler.MeleeJoyDown();
            // Range
            handler.RangedJoyDown(); handler.RangedAngerDown();
            // Dash
            handler.DashAngerDown(); handler.DashSadnessDown(); handler.DashJoyDown(); handler.DashLoveDown(); handler.DashFearDown();
        }

        if (destroyOnPickup) Destroy(gameObject);
    }
}