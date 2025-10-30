using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;

    float dotTickDmg; //dmg per tick
    int dotMaxTicks; //how many ticks per hit
    float dotMaxStacks; //max number of dmg stacks
    float dotTickTimer; //time till next tick
    float dotTickMaxTime; //time between ticks
    bool dotActive; //currently taking dmg over time
    List<int> dotStacks = new List<int>(); //list to store the dot stacks

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (dotActive)
        {
            if (dotTickTimer < 0)
            {
                TakeDOT();
                ResetTimer();
            }
        }
    }
    public void applyDOT(float dotTickDMG, int dotMaxTicks) //apply the dot effect if not at max stacks, set active, start timer
    {
        this.dotTickDmg = dotTickDMG;
        this.dotMaxTicks = dotMaxTicks;

        if (dotStacks.Count < dotMaxStacks) dotStacks.Add(dotMaxTicks);

        dotActive = true;
        ResetTimer();
    }

    void TakeDOT() //take dot damage, reduce all stacks by 1, remove stacks that are now 0, set inactive if no stacks remaining, else reset timer
    {
        for (int i = dotStacks.Count; i > 0; i--) //reverse iteration to prevent out of bounds errors
        {
            currentHealth -= dotTickDmg;
            dotStacks[i]--;

            if (dotStacks[i] == 0) dotStacks.RemoveAt(i);
        }

        if (dotStacks.Count == 0) dotActive = false;
        if (currentHealth <= 0) Die();
    }

    void ResetTimer() => dotTickTimer = dotTickMaxTime;

    void Die() { } //override

}
