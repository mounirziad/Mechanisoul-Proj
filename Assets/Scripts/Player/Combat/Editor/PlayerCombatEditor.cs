using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerCombat))]
public class PlayerCombatEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerCombat playerCombat = (PlayerCombat)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Add helpful information about root motion modes
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Root Motion Mode Guide", EditorStyles.boldLabel);
        
        switch (playerCombat.rootMotionMode)
        {
            case RootMotionMode.LungeOnly:
                EditorGUILayout.HelpBox(
                    "LUNGE ONLY MODE:\n" +
                    "• Root motion disabled\n" +
                    "• Uses your custom AttackSO movement system\n" +
                    "• Full control over attack movement via scripts\n" +
                    "• Best for precise, predictable attack movement", 
                    MessageType.Info);
                break;
                
            case RootMotionMode.RootMotionOnly:
                EditorGUILayout.HelpBox(
                    "ROOT MOTION ONLY MODE:\n" +
                    "• Root motion enabled during attacks\n" +
                    "• Movement driven entirely by animations\n" +
                    "• No additional lunge movement\n" +
                    "• Best for cinematic, animation-heavy combat", 
                    MessageType.Info);
                break;
                
            case RootMotionMode.Hybrid:
                EditorGUILayout.HelpBox(
                    "HYBRID MODE (RECOMMENDED):\n" +
                    "• Root motion enabled for base movement\n" +
                    "• Additional reduced lunge for extra impact\n" +
                    "• Combines animation smoothness with script control\n" +
                    "• Best balance of visual quality and responsiveness", 
                    MessageType.Info);
                break;
        }
        
        if (playerCombat.rootMotionMode == RootMotionMode.Hybrid)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Additional Lunge Strength: {playerCombat.additionalLungeMultiplier:F1}x", EditorStyles.helpBox);
        }
    }
}