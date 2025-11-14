using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private string spawnID = "DefaultSpawn";
    [SerializeField] private bool isDefaultSpawn = true;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float gizmoSize = 1f;
    
    private void Awake()
    {
        if (!CompareTag("PlayerSpawn"))
        {
            Debug.LogWarning($"SpawnPoint '{gameObject.name}' does not have the 'PlayerSpawn' tag. Adding it now.");
            gameObject.tag = "PlayerSpawn";
        }
    }
    
    public string GetSpawnID()
    {
        return spawnID;
    }
    
    public bool IsDefaultSpawn()
    {
        return isDefaultSpawn;
    }
    
    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }
    
    public Quaternion GetSpawnRotation()
    {
        return transform.rotation;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmo)
        {
            return;
        }
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 0.5f);
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, gizmoSize * 0.5f);
        
        Vector3 forward = transform.forward * gizmoSize;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position + forward, transform.position + forward - transform.right * 0.3f + transform.up * 0.1f);
        Gizmos.DrawLine(transform.position + forward, transform.position + forward + transform.right * 0.3f + transform.up * 0.1f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        
        Vector3 labelPosition = transform.position + Vector3.up * (gizmoSize + 0.5f);
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPosition, $"Spawn: {spawnID}");
#endif
    }
}
