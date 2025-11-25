using UnityEngine;
using UnityEditor;

public class AttackAnimationEventHelper : EditorWindow
{
    [MenuItem("Tools/Attack Animation Event Helper")]
    public static void ShowWindow()
    {
        GetWindow<AttackAnimationEventHelper>("Attack Event Helper");
    }

    private AnimationClip selectedClip;
    private float startLungeTime = 0.2f;
    private float stopLungeTime = 1.0f;

    void OnGUI()
    {
        GUILayout.Label("Add Attack Lunge Events", EditorStyles.boldLabel);
        GUILayout.Space(10);

        selectedClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", selectedClip, typeof(AnimationClip), false);

        if (selectedClip != null)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Animation Length: {selectedClip.length:F2} seconds", EditorStyles.helpBox);
            GUILayout.Space(10);

            startLungeTime = EditorGUILayout.Slider("Start Lunge Time (s)", startLungeTime, 0f, selectedClip.length);
            stopLungeTime = EditorGUILayout.Slider("Stop Lunge Time (s)", stopLungeTime, 0f, selectedClip.length);

            GUILayout.Space(20);

            if (GUILayout.Button("Add Lunge Events", GUILayout.Height(30)))
            {
                AddLungeEvents();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Clear All Lunge Events", GUILayout.Height(30)))
            {
                ClearLungeEvents();
            }
        }
        else
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Select an Animation Clip to add lunge events", MessageType.Info);
        }

        GUILayout.Space(20);
        EditorGUILayout.HelpBox(
            "1. Select your attack animation clip\n" +
            "2. Set when the lunge should start and stop\n" +
            "3. Click 'Add Lunge Events'\n" +
            "4. Open Animation window to verify events were added",
            MessageType.Info);
    }

    void AddLungeEvents()
    {
        if (selectedClip == null) return;

        AnimationEvent startEvent = new AnimationEvent
        {
            time = startLungeTime,
            functionName = "StartAttackLunge",
            messageOptions = SendMessageOptions.RequireReceiver
        };

        AnimationEvent stopEvent = new AnimationEvent
        {
            time = stopLungeTime,
            functionName = "StopAttackLunge",
            messageOptions = SendMessageOptions.RequireReceiver
        };

        AnimationUtility.SetAnimationEvents(selectedClip, new AnimationEvent[] { startEvent, stopEvent });
        EditorUtility.SetDirty(selectedClip);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>Added lunge events to {selectedClip.name}:</color>\n" +
                  $"  StartAttackLunge at {startLungeTime:F2}s\n" +
                  $"  StopAttackLunge at {stopLungeTime:F2}s");
    }

    void ClearLungeEvents()
    {
        if (selectedClip == null) return;

        var existingEvents = AnimationUtility.GetAnimationEvents(selectedClip);
        var filteredEvents = System.Array.FindAll(existingEvents, e =>
            e.functionName != "StartAttackLunge" && e.functionName != "StopAttackLunge");

        AnimationUtility.SetAnimationEvents(selectedClip, filteredEvents);
        EditorUtility.SetDirty(selectedClip);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=yellow>Cleared lunge events from {selectedClip.name}</color>");
    }
}
