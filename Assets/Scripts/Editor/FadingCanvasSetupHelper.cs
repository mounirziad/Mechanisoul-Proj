using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FadingCanvasSetupHelper : EditorWindow
{
    [MenuItem("Tools/Scene Setup/Add Fading Canvas to Scene")]
    public static void AddFadingCanvasToScene()
    {
        FadingScript existingFadingScript = FindFirstObjectByType<FadingScript>();
        
        if (existingFadingScript != null)
        {
            EditorUtility.DisplayDialog("Already Exists", 
                "A FadingCanvas already exists in this scene!", "OK");
            Selection.activeGameObject = existingFadingScript.gameObject;
            return;
        }
        
        GameObject fadingCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Camera/FadingCanvas.prefab");
        
        if (fadingCanvasPrefab != null)
        {
            GameObject fadingCanvas = (GameObject)PrefabUtility.InstantiatePrefab(fadingCanvasPrefab);
            fadingCanvas.name = "FadingCanvas";
            Undo.RegisterCreatedObjectUndo(fadingCanvas, "Add Fading Canvas");
            Selection.activeGameObject = fadingCanvas;
            
            EditorUtility.DisplayDialog("Success", 
                "FadingCanvas added to scene!", "OK");
        }
        else
        {
            CreateFadingCanvasFromScratch();
        }
    }
    
    private static void CreateFadingCanvasFromScratch()
    {
        GameObject fadingCanvasGO = new GameObject("FadingCanvas");
        Undo.RegisterCreatedObjectUndo(fadingCanvasGO, "Create Fading Canvas");
        
        Canvas canvas = fadingCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        CanvasScaler scaler = fadingCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        fadingCanvasGO.AddComponent<GraphicRaycaster>();
        
        CanvasGroup canvasGroup = fadingCanvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        GameObject imageGO = new GameObject("FadingImage");
        imageGO.transform.SetParent(fadingCanvasGO.transform);
        
        RectTransform imageRect = imageGO.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.sizeDelta = Vector2.zero;
        imageRect.anchoredPosition = Vector2.zero;
        
        Image image = imageGO.AddComponent<Image>();
        image.color = Color.black;
        
        FadingScript fadingScript = fadingCanvasGO.AddComponent<FadingScript>();
        
        SerializedObject so = new SerializedObject(fadingScript);
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.FindProperty("fadeDuration").floatValue = 1.0f;
        so.FindProperty("fadeIn").boolValue = true;
        so.ApplyModifiedProperties();
        
        Selection.activeGameObject = fadingCanvasGO;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("Success", 
            "FadingCanvas created from scratch and added to scene!", "OK");
    }
    
    [MenuItem("Tools/Scene Setup/Validate All Scenes Have Fading Canvas")]
    public static void ValidateAllScenes()
    {
        ShowWindow();
    }
    
    public static void ShowWindow()
    {
        GetWindow<FadingCanvasSetupHelper>("Fading Canvas Validator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Fading Canvas Scene Validator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool helps ensure all your scenes have a FadingCanvas for smooth respawn transitions.",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add Fading Canvas to Current Scene", GUILayout.Height(30)))
        {
            AddFadingCanvasToScene();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Check All Scenes in Build Settings", GUILayout.Height(30)))
        {
            CheckAllScenesInBuild();
        }
    }
    
    private void CheckAllScenesInBuild()
    {
        int scenesWithoutFading = 0;
        string missingScenes = "";
        
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
                
            string scenePath = scene.path;
            UnityEngine.SceneManagement.Scene loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            FadingScript fadingScript = FindFirstObjectByType<FadingScript>();
            
            if (fadingScript == null)
            {
                scenesWithoutFading++;
                missingScenes += $"- {loadedScene.name}\n";
            }
        }
        
        if (scenesWithoutFading > 0)
        {
            EditorUtility.DisplayDialog("Validation Results", 
                $"Found {scenesWithoutFading} scene(s) without FadingCanvas:\n\n{missingScenes}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Validation Results", 
                "All scenes in build settings have a FadingCanvas!", "OK");
        }
    }
}
