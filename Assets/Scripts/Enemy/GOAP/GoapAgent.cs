using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent (typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor; //bigger radius
    [SerializeField] Sensor attackSensor;

    [Header("Locations")]
    [SerializeField] Transform restingPosition;
    [SerializeField] Transform hidingPosition;

    [Header("Stats")] //temporary implementation
    public float health = 75f;

    NavMeshAgent navMesh;
    Rigidbody rb;

    GameObject target;
    Vector3 destination;

    CountdownTimer timer;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    //public ActionPlan actionPlan; //stack of actions
    public AgentAction currentAction;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    private void Awake()
    {
        navMesh = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Start()
    {
        SetupTimers();
        SetupBeliefs(); //Beliefs have to come first because actions and goals depend on beliefs
        SetupActions();
        SetupGoals();
    }

    void SetupBeliefs()
    {
        beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief("Nothing", () => false);
        factory.AddBelief("AgentIdle", () => !navMesh.hasPath);
        factory.AddBelief("AgentMoving", () => navMesh.hasPath);
        factory.AddBelief("MovingToAttack", () => navMesh.hasPath);
    }

    void SetupActions()
    {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["Nothing"])
            .Build());
    }

    void SetupGoals()
    {
        goals = new HashSet<AgentGoal>();
    }

    void SetupTimers() //currently timers are working to change the health bar, for the final build we do not want this as the enemy should only be taking damage from the player REMOVE SOON
    {
        timer = new CountdownTimer(2f);
        timer.OnTimerStop += () =>
        {
            //UpdateStats();
            timer.Start();
        };
        timer.Start();
    }

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    void HandleTargetChanged() //force planner to change if things aren't going to plan
    {
        Debug.Log("Target changed, clearing action and goal");
        currentAction = null;
        currentGoal = null;
    }
}
