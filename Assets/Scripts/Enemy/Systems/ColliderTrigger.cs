using System;
using UnityEngine;

public class ColliderTrigger : MonoBehaviour
{
    public event EventHandler OnPlayerEnterTrigger;

    private void OnTriggerEnter(Collider collider)
    {
        PlayerManager player = collider.GetComponent<PlayerManager>();

        if (player != null)
        {
            Debug.Log("Player inside trigger area");
            OnPlayerEnterTrigger?.Invoke(this, EventArgs.Empty);
        }
    }
}
