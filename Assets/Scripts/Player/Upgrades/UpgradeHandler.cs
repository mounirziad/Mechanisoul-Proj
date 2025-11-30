using UnityEngine;
using System;

public class UpgradeHandler : MonoBehaviour
{
    public event Action LevelsChanged;
    void RaiseLevelsChanged() => LevelsChanged?.Invoke();

    // melee levels
    [Header("melee levels")]
    public int meleeAngerLvl, meleeSadnessLvl, meleeLoveLvl, meleeFearLvl, meleeJoyLvl;

    // dash levels 0..3 (0 = off, 1 = base, 2 = upgrade 1, 3 = upgrade 2)
    [Header("dash levels (0..3; 0 = off,1 = base,2 = up1,3 = up2)")]
    [Range(0, 3)] public int dashAngerLvl;
    [Range(0, 3)] public int dashSadnessLvl;
    [Range(0, 3)] public int dashJoyLvl;
    [Range(0, 3)] public int dashLoveLvl;
    [Range(0, 3)] public int dashFearLvl;


    // ranged levels
    [Header("ranged levels")]
    public int rangedJoyLvl, rangedAngerLvl, rangedSadnessLvl, rangedLoveLvl, rangedFearLvl;

    [Header("targets")]
    public PlayerManager playerManager;
    public PlayerCombat playerCombat;
    public MeshTrail meshTrail;

    [Header("dash components")]
    public JoyDashUpgrade joyDash;
    public MonoBehaviour angerDash, sadnessDash, loveDash, fearDash;

    [Header("tables")]
    public int maxLvl = 3;
    float[] joyFireRateMult, joyDamageMult, angerAOEPercent;
    int[] sadMaxStacks;
    float[] sadSlowPerStack, loveCharmChance, loveCharmDur, fearStunDur, fearStunAoe;

    void Awake() { AllocateTables(); FillTables(); EnsureTargets(); EnsureDashRefs(); PushAll(); }

    void EnsureTargets()
    {
        if (!playerCombat || !playerManager || !meshTrail)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p)
            {
                if (!playerManager) playerManager = p.GetComponent<PlayerManager>();
                if (!playerCombat) playerCombat = p.GetComponent<PlayerCombat>();
                if (!meshTrail) meshTrail = p.GetComponent<MeshTrail>();
            }
        }
    }

    void AllocateTables()
    {
        joyFireRateMult = new float[maxLvl + 1];
        joyDamageMult = new float[maxLvl + 1];
        angerAOEPercent = new float[maxLvl + 1];
        sadMaxStacks = new int[maxLvl + 1];
        sadSlowPerStack = new float[maxLvl + 1];
        loveCharmChance = new float[maxLvl + 1];
        loveCharmDur = new float[maxLvl + 1];
        fearStunDur = new float[maxLvl + 1];
        fearStunAoe = new float[maxLvl + 1];
    }

    void FillTables()
    {
        // index 0 = no upgrade
        joyFireRateMult[0] = 1f;
        joyDamageMult[0] = 1f;

        angerAOEPercent[0] = 0f;

        sadMaxStacks[0] = 0;
        sadSlowPerStack[0] = 0f;

        loveCharmChance[0] = 0f;
        loveCharmDur[0] = 0f;

        fearStunDur[0] = 0f;
        fearStunAoe[0] = 0f;

        // fill from 1 to maxLvl
        float fr = 1f;
        float dmg = 1f;

        for (int i = 1; i <= maxLvl; i++)
        {
            // joy
            fr += 0.15f;
            dmg += 0.10f;
            joyFireRateMult[i] = fr;
            joyDamageMult[i] = dmg;

            // anger
            angerAOEPercent[i] = 0.05f * i;

            // sadness
            sadMaxStacks[i] = 3 + i;
            sadSlowPerStack[i] = 0.05f + 0.01f * i;

            // love
            loveCharmChance[i] = Mathf.Min(0.25f + 0.03f * i, 0.40f);
            loveCharmDur[i] = 2.5f + 0.5f * i;

            // fear
            fearStunDur[i] = 1.0f + 0.5f * i;
            fearStunAoe[i] = (i < 2) ? 0f : (2.0f + 0.25f * i);
        }
    }


    int ClampUp(int v) => Mathf.Clamp(v + 1, 0, maxLvl);
    int ClampDown(int v) => Mathf.Clamp(v - 1, 0, maxLvl);

    // melee up/down
    public void MeleeAngerUp() { meleeAngerLvl = ClampUp(meleeAngerLvl); RaiseLevelsChanged(); }
    public void MeleeSadnessUp() { meleeSadnessLvl = ClampUp(meleeSadnessLvl); RaiseLevelsChanged(); }
    public void MeleeLoveUp() { meleeLoveLvl = ClampUp(meleeLoveLvl); RaiseLevelsChanged(); }
    public void MeleeFearUp() { meleeFearLvl = ClampUp(meleeFearLvl); RaiseLevelsChanged(); }
    public void MeleeJoyUp() { meleeJoyLvl = ClampUp(meleeJoyLvl); RaiseLevelsChanged(); }

    public void MeleeAngerDown() { meleeAngerLvl = ClampDown(meleeAngerLvl); RaiseLevelsChanged(); }
    public void MeleeSadnessDown() { meleeSadnessLvl = ClampDown(meleeSadnessLvl); RaiseLevelsChanged(); }
    public void MeleeLoveDown() { meleeLoveLvl = ClampDown(meleeLoveLvl); RaiseLevelsChanged(); }
    public void MeleeFearDown() { meleeFearLvl = ClampDown(meleeFearLvl); RaiseLevelsChanged(); }
    public void MeleeJoyDown() { meleeJoyLvl = ClampDown(meleeJoyLvl); RaiseLevelsChanged(); }

    // dash up/down + mutual exclusivity
    public void DashAngerUp()
    {
        dashAngerLvl = Mathf.Clamp(dashAngerLvl + 1, 0, 3);
        dashSadnessLvl = dashJoyLvl = dashLoveLvl = dashFearLvl = 0;
        ApplyDashSelection();
    }

    public void DashSadnessUp()
    {
        dashSadnessLvl = Mathf.Clamp(dashSadnessLvl + 1, 0, 3);
        dashAngerLvl = dashJoyLvl = dashLoveLvl = dashFearLvl = 0;
        ApplyDashSelection();
    }

    public void DashJoyUp()
    {
        dashJoyLvl = Mathf.Clamp(dashJoyLvl + 1, 0, 3);
        dashAngerLvl = dashSadnessLvl = dashLoveLvl = dashFearLvl = 0;
        ApplyDashSelection();
    }

    public void DashLoveUp()
    {
        dashLoveLvl = Mathf.Clamp(dashLoveLvl + 1, 0, 3);
        dashAngerLvl = dashSadnessLvl = dashJoyLvl = dashFearLvl = 0;
        ApplyDashSelection();
    }

    public void DashFearUp()
    {
        dashFearLvl = Mathf.Clamp(dashFearLvl + 1, 0, 3);
        dashAngerLvl = dashSadnessLvl = dashJoyLvl = dashLoveLvl = 0;
        ApplyDashSelection();
    }
    public void DashAngerDown()
    {
        dashAngerLvl = Mathf.Clamp(dashAngerLvl - 1, 0, 3);
        ApplyDashSelection();
    }

    public void DashSadnessDown()
    {
        dashSadnessLvl = Mathf.Clamp(dashSadnessLvl - 1, 0, 3);
        ApplyDashSelection();
    }

    public void DashJoyDown()
    {
        dashJoyLvl = Mathf.Clamp(dashJoyLvl - 1, 0, 3);
        ApplyDashSelection();
    }

    public void DashLoveDown()
    {
        dashLoveLvl = Mathf.Clamp(dashLoveLvl - 1, 0, 3);
        ApplyDashSelection();
    }

    public void DashFearDown()
    {
        dashFearLvl = Mathf.Clamp(dashFearLvl - 1, 0, 3);
        ApplyDashSelection();
    }

    public void ClearDash() { dashAngerLvl = dashSadnessLvl = dashJoyLvl = dashLoveLvl = dashFearLvl = 0; ApplyDashSelection(); }

    void EnsureDashRefs()
    {
        if (!joyDash) joyDash = GetComponent<JoyDashUpgrade>();
    }

    void ApplyDashSelection()
    {
        EnsureDashRefs();
        
        if (joyDash)
        {
            joyDash.enabled = dashJoyLvl > 0;
            joyDash.SetLevel(dashJoyLvl);
            if (dashJoyLvl > 0)
                meshTrail.SetMaterial("joy");
        }
        
        if (angerDash)
        {
            angerDash.enabled = dashAngerLvl > 0;
            if (angerDash is DashUpgradeBase angerBase)
                angerBase.SetLevel(dashAngerLvl);
            if (dashAngerLvl > 0)
                meshTrail.SetMaterial("anger");
        }
        
        if (sadnessDash)
        {
            sadnessDash.enabled = dashSadnessLvl > 0;
            if (sadnessDash is DashUpgradeBase sadBase)
                sadBase.SetLevel(dashSadnessLvl);
            if (dashSadnessLvl > 0)
                meshTrail.SetMaterial("sadness");
        }
        
        if (loveDash)
        {
            loveDash.enabled = dashLoveLvl > 0;
            if (loveDash is DashUpgradeBase loveBase)
                loveBase.SetLevel(dashLoveLvl);
            if (dashLoveLvl > 0)
                meshTrail.SetMaterial("love");
        }
        
        if (fearDash)
        {
            fearDash.enabled = dashFearLvl > 0;
            if (fearDash is DashUpgradeBase fearBase)
                fearBase.SetLevel(dashFearLvl);
            if (dashFearLvl > 0)
                meshTrail.SetMaterial("fear");
        }
        
        RaiseLevelsChanged();
    }

    // ranged up/down
    public void RangedJoyUp() { rangedJoyLvl = ClampUp(rangedJoyLvl); PushRanged(); }
    public void RangedAngerUp() { rangedAngerLvl = ClampUp(rangedAngerLvl); PushRanged(); }
    public void RangedSadnessUp() { rangedSadnessLvl = ClampUp(rangedSadnessLvl); PushRanged(); }
    public void RangedLoveUp() { rangedLoveLvl = ClampUp(rangedLoveLvl); PushRanged(); }
    public void RangedFearUp() { rangedFearLvl = ClampUp(rangedFearLvl); PushRanged(); }

    public void RangedJoyDown() { rangedJoyLvl = ClampDown(rangedJoyLvl); PushRanged(); }
    public void RangedAngerDown() { rangedAngerLvl = ClampDown(rangedAngerLvl); PushRanged(); }
    public void RangedSadnessDown() { rangedSadnessLvl = ClampDown(rangedSadnessLvl); PushRanged(); }
    public void RangedLoveDown() { rangedLoveLvl = ClampDown(rangedLoveLvl); PushRanged(); }
    public void RangedFearDown() { rangedFearLvl = ClampDown(rangedFearLvl); PushRanged(); }

    public void PushAll() { ApplyDashSelection(); PushRanged(); RaiseLevelsChanged(); }

    public void PushRanged()
    {
        if (!playerCombat) return;

        var mods = new RangedModifiers
        {
            // joy
            joyFireRateMultiplier = joyFireRateMult[Mathf.Clamp(rangedJoyLvl, 0, maxLvl)],
            joyDamageMultiplier = joyDamageMult[Mathf.Clamp(rangedJoyLvl, 0, maxLvl)],
            joyCritChance = (rangedJoyLvl >= 1) ? 0.10f : 0f,
            joyCritMultiplier = (rangedJoyLvl >= 1) ? 1.5f : 1f,
            joyExplosionOnHit = rangedJoyLvl > 0,

            // anger
            angerPelletCount = (rangedAngerLvl >= 2) ? 5 : (rangedAngerLvl >= 1 ? 3 : 0),
            angerPelletSpreadDeg = (rangedAngerLvl > 0) ? 8f : 0f,
            angerRangeMultiplier = (rangedAngerLvl > 0) ? 0.7f : 1f,
            angerFireRateMultiplier = (rangedAngerLvl >= 1) ? 1.10f : 1.0f,
            angerAOEPercent = angerAOEPercent[Mathf.Clamp(rangedAngerLvl, 0, maxLvl)],
            angerExplosionOnHit = rangedAngerLvl > 0,

            // sadness
            sadnessMaxStacks = sadMaxStacks[Mathf.Clamp(rangedSadnessLvl, 0, maxLvl)],
            sadnessSlowPerStack = sadSlowPerStack[Mathf.Clamp(rangedSadnessLvl, 0, maxLvl)],

            // love
            loveCharmChance = loveCharmChance[Mathf.Clamp(rangedLoveLvl, 0, maxLvl)],
            loveCharmDuration = loveCharmDur[Mathf.Clamp(rangedLoveLvl, 0, maxLvl)],

            // fear
            fearStunDuration = fearStunDur[Mathf.Clamp(rangedFearLvl, 0, maxLvl)],
            fearStunRadius = fearStunAoe[Mathf.Clamp(rangedFearLvl, 0, maxLvl)],
        };

        playerCombat.SetRangedUpgrades(mods);
        RaiseLevelsChanged();
    }

    // injection from UpgradeRouter
    public void InjectDashComponents(JoyDashUpgrade j, MonoBehaviour a, MonoBehaviour s, MonoBehaviour l, MonoBehaviour f)
    {
        joyDash = j; angerDash = a; sadnessDash = s; loveDash = l; fearDash = f;
    }

    // compatibility shims for melee scripts (DO NOT REMOVE -alex)
    public int MeleeAngerLevel => meleeAngerLvl;
    public int MeleeSadnessLevel => meleeSadnessLvl;
    public int RangedJoyLevel => rangedJoyLvl;

    // combos menu integration
    public bool HasCombo_AngerMelee_JoyRanged =>
        meleeAngerLvl > 0 && rangedJoyLvl > 0;
}
