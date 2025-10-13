using UnityEngine;

public class MeleeUpgrades : Upgrade
{
    float attackSpeedBuff, critChance, critMult; //joy upgrades
    int buffStackCap; float buffPercent; //anger upgrades
    float dotTickDmg, dotMaxTicks; //sadness upgrades

    protected override void Awake()
    {
        base.Awake();

        attackSpeedBuff = 0; critChance = 0; critMult = 0;
        buffStackCap = 0; buffPercent = 0;
    }

    protected override void JoyChange()
    {
        switch (upgradeLevel)
        {
            case 0:
                attackSpeedBuff = 0;
                critChance = 0;
                critMult = 0;
                break;
            case 1:
                attackSpeedBuff = 0.10f;
                critChance = 0.15f;
                critMult = 1.5f;
                break;
            case 2:
                critChance = 0.3f;
                break;
            case 3:
                critMult = 2f;
                break;
        }
    }

    protected override void AngerChange()
    {
        switch (upgradeLevel)
        {
            case 0:
                buffStackCap = 0;
                buffPercent = 0;
                break;
            case 1:
                buffStackCap = 10;
                buffPercent = 0.01f;
                break;
            case 2:
                buffStackCap = 20;
                break;
            case 3:
                buffPercent = 0.02f;
                break;
        }
    }

    protected override void SadnessChange()
    {
        switch (upgradeLevel)
        {
            case 0:
                dotTickDmg = 0;
                dotMaxTicks = 0;
                break;
            case 1:
                dotTickDmg = 0.5f;
                dotMaxTicks = 5;
                break;
            case 2:
                dotTickDmg = 1;
                break;
            case 3:
                dotMaxTicks = 10;
                break;
        }
    }

    protected override void LoveChange()
    { }

    protected override void FearChange()
    { }

    protected override void UpdatePlayer()
    {
        playerManager.UpdateMeleeUpgrades(buffStackCap, buffPercent, attackSpeedBuff, critChance, critMult, dotTickDmg, dotMaxTicks);
    }
}
