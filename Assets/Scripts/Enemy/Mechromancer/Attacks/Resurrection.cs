using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class Resurrection : MonoBehaviour
{
    [Header("Resurrection Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform hidingPos;
    [SerializeField] private float resurrectionTime = 5f;

    private bool hasStarted = false;
    private bool isMovingToHiding = false;

    private Mechromancer mech;
    private NavMeshAgent agent;

    public bool IsResurrectionActive { get; private set; }

    private void Awake()
    {
        mech = GetComponent<Mechromancer>();
        agent = GetComponent<NavMeshAgent>();
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

        HasResurrected = true;
        Debug.Log("StartResurrection() called");
    }

    private IEnumerator ResurrectionRoutine()
    {
        IsResurrectionActive = true;

        //Move to hiding, timer, spawn minions
        float timer = 0f;
        while (timer < resurrectionTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SpawnMinions();

        HasResurrected = true;
        IsResurrectionActive = false;
    }

    private IEnumerator MoveToHidingCoroutine()
    {
        agent.SetDestination(hidingPos.position);

        while (Vector3.Distance(transform.position, hidingPos.position) > 1f)
        {
            yield return null;
        }

        isMovingToHiding = false;
        yield return StartCoroutine(ResurrectionTimer());
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
}
