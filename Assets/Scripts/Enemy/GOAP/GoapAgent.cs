using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent (typeof(NavMeshAgent))]
public partial class GoapAgent : MonoBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor; //bigger radius
    [SerializeField] public Sensor attackSensor;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;
    public GameObject Player => player;
    GameObject target;
    Vector3 destination;

    //Goap core information
    CountdownTimer timer;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan; //stack of actions
    public AgentAction currentAction;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    IGoapPlanner gPlanner;
    IGoapBehaviour behaviour;

    private NavMeshAgent navMesh;
    public NavMeshAgent NavMesh => navMesh;

    private void Awake()
    {
        navMesh = GetComponent<NavMeshAgent>();
        /*rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        mechromancer = GetComponent<Mechromancer>();*/
        gPlanner = new GoapPlanner();
    }

    private void Start()
    {
        SetupTimers();
        //Beliefs have to come first because actions and goals depend on beliefs
        
        behaviour = GetComponent<IGoapBehaviour>();

        if (behaviour == null)
        {
            Debug.LogError("Missing IGoapBehaviour on agent");
            enabled = false;
            return;
        }

        beliefs = behaviour.ProvideBeliefs();
        actions = behaviour.ProvideActions(beliefs);
        goals = behaviour.ProvideGoals(beliefs);

        chaseSensor.OnTargetChanged += HandleTargetChanged;
    }

    private void OnDestroy()
    {
        chaseSensor.OnTargetChanged -= HandleTargetChanged;
    }

    void SetupTimers() //currently timers are working to change the health bar, for the final build we do not want this as the enemy should only be taking damage from the player REMOVE SOON
    {
        timer = new CountdownTimer(2f);
        timer.OnTimerStop += () =>
        {
            timer.Start();
        };
        timer.Start();
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) <= range;

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    public void HandleTargetChanged() //force planner to change if things aren't going to plan
    {
        Debug.Log("Target changed, clearing action and goal");

        currentAction?.Stop();
        currentAction = null;
        currentGoal = null;
        actionPlan = null;
        lastGoal = null;
        CalculatePlan();
    }

    public void ClearCurrentAction()
    {
        Debug.Log("Aborting current action and resetting goal");

        currentAction?.Stop();
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

    public void CalculatePlan()
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
}
