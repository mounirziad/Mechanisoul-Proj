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

    EnemyCharm enemyCharm;

    void Start()
    {
        ragdoll = GetComponent<Ragdoll>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        weapons = GetComponent<AiWeapons>();

        enemyCharm = GetComponent<EnemyCharm>();

        FindPlayer();
        
        stateMachine = new AiStateMachine(this);
        stateMachine.RegisterState(new AiChasePlayerState());
        stateMachine.RegisterState(new AiDeathState());
        stateMachine.RegisterState(new AiIdleState());
        stateMachine.RegisterState(new AiFindWeaponState());
        stateMachine.RegisterState(new AiAttackState());
        stateMachine.RegisterState(new AiMeleeAttackState());
        stateMachine.RegisterState(new AiAfterMeleeAttackState());
        stateMachine.ChangeState(initialState);
    }

    void Update()
    {
        if (playertransform == null)
        {
            FindPlayer();
        }
        
        stateMachine.Update();
    }

    private void FindPlayer()
    {
        if (playertransform == null)
        {
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
    }

    public Transform GetCurrentTarget()
    {
        // if charmed, try to get enemy focus
        if (enemyCharm != null && enemyCharm.ShouldIgnorePlayerAndFightEnemies())
        {
            Transform focus = enemyCharm.GetCharmAttackTarget(transform);
            if (focus != null)
                return focus;
        }

        // fallback to player
        return playertransform;
    }
}
