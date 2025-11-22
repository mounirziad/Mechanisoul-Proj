using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Movement Settings")]
    [Tooltip("Speed at which the belt texture scrolls")]
    public float scrollSpeed = 0.5f;

    [Tooltip("Direction of belt movement (use X for horizontal, Y for vertical scrolling)")]
    public Vector2 scrollDirection = new Vector2(1f, 0f);

    [Header("Physical Movement Settings")]
    [Tooltip("If enabled, objects on the belt will be physically moved")]
    public bool moveObjects = true;

    [Tooltip("Force applied to objects on the belt")]
    public float conveyorForce = 5f;

    [Tooltip("Direction objects should move (in world space)")]
    public Vector3 movementDirection = Vector3.forward;

    [Tooltip("Layer mask for objects that can be moved by the belt")]
    public LayerMask affectedLayers = ~0;

    private Material beltMaterial;
    private MeshRenderer meshRenderer;
    private Vector2 currentOffset;
    private BoxCollider triggerCollider;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            beltMaterial = meshRenderer.material;
            currentOffset = beltMaterial.mainTextureOffset;
        }
        else
        {
            Debug.LogWarning($"ConveyorBelt: No MeshRenderer found on {gameObject.name}");
        }

        if (moveObjects)
        {
            SetupPhysicsTrigger();
        }
    }

    void SetupPhysicsTrigger()
    {
        triggerCollider = GetComponent<BoxCollider>();

        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            Debug.Log($"ConveyorBelt: Created trigger collider on {gameObject.name}");
        }

        triggerCollider.isTrigger = true;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            triggerCollider.center = meshFilter.sharedMesh.bounds.center;
            triggerCollider.size = meshFilter.sharedMesh.bounds.size + new Vector3(0, 0.5f, 0);
            Debug.Log($"ConveyorBelt: Trigger size set to {triggerCollider.size} on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"ConveyorBelt: No MeshFilter found on {gameObject.name}, using default trigger size");
        }
    }

    void Update()
    {
        if (beltMaterial != null)
        {
            ScrollTexture();
        }
    }

    void ScrollTexture()
    {
        Vector2 offset = scrollDirection.normalized * scrollSpeed * Time.deltaTime;
        currentOffset += offset;

        currentOffset.x = currentOffset.x % 1f;
        currentOffset.y = currentOffset.y % 1f;

        beltMaterial.mainTextureOffset = currentOffset;
    }

    void OnTriggerStay(Collider other)
    {
        if (!moveObjects) return;

        if (((1 << other.gameObject.layer) & affectedLayers) == 0) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 worldDirection = transform.TransformDirection(movementDirection.normalized);
            Vector3 force = worldDirection * conveyorForce;
            rb.AddForce(force, ForceMode.Force);

            Debug.DrawRay(rb.position, force, Color.green);
        }
        else
        {
            CharacterController characterController = other.GetComponent<CharacterController>();
            if (characterController != null)
            {
                Vector3 worldDirection = transform.TransformDirection(movementDirection.normalized);
                Vector3 movement = worldDirection * conveyorForce * Time.deltaTime;
                characterController.Move(movement);

                Debug.DrawRay(characterController.transform.position, movement * 10f, Color.blue);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ConveyorBelt: Object entered trigger - {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
    }

    void OnDestroy()
    {
        if (beltMaterial != null)
        {
            Destroy(beltMaterial);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        Vector3 direction = transform.TransformDirection(movementDirection.normalized);

        Gizmos.DrawRay(center, direction * 2f);
        Gizmos.DrawWireSphere(center + direction * 2f, 0.2f);
    }
}
