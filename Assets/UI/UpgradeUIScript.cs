using GLTFast.Schema;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradeUIScript : MonoBehaviour
{
    private CombosMenuUI combosMenuUI; 

    public UpgradeHandler upgradeHandler;

    public VisualElement root;

    public VisualElement skillMenu;
    public VisualElement pauseMenu;

    private Button resumeButton;
    private Button optionsButton;
    private Button quitButton;

    // MELEE
    private Button angerMelee1;
    private Button angerMelee2;
    private Button loveMelee1;   
    private Button loveMelee2;   

    // RANGE
    private Button joyRange1;
    private Button joyRange2;
    private Button angerRange1;
    private Button angerRange2;

    // DASH
    private Button sadDash1;
    private Button sadDash2;
    private Button angerDash1;
    private Button angerDash2;

    private Button upgradeTab;
    private Button comboTab;

    private VisualElement skillTree;
    private VisualElement combos;

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

        // ---- Query buttons ----
        // Melee (ANGER)
        angerMelee1 = root.Q<Button>("AngerMelee1");
        angerMelee2 = root.Q<Button>("AngerMelee2");
        // Melee (LOVE)
        loveMelee1 = root.Q<Button>("LoveMelee1");
        loveMelee2 = root.Q<Button>("LoveMelee2");

        // Range (JOY only in UI)
        joyRange1 = root.Q<Button>("JoyRange1");
        joyRange2 = root.Q<Button>("JoyRange2");

        angerRange1 = root.Q<Button>("AngerRange1");
        angerRange2 = root.Q<Button>("AngerRange2");

        // Dash (SAD / ANGER)
        sadDash1 = root.Q<Button>("SadDash1");
        sadDash2 = root.Q<Button>("SadDash2");
        angerDash1 = root.Q<Button>("AngerDash1");
        angerDash2 = root.Q<Button>("AngerDash2");

        upgradeTab.clicked += () =>
        {
            skillTree.style.display = DisplayStyle.Flex;
            combos.style.display = DisplayStyle.None;
        };

        comboTab.clicked += () =>
        {
            skillTree.style.display = DisplayStyle.None;
            combos.style.display = DisplayStyle.Flex;

            // NEW: refresh the combos card when the tab opens
            if (combosMenuUI != null) combosMenuUI.Refresh();
        };


        skillTree = root.Q<VisualElement>("SkillTreeEL");
        combos = root.Q<VisualElement>("CombosEL");

        if (resumeButton != null) resumeButton.clicked += OnResumeClicked;
        if (optionsButton != null) optionsButton.clicked += OnOptionsClicked;
        if (quitButton != null) quitButton.clicked += OnQuitClicked;

        if (angerMelee1 != null) angerMelee1.clicked += OnAngerMelee1Clicked;
        if (loveMelee1 != null) loveMelee1.clicked += OnLoveMelee1Clicked;
        if (joyRange1 != null) joyRange1.clicked += OnJoyRange1Clicked;
        if (angerRange1 != null) angerRange1.clicked += OnAngerRange1Clicked;
        if (sadDash1 != null) sadDash1.clicked += OnSadDash1Clicked;
        if (angerDash1 != null) angerDash1.clicked += OnAngerDash1Clicked;

        if (angerMelee2 != null) angerMelee2.clicked += OnAngerMelee2Clicked;
        if (loveMelee2 != null) loveMelee2.clicked += OnLoveMelee2Clicked;
        if (joyRange2 != null) joyRange2.clicked += OnJoyRange2Clicked;
        if (angerRange2 != null) angerRange2.clicked += OnAngerRange2Clicked;
        if (sadDash2 != null) sadDash2.clicked += OnSadDash2Clicked;
        if (angerDash2 != null) angerDash2.clicked += OnAngerDash2Clicked;

        if (upgradeTab != null) upgradeTab.clicked += () =>
        {
            if (skillTree != null) skillTree.style.display = DisplayStyle.Flex;
            if (combos != null) combos.style.display = DisplayStyle.None;
        };
        if (comboTab != null) comboTab.clicked += () =>
        {
            if (skillTree != null) skillTree.style.display = DisplayStyle.None;
            if (combos != null) combos.style.display = DisplayStyle.Flex;
        };

    }

    void OnDisable()
    {
        if (angerMelee1 != null) angerMelee1.clicked -= OnAngerMelee1Clicked;
        if (loveMelee1 != null) loveMelee1.clicked -= OnLoveMelee1Clicked;
        if (joyRange1 != null) joyRange1.clicked -= OnJoyRange1Clicked;
        if (angerRange1 != null) angerRange1.clicked -= OnAngerRange1Clicked;
        if (sadDash1 != null) sadDash1.clicked -= OnSadDash1Clicked;
        if (angerDash1 != null) angerDash1.clicked -= OnAngerDash1Clicked;

        if (angerMelee2 != null) angerMelee2.clicked -= OnAngerMelee2Clicked;
        if (loveMelee2 != null) loveMelee2.clicked -= OnLoveMelee2Clicked;
        if (joyRange2 != null) joyRange2.clicked -= OnJoyRange2Clicked;
        if (angerRange2 != null) angerRange2.clicked -= OnAngerRange2Clicked;
        if (sadDash2 != null) sadDash2.clicked -= OnSadDash2Clicked;
        if (angerDash2 != null) angerDash2.clicked -= OnAngerDash2Clicked;

        if (resumeButton != null) resumeButton.clicked -= OnResumeClicked;
        if (optionsButton != null) optionsButton.clicked -= OnOptionsClicked;
        if (quitButton != null) quitButton.clicked -= OnQuitClicked;
    }

    // ====== HELPERS: drive levels via handler Up/Down (no direct setters needed) ======
    void ZeroMelee()
    {
        // Call downs several times to guarantee 0 regardless of current level caps
        for (int i = 0; i < 10; i++)
        {
            upgradeHandler.MeleeAngerDown();
            upgradeHandler.MeleeLoveDown();
            upgradeHandler.MeleeSadDown();
            upgradeHandler.MeleeFearDown();
            upgradeHandler.MeleeJoyDown();
        }
    }
    void SetMeleeAngerLevel(int level)
    {
        ZeroMelee();
        for (int i = 0; i < level; i++) upgradeHandler.MeleeAngerUp();
    }
    void SetMeleeLoveLevel(int level)
    {
        ZeroMelee();
        for (int i = 0; i < level; i++) upgradeHandler.MeleeLoveUp();
    }

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
            upgradeHandler.DashSadDown();
            upgradeHandler.DashAngerDown();
        }
    }
    void SetDashSadLevel(int level)
    {
        ZeroDash(); // mutually exclusive with Anger
        for (int i = 0; i < level; i++) upgradeHandler.DashSadUp();
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

    // ====== Skill Tree Clicks ======
    // MELEE (ANGER)
    private void OnAngerMelee1Clicked()
    {
        SetMeleeAngerLevel(1);
        Debug.Log("Anger Melee Upgrade 1 Clicked!");
    }
    private void OnAngerMelee2Clicked()
    {
        SetMeleeAngerLevel(2);
        Debug.Log("Anger Melee Upgrade 2 Clicked!");
    }

    // MELEE (LOVE)
    private void OnLoveMelee1Clicked()
    {
        SetMeleeLoveLevel(1);
        Debug.Log("Love Melee Upgrade 1 Clicked!");
    }
    private void OnLoveMelee2Clicked()
    {
        SetMeleeLoveLevel(2);
        Debug.Log("Love Melee Upgrade 2 Clicked!");
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

    // DASH (SAD / ANGER)
    private void OnSadDash1Clicked()
    {
        SetDashSadLevel(1);
        Debug.Log("Sad Dash Upgrade 1 Clicked!");
    }
    private void OnSadDash2Clicked()
    {
        SetDashSadLevel(2);
        Debug.Log("Sad Dash Upgrade 2 Clicked!");
    }
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
}
