using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR.Haptics;

public class AiAgent : MonoBehaviour
{
   public Ragdoll ragdoll;
   public SkinnedMeshRenderer skinnedMeshRenderer;
   public bool isDead = false;


    public AiStateMachine stateMachine;
    public AiStateId initialState;
    public NavMeshAgent navMeshAgent;
    public AiAgentConfig config;
    public Transform playertransform;
    public AiWeapons weapons;

    void Start()
    {
        ragdoll = GetComponent<Ragdoll>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        weapons = GetComponent<AiWeapons>();
        stateMachine = new AiStateMachine(this);
        stateMachine.RegisterState(new AiChasePlayerState());
        stateMachine.RegisterState(new AiDeathState());
        stateMachine.RegisterState(new AiIdleState());
        stateMachine.RegisterState(new AiFindWeaponState());
        stateMachine.RegisterState(new AiAttackState());
        stateMachine.RegisterState(new AiMeleeAttackState());
        stateMachine.RegisterState(new AiAfterMeleeAttackState());
        stateMachine.ChangeState(initialState);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playertransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"Player not found for AI Agent on {gameObject.name}. AI may not function correctly.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

   
}
