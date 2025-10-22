using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

class ComboUpgrade
{   
    public string name { get; }
    public string description { get; set; }

    public Emotions e1 { get; }
    public Emotions e2 { get; }

    public int e1Level { get; }
    public int e2Level { get; }

    public Upgrades u1 { get; }
    public Upgrades u2 { get; }



    //constructor w/ just name
    public ComboUpgrade(string name) => this.name = name;

    //constructor with all info except effect
    public ComboUpgrade(string name, Upgrades u1, Emotions e1, int e1Level, Upgrades u2, Emotions e2, int e2Level, string description = "")
    {
        this.name = name;
        this.u1 = u1;
        this.u2 = u2;
        this.e1 = e1;
        this.e2 = e2;
        this.e1Level = e1Level;
        this.e2Level = e2Level;
        this.description = description;
    }
}

public class ComboUpgrades : MonoBehaviour
{
    MeleeUpgrades meleeUpgrades;
    DashUpgradeBase dashUpgrade;
    //ranged upgrades

    //all temp names - in future initialize with all info
    static readonly ComboUpgrade[] comboUpgrades =
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
        InitializeComboDescriptions();
    }

    void InitializeComboDescriptions()
    {
        var comboList = new Dictionary<string, ComboUpgrade>();
        foreach (ComboUpgrade combo in comboUpgrades)
        {
            comboList[combo.name] = combo;
        }

        //write all combo descriptions ex:
        comboList["meleeS3dashJ3"].description = "description for this combo upgrade";
    }

    public void CheckForCombos() //check upgrade scripts to see if any combos exist **change return type to a list**
    {
        foreach (ComboUpgrade combo in comboUpgrades)
        {
            //logic for checking if combo is active
            
            //check 1st stats, if any dont match - continue

            //check 2nd stats, if any dont match - continue

            //return all valid combos
        }
    }
}


