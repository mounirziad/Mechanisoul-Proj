using UnityEngine;
using UnityEngine.InputSystem; // New Input System (for Keyboard.current)

public class UpgradeHandler : MonoBehaviour
{
    [Header("Current Levels (0..maxLvl)")]
    [SerializeField] int meleeAngerLvl, meleeSadnessLvl, meleeLoveLvl, meleeFearLvl;

    [Header("Lookup Tables (index by level)")]
    float[] meleeAOE, meleeSlow, meleeSlowLength, meleeLifeSteal, meleeStun;

    [Header("Config")]
    [SerializeField] int maxLvl = 5;               // highest level for upgrades
    [SerializeField] bool enableHotkeys = true;    // 1/2 anger, 3/4 sadness
    [SerializeField] bool allowNumpad = true;      // also read numpad 1-4

    PlayerManager playerManager;

    void Awake()
    {
        Respec();                     // set all upgrade levels to 0
        InitializeUpgradeArrays();    // allocate arrays
        SetUpgradeValues();           // fill arrays with values

        // get player manager script
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO) playerManager = playerGO.GetComponent<PlayerManager>();

        // push initial values to player (level 0)
        SendChanges();
    }

    void Update()
    {
        if (!enableHotkeys) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        // 1 = Anger -
        if ((kb.digit1Key?.wasPressedThisFrame ?? false) || (allowNumpad && (kb.numpad1Key?.wasPressedThisFrame ?? false)))
            MeleeAngerDown();

        // 2 = Anger +
        if ((kb.digit2Key?.wasPressedThisFrame ?? false) || (allowNumpad && (kb.numpad2Key?.wasPressedThisFrame ?? false)))
            MeleeAngerUp();

        // 3 = Sadness -
        if ((kb.digit3Key?.wasPressedThisFrame ?? false) || (allowNumpad && (kb.numpad3Key?.wasPressedThisFrame ?? false)))
            MeleeSadnessDown();

        // 4 = Sadness +
        if ((kb.digit4Key?.wasPressedThisFrame ?? false) || (allowNumpad && (kb.numpad4Key?.wasPressedThisFrame ?? false)))
            MeleeSadnessUp();
    }

    // ---------- Tables ----------

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
        // set the upgrade values for all types
        SetAOE();
        SetSlow();
        SetLifeSteal();
        SetStun();
    }

    void SetAOE()
    {
        float baseAmt = 0.05f; // level one amount (% of dmg dealt)
        float amt = 0f;
        meleeAOE[0] = 0f;
        for (int i = 1; i < meleeAOE.Length; i++)
        {
            amt += baseAmt;
            meleeAOE[i] = amt;
        }
    }

    void SetSlow()
    {
        float baseAmt = 0.10f;      // level one slow (% of base speed)
        float baseAmtLength = 2f;   // level one slow length (seconds)
        float amt = 0f;
        float amtLength = 0f;
        meleeSlow[0] = 0f;
        meleeSlowLength[0] = 0f;
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
        float baseAmt = 0.10f; // level one amount (% of dmg dealt)
        float amt = 0f;
        meleeLifeSteal[0] = 0f;
        for (int i = 1; i < meleeLifeSteal.Length; i++)
        {
            amt += baseAmt;
            meleeLifeSteal[i] = amt;
        }
    }

    void SetStun()
    {
        float baseAmt = 0.5f; // level one amount (seconds)
        float amt = 0f;
        meleeStun[0] = 0f;
        for (int i = 1; i < meleeStun.Length; i++)
        {
            amt += baseAmt;
            meleeStun[i] = amt;
        }
    }

    // ---------- Public API ----------

    public void Respec()
    {
        meleeAngerLvl = 0;
        meleeSadnessLvl = 0;
        meleeLoveLvl = 0;
        meleeFearLvl = 0;
    }

    public void MeleeAngerUp() { meleeAngerLvl = Mathf.Clamp(meleeAngerLvl + 1, 0, maxLvl); SendChanges(); }
    public void MeleeAngerDown() { meleeAngerLvl = Mathf.Clamp(meleeAngerLvl - 1, 0, maxLvl); SendChanges(); }

    public void MeleeSadnessUp() { meleeSadnessLvl = Mathf.Clamp(meleeSadnessLvl + 1, 0, maxLvl); SendChanges(); }
    public void MeleeSadnessDown() { meleeSadnessLvl = Mathf.Clamp(meleeSadnessLvl - 1, 0, maxLvl); SendChanges(); }

    public void MeleeLoveUp() { meleeLoveLvl = Mathf.Clamp(meleeLoveLvl + 1, 0, maxLvl); SendChanges(); }
    public void MeleeLoveDown() { meleeLoveLvl = Mathf.Clamp(meleeLoveLvl - 1, 0, maxLvl); SendChanges(); }

    public void MeleeFearUp() { meleeFearLvl = Mathf.Clamp(meleeFearLvl + 1, 0, maxLvl); SendChanges(); }
    public void MeleeFearDown() { meleeFearLvl = Mathf.Clamp(meleeFearLvl - 1, 0, maxLvl); SendChanges(); }

    // ---------- Internals ----------

    void SendChanges()
    {
        if (!playerManager) return;

        // send new values to Player
        playerManager.UpdateUpgrades(
            meleeAOE[Mathf.Clamp(meleeAngerLvl, 0, maxLvl)],
            meleeSlow[Mathf.Clamp(meleeSadnessLvl, 0, maxLvl)],
            meleeSlowLength[Mathf.Clamp(meleeSadnessLvl, 0, maxLvl)],
            meleeLifeSteal[Mathf.Clamp(meleeLoveLvl, 0, maxLvl)],
            meleeStun[Mathf.Clamp(meleeFearLvl, 0, maxLvl)]
        );
    }
}