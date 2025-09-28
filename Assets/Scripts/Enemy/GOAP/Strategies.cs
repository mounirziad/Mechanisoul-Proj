using System;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public interface IActionStrategy
{
    bool CanPerform { get; } //Can we execute the strategy
    bool Complete { get; } //Is the strategy finished

    void Start() //run everytime we want to execute a strategy
    {
        //interface needs them but not using at the moment
    }

    void Update(float deltaTime) //update frame by frame using delta time
    {
        //interface needs them but not using at the moment
    }

    void Stop() //stopping strategy
    {
        //interface needs them but not using at the moment
    }
}

public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true; //Agent can always idle
    public bool Complete { get; private set; } //set complete after timer

    readonly CountdownTimer timer;

    public IdleStrategy(float duration)
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public class WanderStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly float wanderRadius;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius)
    {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
    }

    public void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius);//.With(y: 0);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}

public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
    {
        this.agent = agent;
        this.destination = destination;
    }

    public void Start() => agent.SetDestination(destination());
    public void Stop() => agent.ResetPath();
}

public class AttackStrategy : IActionStrategy
{
    public bool CanPerform => true; //agent can always attack
    public bool Complete {  get; private set; }

    readonly GoapAgent agent;
    private Mechromancer mechromancer;
    readonly NavMeshAgent navMesh;
    readonly float attackDuration = 1.5f;
    readonly CountdownTimer timer;

    public AttackStrategy(GoapAgent agent)
    {
        this.agent = agent;
        this.navMesh = agent.GetComponent<NavMeshAgent>();

        this.mechromancer = agent.GetComponent<Mechromancer>();

        timer = new CountdownTimer(attackDuration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () =>
        {
            Complete = true;
            DealDamage();
        };
    }

    public void Start()
    {
        Debug.Log("Starting attack strategy");

        navMesh.isStopped = true;
        navMesh.updateRotation = false;

        timer.Start();
    }

    public void Update(float deltaTime)
    {
        float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.Player.transform.position);
        float attackRange = agent.attackSensor.detectionRadius;

        if (distanceToPlayer < attackRange)
        {
            Debug.Log("Player moved out of attack range. Aborting attack");
            AbortAttack();
            return;
        }

        timer.Tick(deltaTime);

        if (agent.Player != null)
        {
            Vector3 direction = (agent.Player.transform.position - agent.transform.position).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, deltaTime * 5f);
            }
        }

        if (agent.Player == null)
        {
            Debug.LogWarning("No player assigned, aborting attack");
            AbortAttack();
            return;
        }
    }

    public void Stop()
    {
        navMesh.isStopped = false;
        navMesh.updateRotation = true;
    }

    void AbortAttack()
    {
        timer.Stop();
        Complete = true;

        navMesh.isStopped = false;
        navMesh.updateRotation = true;

        agent.ClearCurrentAction();
    }

    void DealDamage()
    {
        if (agent.Player == null)
        {
            Debug.LogWarning("AttackStrategy: No player reference found");
            return;
        }

        PlayerHealth playerHealth = agent.Player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("AttackStrategy: Player does not have a PlayerHealth component");
            return;
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(agent.damage);
            Debug.Log($"Boss dealt {agent.damage} damage to the player");
        }
    }
}

public class ResurrectStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete { get; private set; }

    readonly GoapAgent agent;
    readonly float resurrectionTime = 5f;
    CountdownTimer timer;

    bool hasStarted = false;

    public ResurrectStrategy(GameObject boss)
    {
        this.agent = boss.GetComponent<GoapAgent>();
        timer = new CountdownTimer(resurrectionTime);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () =>
        {
            Complete = true;
            ResurrectRobots();
        };
    }

    public void Start()
    {
        if (hasStarted) return; //prevent it happening multiple times
        hasStarted = true;

        timer.Start();
        agent.MarkResurrected();
    }

    public void Update(float deltaTime) => timer.Tick(deltaTime);

    void ResurrectRobots()
    {
        Debug.Log("Summoning minions");
    }
}
