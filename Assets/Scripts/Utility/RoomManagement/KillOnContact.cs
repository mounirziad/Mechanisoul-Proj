using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class KillOnContact : MonoBehaviour
{
    PlayerHealth playerHealth;
    

    private void Awake()
    {
        playerHealth = GameObject.Find("PlayerCharacter").GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            playerHealth.Die();
    }
}
