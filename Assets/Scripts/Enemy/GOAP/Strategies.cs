using System;
using System.Collections;
using System.ComponentModel;
using JetBrains.Annotations;
using Unity.VisualScripting;
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

public class IntroStrategy : IActionStrategy
{
    public bool CanPerform => false; //Can only be achieved after player is noticed
    public bool Complete { get; private set; } //set complete after intro scene is played

    public IntroStrategy()
    {
        //put in cinematic information
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

    //Idle animations
    //Need to add animation state machine in order to get this to work
    //Save for next SCRUM
    /*
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
    */
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
    public bool CanPerform => true;
    public bool Complete {  get; private set; }

    private readonly IDamage damageProvider;
    private readonly GoapAgent agent;
    private Mechromancer mechromancer;
    private MechAnimationController animationController;
    private readonly NavMeshAgent navMesh;
    private readonly float attackDuration = 1.5f;
    private readonly CountdownTimer timer;

    public AttackStrategy(GoapAgent agent)
    {
        this.agent = agent;
        this.navMesh = agent.GetComponent<NavMeshAgent>();

        this.damageProvider = agent.GetComponent<IDamage>();
        this.mechromancer = agent.GetComponent<Mechromancer>();
        this.animationController = agent.GetComponent<MechAnimationController>();

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

        if (animationController != null)
        {
            animationController.SetIsAttacking(true);
        }

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
        if (animationController != null)
        {
            animationController.SetIsAttacking(false);
        }

        navMesh.isStopped = false;
        navMesh.updateRotation = true;
    }

    void AbortAttack()
    {
        timer.Stop();
        Complete = true;

        if (animationController != null)
        {
            animationController.SetIsAttacking(false);
        }

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
            //float damage = damageProvider != null ? damageProvider.GetDamage() : 8f;
            //playerHealth.TakeDamage(damage);
            //Debug.Log($"Boss dealt {damage} damage to the player");
        }
    }
}

public class LightningAttackStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete {  get; private set; }

    private readonly GoapAgent agent;
    private readonly NavMeshAgent navMesh;
    private readonly IDamage damageProvider;
    private readonly LightningController lightningController;
    private readonly CountdownTimer timer;

    private readonly float attackDuration = 2.5f;

    private Mechromancer mechromancer;
    private Vector3 targetPosition;
    private bool lightningFired = false;

    public LightningAttackStrategy(GoapAgent agent, LightningController lightningController)
    {
        this.agent = agent;
        this.navMesh = agent.GetComponent<NavMeshAgent>();
        this.damageProvider = agent.GetComponent<IDamage>();
        this.mechromancer = agent.GetComponent<Mechromancer>();
        this.lightningController = lightningController;

        timer = new CountdownTimer(attackDuration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () =>
        {
            Complete = true;
            lightningController.StopLightning();
        };
    }

    public void Start()
    {
        Debug.Log("Starting lightning strategy");
        navMesh.isStopped = true;

        if (agent.Player == null)
        {
            Debug.LogWarning("No player found, cannot cast lightning. Check inspector");
            Complete = true;
            return;
        }

        targetPosition = agent.Player.transform.position;

        //animation
        //mechromancer?.TriggerLightningAnimation();

        timer.Start();

        float castDelay = 0.75f; //so boss can raise their hand
        agent.StartCoroutine(DelayedLightningCast(castDelay));
    }

    private System.Collections.IEnumerator DelayedLightningCast(float delay)
    {
        yield return new WaitForSeconds(delay);
        FireLightning();
    }

    private void FireLightning()
    {
        if (lightningFired) return;

        lightningFired = true;
        //lightningController.CastLightning(targetPosition);
        Debug.Log("Lightning casted toward {targetPosition}");
    }

    public void Update(float deltaTime)
    {
        timer.Tick(deltaTime);

        if (agent.Player != null && !Complete)
        {
            Vector3 direction = (agent.Player.transform.position - agent.transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, Quaternion.LookRotation(direction), deltaTime * 3f);
            }
        }
    }

    public void Stop()
    {
        lightningController.StopLightning();
        navMesh.isStopped = false;
        Complete = true;
    }

}

public class ResurrectStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete { get; private set; }

    readonly MechromancerBehaviour mechromancer;
    readonly GoapAgent agent;
    readonly GameObject enemyPrefab;
    readonly Transform[] spawnPoints;
    readonly Transform hidingPos;
    readonly float resurrectionTime = 5f;
    CountdownTimer timer;

    bool hasStarted = false;
    bool movingToHiding = false;

    public ResurrectStrategy(GoapAgent agent, GameObject enemyPrefab, Transform[] spawnPoints, Transform hidingPos)
    {
        this.agent = agent;
        this.enemyPrefab = enemyPrefab;
        this.spawnPoints = spawnPoints;
        this.hidingPos = hidingPos;

        mechromancer = agent.GetComponent<MechromancerBehaviour>();

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

        if (Vector3.Distance(agent.transform.position, hidingPos.position) > 1f)
        {
            movingToHiding = true;
            agent.StartCoroutine(MoveToHidingCoroutine());
        }
        else
        {
            timer.Start();
            mechromancer?.MarkResurrected();
        }
    }

    public void Update(float deltaTime)
    {
        if (movingToHiding) return;
        timer.Tick(deltaTime);
    }

    private IEnumerator MoveToHidingCoroutine()
    {
        var nav = agent.GetComponent<NavMeshAgent>();
        nav.SetDestination(hidingPos.position);

        while (Vector3.Distance(agent.transform.position, hidingPos.position) > 1f)
        {
            yield return null;
        }

        movingToHiding = false;
        timer.Start();
        mechromancer.MarkResurrected();
    }

    void ResurrectRobots()
    {
        Debug.Log("Summoning minions");

        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject minion = UnityEngine.Object.Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log($"Spawned enemy at {spawnPoint.name}");
        }
    }
}

public class DropDownStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete { get; private set; }

    readonly Transform boss;
    readonly Vector3 groundPosition;
    readonly float dropSpeed = 7f;

    public DropDownStrategy(Transform boss, Vector3 dropPosition)
    {
        this.boss = boss;
        this.groundPosition = dropPosition;
    }

    public void Start()
    {
        Complete = false;
    }

    public void Update(float deltaTime)
    {
        boss.position = Vector3.MoveTowards(boss.position, groundPosition, dropSpeed * deltaTime);
        if (Vector3.Distance(boss.position, groundPosition) < 0.2f)
        {
            Complete = true;
            Debug.Log("Boss landed for attack");
        }
    }
}
