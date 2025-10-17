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
        
    }

    private void StartBattle()
    {
        Debug.Log("StartBattle");
        //enemyTransform.GetComponent<>().Spawn();
        //Invoke Mechromancer spawner
        //Unsure if needed
    }
}
