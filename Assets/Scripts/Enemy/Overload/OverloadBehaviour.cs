using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Overload))]
public class OverloadBehaviour : MonoBehaviour, IGoapBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor; //bigger radius
    [SerializeField] Sensor attackSensor;

    [Header("Locations")]
    [SerializeField] Transform restingPosition;
    [SerializeField] Transform hidingPosition;

    [Header("Player Reference")]
    [SerializeField] GameObject player;

    private Overload overload;

    private GoapAgent agent;

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
        overload = GetComponent<Overload>();

        var beliefs = new Dictionary<string, AgentBelief>();
        var factory = new BeliefFactory(GetComponent<GoapAgent>(), beliefs);

        factory.AddBelief("Nothing", () => false);

        factory.AddBelief("AgentIdle", () => !GetComponent<NavMeshAgent>().hasPath);
        factory.AddBelief("AgentMoving", () => GetComponent<NavMeshAgent>().hasPath);

        factory.AddLocationBelief("AgentAtHidingPosition", 8f, hidingPosition);
        factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);

        factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        factory.AddSensorBelief("PlayerInAttackRange", attackSensor);

        factory.AddBelief("AttackingPlayer", () => false); //Player can always be attacked, will never come true

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

        goals.Add(new AgentGoal.Builder("Ambush Player")
            .WithPriority(4)
            .WithDesiredEffect(beliefs["PlayerInAttackRange"])
            .Build());

        goals.Add(new AgentGoal.Builder("SeekAndKill")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AttackingPlayer"])
            .Build());

        return goals;
    }

    private void HandleTargetChanged()
    {
        Debug.Log("Target changed, clearing action and goal");
        agent.ClearCurrentAction();
        agent.CalculatePlan();
    }
}
