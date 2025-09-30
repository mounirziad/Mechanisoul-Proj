using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeSadnessPickup : MonoBehaviour
{
    [SerializeField] private UpgradeHandler handler;
    [SerializeField] private bool destroyOnPickup = true;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!handler) handler = FindObjectOfType<UpgradeHandler>();
        if (!handler) { Debug.LogWarning("[MeleeSadnessPickup] UpgradeHandler not found."); return; }

        // Zero other melee emotions
        for (int i = 0; i < 10; i++) { handler.MeleeAngerDown(); handler.MeleeLoveDown(); handler.MeleeFearDown(); handler.MeleeSadDown(); }
        // Set Sadness to level 2
        handler.MeleeSadUp();
        handler.MeleeSadUp(); 

        Debug.Log("Picked up Melee Sadness!");

        if (destroyOnPickup) Destroy(gameObject);
    }
}
