using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Header("XP Settings")]
    [SerializeField] int startingXPThreshold = 100;
    [SerializeField] float xpScalingFactor = 0.05f;

    int currentXP;
    int lvlUpXP;
    int skillPoints;

    // track the XP at the start of the current level for the bar
    int currentLevelStartXP;

    // public read-only accessors
    public int CurrentXP
    {
        get { return currentXP; }
    }

    public int CurrentSkillPoints
    {
        get { return skillPoints; }
    }

    public int CurrentLevelStartXP
    {
        get { return currentLevelStartXP; }
    }

    public int CurrentXPInLevel
    {
        get { return currentXP - currentLevelStartXP; }
    }

    public int CurrentLevelXPSpan
    {
        get { return lvlUpXP - currentLevelStartXP; }
    }

    public float CurrentXPPercent
    {
        get
        {
            int span = CurrentLevelXPSpan;
            if (span <= 0) return 1f;
            return (float)CurrentXPInLevel / span;
        }
    }

    void Awake()
    {
        // singleton + persist
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (lvlUpXP <= 0)
        {
            lvlUpXP = startingXPThreshold;
        }

        currentLevelStartXP = 0;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        CheckLevelUp();
    }

    void CheckLevelUp()
    {
        // handle large XP gains that cross multiple levels
        while (currentXP >= lvlUpXP)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        int previousThreshold = lvlUpXP;

        // 1 upgrade point per level
        skillPoints += 1;

        // compute new threshold
        lvlUpXP += (int)(startingXPThreshold * (1f + xpScalingFactor));

        // new level starts at the old threshold
        currentLevelStartXP = previousThreshold;
    }

    // used by Upgrade UI for spending arbitrary amounts
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

    // used by the snapshot manager to restore available points on floor respawn
    public void SetSkillPoints(int amount)
    {
        skillPoints = Mathf.Max(0, amount);
    }

    // LEGACY CALL ONLY - DO NOT USE
    public bool UseSkillPoint()
    {
        return TrySpendPoints(1);
    }
}
