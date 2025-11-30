using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class DoorNavMeshControl : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool startClosed = true;
    
    private Animator animator;
    private NavMeshObstacle[] obstacles;
    private BoxCollider[] colliders;
    private bool isOpen = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        obstacles = GetComponentsInChildren<NavMeshObstacle>();
        colliders = GetComponentsInChildren<BoxCollider>();

        if (!startClosed)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        
        if (animator != null)
        {
            animator.SetBool("IsDoorOpen", true);
        }

        foreach (NavMeshObstacle obstacle in obstacles)
        {
            if (obstacle != null)
            {
                obstacle.carving = false;
                obstacle.enabled = false;
            }
        }

        foreach (BoxCollider collider in colliders)
        {
            if (collider != null && !collider.isTrigger)
            {
                collider.enabled = false;
            }
        }

        Debug.Log($"Door {gameObject.name} opened - obstacles and colliders disabled");
    }

    public void CloseDoor()
    {
        if (!isOpen && Time.time > 0.1f) return;

        isOpen = false;

        if (animator != null)
        {
            animator.SetBool("IsDoorOpen", false);
        }

        foreach (NavMeshObstacle obstacle in obstacles)
        {
            if (obstacle != null)
            {
                obstacle.enabled = true;
                obstacle.carving = true;
            }
        }

        foreach (BoxCollider collider in colliders)
        {
            if (collider != null && !collider.isTrigger)
            {
                collider.enabled = true;
            }
        }

        Debug.Log($"Door {gameObject.name} closed - obstacles and colliders enabled");
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
