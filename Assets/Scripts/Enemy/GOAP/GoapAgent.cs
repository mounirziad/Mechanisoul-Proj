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
    public int damage = 10;

    [Header("Player Information")]
    [SerializeField] private GameObject player;
    public GameObject Player => player;

    NavMeshAgent navMesh;
    Rigidbody rb;

    GameObject target;
    Vector3 destination;

    CountdownTimer timer;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan; //stack of actions
    public AgentAction currentAction;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    private bool resurrectedInPhaseOne = false;
    private bool resurrectedInPhaseTwo = false;
    public bool canResurrect => EvaluateResurrection();

    IGoapPlanner gPlanner;

    private void Awake()
    {
        navMesh = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        gPlanner = new GoapPlanner();
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
        //factory.AddBelief("MovingToAttack", () => navMesh.hasPath);

        factory.AddBelief("PhaseOne", () => health >= 50);
        factory.AddBelief("PhaseTwo", () => health < 50 && health >= 35);
        factory.AddBelief("Rage", () => health < 35);

        factory.AddLocationBelief("AgentAtHidingPosition", 8f, hidingPosition);
        factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);

        factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        factory.AddSensorBelief("PlayerInAttackRange", attackSensor);

        factory.AddBelief("AttackingPlayer", () => false); //Player can always be attacked, will never come true
        factory.AddBelief("CanResurrect", () => canResurrect);
        factory.AddBelief("HasResurrectedThisPhase", () => !canResurrect);
    }

    void SetupActions()
    {
        actions = new HashSet<AgentAction>();

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
            .WithStrategy(new AttackStrategy(this))
            .AddPrecondition(beliefs["PlayerInAttackRange"])
            .AddEffect(beliefs["AttackingPlayer"])
            .Build());

        actions.Add(new AgentAction.Builder("Resurrect Robots")
            .WithStrategy(new ResurrectStrategy(this.gameObject))
            .AddPrecondition(beliefs["AgentAtHidingPosition"])
            .AddPrecondition(beliefs["CanResurrect"])
            .AddEffect(beliefs["HasResurrectedThisPhase"])
            .Build());
    }

    void SetupGoals()
    {
        goals = new HashSet<AgentGoal>();

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

        goals.Add(new AgentGoal.Builder("Resurrect")
            .WithPriority(5)
            .WithDesiredEffect(beliefs["HasResurrectedThisPhase"])
            .Build());
    }

    void SetupTimers() //currently timers are working to change the health bar, for the final build we do not want this as the enemy should only be taking damage from the player REMOVE SOON
    {
        timer = new CountdownTimer(2f);
        timer.OnTimerStop += () =>
        {
            UpdateStats();
            timer.Start();
        };
        timer.Start();
    }

    void UpdateStats()
    {
        health += InRangeOf(restingPosition.position, 3f) ? 20 : -10;
        health = Mathf.Clamp(health, 0, 75);
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) <= range;

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    void HandleTargetChanged() //force planner to change if things aren't going to plan
    {
        Debug.Log("Target changed, clearing action and goal");
        currentAction = null;
        currentGoal = null;
    }

    private void Update()
    {
        //StatsTimer and Animations update

        //Update the plan and current action if there is one
        if (currentAction == null)
        {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0)
            {
                navMesh.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                Debug.Log($"Goal: {currentGoal.Name} with {actionPlan.Actions.Count} actions in plan");
                currentAction = actionPlan.Actions.Pop();
                Debug.Log($"Popped action: {currentAction.Name}");
                //Verify all precondition effects are true
                if (currentAction.Preconditions.All(AgentBelief => AgentBelief.Evaluate()))
                {
                    currentAction.Start();
                }

                else
                {
                    Debug.Log("Preconditions not met, clearing current action and goal");
                    currentAction = null;
                    currentGoal = null;
                }
            }
        }

        //If we have a current action, execute it
        if (actionPlan != null && currentAction != null)
        {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete)
            {
                Debug.Log($"{currentAction.Name} complete");
                currentAction.Stop();
                currentAction = null;

                if (actionPlan.Actions.Count == 0)
                {
                    Debug.Log("Plan compelte");
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    void CalculatePlan()
    {
        var priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

        //If we have a current goal, we only want to check goals with a higher priority
        if (currentGoal != null)
        {
            Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
        }

        var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null)
        {
            actionPlan = potentialPlan;
        }
    }

    bool EvaluateResurrection()
    {
        if (health > 50) //phase one
        {
            return !resurrectedInPhaseOne;
        }

        else if (health > 35) //phase two
        {
            return !resurrectedInPhaseTwo;
        }

        else //rage
        {
            return false;
        }
    }

    public void MarkResurrected()
    {
        if (health > 50)
        {
            resurrectedInPhaseOne = true;
        }
        else if (health > 35)
        {
            resurrectedInPhaseTwo = true;
        }
    }
}
