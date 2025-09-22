using UnityEngine;

public class UpgradeHandler : MonoBehaviour
{
    int meleeAngerLvl, meleeSadnessLvl, meleeLoveLvl, meleeFearLvl;

    float[] meleeAOE, meleeSlow, meleeSlowLength, meleeLifeSteal, meleeStun;
    int maxLvl = 5; //highest level for upgrades

    PlayerManager playerManager;

    private void Awake()
    {
        Respec(); //set all upgrade values to 0    !!potential to continually reset player upgrades on scene change, needs testing!!
        InitializeUpgradeArrays(); //Initialize all the arrays used to store upgrade values
        SetUpgradeValues(); //sets all the upgrade values

        //get player manager script
        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();

        //for testing purposes
        //TestUpgrades();
    }

    //for testing purposes !!remove once testing can be done outside of script
    void TestUpgrades()
    {
        meleeAngerLvl = 1;
        meleeSadnessLvl = 2;
        meleeLoveLvl = 3;
        meleeFearLvl = 4;

        SendChanges();
    }

    void InitializeUpgradeArrays()
    {
        meleeAOE = new float[maxLvl + 1];
        meleeSlow = new float[maxLvl + 1];
        meleeSlowLength = new float[maxLvl + 1];
        meleeLifeSteal = new float[maxLvl + 1];
        meleeStun = new float[maxLvl + 1];
    }

    void SetUpgradeValues()
    {
        //set the upgrade values for all types
        SetAOE();
        SetSlow();
        SetLifeSteal();
        SetStun();
    }

    void SetAOE()
    {
        float baseAmt = 0.05f; //level one amount (% of dmg dealt)
        float amt = 0;

        for (int i = 1; i < meleeAOE.Length; i++)
        {
            amt += baseAmt;
            meleeAOE[i] = amt;
        }
    }

    void SetSlow()
    {
        float baseAmt = 0.1f; //level one amount of slow (% of base speed)
        float baseAmtLength = 2f; //level one amount of slow length (seconds)
        float amt = 0;
        float amtLength = 0;

        for (int i = 1; i < meleeSlow.Length; i++)
        {
            amt += baseAmt;
            meleeSlow[i] = amt;
            amtLength += baseAmtLength;
            meleeSlowLength[i] = amtLength;
        }
    }

    void SetLifeSteal()
    {
        float baseAmt = 0.1f; //level one amount (% of dmg dealt)
        float amt = 0;

        for (int i = 1; i < meleeLifeSteal.Length; i++)
        {
            amt += baseAmt;
            meleeLifeSteal[i] = amt;
        }
    }

    void SetStun()
    {
        float baseAmt = 0.5f; //level one amount (seconds)
        float amt = 0;

        for (int i = 1; i < meleeStun.Length; i++)
        {
            amt += baseAmt;
            meleeStun[i] = amt;
        }
    }

    public void Respec()
    {
        meleeAngerLvl = 0;
        meleeSadnessLvl = 0;
        meleeLoveLvl = 0;
        meleeFearLvl = 0;
    }

    void SendChanges()
    {
        //send new values to Player
        playerManager.UpdateUpgrades(meleeAOE[meleeAngerLvl], 
                                     meleeSlow[meleeSadnessLvl], 
                                     meleeSlowLength[meleeSadnessLvl], 
                                     meleeLifeSteal[meleeLoveLvl],
                                     meleeStun[meleeFearLvl]);
    }

    public void MeleeAngerUp()
    {
        //update anger level
        meleeAngerLvl++;
        
        //update player
        SendChanges();
    }

    public void MeleeAngerDown()
    {
        //update anger level
        meleeAngerLvl--;

        //update player
        SendChanges();
    }

    public void MeleeSadnessUp()
    {
        //update sadness level
        meleeSadnessLvl++;

        //update player
        SendChanges();
    }
    public void MeleeSadnessDown()
    {
        //update sadness level
        meleeSadnessLvl--;

        //update player
        SendChanges();
    }

    public void MeleeLoveUp()
    {
        //update love level
        meleeLoveLvl++;

        //update player
        SendChanges();
    }

    public void MeleeLoveDown()
    {
        //update love level
        meleeLoveLvl--;

        //update player
        SendChanges();
    }

    public void MeleeFearUp()
    {
        //update fear level
        meleeFearLvl++;

        //update player
        SendChanges();
    }

    public void MeleeFearDown()
    {
        //update fear level
        meleeFearLvl--;

        //update player
        SendChanges();
    }
}
