using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Mechromancer))]
public class MechromancerBehaviour : MonoBehaviour, IGoapBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor; //medium radius
    [SerializeField] Sensor attackSensor; //smallest radius
    [SerializeField] Sensor lightningSensor; //biggest radius

    [Header("Locations")]
    [SerializeField] Transform restingPosition;
    [SerializeField] public Transform hidingPosition;
    [SerializeField] Transform hidingPos;

    [Header("Player Reference")]
    [SerializeField] GameObject player;
    private Transform playerTransform; //for lightning attack

    [Header("Spawning Enemies")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Transform[] spawnPoints;

    private Mechromancer mechromancer;
    private AgentGoal resurrectGoal;

    private bool resurrectedPhase1 = false;
    private bool resurrectedPhase2 = false;
    private bool isResurrecting = false;

    private GoapAgent agent;

    private int health; //placeholder until health system fixed

    public void Awake()
    {
        agent = GetComponent<GoapAgent>();
    }

    private void OnEnable()
    {
        chaseSensor.OnTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        chaseSensor.OnTargetChanged -= HandleTargetChanged;
    }

    public Dictionary<string, AgentBelief> ProvideBeliefs()
    {
        mechromancer = GetComponent<Mechromancer>();

        var beliefs = new Dictionary<string, AgentBelief>();
        var factory = new BeliefFactory(GetComponent<GoapAgent>(), beliefs);

        factory.AddBelief("Nothing", () => false);

        factory.AddBelief("AgentIdle", () => !GetComponent<NavMeshAgent>().hasPath);
        factory.AddBelief("AgentMoving", () => GetComponent<NavMeshAgent>().hasPath);

        /*factory.AddBelief("PhaseOne", () => mechromancer.currentHealth >= 50);
        factory.AddBelief("PhaseTwo", () => mechromancer.currentHealth < 50 && mechromancer.currentHealth >= 35);
        factory.AddBelief("Rage", () => mechromancer.currentHealth < 35);*/

        factory.AddLocationBelief("AgentAtHidingPosition", 8f, hidingPosition);
        factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);

        factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        factory.AddSensorBelief("PlayerInAttackRange", attackSensor);
        factory.AddSensorBelief("PlayerInLightningRange", lightningSensor);

        factory.AddBelief("AttackingPlayer", () => false); //Player can always be attacked, will never come true
        factory.AddBelief("CanResurrect", () => EvaluateResurrection());
        factory.AddBelief("HasResurrectedThisPhase", () => !EvaluateResurrection());

        return beliefs;
    }

    public HashSet<AgentAction> ProvideActions(Dictionary<string, AgentBelief> beliefs)
    {
        var navMesh = GetComponent<NavMeshAgent>();

        var actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["Nothing"])
            .Build());

        actions.Add(new AgentAction.Builder("Wander Around")
            .WithStrategy(new WanderStrategy(navMesh, 10))
            .AddEffect(beliefs["AgentMoving"])
            .Build());

        actions.Add(new AgentAction.Builder("Move To Hiding Position")
            .WithStrategy(new MoveStrategy(navMesh, () => hidingPosition.position))
            .AddEffect(beliefs["AgentAtHidingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("Hiding To Rest Area")
            .WithStrategy(new MoveStrategy(navMesh, () => restingPosition.position))
            .AddPrecondition(beliefs["AgentAtHidingPosition"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("Scan")
            .WithStrategy(new WanderStrategy(navMesh, 10))
            .AddPrecondition(beliefs["AgentAtRestingPosition"])
            .AddEffect(beliefs["AgentMoving"])
            .Build());

        actions.Add(new AgentAction.Builder("Chase Player")
            .WithStrategy(new MoveStrategy(navMesh, () => beliefs["PlayerInChaseRange"].Location))
            .AddPrecondition(beliefs["PlayerInChaseRange"])
            .AddEffect(beliefs["PlayerInAttackRange"])
            .Build());

        actions.Add(new AgentAction.Builder("Attack Player")
            .WithStrategy(new AttackStrategy(GetComponent<GoapAgent>()))
            .AddPrecondition(beliefs["PlayerInAttackRange"])
            .AddEffect(beliefs["AttackingPlayer"])
            .Build());

        actions.Add(new AgentAction.Builder("Lightning Attack") //Possible integration with the Machine Learning AI where the mech chooses if it wants to melee or range attack player
            .WithStrategy(new LightningAttackStrategy(GetComponent<GoapAgent>(), GetComponent<LightningController>()))
            .AddPrecondition(beliefs["PlayerInLightningRange"])
            .AddEffect(beliefs["AttackingPlayer"])
            .Build());

        actions.Add(new AgentAction.Builder("Drop Down")
            .WithStrategy(new DropDownStrategy(transform, restingPosition.position))
            .AddPrecondition(beliefs["AgentAtHidingPosition"])
            .AddPrecondition(beliefs["PlayerInChaseRange"])
            .AddEffect(beliefs["PlayerInAttackRange"])
            .Build());

        actions.Add(new AgentAction.Builder("Resurrect Robots")
            .WithStrategy(new ResurrectStrategy(GetComponent<GoapAgent>(), enemyPrefab, spawnPoints, hidingPos))
            .AddPrecondition(beliefs["AgentAtHidingPosition"])
            //.AddPrecondition(beliefs["CanResurrect"])
            .AddEffect(beliefs["HasResurrectedThisPhase"])
            .Build());

        return actions;
    }

    public HashSet<AgentGoal> ProvideGoals(Dictionary<string, AgentBelief> beliefs)
    {
        var goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Chill out")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["Nothing"])
            .Build());

        goals.Add(new AgentGoal.Builder("Wander")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["AgentMoving"])
            .Build());

        goals.Add(new AgentGoal.Builder("Hide")
            .WithPriority(2)
            .WithDesiredEffect(beliefs["AgentAtHidingPosition"])
            .Build());

        goals.Add(new AgentGoal.Builder("SeekAndKill")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AttackingPlayer"])
            .Build());

        goals.Add(new AgentGoal.Builder("Ambush Player")
            .WithPriority(4)
            .WithDesiredEffect(beliefs["PlayerInAttackRange"])
            .Build());

        resurrectGoal = new AgentGoal.Builder("Resurrect")
            .WithPriority(5)
            .WithDesiredEffect(beliefs["HasResurrectedThisPhase"])
            .Build();

        goals.Add(resurrectGoal);

        return goals;
    }

    public bool EvaluateResurrection()
    {
        //float health = mechromancer.currentHealth;

        if (health > 50)
            return !resurrectedPhase1;
        if (health > 35)
            return !resurrectedPhase2;
        return false;
    }

    public void MarkResurrected()
    {
        //float health = mechromancer.currentHealth;

        if (health > 50)
            resurrectedPhase1 = true;
        else if (health > 35)
            resurrectedPhase2 = true;
    }

    private void HandleTargetChanged()
    {
        Debug.Log("Target changed, clearing action and goal");
        agent.ClearCurrentAction();
        agent.CalculatePlan();
    }

    public void TriggerResurrectionPhase()
    {
        if (resurrectGoal == null)
        {
            Debug.Log("Resurrect goal not found");
            return;
        }
        if (!isResurrecting)
        {
            isResurrecting = true;
            Debug.Log("Trigger resurrect phase");

            agent.ClearCurrentAction();
            agent.EnableOnlyThisGoal(resurrectGoal);

            if (Vector3.Distance(transform.position, hidingPosition.position) > 1f)
            {
                StartCoroutine(MoveToHidingAndResurrect());
            }
            else
            {
                agent.CalculatePlan();
                isResurrecting = false;
            }
        }
    }

    private IEnumerator MoveToHidingAndResurrect()
    {
        var nav = GetComponent<NavMeshAgent>();
        nav.SetDestination(hidingPosition.position);

        while (Vector3.Distance(transform.position, hidingPosition.position) > 1f)
        {
            yield return null;
        }

        Debug.Log("Arrived at hidingPosition, now resurrecting");

        agent.ClearCurrentAction();
        agent.CalculatePlan();

        isResurrecting = false;
    }
}
