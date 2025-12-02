using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class Resurrection : MonoBehaviour
{
    [Header("Resurrection Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform teslaTarget;
    [SerializeField] private Transform lightningOrigin;
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private float resurrectionTime = 5f;

    //private bool hasStarted = false;

    private Mechromancer mech;
    private NavMeshAgent agent;
    private MechAnimationController animationController;

    public bool IsResurrectionActive { get; private set; }

    private void Awake()
    {
        mech = GetComponent<Mechromancer>();
        agent = GetComponent<NavMeshAgent>();
        animationController = GetComponent<MechAnimationController>();
    }

    private void Start()
    {
        var graph = GetComponent<BehaviorGraphAgent>().BlackboardReference;
        graph.SetVariableValue("alreadyResurrected", false);

        HasResurrected = false;
    }

    public bool HasResurrected
    {
        get
        {
            bool value = false;
            var graph = GetComponent<BehaviorGraphAgent>().BlackboardReference;
            graph.GetVariableValue("alreadyResurrected", out value);
            return value;
        }
        set
        {
            var graph = GetComponent<BehaviorGraphAgent>().BlackboardReference;
            graph.SetVariableValue("alreadyResurrected", value);
        }
    }

    public void StartResurrection()
    {
        if (HasResurrected || IsResurrectionActive) return;

        StartCoroutine(ResurrectionRoutine());
        Debug.Log("StartResurrection() called");
    }

    public void SpawnResurrectionLightning()
    {
        if (lightningPrefab == null || lightningOrigin == null || teslaTarget == null)
        {
            return;
        }

        GameObject lightningObj = Instantiate(lightningPrefab, lightningOrigin.position, Quaternion.identity);

        lightningObj.transform.LookAt(teslaTarget.position);

        var rb = lightningObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (teslaTarget.position - lightningOrigin.position).normalized;
            rb.linearVelocity = direction * 50f;
        }

        Debug.Log("Resurrection lightning spawned");
    }

    private IEnumerator ResurrectionRoutine()
    {
        IsResurrectionActive = true;

<<<<<<< Updated upstream
        if (animationController != null)
        {
            Debug.Log("Setting IsResurrecting to TRUE");
            animationController.SetIsResurrecting(true);
        }
        else
        {
            Debug.LogWarning("MechAnimationController is NULL! Cannot play resurrection animation.");
        }

        Debug.Log($"Starting resurrection timer for {resurrectionTime} seconds");
=======
        yield return new WaitForSeconds(1.0f);

        //Move to hiding, timer, spawn minions
>>>>>>> Stashed changes
        float timer = 0f;
        while (timer < resurrectionTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SpawnMinions();

        if (animationController != null)
        {
            Debug.Log("Setting IsResurrecting to FALSE");
            animationController.SetIsResurrecting(false);
        }

        HasResurrected = true;
        IsResurrectionActive = false;

        Debug.Log("Resurrection complete");
    }

    private IEnumerator ResurrectionTimer()
    {
        float timer = 0f;

        while (timer < resurrectionTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SpawnMinions();
        HasResurrected = true;
    }

    private void SpawnMinions()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("Spawned minions");
        }
    }

    public void OnResurrectionSpawn()
    {
        SpawnMinions();
    }

    public void OnResurrectionComplete()
    {
        if (animationController != null)
        {
            animationController.SetIsResurrecting(false);
        }

        HasResurrected = true;
        IsResurrectionActive = false;

        Debug.Log("Resurrection animation complete");
    }
}
