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

    private void Start()
    {
        StartBattle();
    }

    private void Update()
    {
        //switch statement for boss phases
        switch (BossPhase.Waiting)
        {
            case BossPhase.Waiting:
                //Waiting for player to enter area
                break;

            case BossPhase.Cinematic:
                //Play cinematic
                break;

            case BossPhase.Phase1:
                //Boss Phase 1 behavior
                break;

            case BossPhase.MinionWave1:
                //MechromancerBehaviour.TriggerResurrectionPhase();
                //currentPhase = BossPhase.Waiting;
                break;

            case BossPhase.Phase2:
                //Boss Phase 2 behavior
                break;

            case BossPhase.MinionWave2:
                //Spawn minion wave 2
                break;

            case BossPhase.Rage:
                //Boss Rage behavior
                break;

            case BossPhase.Death:
                //Boss Death behavior
                break;
        }
    }

    private void StartBattle()
    {
        Debug.Log("StartBattle");
        //enemyTransform.GetComponent<>().Spawn();
        //Invoke Mechromancer spawner
        //Unsure if needed
    }
}
