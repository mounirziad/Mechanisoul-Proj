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
        if (animator == null) return;
        animator.SetTrigger(triggerName);
    }

    public void ResetTrigger(string triggerName)
    {
        if (animator == null) return;
        animator.ResetTrigger(triggerName);
    }

    public void SetBool(string name, bool value)
    {
        if (animator == null) return;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == name)
            {
                animator.SetBool(name, value);
                return;
            }
        }
        
        Debug.LogWarning($"Animator parameter '{name}' not found! Please add it to the animator controller.");
    }

    public void SetFloat(string name, float value)
    {
        if (animator == null) return;
        animator.SetFloat(name, value);
    }

    public void TriggerLunge()
    {
        SetTrigger("Lunge");
    }

    public void TriggerCastLightning()
    {
        SetTrigger("CastLightning");
    }

    public void SetIsAttacking(bool value)
    {
        SetBool("IsAttacking", value);
    }

    public void SetIsResurrecting(bool value)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is NULL in SetIsResurrecting!");
            return;
        }
        
        Debug.Log($"Setting IsResurrecting animator parameter to {value}");
        SetBool("IsResurrecting", value);
    }
}
