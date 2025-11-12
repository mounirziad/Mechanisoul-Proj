using UnityEngine;

public class XPManager : MonoBehaviour
{
    int currentXP;
    int lvlUpXP;
    int skillPoints;

    [SerializeField] int startingXPThreshold = 100;
    [SerializeField] float xpScalingFactor = 0.05f;

    private void Awake()
    {
        lvlUpXP = 100;
        skillPoints = 0;
    }


    public void AddXP(int amount)
    {
        currentXP += amount;

        CheckLevelUp();
    }

    void CheckLevelUp()
    {
        if (currentXP >= lvlUpXP) LevelUp();
    }

    void LevelUp()
    {
        skillPoints++;
        lvlUpXP += (int)(startingXPThreshold * (1 + xpScalingFactor));
        CheckLevelUp();
    }

    public bool UseSkillPoint()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            return true;
        }
        Debug.Log("No Skill Point");
        return false;
    }
}
