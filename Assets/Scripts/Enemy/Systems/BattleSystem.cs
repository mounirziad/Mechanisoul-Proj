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
        //currentPhase = BossPhase.Cinematic;
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
                Debug.Log("Waiting for player to enter area");
                if (playerInArena)
                {
                    TransitionToPhase(BossPhase.Cinematic);
                }
                break;

            case BossPhase.Cinematic:
                if (phaseTimer >= 5f) //Assuming cinematic lasts 5 seconds, change as needed
                {
                    TransitionToPhase(BossPhase.Phase1);
                }
                break;

            case BossPhase.Phase1:
                if (mechromancerBehaviour.EvaluateResurrection())
                {
                    TransitionToPhase(BossPhase.MinionWave1);
                }
                break;

            case BossPhase.MinionWave1:
                mechromancerBehaviour.TriggerResurrectionPhase();
                if (phaseTimer >= 10f) //Assuming minion wave lasts 10 seconds, change as needed
                {
                    TransitionToPhase(BossPhase.Phase2);
                }
                break;

            case BossPhase.Phase2:
                if (mechromancerBehaviour.EvaluateResurrection())
                {
                    TransitionToPhase(BossPhase.MinionWave2);
                }
                break;

            case BossPhase.MinionWave2:
                mechromancerBehaviour.TriggerResurrectionPhase();
                if (phaseTimer >= 10f) //Assuming minion wave lasts 10 seconds, change as needed
                {
                    TransitionToPhase(BossPhase.Rage);
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
        Debug.Log("StartBattle");
        TransitionToPhase(BossPhase.Waiting);
        //Unsure if needed
    }

    private void TransitionToPhase(BossPhase nextPhase)
    {
        currentPhase = nextPhase;
        phaseTimer = 0f;
        Debug.Log("Start battle");
    }
}
