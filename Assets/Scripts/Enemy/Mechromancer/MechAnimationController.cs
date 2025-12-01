using UnityEngine;
using UnityEngine.AI;

public class MechAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.angularSpeed = 180;
        }
    }

    private void Update()
    {
        UpdateLocomotion();
    }

    private void UpdateLocomotion()
    {
        if (animator == null || agent == null) return;

        Vector3 velocity = agent.velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        float horizontal = localVelocity.x;
        float vertical = localVelocity.z;

        animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);
    }

    public void SetTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void ResetTrigger(string triggerName)
    {
        animator.ResetTrigger(triggerName);
    }

    public void SetBool(string name, bool value)
    {
        animator.SetBool(name, value);
    }

    public void SetFloat(string name, float value)
    {
        animator.SetFloat(name, value);
    }
}
