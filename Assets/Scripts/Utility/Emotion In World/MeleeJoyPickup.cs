using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeJoyPickup : MonoBehaviour
{
    [SerializeField] private UpgradeHandler handler;   // drag your scene's handler here (or auto-find)
    [SerializeField] private bool destroyOnPickup = true;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!handler) handler = FindObjectOfType<UpgradeHandler>();
        if (!handler) { Debug.LogWarning("[MeleeLovePickup] UpgradeHandler not found."); return; }

        // Zero other melee emotions first (call 'down' a few times to guarantee 0)
        for (int i = 0; i < 10; i++) { handler.MeleeAngerDown(); handler.MeleeSadDown(); handler.MeleeFearDown(); handler.MeleeLoveDown(); }
        // Set Love to level 2
        handler.MeleeJoyUp();
        handler.MeleeJoyUp();

        Debug.Log("Picked up Melee Joy!");

        if (destroyOnPickup) Destroy(gameObject);
    }
}
