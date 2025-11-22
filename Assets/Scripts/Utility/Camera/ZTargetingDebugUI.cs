using UnityEngine;

public class ZTargetingDebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZTargetingSystem zTargeting;
    [SerializeField] private TargetedPlayerRotation playerRotation;
    
    [Header("UI Settings")]
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private Color textColor = Color.white;
    
    private GUIStyle style;
    
    private void Awake()
    {
        if (zTargeting == null)
            zTargeting = GetComponent<ZTargetingSystem>();
        
        if (playerRotation == null)
            playerRotation = GetComponent<TargetedPlayerRotation>();
    }
    
    private void OnGUI()
    {
        if (!showDebugUI)
            return;
        
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.normal.textColor = textColor;
            style.alignment = TextAnchor.UpperLeft;
        }
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== Z-TARGETING DEBUG ===", style);
        GUILayout.Space(5);
        
        if (zTargeting != null)
        {
            GUILayout.Label($"Lock State: {(zTargeting.IsLocked ? "LOCKED" : "FREE")}", style);
            
            if (zTargeting.IsLocked && zTargeting.CurrentTarget != null)
            {
                GUILayout.Label($"Target: {zTargeting.CurrentTarget.name}", style);
                
                float distance = Vector3.Distance(transform.position, zTargeting.CurrentTarget.position);
                GUILayout.Label($"Distance: {distance:F2}m", style);
                
                Vector3 direction = zTargeting.GetTargetDirection();
                float angle = Vector3.Angle(transform.forward, direction);
                GUILayout.Label($"Angle to Target: {angle:F1}°", style);
            }
            else
            {
                GUILayout.Label("Target: None", style);
            }
        }
        else
        {
            GUILayout.Label("ZTargetingSystem: NOT FOUND", style);
        }
        
        GUILayout.Space(10);
        
        if (playerRotation != null)
        {
            GUILayout.Label($"Rotating: {playerRotation.IsRotatingTowardsTarget()}", style);
            GUILayout.Label($"Forward: {playerRotation.GetForwardDirection()}", style);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:", style);
        GUILayout.Label("- Lock-On Button: Toggle Lock", style);
        GUILayout.Label("- Movement: Free rotation", style);
        GUILayout.Label("- Locked: Auto-face target", style);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
