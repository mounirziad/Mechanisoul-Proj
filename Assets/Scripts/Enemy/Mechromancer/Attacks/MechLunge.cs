using JetBrains.Annotations;
using UnityEngine;

public class MechLunge : MonoBehaviour
{
    public Transform playerTransform;
    public float range = 5f;
    public float force = 10f;
    
    private Rigidbody rb;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        var player = GameObject.FindWithTag("Player");
        playerTransform = player.transform;

        if (player == null)
        {
            Debug.LogError("Player not found in scene");
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < range)
        {
            LungeAttack();
        }
    }

    public void LungeAttack()
    {
        //animator.SetTrigger("Lunge");

        Vector3 lungeDirection = (playerTransform.position - transform.position).normalized;

        if (rb != null)
        {
            rb.AddForce(lungeDirection * force, ForceMode.Impulse);
        }
    }
}
