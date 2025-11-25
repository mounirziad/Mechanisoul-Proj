using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillOnContact : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        //if player, kill, else return
    }
}
