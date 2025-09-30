using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeFearPickup : MonoBehaviour
{
    [SerializeField] private UpgradeHandler handler;
    [SerializeField] private bool destroyOnPickup = true;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!handler) handler = FindObjectOfType<UpgradeHandler>();
        if (!handler) { Debug.LogWarning("[MeleeFearPickup] UpgradeHandler not found."); return; }

        // Zero other melee emotions
        for (int i = 0; i < 10; i++) { handler.MeleeAngerDown(); handler.MeleeSadDown(); handler.MeleeLoveDown(); handler.MeleeFearDown(); }
        // Set Fear to level 2
        handler.MeleeFearUp();
        handler.MeleeFearUp();

        Debug.Log("Picked up Melee Fear!");

        if (destroyOnPickup) Destroy(gameObject);
    }
}
