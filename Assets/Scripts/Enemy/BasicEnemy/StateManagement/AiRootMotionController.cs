using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class AiRootMotionController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void OnAnimatorMove()
    {
        // Only apply if root motion is enabled and NavMeshAgent is not controlling movement
        if (animator.applyRootMotion && !agent.updatePosition)
        {
            // Move the transform using the root motion delta
            Vector3 rootMotionDelta = animator.deltaPosition;
            Quaternion rootMotionRotation = animator.deltaRotation;

            // Apply translation and rotation from animation
            transform.position += rootMotionDelta;
            transform.rotation *= rootMotionRotation;

            // Sync NavMeshAgent internal position to this new transform
            agent.nextPosition = transform.position;
        }
    }
}
