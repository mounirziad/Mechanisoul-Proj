using System.Collections;
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
    public bool hasResurrected;

    private Mechromancer mech;
    private NavMeshAgent agent;

    public bool IsResurrectionActive { get; private set; }

    private void Awake()
    {
        mech = GetComponent<Mechromancer>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void StartResurrection()
    {
        if (hasResurrected || IsResurrectionActive) return;
        StartCoroutine(ResurrectionRoutine());
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
        hasResurrected = true;
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
        hasResurrected = true;
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
