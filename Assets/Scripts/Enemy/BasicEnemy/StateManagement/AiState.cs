using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AiStateId
{
    ChasePlayer,
    Death,
    Idle,
    FindWeapon,
    Attack,
    MeleeAttack,
    AfterMeleeAttack
}

public interface AiState
{
    AiStateId GetId();
    void Enter(AiAgent agent);
    void Update(AiAgent agent);

    void Exit(AiAgent agent);


}


