using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System;

public class UpgradeHandler : MonoBehaviour
{
    // UI/Combos observers
    public event Action LevelsChanged;
    void RaiseLevelsChanged() => LevelsChanged?.Invoke();
    public int MeleeAngerLevel => meleeAngerLvl;
    public int RangedJoyLevel => rangedJoyLvl;

    public bool HasCombo_AngerMelee_JoyRanged => meleeAngerLvl > 0 && rangedJoyLvl > 0;

    // MELEE LEVELS (0 to 2)
    [Header(" MELEE LEVELS (0 to 2) ")]
    [SerializeField] int meleeAngerLvl = 0;
    [SerializeField] int meleeSadnessLvl = 0;
    [SerializeField] int meleeLoveLvl = 0;
    [SerializeField] int meleeFearLvl = 0;
    [SerializeField] int meleeJoyLvl = 0;

    // DASH LEVELS (0 to 2)
    [Header(" DASH LEVELS (0 to 2) ")]
    [SerializeField] int dashAngerLvl = 0;
    [SerializeField] int dashSadnessLvl = 0;
    [SerializeField] int dashJoyLvl = 0;
    [SerializeField] int dashLoveLvl = 0;
    [SerializeField] int dashFearLvl = 0;

    // RANGED LEVELS (0 to 2)
    [Header(" RANGED LEVELS (0 to 2) ")]
    [SerializeField] int rangedJoyLvl = 0;
    [SerializeField] int rangedAngerLvl = 0;

    [Header("Tables (index by level)")]
    // Melee
    float[] meleeAOE, meleeSlow, meleeSlowLength, meleeLifeSteal, meleeStun, meleeDMG;
    // Ranged
    float[] joyFireRateMult, joyDamageMult, angerAOEPercent;
    // Dash tables (two levels)
    float[] dashAngerAOE;      // % per level
    int[] dashAngerNodes;    // nodes per level
    float[] dashSadSlow;       // percent
    float[] dashSadSlowLen;    // seconds
    float[] dashJoyChance;     // 0.40 -> 0.60
    float[] dashJoyRadius;     // AOE size
    float[] dashJoyCritDamage; // burst damage
    float[] dashLoveWeakness;  // 0.15 -> 0.30
    float[] dashLoveSeconds;   // duration
    float[] dashLoveRadius;    // size
    float[] dashFearSeconds;   // duration
    float[] dashFearRadius;    // size

    [Header("Config")][SerializeField] int maxLvl = 5;
    [SerializeField] bool enableHotkeys = false;
    [SerializeField] bool allowNumpad = true;
    [SerializeField] Material[] vfxMaterials;
    [SerializeField] GameObject[] impactVFXPrefabs;

    // Targets (resolve by tag or drag in via Inspector)
    [Header("Targets")]
    [SerializeField] PlayerManager playerManager;   // melee sink
    [SerializeField] PlayerCombat playerCombat;     // ranged sink
    [SerializeField] MeshTrail meshTrail;

    // dash emotion components
    [Header("Dash Upgrade Components (one per emotion)")]
    [SerializeField] JoyDashUpgrade joyDash;
    [SerializeField] AngerDashUpgrade angerDash;
    [SerializeField] SadnessDashUpgrade sadnessDash;
    [SerializeField] LoveDashUpgrade loveDash;
    [SerializeField] FearDashUpgrade fearDash;

    // dash prefab components for effects
    [Header("Dash Prefabs (assigned once, used for auto-wiring)")]
    [SerializeField] JoyCritZone joyCritPrefab;
    [SerializeField] AngerDoTZone angerDoTPrefab;
    [SerializeField] SlowZone slowZonePrefab;
    [SerializeField] WeaknessZone weaknessZonePrefab;
    [SerializeField] FearZone fearZonePrefab;

    void Awake()
    {
        AllocateTables();
        FillTables();

        // wire scripts on prefab/player object to UpgradeHandler
        if (!playerManager || !playerCombat || !meshTrail)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                if (!playerManager) playerManager = player.GetComponent<PlayerManager>();
                if (!playerCombat) playerCombat = player.GetComponent<PlayerCombat>();
                if (!meshTrail) meshTrail = player.GetComponent<MeshTrail>();
            }
        }

        EnsureDashRefs();
        PushAll();
    }

    void Update()
    {

    }

    // Tables
    void AllocateTables()
    {
        meleeAOE = new float[maxLvl + 1];
        meleeSlow = new float[maxLvl + 1];
        meleeSlowLength = new float[maxLvl + 1];
        meleeLifeSteal = new float[maxLvl + 1];
        meleeStun = new float[maxLvl + 1];
        meleeDMG = new float[maxLvl + 1];

        joyFireRateMult = new float[maxLvl + 1];
        joyDamageMult = new float[maxLvl + 1];
        angerAOEPercent = new float[maxLvl + 1];

        dashAngerAOE = new float[3];
        dashAngerNodes = new int[3];
        dashSadSlow = new float[3];
        dashSadSlowLen = new float[3];
        dashJoyChance = new float[3];
        dashJoyRadius = new float[3];
        dashJoyCritDamage = new float[3];
        dashLoveWeakness = new float[3];
        dashLoveSeconds = new float[3];
        dashLoveRadius = new float[3];
        dashFearSeconds = new float[3];
        dashFearRadius = new float[3];
    }

    void FillTables()
    {
        // ---- MELEE ----
        FillLinear(meleeAOE, 0f, 0.05f);
        FillLinear(meleeSlow, 0f, 0.10f);
        FillLinear(meleeSlowLength, 0f, 2.00f);
        FillLinear(meleeLifeSteal, 0f, 0.10f);
        FillLinear(meleeStun, 0f, 0.50f);
        FillLinear(meleeDMG, 0f, 2.00f);

        // ---- DASH ----
        dashAngerAOE[0] = 0f; dashAngerNodes[0] = 0;
        dashAngerAOE[1] = 0.10f; dashAngerNodes[1] = 1;
        dashAngerAOE[2] = 0.15f; dashAngerNodes[2] = 2;

        dashSadSlow[0] = 0f; dashSadSlowLen[0] = 0f;
        dashSadSlow[1] = 0.35f; dashSadSlowLen[1] = 2.5f;
        dashSadSlow[2] = 0.35f; dashSadSlowLen[2] = 2.5f;

        dashJoyChance[0] = 0f; dashJoyRadius[0] = 0f; dashJoyCritDamage[0] = 0f;
        dashJoyChance[1] = 0.40f; dashJoyRadius[1] = 3.0f; dashJoyCritDamage[1] = 35f;
        dashJoyChance[2] = 0.60f; dashJoyRadius[2] = 3.0f; dashJoyCritDamage[2] = 35f;

        dashLoveWeakness[0] = 0f; dashLoveSeconds[0] = 0f; dashLoveRadius[0] = 0f;
        dashLoveWeakness[1] = 0.15f; dashLoveSeconds[1] = 4f; dashLoveRadius[1] = 2.5f;
        dashLoveWeakness[2] = 0.30f; dashLoveSeconds[2] = 7f; dashLoveRadius[2] = 2.5f;

        dashFearSeconds[0] = 0f; dashFearRadius[0] = 0f;
        dashFearSeconds[1] = 4f; dashFearRadius[1] = 2.5f;
        dashFearSeconds[2] = 7f; dashFearRadius[2] = 3.0f;

        // ---- RANGED ----
        float fr = 1f, dmg = 1f;
        for (int i = 0; i <= maxLvl; i++)
        {
            joyFireRateMult[i] = fr; fr += 0.15f;
            joyDamageMult[i] = dmg; dmg += 0.10f;
        }
        FillLinear(angerAOEPercent, 0f, 0.05f);
    }

    void FillLinear(float[] arr, float start, float step)
    {
        float cur = start;
        for (int i = 0; i < arr.Length; i++) { arr[i] = cur; cur += step; }
    }

    // MELEE UP
    public void MeleeAngerUp() { meleeAngerLvl = ClampUp(meleeAngerLvl); PushMelee(); }
    public void MeleeSadnessUp() { meleeSadnessLvl = ClampUp(meleeSadnessLvl); PushMelee(); }
    public void MeleeLoveUp() { meleeLoveLvl = ClampUp(meleeLoveLvl); PushMelee(); }
    public void MeleeFearUp() { meleeFearLvl = ClampUp(meleeFearLvl); PushMelee(); }
    public void MeleeJoyUp() { meleeJoyLvl = ClampUp(meleeJoyLvl); PushMelee(); }

    // MELEE DOWN
    public void MeleeAngerDown() { meleeAngerLvl = ClampDown(meleeAngerLvl); PushMelee(); }
    public void MeleeSadnessDown() { meleeSadnessLvl = ClampDown(meleeSadnessLvl); PushMelee(); }
    public void MeleeLoveDown() { meleeLoveLvl = ClampDown(meleeLoveLvl); PushMelee(); }
    public void MeleeFearDown() { meleeFearLvl = ClampDown(meleeFearLvl); PushMelee(); }
    public void MeleeJoyDown() { meleeJoyLvl = ClampDown(meleeJoyLvl); PushMelee(); }

    // DASH UP
    public void DashAngerUp() { dashAngerLvl = Mathf.Clamp(dashAngerLvl + 1, 0, 2); dashSadnessLvl = dashJoyLvl = dashLoveLvl = dashFearLvl = 0; ApplyDashSelection(); }
    public void DashSadnessUp() { dashSadnessLvl = Mathf.Clamp(dashSadnessLvl + 1, 0, 2); dashAngerLvl = dashJoyLvl = dashLoveLvl = dashFearLvl = 0; ApplyDashSelection(); }
    public void DashJoyUp() { dashJoyLvl = Mathf.Clamp(dashJoyLvl + 1, 0, 2); dashAngerLvl = dashSadnessLvl = dashLoveLvl = dashFearLvl = 0; ApplyDashSelection(); }
    public void DashLoveUp() { dashLoveLvl = Mathf.Clamp(dashLoveLvl + 1, 0, 2); dashAngerLvl = dashSadnessLvl = dashJoyLvl = dashFearLvl = 0; ApplyDashSelection(); }
    public void DashFearUp() { dashFearLvl = Mathf.Clamp(dashFearLvl + 1, 0, 2); dashAngerLvl = dashSadnessLvl = dashJoyLvl = dashLoveLvl = 0; ApplyDashSelection(); }

    // DASH DOWN
    public void DashAngerDown() { dashAngerLvl = Mathf.Clamp(dashAngerLvl - 1, 0, 2); ApplyDashSelection(); }
    public void DashSadnessDown() { dashSadnessLvl = Mathf.Clamp(dashSadnessLvl - 1, 0, 2); ApplyDashSelection(); }
    public void DashJoyDown() { dashJoyLvl = Mathf.Clamp(dashJoyLvl - 1, 0, 2); ApplyDashSelection(); }
    public void DashLoveDown() { dashLoveLvl = Mathf.Clamp(dashLoveLvl - 1, 0, 2); ApplyDashSelection(); }
    public void DashFearDown() { dashFearLvl = Mathf.Clamp(dashFearLvl - 1, 0, 2); ApplyDashSelection(); }

    // RANGED UP
    public void RangedJoyUp() { rangedJoyLvl = ClampUp(rangedJoyLvl); PushRanged(); }
    public void RangedAngerUp() { rangedAngerLvl = ClampUp(rangedAngerLvl); PushRanged(); }

    // RANGED DOWN
    public void RangedJoyDown() { rangedJoyLvl = ClampDown(rangedJoyLvl); PushRanged(); }
    public void RangedAngerDown() { rangedAngerLvl = ClampDown(rangedAngerLvl); PushRanged(); }

    // clamp helper functions
    int ClampUp(int v) => Mathf.Clamp(v + 1, 0, maxLvl);
    int ClampDown(int v) => Mathf.Clamp(v - 1, 0, maxLvl);
    int ClampUp2(ref int v) { v = Mathf.Clamp(v + 1, 0, 2); return v; }

    void PushAll() { PushMelee(); ApplyDashSelection(); PushRanged(); RaiseLevelsChanged(); }

    void PushMelee()
    {
        if (!playerManager) return;
        playerManager.UpdateUpgrades(
            meleeAOE[Mathf.Clamp(meleeAngerLvl, 0, maxLvl)],
            meleeSlow[Mathf.Clamp(meleeSadnessLvl, 0, maxLvl)],
            meleeSlowLength[Mathf.Clamp(meleeSadnessLvl, 0, maxLvl)],
            meleeLifeSteal[Mathf.Clamp(meleeLoveLvl, 0, maxLvl)],
            meleeStun[Mathf.Clamp(meleeFearLvl, 0, maxLvl)]
        );
        RaiseLevelsChanged();
    }

    // auto create dash components at runtime
    // this is partially GPT'd bc I was debugging at 8am and tired
    // will look into later if it doesnt work well
    void EnsureDashRefs()
    {
        // ensure or create each dash component on this GameObject, keep them disabled until selected
        joyDash = EnsureAndSetup(joyDash);
        angerDash = EnsureAndSetup(angerDash);
        sadnessDash = EnsureAndSetup(sadnessDash);
        loveDash = EnsureAndSetup(loveDash);
        fearDash = EnsureAndSetup(fearDash);

        // ensure dash components have their tables filled
        T EnsureAndSetup<T>(T existing) where T : Component
        {
            var c = existing;
            if (!c) c = GetComponent<T>();
            if (!c) c = gameObject.AddComponent<T>();

            // Disable by default; ApplyDashSelection() will enable the chosen one
            if (c is Behaviour b) b.enabled = false;

            // Push prefabs once so you don’t need to touch the component inspectors
            if (c is JoyDashUpgrade joy && joy.joyCritPrefab == null) joy.joyCritPrefab = joyCritPrefab;
            if (c is AngerDashUpgrade anger && anger.fireDoTPrefab == null) anger.fireDoTPrefab = angerDoTPrefab;
            if (c is SadnessDashUpgrade sad && sad.slowZonePrefab == null) sad.slowZonePrefab = slowZonePrefab;
            if (c is LoveDashUpgrade love && love.weaknessZonePrefab == null) love.weaknessZonePrefab = weaknessZonePrefab;
            if (c is FearDashUpgrade fear && fear.fearZonePrefab == null) fear.fearZonePrefab = fearZonePrefab;

            return c as T;
        }
    }


    // accepts injected dash components (for prefab not on player)
    // purely for cosmetic reasons bc there's too much shit on the prefab rn
    public void InjectDashComponents(
    JoyDashUpgrade joy, AngerDashUpgrade anger,
    SadnessDashUpgrade sad, LoveDashUpgrade love, FearDashUpgrade fear)
    {
        joyDash = joy;
        angerDash = anger;
        sadnessDash = sad;
        loveDash = love;
        fearDash = fear;
    }

    void ApplyDashSelection()
    {
        EnsureDashRefs();

        // Enable only the chosen component
        bool anger = dashAngerLvl > 0;
        bool sadness = dashSadnessLvl > 0;
        bool joy = dashJoyLvl > 0;
        bool love = dashLoveLvl > 0;
        bool fear = dashFearLvl > 0;

        if (angerDash) angerDash.enabled = anger;
        if (sadnessDash) sadnessDash.enabled = sadness;
        if (joyDash) joyDash.enabled = joy;
        if (loveDash) loveDash.enabled = love;
        if (fearDash) fearDash.enabled = fear;

        // Set levels on each component (0 means inactive)
        if (angerDash) angerDash.SetLevel(dashAngerLvl);
        if (sadnessDash) sadnessDash.SetLevel(dashSadnessLvl);
        if (joyDash) joyDash.SetLevel(dashJoyLvl);
        if (loveDash) loveDash.SetLevel(dashLoveLvl);
        if (fearDash) fearDash.SetLevel(dashFearLvl);

        // change dash trail material quickly based on emotion
        if (meshTrail && vfxMaterials != null)
        {
            string want =
                anger ? "angerDashTrail" :
                sadness ? "sadnessDashTrail" :
                joy ? "basicDashTrail" :
                love ? "basicDashTrail" :
                fear ? "basicDashTrail" : null;

            if (!string.IsNullOrEmpty(want))
            {
                for (int i = 0; i < vfxMaterials.Length; i++)
                    if (vfxMaterials[i] && vfxMaterials[i].name == want) { meshTrail.mat = vfxMaterials[i]; break; }
            }
        }

        RaiseLevelsChanged();
    }

    void PushRanged()
    {
        if (!playerCombat) return;

        bool joyExplode = rangedJoyLvl > 0 && meleeAngerLvl > 0;

        var mods = new RangedModifiers
        {
            joyFireRateMultiplier = joyFireRateMult[Mathf.Clamp(rangedJoyLvl, 0, maxLvl)],
            joyDamageMultiplier = joyDamageMult[Mathf.Clamp(rangedJoyLvl, 0, maxLvl)],
            joyExplosionOnHit = joyExplode,

            angerAOEPercent = angerAOEPercent[Mathf.Clamp(rangedAngerLvl, 0, maxLvl)],
            angerExplosionOnHit = rangedAngerLvl > 0
        };
        RaiseLevelsChanged();
        playerCombat.SetRangedUpgrades(mods);
    }
}
