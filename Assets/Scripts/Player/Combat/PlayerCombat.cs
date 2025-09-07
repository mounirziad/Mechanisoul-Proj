using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public List<AttackSO> combo;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;

    // Add these new variables
    private bool isAttacking = false;
    private float minAnimationPlayTime = 0.4f; // Minimum time an animation must play before next attack can be triggered
    private float attackStartTime;
    private bool attackQueued = false;

    // Reference to the existing input system from InputManager
    private PlayerControls playerControls;
    private InputManager inputManager;

    Animator anim;
    [SerializeField] Weapon weapon;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
    }

    void Start()
    {
        // Use the existing input system instance
        if (inputManager != null && inputManager.playerControls != null)
        {
            playerControls = inputManager.playerControls;
            playerControls.PlayerActions.Attack.performed += OnAttackPerformed;
        }

        if (weapon != null)
        {
            weapon.DisableTriggerBox();
        }
    }

    private void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.PlayerActions.Attack.performed -= OnAttackPerformed;
        }
    }

    // Event-based approach instead of polling
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (CanAttack())
        {
            Attack();
        }
        else if (isAttacking)
        {
            // Queue the attack for later
            attackQueued = true;
        }
    }

    void Update()
    {
        CheckAttackCompletion();
        ProcessQueuedAttack();
    }

    bool CanAttack()
    {
        // Can attack if not currently attacking and enough time has passed since last combo ended
        return !isAttacking && Time.time - lastComboEnd > 0.2f;
    }

    void Attack()
    {
        if (comboCounter < combo.Count && combo[comboCounter] != null)
        {
            CancelInvoke("EndCombo");

            // Set up the attack
            anim.runtimeAnimatorController = combo[comboCounter].animatorOV;
            anim.Play("Attack", 0, 0);
            weapon.damage = combo[comboCounter].damage;

            // Update state
            isAttacking = true;
            attackStartTime = Time.time;
            comboCounter++;
            lastClickedTime = Time.time;
            attackQueued = false;

            if (comboCounter >= combo.Count)
            {
                comboCounter = 0;
            }
        }
    }

    void CheckAttackCompletion()
    {
        if (isAttacking)
        {
            // Check if the current animation has played enough
            float normalizedTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;

            // Allow next attack only after minimum play time has passed
            if (Time.time - attackStartTime >= minAnimationPlayTime && normalizedTime > 0.7f)
            {
                // Ready for next attack if one is queued
                if (attackQueued)
                {
                    isAttacking = false;
                }
            }

            // Check if animation is completely finished
            if (normalizedTime > 0.95f && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                CompleteAttack();
            }
        }
    }

    void ProcessQueuedAttack()
    {
        if (attackQueued && !isAttacking && Time.time - attackStartTime >= minAnimationPlayTime)
        {
            Attack();
        }
    }

    void CompleteAttack()
    {
        isAttacking = false;

        // If no attack is queued and we're at the end of combo, start ending the combo
        if (!attackQueued)
        {
            Invoke("EndCombo", 0.5f); // Give a small buffer before combo ends
        }
    }

    void EndCombo()
    {
        // Only end combo if no new attack has started
        if (!isAttacking)
        {
            comboCounter = 0;
            lastComboEnd = Time.time;
            attackQueued = false;
        }
    }

    public void OnAnimationEnableWeapon()
    {
        if (weapon != null)
        {
            weapon.EnableTriggerBox();
        }
    }

    public void OnAnimationDisableWeapon()
    {
        if (weapon != null)
        {
            weapon.DisableTriggerBox();
        }
    }
}