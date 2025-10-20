using UnityEngine;

public class MeleeUpgrades : Upgrade
{
    float attackSpeedBuff, critChance, critMult; //joy upgrades
    int buffStackCap; float buffPercent; //anger upgrades
    float dotTickDmg, dotMaxTicks; //sadness upgrades
    float lsAmt; bool lsDoubleActive; //love upgrades

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
                critMult = 1.5f;
                critChance = 0.3f;
                break;
            case 3:
                critMult = 2f;
                break;
            default:
                Debug.LogError("upgrade level out of desired range");
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
                buffPercent = 0.01f;
                break;
            case 3:
                buffPercent = 0.02f;
                break;
            default:
                Debug.LogError("upgrade level out of desired range");
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
                dotMaxTicks = 5;
                break;
            case 3:
                dotMaxTicks = 10;
                break;
            default:
                Debug.LogError("upgrade level out of desired range");
                break;
        }
    }

    protected override void LoveChange()
    {
        switch (upgradeLevel)
        {
            case 0:
                lsAmt = 0;
                lsDoubleActive = false;
                break;
            case 1:
                lsAmt = 0.05f;
                break;
            case 2:
                lsAmt = 0.1f;
                lsDoubleActive = false;
                break;
            case 3:
                lsDoubleActive = true;
                break;
            default:
                Debug.LogError("upgrade level out of desired range");
                break;
        }
    }

    protected override void FearChange()
    { }

    protected override void UpdatePlayer()
    {
        playerManager.UpdateMeleeUpgrades(buffStackCap, buffPercent, attackSpeedBuff, critChance, critMult, dotTickDmg, dotMaxTicks);
    }
}
