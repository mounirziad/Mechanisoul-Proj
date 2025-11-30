using System.Collections;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 0.28f;
    private bool isTrailActive;
    public float meshRefreshRate = 0.02f;
    public Transform positionToSpawn;
    public Material emotionMat;
    [SerializeField] private Material[] materials;
    public float meshDestroyDelay = 0.1f;

    [Header("Input References")]
    private PlayerControls playerControls;

    [Header("Grounded Requirement")]
    public bool requireGrounded = true;
    private PlayerLocomotion playerLocomotion;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerControls = new PlayerControls();
        playerLocomotion = GetComponent<PlayerLocomotion>();
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
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDisplaying)
        {
            return;
        }

        if (requireGrounded && playerLocomotion != null && !playerLocomotion.isGrounded)
        {
            return;
        }

        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    public void SetMaterial(string emotion)
    {
        if (materials == null || materials.Length == 0)
        {
            Debug.LogWarning("MeshTrail: No materials assigned in the array!");
            return;
        }

        foreach (var mat in materials)
        {
            if (mat == null) continue;

            if (mat.name.ToLower().Contains(emotion.ToLower()))
            {
                emotionMat = mat;
                Debug.Log($"MeshTrail: Material set to {mat.name} for emotion '{emotion}'");
                return;
            }
        }

        Debug.LogWarning($"MeshTrail: No material found matching emotion '{emotion}'");
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
                meshRenderer.material = emotionMat; ;

                Destroy(gObject, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }
}
