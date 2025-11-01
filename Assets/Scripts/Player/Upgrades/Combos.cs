using UnityEngine;

public class Combos : MonoBehaviour
{
    ComboUpgrades comboUpgrades;
    PlayerManager playerManager;

    private void Awake()
    {
        comboUpgrades = gameObject.GetComponent<ComboUpgrades>();
        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    //need to update these names to match the accurate combo names once assigned
    public void UpdateCombos()
    {
        foreach (ComboUpgrade combo in comboUpgrades.comboUpgrades)
        {
            switch (combo.name)
            {
                case "meleeS3dashJ3":
                    Combo1(combo.hasCombo);
                    break;
                case "meleeJ3rangeA3":
                    Combo2(combo.hasCombo);
                    break;
                case "meleeA3dashL3":
                    Combo3(combo.hasCombo);
                    break;
                case "meleeF3dashA3":
                    Combo4(combo.hasCombo);
                    break;
                case "meleeL3rangeF3":
                    Combo5(combo.hasCombo);
                    break;
                case "dashS3rangeJ3":
                    Combo6(combo.hasCombo);
                    break;
                case "dashF3rangeL3":
                    Combo7(combo.hasCombo);
                    break;
                case "dashF3rangeS3":
                    Combo8(combo.hasCombo);
                    break;
                default:
                    Debug.LogError("Combo Name does not match any stored combos");
                    break;
            }
        }
    }

    //update all combo names later

    void Combo1(bool active)
    {
        if (active) { }
        else { }
    }

    void Combo2(bool active)
    {
        if (active) { }
        else { }
    }

    void Combo3(bool active)
    {
        if (active) { }
        else { }
    }

    void Combo4(bool active)
    {
        if (active) { }
        else { }
    }

    void Combo5(bool active)
    {
        if (active) { }
        else { }
    }

    void Combo6(bool active)
    {
        if (active) { }
        else { }
    }

    void Combo7(bool active)
    {
        if (active) { }
        else { }
    }

    void Combo8(bool active)
    {
        if (active) { }
        else { }
    }


}
