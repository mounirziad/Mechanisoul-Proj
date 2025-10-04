using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SoundManager soundManager = (SoundManager)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Quick Test Buttons", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Test Attack Sound"))
            {
                soundManager.PlayAttackSound();
            }
            
            if (GUILayout.Button("Test Hit Sound"))
            {
                soundManager.PlayHitSound();
            }
            
            if (GUILayout.Button("Test Dash Sound"))
            {
                soundManager.PlayDashSound();
            }
        }
        else
        {
            GUILayout.Label("Enter Play Mode to test sounds", EditorStyles.helpBox);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Quick Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Assign Audio Files"))
        {
            AutoAssignAudioFiles(soundManager);
        }
    }
    
    private void AutoAssignAudioFiles(SoundManager soundManager)
    {
        // Find and assign audio files automatically
        string[] audioGuids;
        
        // Look for dash sound
        audioGuids = AssetDatabase.FindAssets("dash t:AudioClip", new[] { "Assets/Art/Audio" });
        if (audioGuids.Length > 0)
        {
            string dashPath = AssetDatabase.GUIDToAssetPath(audioGuids[0]);
            AudioClip dashClip = AssetDatabase.LoadAssetAtPath<AudioClip>(dashPath);
            SerializedObject so = new SerializedObject(soundManager);
            so.FindProperty("dashSound").objectReferenceValue = dashClip;
            so.ApplyModifiedProperties();
            Debug.Log($"Assigned dash sound: {dashClip.name}");
        }
        
        // Look for metal/hit sounds
        audioGuids = AssetDatabase.FindAssets("metal t:AudioClip", new[] { "Assets/Art/Audio" });
        if (audioGuids.Length > 0)
        {
            SerializedObject so = new SerializedObject(soundManager);
            SerializedProperty hitSoundsProperty = so.FindProperty("hitSounds");
            hitSoundsProperty.arraySize = audioGuids.Length;
            
            for (int i = 0; i < audioGuids.Length; i++)
            {
                string audioPath = AssetDatabase.GUIDToAssetPath(audioGuids[i]);
                AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                hitSoundsProperty.GetArrayElementAtIndex(i).objectReferenceValue = audioClip;
                Debug.Log($"Assigned hit sound {i}: {audioClip.name}");
            }
            
            so.ApplyModifiedProperties();
        }
        
        // Look for laser/attack sounds
        audioGuids = AssetDatabase.FindAssets("laser t:AudioClip", new[] { "Assets/Art/Audio" });
        if (audioGuids.Length > 0)
        {
            SerializedObject so = new SerializedObject(soundManager);
            SerializedProperty attackSoundsProperty = so.FindProperty("attackSounds");
            attackSoundsProperty.arraySize = audioGuids.Length;
            
            for (int i = 0; i < audioGuids.Length; i++)
            {
                string audioPath = AssetDatabase.GUIDToAssetPath(audioGuids[i]);
                AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                attackSoundsProperty.GetArrayElementAtIndex(i).objectReferenceValue = audioClip;
                Debug.Log($"Assigned attack sound {i}: {audioClip.name}");
            }
            
            so.ApplyModifiedProperties();
        }
        
        EditorUtility.SetDirty(soundManager);
    }
}
#endif