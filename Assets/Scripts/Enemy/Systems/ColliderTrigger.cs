using System;
using UnityEngine;

public class ColliderTrigger : MonoBehaviour
{
    public event EventHandler OnPlayerEnterTrigger;
    public bool hasTriggered = false;

    public void OnTriggerEnter(Collider collider)
    {
        PlayerManager player = collider.GetComponent<PlayerManager>();

        if (player != null)
        {
            hasTriggered = true;
            Debug.Log("Player inside trigger area");
            OnPlayerEnterTrigger?.Invoke(this, EventArgs.Empty);
        }
    }
}
