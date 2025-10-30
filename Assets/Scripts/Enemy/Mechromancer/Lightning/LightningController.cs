using System.Collections;
using UnityEngine;

public class LightningController : MonoBehaviour
{
    [Header("Targeting")]
    public Transform playerTransform;
    public LayerMask groundMask;
    public float raycastHeight = 50f;
    public Vector3 positionOffset = Vector3.zero;

    [Header("Timing")]
    public float warmupTime = 2.0f;

    [Header("Prefabs")]
    public GameObject indicatorPrefab; //in case we want an indicator circle on the floor
    public GameObject lightningPrefab; //lightning VFX

    [Header("Strike")]
    public float strikeHeight = 0f;
    public bool alignToNormal = true;
    public int damage = 15;

    [Header("Options")]
    public bool debugDrawRay = false;

    private Coroutine strikeRoutine;

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = FindFirstObjectByType<PlayerManager>().transform;
        }
    }

    public void StartLightningAtPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform not assigned");
            return;
        }

        if (strikeRoutine != null)
        {
            StopCoroutine(strikeRoutine);
            //strikeRoutine = StartCoroutine(DoLightningRoutine());
        }
    }

    /*private IEnumerator DoLightningRoutine()
    {
        Vector3 playerPos = playerTransform.position + positionOffset;

        Vector3 rayStart = playerPos + Vector3.up * raycastHeight;
        RaycastHit hit;
        Vector3 strikePos = playerPos;
        Vector3 strikeNormal = Vector3.up;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastHeight * 2f, groundMask))
        {
            strikePos = hit.point;
            strikeNormal = hit.normal;
            if (debugDrawRay)
            {
                Debug.DrawLine(rayStart, hit.point, Color.cyan, warmupTime);
            }
        }

        else
        {
            strikePos.y = playerPos.y + strikeHeight;
            if (debugDrawRay)
            {
                Debug.DrawLine(rayStart, rayStart - Vector3.up * (raycastHeight * 2f), Color.red, warmupTime);
            }
        }

        GameObject indicator = null;
        if (indicatorPrefab != null)
        {
            indicator = Instantiate(indicatorPrefab, strikePos, Quaternion.identity);
            if (alignToNormal)
            {
                indicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, strikeNormal);
            }
        }

        strikeRoutine = null;
    }*/
}
