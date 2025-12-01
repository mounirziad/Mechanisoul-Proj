using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Header("XP Settings")]
    [SerializeField] private int startingXPThreshold = 100;
    [SerializeField] private float xpScalingFactor = 0.05f;

    int currentXP;
    int lvlUpXP;
    int skillPoints;

    // expose XP and points to other systems
    public int CurrentXP { get { return currentXP; } }
    public int CurrentSkillPoints { get { return skillPoints; } }
    public int CurrentLevelXPThreshold { get { return lvlUpXP; } }

    void Awake()
    {
        // singleton and persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // initialize threshold once
        if (lvlUpXP <= 0)
        {
            lvlUpXP = startingXPThreshold;
        }
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        CheckLevelUp();
    }

    void CheckLevelUp()
    {
        // handle big XP gains that might level multiple times
        while (currentXP >= lvlUpXP)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        // 1 upgrade point per level
        skillPoints += 1;

        // increase next threshold
        lvlUpXP += (int)(startingXPThreshold * (1f + xpScalingFactor));

        // optional: clamp currentXP if you want, or leave overflow as is
        // currentXP -= lvlUpXP;  // only if you want "progress within level" style
    }

    // General spending function used by Upgrade UI
    public bool TrySpendPoints(int amount)
    {
        if (amount <= 0) return true;

        if (skillPoints >= amount)
        {
            skillPoints -= amount;
            return true;
        }

        Debug.Log("Not enough upgrade points. Need " + amount + ", have " + skillPoints);
        return false;
    }

    // Used by the snapshot system to restore available points when respawning on a floor
    public void SetSkillPoints(int amount)
    {
        skillPoints = Mathf.Max(0, amount);
    }

    // Kept for backwards compatibility with any old calls
    public bool UseSkillPoint()
    {
        return TrySpendPoints(1);
    }
}
