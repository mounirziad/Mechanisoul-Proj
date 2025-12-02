using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;

    float dotTickDmg; //dmg per tick
    float dotTickTimer; //time till next tick
    float dotTickMaxTime = 0.25f; //time between ticks
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
            //Debug.Log("DoT Active on this frame");
            dotTickTimer -= Time.deltaTime;
            if (dotTickTimer < 0)
            {
                //Debug.Log("should be taking DoT now");
                TakeDOT();
                ResetTimer();
            }
        }
    }

    public void applyDOT(float dotTickDmg, int dotMaxTicks, int dotMaxStacks) //apply the dot effect if not at max stacks, set active, start timer
    {
        this.dotTickDmg = dotTickDmg;

        if (dotStacks.Count < dotMaxStacks) dotStacks.Add(dotMaxTicks);

        dotActive = true;
        ResetTimer();
        //Debug.Log($"dot tick damage {this.dotTickDmg}");
    }

    void TakeDOT() //take dot damage, reduce all stacks by 1, remove stacks that are now 0, set inactive if no stacks remaining, else reset timer
    {
        float dotTickTotalDamage = 0;
        for (int i = dotStacks.Count - 1; i >= 0; i--) //reverse iteration to prevent out of bounds errors
        {
            currentHealth -= dotTickDmg;
            dotStacks[i]--;
            dotTickTotalDamage += dotTickDmg;
            

            if (dotStacks[i] == 0) dotStacks.RemoveAt(i);
        }
        //Debug.Log($"Took a total of {dotTickTotalDamage} sadness tick damage");

        if (dotStacks.Count == 0) dotActive = false;
        if (currentHealth <= 0) Die();
    }

    void ResetTimer() => dotTickTimer = dotTickMaxTime;

    protected virtual void Die() { } //override
}
