using UnityEngine;

public class MechromancerHitbox : MonoBehaviour
{
    [SerializeField] private int hitboxIndex;
    
    private Mechromancer mechromancer;

    private void Awake()
    {
        mechromancer = GetComponentInParent<Mechromancer>();
        if (mechromancer == null)
        {
            Debug.LogError($"MechromancerHitbox on {gameObject.name}: Could not find Mechromancer component in parent!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (mechromancer == null) return;

        mechromancer.OnHitboxCollision(hitboxIndex, other);
    }

    public void SetHitboxIndex(int index)
    {
        hitboxIndex = index;
    }
}
