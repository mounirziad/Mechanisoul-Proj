using UnityEditor;
using UnityEngine;

public class VFXCameraLookAt : MonoBehaviour
{
    bool flip;

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            transform.LookAt(Camera.main.transform.position, Vector3.up);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            transform.LookAt(SceneView.GetAllSceneCameras()[0].transform.position, Vector3.up);
        }
        #endif
    }
}
