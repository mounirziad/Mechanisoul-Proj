using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ComboUpgrade
{   
    public string name { get; }
    public string description { get; set; }

    public Emotions emotion1 { get; }
    public Emotions emotion2 { get; }

    public int emotion1Level { get; }
    public int emotion2Level { get; }

    public Upgrades upgrade1 { get; }
    public Upgrades upgrade2 { get; }

    public bool hasCombo;



    //constructor w/ just name
    public ComboUpgrade(string name) => this.name = name;

    //constructor with all info except effect
    public ComboUpgrade(string name, Upgrades upgrade1, Emotions emotion1, int emotion1Level, Upgrades upgrade2, Emotions emotion2, int emotion2Level, string description = "", bool hasCombo = false)
    {
        this.name = name;
        this.upgrade1 = upgrade1;
        this.upgrade2 = upgrade2;
        this.emotion1 = emotion1;
        this.emotion2 = emotion2;
        this.emotion1Level = emotion1Level;
        this.emotion2Level = emotion2Level;
        this.description = description;
        this.hasCombo = hasCombo;
    }
}

public class ComboUpgrades : MonoBehaviour
{
    MeleeUpgrades meleeUpgrades;
    DashUpgradeBase dashUpgrades;
    //ranged upgrades
    PlayerManager playerManager;

    public Dictionary<string, ComboUpgrade> comboList { get; private set; }

    //all temp names - in future initialize with all info
    public readonly ComboUpgrade[] comboUpgrades =
    {
        new ComboUpgrade("meleeS3dashJ3"),
        new ComboUpgrade("meleeJ3rangeA3"),
        new ComboUpgrade("meleeA3dashL3"),
        new ComboUpgrade("meleeF3dashA3"),
        new ComboUpgrade("meleeL3rangeF3"),
        new ComboUpgrade("dashS3rangeJ3"),
        new ComboUpgrade("dashF3rangeL3"),
        new ComboUpgrade("dashF3rangeS3")
    };

    private void Awake()
    {
        InitializeComboDictionary();
        InitializeComboDescriptions();

        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    void InitializeComboDictionary()
    {
        comboList = new Dictionary<string, ComboUpgrade>();
        foreach (ComboUpgrade combo in comboUpgrades)
        {
            comboList[combo.name] = combo;
        }
    }

    void InitializeComboDescriptions()
    {
        if (comboList == null)
        {
            Debug.LogError("Combo Dictionary not initialized before trying to write descriptions");
            return;
        }
        
        //write all combo descriptions ex:
        comboList["meleeS3dashJ3"].description = "description for this combo upgrade";
        //
        //
        //
        //
    }

    public void CheckCombos() //check upgrade scripts to see if any combos exist **change return type to a list**
    {
        foreach (ComboUpgrade combo in comboUpgrades)
        {
            Upgrade upgradeScript;

            //check 1st stats, if any dont match - hasCombo = false, continue
            upgradeScript = GetUpgradeScript(combo.upgrade1);
            if (upgradeScript.selectedEmotion != combo.emotion1) { combo.hasCombo = false; continue; }
            if (upgradeScript.upgradeLevel <= combo.emotion1Level) { combo.hasCombo = false; continue; }

            //check 2nd stats, if any dont match - hasCombo = false, continue
            upgradeScript = GetUpgradeScript(combo.upgrade2);
            if (upgradeScript.selectedEmotion != combo.emotion2) { combo.hasCombo = false; continue; }
            if (upgradeScript.upgradeLevel <= combo.emotion2Level) { combo.hasCombo = false; continue; }

            combo.hasCombo = true;
            //break; uncomment if we are sure one upgrade is tied to only 1 combo

            //update playermanager with new combo status --- update names once combo names are finalized
            switch (combo.name)
            {
                case "meleeS3dashJ3":
                    playerManager.UpdateCombo1(combo.hasCombo);
                    break;
                case "meleeJ3rangeA3":
                    playerManager.UpdateCombo2(combo.hasCombo);
                    break;
                case "meleeA3dashL3":
                    playerManager.UpdateCombo3(combo.hasCombo);
                    break;
                case "meleeF3dashA3":
                    playerManager.UpdateCombo4(combo.hasCombo);
                    break;
                case "meleeL3rangeF3":
                    playerManager.UpdateCombo5(combo.hasCombo);
                    break;
                case "dashS3rangeJ3":
                    playerManager.UpdateCombo6(combo.hasCombo);
                    break;
                case "dashF3rangeL3":
                    playerManager.UpdateCombo7(combo.hasCombo);
                    break;
                case "dashF3rangeS3":
                    playerManager.UpdateCombo8(combo.hasCombo);
                    break;
                default:
                    Debug.LogError("Combo Name does not match any stored combos");
                    break;
            }

        }
    }

    Upgrade GetUpgradeScript(Upgrades upgradeType)
    {
        switch (upgradeType)
        {
            case Upgrades.Melee:
                return meleeUpgrades;
            case Upgrades.Range:
                return meleeUpgrades; //replace with range upgrade script once available
            case Upgrades.Dash:
                return dashUpgrades;
            default:
                Debug.LogError("Combo Upgrade not in upgrades list");
                return null;
        }
    }
}


