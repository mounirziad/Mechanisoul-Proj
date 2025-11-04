using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    enum BossPhase
    {
        Waiting,
        Cinematic,
        Phase1,
        MinionWave1,
        Phase2,
        MinionWave2,
        Rage,
        Death
    }

    [SerializeField] private MechromancerBehaviour mechromancerBehaviour;
    [SerializeField] private Mechromancer mechromancer;
    [SerializeField] private ColliderTrigger arenaTrigger;
    [SerializeField] private GoapAgent agent;

    private BossPhase currentPhase = BossPhase.Waiting;
    private float phaseTimer;
    private bool playerInArena = false;

    private void OnEnable()
    {
        if (arenaTrigger != null)
        {
            arenaTrigger.OnPlayerEnterTrigger += HandlePlayerEnterArena;
        }
    }

    private void OnDisable()
    {
        if (arenaTrigger != null)
        {
            arenaTrigger.OnPlayerEnterTrigger -= HandlePlayerEnterArena;
        }
    }

    private void HandlePlayerEnterArena(object sender, System.EventArgs e)
    {
        Debug.Log("Player has entered the arena");
        playerInArena = true;
        currentPhase = BossPhase.Cinematic;
    }

    private void Start()
    {
        Debug.Log("Battle system started, waiting for player to enter arena");
        currentPhase = BossPhase.Waiting;
    }

    private void Update()
    {
        phaseTimer += Time.deltaTime;

        //switch statement for boss phases
        switch (currentPhase)
        {
            case BossPhase.Waiting:
                Debug.Log("Waiting for player to enter arena");
                if (playerInArena)
                {
                    TransitionToPhase(BossPhase.Cinematic);
                    Debug.Log("Transition to cinematic");
                }
                break;

            case BossPhase.Cinematic:
                if (phaseTimer >= 5f) //Assuming cinematic lasts 5 seconds, change as needed
                {
                    Debug.Log("Cinematic over");
                    StartBattle();
                    agent.Activate();
                    TransitionToPhase(BossPhase.Phase1);
                    Debug.Log("Transition to phase 1");
                }
                break;

            case BossPhase.Phase1:
                if (mechromancer.currentHealth <= 75f)
                {
                    TransitionToPhase(BossPhase.MinionWave1);
                    Debug.Log("Transition to minion wave 1");
                }
                /*if (mechromancerBehaviour.EvaluateResurrection())
                {
                    TransitionToPhase(BossPhase.MinionWave1);
                    Debug.Log("Transition to minion wave 1");
                }*/
                break;

            case BossPhase.MinionWave1:
                mechromancerBehaviour.TriggerResurrectionPhase();
                if (phaseTimer >= 10f) //Assuming minion wave lasts 10 seconds, change as needed
                {
                    TransitionToPhase(BossPhase.Phase2);
                    Debug.Log("Transition to phase 2");
                }
                break;

            case BossPhase.Phase2:
                agent.CalculatePlan();
                if (mechromancer.currentHealth <= 50f)
                {
                    TransitionToPhase(BossPhase.MinionWave2);
                    Debug.Log("Transition to minion wave 2");
                }
                /*if (mechromancerBehaviour.EvaluateResurrection())
                {
                    TransitionToPhase(BossPhase.MinionWave2);
                    Debug.Log("Transition to minion wave 2");
                }*/
                break;

            case BossPhase.MinionWave2:
                mechromancerBehaviour.TriggerResurrectionPhase();
                if (phaseTimer >= 10f && mechromancer.currentHealth <= 25f) //Assuming minion wave lasts 10 seconds, change as needed
                {
                    TransitionToPhase(BossPhase.Rage);
                    Debug.Log("Transition to rage");
                }
                break;

            case BossPhase.Rage:
                if (mechromancer.isDead)
                {
                    TransitionToPhase(BossPhase.Death);
                }
                break;

            case BossPhase.Death:
                Debug.Log("Boss defeated!");
                break;
        }
    }

    private void StartBattle()
    {
        TransitionToPhase(BossPhase.Waiting);
        Debug.Log("Start battle");
        //Unsure if needed
    }

    private void TransitionToPhase(BossPhase nextPhase)
    {
        currentPhase = nextPhase;
        phaseTimer = 0f;
    }
}
