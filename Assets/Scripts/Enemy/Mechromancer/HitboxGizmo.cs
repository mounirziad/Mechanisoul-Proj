using UnityEngine;

[ExecuteAlways]
public class HitboxGizmo : MonoBehaviour
{
    public Color gizmoColor = new Color(1, 0, 0, 0.25f);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        if (col is BoxCollider box)
        {
            Gizmos.matrix = box.transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = sphere.transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider cap)
        {
            Gizmos.matrix = cap.transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(cap.center + Vector3.up * (cap.height / 2 - cap.radius), cap.radius);
            Gizmos.DrawWireSphere(cap.center + Vector3.down * (cap.height / 2 - cap.radius), cap.radius);
        }
    }
}
