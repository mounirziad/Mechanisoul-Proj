using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DashLovePickup : MonoBehaviour
{
    [SerializeField] private UpgradeHandler handler;
    [SerializeField, Range(1,2)] private int targetLevel = 2;
    [SerializeField] private bool destroyOnPickup = true;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!handler) handler = FindObjectOfType<UpgradeHandler>();
        if (!handler) { Debug.LogWarning("[DashLovePickup] UpgradeHandler not found."); return; }

        for (int i = 0; i < 3; i++) { handler.DashAngerDown(); handler.DashSadnessDown(); handler.DashJoyDown(); handler.DashLoveDown(); handler.DashFearDown(); }
        for (int i = 0; i < targetLevel; i++) handler.DashLoveUp();

        for (int i = 0; i < 3; i++) { handler.RangedAngerDown(); handler.RangedSadnessDown(); handler.RangedJoyDown(); handler.RangedLoveDown(); handler.RangedFearDown(); }
        for (int i = 0; i < targetLevel; i++) handler.RangedLoveUp();

        Debug.Log("Picked up Dash Love!");
        if (destroyOnPickup) Destroy(gameObject);
    }
}