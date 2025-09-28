using System.Collections;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.WSA;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 0.28f;
    private bool isTrailActive;
    public float meshRefreshRate = 0.02f;
    public Transform positionToSpawn;
    public Material mat;
    public float meshDestroyDelay = 0.1f;

    [Header("Input References")]
    private PlayerControls playerControls;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.PlayerActions.Dodge.performed += OnDodge;
    }

    private void OnDisable()
    {
        playerControls.PlayerActions.Dodge.performed -= OnDodge;
        playerControls.Disable();
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    IEnumerator ActivateTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            if (skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject gObject = new GameObject();
                gObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer meshRenderer = gObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = gObject.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                meshFilter.mesh = mesh;
                meshRenderer.material = mat;

                Destroy(gObject, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }
}
