using GLTFast.Schema;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradeUIScript : MonoBehaviour
{
    private CombosMenuUI combosMenuUI;

    public UpgradeHandler upgradeHandler;
    public MeleeUpgrades meleeUpgrades;

    public VisualElement root;

    public VisualElement skillMenu;
    public VisualElement pauseMenu;

    private Button resumeButton;
    private Button optionsButton;
    private Button quitButton;

    //UPGRADE TREES
    public VisualElement meleeTree;
    public VisualElement rangeTree;
    public VisualElement dashTree;

    //UPGRADE TREE BUTTONS
    private Button meleeTreeButton;
    private Button rangeTreeButton;
    private Button dashTreeButton;

    // MELEE
    private Button joyMelee1;
    private Button joyMelee2;
    private Button joyMelee3;
    private Button angerMelee1;
    private Button angerMelee2;
    private Button angerMelee3;
    private Button sadnessMelee1;
    private Button sadnessMelee2;
    private Button sadnessMelee3;
    private Button loveMelee1;
    private Button loveMelee2;
    private Button loveMelee3;
    private Button fearMelee1;
    private Button fearMelee2;
    private Button fearMelee3;

    // RANGE
    private Button joyRange1;
    private Button joyRange2;
    private Button joyRange3;
    private Button angerRange1;
    private Button angerRange2;
    private Button angerRange3;
    private Button sadnessRange1;
    private Button sadnessRange2;
    private Button sadnessRange3;
    private Button loveRange1;
    private Button loveRange2;
    private Button loveRange3;
    private Button fearRange1;
    private Button fearRange2;
    private Button fearRange3;

    // DASH
    private Button joyDash1;
    private Button joyDash2;
    private Button joyDash3;
    private Button angerDash1;
    private Button angerDash2;
    private Button angerDash3;
    private Button sadnessDash1;
    private Button sadnessDash2;
    private Button sadnessDash3;
    private Button loveDash1;
    private Button loveDash2;
    private Button loveDash3;
    private Button fearDash1;
    private Button fearDash2;
    private Button fearDash3;

    private Button purchaseButton;

    private Button upgradeTab;
    private Button synergyTab;

    private VisualElement skillTree;
    private VisualElement synergies;

    private void Awake()
    {
        meleeUpgrades = GameObject.Find("UpgradeHolder").GetComponent<MeleeUpgrades>();
    }

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        combosMenuUI = GetComponent<CombosMenuUI>(); // on same GO as UIDocument


        // Find handler if not dragged in
        if (!upgradeHandler)
        {
            upgradeHandler = FindObjectOfType<UpgradeHandler>();
            if (!upgradeHandler)
            {
                Debug.LogError("[UpgradeUI] UpgradeHandler not found in scene. Drag it into the field on this component.");
                return;
            }
        }

        skillMenu = root.Q<VisualElement>("SkillMenu");
        pauseMenu = root.Q<VisualElement>("PauseMenu");

        resumeButton = root.Q<Button>("ResumeButton");
        optionsButton = root.Q<Button>("OptionsButton");
        quitButton = root.Q<Button>("QuitButton");

        // Upgrade Trees
        meleeTree = root.Q<VisualElement>("MeleeTreeEL");
        rangeTree = root.Q<VisualElement>("RangeTreeEL");
        dashTree = root.Q<VisualElement>("DashTreeEL");

        // Upgrade Tree Buttons
        meleeTreeButton = root.Q<Button>("MeleeTreeButton");
        rangeTreeButton = root.Q<Button>("RangeTreeButton");
        dashTreeButton = root.Q<Button>("DashTreeButton");

        // ---- Query buttons ----
        // Melee (JOY)
        joyMelee1 = root.Q<Button>("JoyMelee1");
        joyMelee2 = root.Q<Button>("JoyMelee2");
        joyMelee3 = root.Q<Button>("JoyMelee3");
        // Melee (ANGER)
        angerMelee1 = root.Q<Button>("AngerMelee1");
        angerMelee2 = root.Q<Button>("AngerMelee2");
        angerMelee3 = root.Q<Button>("AngerMelee3");
        // Melee (SADNESS)
        sadnessMelee1 = root.Q<Button>("SadnessMelee1");
        sadnessMelee2 = root.Q<Button>("SadnessMelee2");
        sadnessMelee3 = root.Q<Button>("SadnessMelee3");
        // Melee (LOVE)
        loveMelee1 = root.Q<Button>("LoveMelee1");
        loveMelee2 = root.Q<Button>("LoveMelee2");
        loveMelee3 = root.Q<Button>("LoveMelee3");
        // Melee (FEAR)
        fearMelee1 = root.Q<Button>("FearMelee1");
        fearMelee2 = root.Q<Button>("FearMelee2");
        fearMelee3 = root.Q<Button>("FearMelee3");

        // Range (JOY)
        joyRange1 = root.Q<Button>("JoyRange1");
        joyRange2 = root.Q<Button>("JoyRange2");
        joyRange3 = root.Q<Button>("JoyRange3");
        // Range (ANGER)
        angerRange1 = root.Q<Button>("AngerRange1");
        angerRange2 = root.Q<Button>("AngerRange2");
        angerRange3 = root.Q<Button>("AngerRange3");
        // Range (SADNESS)
        sadnessRange1 = root.Q<Button>("SadnessRange1");
        sadnessRange2 = root.Q<Button>("SadnessRange2");
        sadnessRange3 = root.Q<Button>("SadnessRange3");
        // Range (LOVE)
        loveRange1 = root.Q<Button>("LoveRange1");
        loveRange2 = root.Q<Button>("LoveRange2");
        loveRange3 = root.Q<Button>("LoveRange3");
        // Range (FEAR)
        fearRange1 = root.Q<Button>("FearRange1");
        fearRange2 = root.Q<Button>("FearRange2");
        fearRange3 = root.Q<Button>("FearRange3");

        // Dash (JOY)
        joyDash1 = root.Q<Button>("JoyDash1");
        joyDash2 = root.Q<Button>("JoyDash2");
        joyDash3 = root.Q<Button>("JoyDash3");
        // Dash (ANGER)
        angerDash1 = root.Q<Button>("AngerDash1");
        angerDash2 = root.Q<Button>("AngerDash2");
        angerDash3 = root.Q<Button>("AngerDash3");
        // Dash (SADNESS)
        sadnessDash1 = root.Q<Button>("SadnessDash1");
        sadnessDash2 = root.Q<Button>("SadnessDash2");
        sadnessDash3 = root.Q<Button>("SadnessDash3");
        // Dash (LOVE)
        loveDash1 = root.Q<Button>("LoveDash1");
        loveDash2 = root.Q<Button>("LoveDash2");
        loveDash3 = root.Q<Button>("LoveDash3");
        // Dash (FEAR)
        fearDash1 = root.Q<Button>("FearDash1");
        fearDash2 = root.Q<Button>("FearDash2");
        fearDash3 = root.Q<Button>("FearDash3");

        purchaseButton = root.Q<Button>("PurchaseButton");

        upgradeTab = root.Q<Button>("UpgradeTab");
        synergyTab = root.Q<Button>("SynergyTab");

        skillTree = root.Q<VisualElement>("SkillTreeEL");
        synergies = root.Q<VisualElement>("SynergiesEL");

        if (resumeButton != null) resumeButton.clicked += OnResumeClicked;
        if (optionsButton != null) optionsButton.clicked += OnOptionsClicked;
        if (quitButton != null) quitButton.clicked += OnQuitClicked;
        // Skill Trees
        if (meleeTreeButton != null) meleeTreeButton.clicked += OnMeleeTreeClicked;
        if (rangeTreeButton != null) rangeTreeButton.clicked += OnRangeTreeClicked;
        if (dashTreeButton != null) dashTreeButton.clicked += OnDashTreeClicked;
        // Melee 1
        if (joyMelee1 != null) joyMelee1.clicked += OnJoyMelee1Clicked;
        if (angerMelee1 != null) angerMelee1.clicked += OnAngerMelee1Clicked;
        if (sadnessMelee1 != null) sadnessMelee1.clicked += OnSadnessMelee1Clicked;
        if (loveMelee1 != null) loveMelee1.clicked += OnLoveMelee1Clicked;
        if (fearMelee1 != null) fearMelee1.clicked += OnFearMelee1Clicked;
        // Range 1
        if (joyRange1 != null) joyRange1.clicked += OnJoyRange1Clicked;
        if (angerRange1 != null) angerRange1.clicked += OnAngerRange1Clicked;
        if (sadnessRange1 != null) sadnessRange1.clicked += OnSadnessRange1Clicked;
        if (loveRange1 != null) loveRange1.clicked += OnLoveRange1Clicked;
        if (fearRange1 != null) fearRange1.clicked += OnFearRange1Clicked;
        // Dash 1
        if (joyDash1 != null) joyDash1.clicked += OnJoyDash1Clicked;
        if (angerDash1 != null) angerDash1.clicked += OnAngerDash1Clicked;
        if (sadnessDash1 != null) sadnessDash1.clicked += OnSadnessDash1Clicked;
        if (loveDash1 != null) loveDash1.clicked += OnLoveDash1Clicked;
        if (fearDash1 != null) fearDash1.clicked += OnFearDash1Clicked;

        // Melee 2
        if (joyMelee2 != null) joyMelee2.clicked += OnJoyMelee2Clicked;
        if (angerMelee2 != null) angerMelee2.clicked += OnAngerMelee2Clicked;
        if (sadnessMelee2 != null) sadnessMelee2.clicked += OnSadnessMelee2Clicked;
        if (loveMelee2 != null) loveMelee2.clicked += OnLoveMelee2Clicked;
        if (fearMelee2 != null) fearMelee2.clicked += OnFearMelee2Clicked;
        // Range 2
        if (joyRange2 != null) joyRange2.clicked += OnJoyRange2Clicked;
        if (angerRange2 != null) angerRange2.clicked += OnAngerRange2Clicked;
        if (sadnessRange2 != null) sadnessRange2.clicked += OnSadnessRange2Clicked;
        if (loveRange2 != null) loveRange2.clicked += OnLoveRange2Clicked;
        if (fearRange2 != null) fearRange2.clicked += OnFearRange2Clicked;
        // Dash 2
        if (joyDash2 != null) joyDash2.clicked += OnJoyDash2Clicked;
        if (angerDash2 != null) angerDash2.clicked += OnAngerDash2Clicked;
        if (sadnessDash2 != null) sadnessDash2.clicked += OnSadnessDash2Clicked;
        if (loveDash2 != null) loveDash2.clicked += OnLoveDash2Clicked;
        if (fearDash2 != null) fearDash2.clicked += OnFearDash2Clicked;

        // Melee 3
        if (joyMelee3 != null) joyMelee3.clicked += OnJoyMelee3Clicked;
        if (angerMelee3 != null) angerMelee3.clicked += OnAngerMelee3Clicked;
        if (sadnessMelee3 != null) sadnessMelee3.clicked += OnSadnessMelee3Clicked;
        if (loveMelee3 != null) loveMelee3.clicked += OnLoveMelee3Clicked;
        if (fearMelee3 != null) fearMelee3.clicked += OnFearMelee3Clicked;
        // Range 3
        if (joyRange3 != null) joyRange3.clicked += OnJoyRange3Clicked;
        if (angerRange3 != null) angerRange3.clicked += OnAngerRange3Clicked;
        if (sadnessRange3 != null) sadnessRange3.clicked += OnSadnessRange3Clicked;
        if (loveRange3 != null) loveRange3.clicked += OnLoveRange3Clicked;
        if (fearRange3 != null) fearRange3.clicked += OnFearRange3Clicked;
        // Dash 3
        if (joyDash3 != null) joyDash3.clicked += OnJoyDash3Clicked;
        if (sadnessDash3 != null) sadnessDash3.clicked += OnSadnessDash3Clicked;
        if (loveDash3 != null) loveDash3.clicked += OnLoveDash3Clicked;
        if (fearDash3 != null) fearDash3.clicked += OnFearDash3Clicked;
        if (angerDash3 != null) angerDash3.clicked += OnAngerDash3Clicked;

        if (purchaseButton != null) purchaseButton.clicked += OnPurchaseClicked;

        if (upgradeTab != null) upgradeTab.clicked += () =>
        {
            skillTree.style.display = DisplayStyle.Flex;
            synergies.style.display = DisplayStyle.None;
        };

        if (synergyTab != null) synergyTab.clicked += () =>
        {
            skillTree.style.display = DisplayStyle.None;
            synergies.style.display = DisplayStyle.Flex;

            // NEW: refresh the combos card when the tab opens
            if (combosMenuUI != null) combosMenuUI.Refresh();
        };


    }

    void OnDisable()
    {
        // Skill Trees
        if (meleeTreeButton != null) meleeTreeButton.clicked -= OnMeleeTreeClicked;
        if (rangeTreeButton != null) rangeTreeButton.clicked -= OnRangeTreeClicked;
        if (dashTreeButton != null) dashTreeButton.clicked -= OnDashTreeClicked;
        // Melee 1
        if (joyMelee1 != null) joyMelee1.clicked -= OnJoyMelee1Clicked;
        if (angerMelee1 != null) angerMelee1.clicked -= OnAngerMelee1Clicked;
        if (sadnessMelee1 != null) sadnessMelee1.clicked -= OnSadnessMelee1Clicked;
        if (loveMelee1 != null) loveMelee1.clicked -= OnLoveMelee1Clicked;
        if (fearMelee1 != null) fearMelee1.clicked -= OnFearMelee1Clicked;
        // Range 1
        if (joyRange1 != null) joyRange1.clicked -= OnJoyRange1Clicked;
        if (angerRange1 != null) angerRange1.clicked -= OnAngerRange1Clicked;
        if (sadnessRange1 != null) sadnessRange1.clicked -= OnSadnessRange1Clicked;
        if (loveRange1 != null) loveRange1.clicked -= OnLoveRange1Clicked;
        if (fearRange1 != null) fearRange1.clicked -= OnFearRange1Clicked;
        // Dash 1
        if (joyDash1 != null) joyDash1.clicked -= OnJoyDash1Clicked;
        if (angerDash1 != null) angerDash1.clicked -= OnAngerDash1Clicked;
        if (sadnessDash1 != null) sadnessDash1.clicked -= OnSadnessDash1Clicked;
        if (loveDash1 != null) loveDash1.clicked -= OnLoveDash1Clicked;
        if (fearDash1 != null) fearDash1.clicked -= OnFearDash1Clicked;

        // Melee 2
        if (joyMelee2 != null) joyMelee2.clicked -= OnJoyMelee2Clicked;
        if (angerMelee2 != null) angerMelee2.clicked -= OnAngerMelee2Clicked;
        if (sadnessMelee2 != null) sadnessMelee2.clicked -= OnSadnessMelee2Clicked;
        if (loveMelee2 != null) loveMelee2.clicked -= OnLoveMelee2Clicked;
        if (fearMelee2 != null) fearMelee2.clicked -= OnFearMelee2Clicked;
        // Range 2
        if (joyRange2 != null) joyRange2.clicked -= OnJoyRange2Clicked;
        if (angerRange2 != null) angerRange2.clicked -= OnAngerRange2Clicked;
        if (sadnessRange2 != null) sadnessRange2.clicked -= OnSadnessRange2Clicked;
        if (loveRange2 != null) loveRange2.clicked -= OnLoveRange2Clicked;
        if (fearRange2 != null) fearRange2.clicked -= OnFearRange2Clicked;
        // Dash 2
        if (joyDash2 != null) joyDash2.clicked -= OnJoyDash2Clicked;
        if (angerDash2 != null) angerDash2.clicked -= OnAngerDash2Clicked;
        if (sadnessDash2 != null) sadnessDash2.clicked -= OnSadnessDash2Clicked;
        if (loveDash2 != null) loveDash2.clicked -= OnLoveDash2Clicked;
        if (fearDash2 != null) fearDash2.clicked -= OnFearDash2Clicked;

        // Melee 3
        if (joyMelee3 != null) joyMelee3.clicked -= OnJoyMelee3Clicked;
        if (angerMelee3 != null) angerMelee3.clicked -= OnAngerMelee3Clicked;
        if (sadnessMelee3 != null) sadnessMelee3.clicked -= OnSadnessMelee3Clicked;
        if (loveMelee3 != null) loveMelee3.clicked -= OnLoveMelee3Clicked;
        if (fearMelee3 != null) fearMelee3.clicked -= OnFearMelee3Clicked;
        // Range 3
        if (joyRange3 != null) joyRange3.clicked -= OnJoyRange3Clicked;
        if (angerRange3 != null) angerRange3.clicked -= OnAngerRange3Clicked;
        if (sadnessRange3 != null) sadnessRange3.clicked -= OnSadnessRange3Clicked;
        if (loveRange3 != null) loveRange3.clicked -= OnLoveRange3Clicked;
        if (fearRange3 != null) fearRange3.clicked -= OnFearRange3Clicked;
        // Dash 3
        if (joyDash3 != null) joyDash3.clicked -= OnJoyDash3Clicked;
        if (sadnessDash3 != null) sadnessDash3.clicked -= OnSadnessDash3Clicked;
        if (loveDash3 != null) loveDash3.clicked -= OnLoveDash3Clicked;
        if (fearDash3 != null) fearDash3.clicked -= OnFearDash3Clicked;
        if (angerDash3 != null) angerDash3.clicked -= OnAngerDash3Clicked;

        if (purchaseButton != null) purchaseButton.clicked -= OnPurchaseClicked;

        if (resumeButton != null) resumeButton.clicked -= OnResumeClicked;
        if (optionsButton != null) optionsButton.clicked -= OnOptionsClicked;
        if (quitButton != null) quitButton.clicked -= OnQuitClicked;
    }

    // ====== HELPERS: drive levels via handler Up/Down (no direct setters needed) ======
    void ZeroRange()
    {
        for (int i = 0; i < 10; i++)
        {
            upgradeHandler.RangedJoyDown();
            upgradeHandler.RangedAngerDown();
        }
    }
    void SetRangeJoyLevel(int level)
    {
        ZeroRange();
        for (int i = 0; i < level; i++) upgradeHandler.RangedJoyUp();
    }

    void SetRangeAngerLevel(int level)
    {
        ZeroRange();
        for (int i = 0; i < level; i++) upgradeHandler.RangedAngerUp();
    }

    void ZeroDash()
    {
        for (int i = 0; i < 10; i++)
        {
            upgradeHandler.DashSadnessDown();
            upgradeHandler.DashAngerDown();
        }
    }
    void SetDashSadLevel(int level)
    {
        ZeroDash(); // mutually exclusive with Anger
        for (int i = 0; i < level; i++) upgradeHandler.DashSadnessUp();
    }
    void SetDashAngerLevel(int level)
    {
        ZeroDash(); // mutually exclusive with Sad
        for (int i = 0; i < level; i++) upgradeHandler.DashAngerUp();
    }

    // ====== Pause Menu Buttons ======
    private void OnResumeClicked()
    {
        Debug.Log("Resume Button Clicked!");
    }
    private void OnOptionsClicked()
    {
        Debug.Log("Options Button Clicked!");
    }
    private void OnQuitClicked()
    {
        Application.Quit();
        Debug.Log("Quit Button Clicked!");
    }

    // ====== Swap Skill Trees ======
    private void OnMeleeTreeClicked()
    {
        meleeTree.style.display = DisplayStyle.Flex;
        rangeTree.style.display = DisplayStyle.None;
        dashTree.style.display = DisplayStyle.None;
        Debug.Log("melee tree click");
    }
    private void OnRangeTreeClicked()
    {
        meleeTree.style.display = DisplayStyle.None;
        rangeTree.style.display = DisplayStyle.Flex;
        dashTree.style.display = DisplayStyle.None;
        Debug.Log("range tree click");
    }
    private void OnDashTreeClicked()
    {
        meleeTree.style.display = DisplayStyle.None;
        rangeTree.style.display = DisplayStyle.None;
        dashTree.style.display = DisplayStyle.Flex;
        Debug.Log("dash tree click");
    }

    // ====== Skill Tree Clicks ======
    // MELEE (JOY)
    private void OnJoyMelee1Clicked()
    {
        meleeUpgrades.SelectEmotion(Emotions.Joy);
        Debug.Log("Joy Melee Upgrade 1 Clicked!");
    }
    private void OnJoyMelee2Clicked()
    {
        Debug.Log("Joy Melee Upgrade 2 Clicked!");
        meleeUpgrades.UpgradeEmotion();
    }
    private void OnJoyMelee3Clicked()
    {
        Debug.Log("Joy Melee Upgrade 3 Clicked!");
        meleeUpgrades.UpgradeEmotion();
    }

    // MELEE (ANGER)
    private void OnAngerMelee1Clicked()
    {
        meleeUpgrades.SelectEmotion(Emotions.Anger);
        Debug.Log("Anger Melee Upgrade 1 Clicked!");
    }
    private void OnAngerMelee2Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Anger Melee Upgrade 2 Clicked!");
    }
    private void OnAngerMelee3Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Anger Melee Upgrade 3 Clicked!");
    }

    // MELEE (SADNESS)
    private void OnSadnessMelee1Clicked()
    {
        meleeUpgrades.SelectEmotion(Emotions.Sadness);
        Debug.Log("Sadness Melee Upgrade 1 Clicked!");
    }
    private void OnSadnessMelee2Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Sadness Melee Upgrade 2 Clicked!");
    }
    private void OnSadnessMelee3Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Sadness Melee Upgrade 3 Clicked!");
    }

    // MELEE (LOVE)
    private void OnLoveMelee1Clicked()
    {
        meleeUpgrades.SelectEmotion(Emotions.Love);
        Debug.Log("Love Melee Upgrade 1 Clicked!");
    }
    private void OnLoveMelee2Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Love Melee Upgrade 2 Clicked!");
    }
    private void OnLoveMelee3Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Love Melee Upgrade 3 Clicked!");
    }

    // MELEE (FEAR)
    private void OnFearMelee1Clicked()
    {
        meleeUpgrades.SelectEmotion(Emotions.Fear);
        Debug.Log("Fear Melee Upgrade 1 Clicked!");
    }
    private void OnFearMelee2Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Fear Melee Upgrade 2 Clicked!");
    }
    private void OnFearMelee3Clicked()
    {
        meleeUpgrades.UpgradeEmotion();
        Debug.Log("Fear Melee Upgrade 3 Clicked!");
    }

    // RANGE (JOY)
    private void OnJoyRange1Clicked()
    {
        SetRangeJoyLevel(1);
        Debug.Log("Joy Range Upgrade 1 Clicked!");
    }
    private void OnJoyRange2Clicked()
    {
        SetRangeJoyLevel(2);
        Debug.Log("Joy Range Upgrade 2 Clicked!");
    }
    private void OnJoyRange3Clicked()
    {
        Debug.Log("Joy Range Upgrade 3 Clicked!");
    }

    // RANGE (ANGER)
    private void OnAngerRange1Clicked()
    {
        SetRangeAngerLevel(1);
        Debug.Log("Anger Range Upgrade 1 Clicked!");
    }
    private void OnAngerRange2Clicked()
    {
        SetRangeAngerLevel(2);
        Debug.Log("Anger Range Upgrade 2 Clicked!");
    }
    private void OnAngerRange3Clicked()
    {
        Debug.Log("Anger Range Upgrade 3 Clicked!");
    }

    // RANGE (SADNESS)
    private void OnSadnessRange1Clicked()
    {
        Debug.Log("Sadness Range Upgrade 1 Clicked!");
    }
    private void OnSadnessRange2Clicked()
    {
        Debug.Log("Sadness Range Upgrade 2 Clicked!");
    }
    private void OnSadnessRange3Clicked()
    {
        Debug.Log("Sadness Range Upgrade 3 Clicked!");
    }

    // RANGE (LOVE)
    private void OnLoveRange1Clicked()
    {
        Debug.Log("Love Range Upgrade 1 Clicked!");
    }
    private void OnLoveRange2Clicked()
    {
        Debug.Log("Love Range Upgrade 2 Clicked!");
    }
    private void OnLoveRange3Clicked()
    {
        Debug.Log("Love Range Upgrade 3 Clicked!");
    }

    // RANGE (FEAR)
    private void OnFearRange1Clicked()
    {
        Debug.Log("Fear Range Upgrade 1 Clicked!");
    }
    private void OnFearRange2Clicked()
    {
        Debug.Log("Fear Range Upgrade 2 Clicked!");
    }
    private void OnFearRange3Clicked()
    {
        Debug.Log("Fear Range Upgrade 3 Clicked!");
    }

    // DASH (JOY)
    private void OnJoyDash1Clicked()
    {
        Debug.Log("Joy Dash Upgrade 1 Clicked!");
    }
    private void OnJoyDash2Clicked()
    {
        Debug.Log("Joy Dash Upgrade 2 Clicked!");
    }
    private void OnJoyDash3Clicked()
    {
        Debug.Log("Joy Dash Upgrade 3 Clicked!");
    }

    // DASH (ANGER)
    private void OnAngerDash1Clicked()
    {
        SetDashAngerLevel(1);
        Debug.Log("Anger Dash Upgrade 1 Clicked!");
    }
    private void OnAngerDash2Clicked()
    {
        SetDashAngerLevel(2);
        Debug.Log("Anger Dash Upgrade 2 Clicked!");
    }
    private void OnAngerDash3Clicked()
    {
        Debug.Log("Anger Dash Upgrade 3 Clicked!");
    }

    // DASH (SADNESS)
    private void OnSadnessDash1Clicked()
    {
        SetDashSadLevel(1);
        Debug.Log("Sad Dash Upgrade 1 Clicked!");
    }
    private void OnSadnessDash2Clicked()
    {
        SetDashSadLevel(2);
        Debug.Log("Sad Dash Upgrade 2 Clicked!");
    }
    private void OnSadnessDash3Clicked()
    {
        Debug.Log("Sad Dash Upgrade 3 Clicked!");
    }

    // DASH (LOVE)
    private void OnLoveDash1Clicked()
    {
        Debug.Log("Love Dash Upgrade 1 Clicked!");
    }
    private void OnLoveDash2Clicked()
    {
        Debug.Log("Love Dash Upgrade 2 Clicked!");
    }
    private void OnLoveDash3Clicked()
    {
        Debug.Log("Love Dash Upgrade 3 Clicked!");
    }

    // DASH (FEAR)
    private void OnFearDash1Clicked()
    {
        Debug.Log("Fear Dash Upgrade 1 Clicked!");
    }
    private void OnFearDash2Clicked()
    {
        Debug.Log("Fear Dash Upgrade 2 Clicked!");
    }
    private void OnFearDash3Clicked()
    {
        Debug.Log("Fear Dash Upgrade 3 Clicked!");
    }

    private void OnPurchaseClicked()
    {
               Debug.Log("Purchase Button Clicked!");
    }
}
