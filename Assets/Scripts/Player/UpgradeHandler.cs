using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class UpgradeHandler : MonoBehaviour
{
    [Header(" MELEE LEVELS (0..maxLvl) ")]
    [SerializeField] int meleeAngerLvl = 0;
    [SerializeField] int meleeSadnessLvl = 0;
    [SerializeField] int meleeLoveLvl = 0;
    [SerializeField] int meleeFearLvl = 0;

    [Header(" DASH LEVELS (0..maxLvl) ")]
    [SerializeField] int dashAngerLvl = 0;
    [SerializeField] int dashSadnessLvl = 0;

    [Header(" RANGED LEVELS (0..maxLvl) ")]
    [SerializeField] int rangedJoyLvl = 0;
    [SerializeField] int rangedAngerLvl = 0;

    [Header("Tables (index by level)")]
    // Melee
    float[] meleeAOE, meleeSlow, meleeSlowLength, meleeLifeSteal, meleeStun;
    // Dash
    float[] dashAngerAOE, dashSadSlow, dashSadSlowLen;
    // Ranged
    float[] joyFireRateMult, joyDamageMult, angerAOEPercent;

    [Header("Config")]
    [SerializeField] int maxLvl = 5;
    [SerializeField] bool enableHotkeys = false; // default off
    [SerializeField] bool allowNumpad = true;
    [SerializeField] Material[] vfxMaterials;


    // Targets (resolve by tag or drag in via Inspector)
    [SerializeField] PlayerManager playerManager;   // melee sink
    [SerializeField] DashAbility dashAbility;       // dash sink
    [SerializeField] PlayerCombat playerCombat;     // ranged sink
    [SerializeField] MeshTrail meshTrail;

    void Awake()
    {
        AllocateTables();
        FillTables();

        // Fallback: find by tag if not set via Inspector
        if (!playerManager || !dashAbility || !playerCombat)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                if (!playerManager) playerManager = player.GetComponent<PlayerManager>();
                if (!dashAbility) dashAbility = player.GetComponent<DashAbility>();
                if (!playerCombat) playerCombat = player.GetComponent<PlayerCombat>();
                if (!meshTrail) meshTrail = player.GetComponent<MeshTrail>();
            }
        }

        PushAll();
    }

    void Update()
    {
        if (!enableHotkeys) return;
        var kb = Keyboard.current; if (kb == null) return;

        // --- MELEE ---
        if (Pressed(kb.digit1Key)) MeleeAngerDown();
        if (Pressed(kb.digit2Key)) MeleeAngerUp();
        if (Pressed(kb.digit3Key)) MeleeSadDown();
        if (Pressed(kb.digit4Key)) MeleeSadUp();
        if (Pressed(kb.digit5Key)) MeleeLoveDown();
        if (Pressed(kb.digit6Key)) MeleeLoveUp();
        if (Pressed(kb.digit7Key)) MeleeFearDown();
        if (Pressed(kb.digit8Key)) MeleeFearUp();

        // --- RANGED ---
        if (Pressed(kb.jKey)) RangedJoyDown();
        if (Pressed(kb.kKey)) RangedJoyUp();
        if (Pressed(kb.digit9Key)) RangedAngerDown();
        if (Pressed(kb.digit0Key)) RangedAngerUp();

        // --- DASH ---
        if (Pressed(kb.nKey)) DashAngerDown();
        if (Pressed(kb.mKey)) DashAngerUp();
        if (Pressed(kb.hKey)) DashSadDown();
        if (Pressed(kb.semicolonKey)) DashSadUp();
    }

    bool Pressed(KeyControl key) => key != null && key.wasPressedThisFrame;

    // ---------- Tables ----------
    void AllocateTables()
    {
        meleeAOE = new float[maxLvl + 1];
        meleeSlow = new float[maxLvl + 1];
        meleeSlowLength = new float[maxLvl + 1];
        meleeLifeSteal = new float[maxLvl + 1];
        meleeStun = new float[maxLvl + 1];

        dashAngerAOE = new float[maxLvl + 1];
        dashSadSlow = new float[maxLvl + 1];
        dashSadSlowLen = new float[maxLvl + 1];

        joyFireRateMult = new float[maxLvl + 1];
        joyDamageMult = new float[maxLvl + 1];
        angerAOEPercent = new float[maxLvl + 1];
    }

    void FillTables()
    {
        // ---- MELEE ----
        FillLinear(meleeAOE, 0f, 0.05f); // +5%/lvl
        FillLinear(meleeSlow, 0f, 0.10f); // +10%/lvl
        FillLinear(meleeSlowLength, 0f, 2.00f); // +2s/lvl
        FillLinear(meleeLifeSteal, 0f, 0.10f); // +10%/lvl
        FillLinear(meleeStun, 0f, 0.50f); // +0.5s/lvl

        // ---- DASH ----
        FillLinear(dashAngerAOE, 0f, 0.05f); // +5%/lvl
        FillLinear(dashSadSlow, 0f, 0.08f); // +8%/lvl
        FillLinear(dashSadSlowLen, 0f, 1.20f); // +1.2s/lvl

        // ---- RANGED ----
        // JOY: +15% fire rate / +10% damage per level
        float fr = 1f, dmg = 1f;
        for (int i = 0; i <= maxLvl; i++)
        {
            joyFireRateMult[i] = fr; fr += 0.15f;
            joyDamageMult[i] = dmg; dmg += 0.10f;
        }
        // ANGER: +5% AoE per level
        FillLinear(angerAOEPercent, 0f, 0.05f);
    }

    void FillLinear(float[] arr, float start, float step)
    {
        float cur = start;
        for (int i = 0; i < arr.Length; i++) { arr[i] = cur; cur += step; }
    }

    // ---------- Public (UI) API ----------
    // Melee
    public void MeleeAngerUp() { meleeAngerLvl = ClampUp(meleeAngerLvl); PushMelee(); PushRanged(); }
    public void MeleeAngerDown() { meleeAngerLvl = ClampDown(meleeAngerLvl); PushMelee(); PushRanged(); }
    public void MeleeSadUp() { meleeSadnessLvl = ClampUp(meleeSadnessLvl); PushMelee(); }
    public void MeleeSadDown() { meleeSadnessLvl = ClampDown(meleeSadnessLvl); PushMelee(); }
    public void MeleeLoveUp() { meleeLoveLvl = ClampUp(meleeLoveLvl); PushMelee(); }
    public void MeleeLoveDown() { meleeLoveLvl = ClampDown(meleeLoveLvl); PushMelee(); }
    public void MeleeFearUp() { meleeFearLvl = ClampUp(meleeFearLvl); PushMelee(); }
    public void MeleeFearDown() { meleeFearLvl = ClampDown(meleeFearLvl); PushMelee(); }

    // Dash
    public void DashAngerUp()
    {
        dashAngerLvl = ClampUp(dashAngerLvl); PushDash();

        for (int i = 0; i < vfxMaterials.Length; i++)
        {
            if (vfxMaterials[i].name == "angerDashTrail")
            {
                meshTrail.mat = vfxMaterials[i];
                break;
            }
        }
    }
    public void DashAngerDown()
    {
        dashAngerLvl = ClampDown(dashAngerLvl); PushDash();

        if (dashAngerLvl == 0)
        {
            for (int i = 0; i < vfxMaterials.Length; i++)
            {
                if (vfxMaterials[i].name == "basicDashTrail")
                {
                    meshTrail.mat = vfxMaterials[i];
                    break;
                }
            }
        }
    }
    public void DashSadUp()
    {
        dashSadnessLvl = ClampUp(dashSadnessLvl); PushDash();

        for (int i = 0; i < vfxMaterials.Length; i++)
        {
            if (vfxMaterials[i].name == "sadnessDashTrail")
            {
                meshTrail.mat = vfxMaterials[i];
                break;
            }
        }
    }
    public void DashSadDown()
    {
        dashSadnessLvl = ClampDown(dashSadnessLvl); PushDash();

        if (dashSadnessLvl == 0)
        {
            for (int i = 0; i < vfxMaterials.Length; i++)
            {
                if (vfxMaterials[i].name == "basicDashTrail")
                {
                    meshTrail.mat = vfxMaterials[i];
                    break;
                }
            }
        }
    }

    // Ranged
    public void RangedJoyUp() { rangedJoyLvl = ClampUp(rangedJoyLvl); PushRanged(); }
    public void RangedJoyDown() { rangedJoyLvl = ClampDown(rangedJoyLvl); PushRanged(); }
    public void RangedAngerUp() { rangedAngerLvl = ClampUp(rangedAngerLvl); PushRanged(); }
    public void RangedAngerDown() { rangedAngerLvl = ClampDown(rangedAngerLvl); PushRanged(); }

    int ClampUp(int v) => Mathf.Clamp(v + 1, 0, maxLvl);
    int ClampDown(int v) => Mathf.Clamp(v - 1, 0, maxLvl);

    // ---------- Push ----------
    void PushAll() { PushMelee(); PushDash(); PushRanged(); }

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
    }

    void PushDash()
    {
        if (!dashAbility) return;

        float aoe = dashAngerAOE[Mathf.Clamp(dashAngerLvl, 0, maxLvl)];
        float slow = dashSadSlow[Mathf.Clamp(dashSadnessLvl, 0, maxLvl)];
        float slowL = dashSadSlowLen[Mathf.Clamp(dashSadnessLvl, 0, maxLvl)];

        dashAbility.SetUpgrades(aoe, slow, slowL);
        // If you still have older PlayerManager calling SetUpgrades(...), that will also work due to legacy shim.
    }

    void PushRanged()
    {
        if (!playerCombat) return;

        // JOY x MELEE-ANGER synergy: turn on Joy VFX when both are > 0
        bool joyExplode = rangedJoyLvl > 0 && meleeAngerLvl > 0;

        var mods = new RangedModifiers
        {
            joyFireRateMultiplier = joyFireRateMult[Mathf.Clamp(rangedJoyLvl, 0, maxLvl)],
            joyDamageMultiplier = joyDamageMult[Mathf.Clamp(rangedJoyLvl, 0, maxLvl)],
            joyExplosionOnHit = joyExplode,

            angerAOEPercent = angerAOEPercent[Mathf.Clamp(rangedAngerLvl, 0, maxLvl)],
            angerExplosionOnHit = rangedAngerLvl > 0
        };

        playerCombat.SetRangedUpgrades(mods);
    }
}
