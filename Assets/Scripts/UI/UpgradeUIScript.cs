using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UpgradeUIScript : MonoBehaviour
{
    private CombosMenuUI combosMenuUI;

    public UpgradeHandler upgradeHandler;
    public MeleeUpgrades meleeUpgrades;

    [Header("Menus")]
    public GameObject skillMenu;
    [SerializeField] private GameObject skillTree;
    [SerializeField] private GameObject synergies;

    [Header("Description Panel")]
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeDescriptionText;
    private UpgradeButton currentlySelectedButton;

    //UPGRADE TREES
    [Header("Upgrade Trees")]
    public GameObject meleeTree;
    public GameObject rangeTree;
    public GameObject dashTree;

    //UPGRADE TREE BUTTONS
    [Header("Skill Tree Buttons")]
    [SerializeField] private Button meleeTreeButton;
    [SerializeField] private Button rangeTreeButton;
    [SerializeField] private Button dashTreeButton;
    [SerializeField] private Button targetMeleeButton;
    [SerializeField] private Button targetRangeButton;
    [SerializeField] private Button targetDashButton;

    // MELEE
    [Header("Melee Buttons")]
    [SerializeField] private Button joyMelee1;
    [SerializeField] private Button joyMelee2;
    [SerializeField] private Button joyMelee3;
    [SerializeField] private Button angerMelee1;
    [SerializeField] private Button angerMelee2;
    [SerializeField] private Button angerMelee3;
    [SerializeField] private Button sadnessMelee1;
    [SerializeField] private Button sadnessMelee2;
    [SerializeField] private Button sadnessMelee3;
    [SerializeField] private Button loveMelee1;
    [SerializeField] private Button loveMelee2;
    [SerializeField] private Button loveMelee3;
    [SerializeField] private Button fearMelee1;
    [SerializeField] private Button fearMelee2;
    [SerializeField] private Button fearMelee3;

    // RANGE
    [Header("Range Buttons")]
    [SerializeField] private Button joyRange1;
    [SerializeField] private Button joyRange2;
    [SerializeField] private Button joyRange3;
    [SerializeField] private Button angerRange1;
    [SerializeField] private Button angerRange2;
    [SerializeField] private Button angerRange3;
    [SerializeField] private Button sadnessRange1;
    [SerializeField] private Button sadnessRange2;
    [SerializeField] private Button sadnessRange3;
    [SerializeField] private Button loveRange1;
    [SerializeField] private Button loveRange2;
    [SerializeField] private Button loveRange3;
    [SerializeField] private Button fearRange1;
    [SerializeField] private Button fearRange2;
    [SerializeField] private Button fearRange3;

    // DASH
    [Header("Dash Buttons")]
    [SerializeField] private Button joyDash1;
    [SerializeField] private Button joyDash2;
    [SerializeField] private Button joyDash3;
    [SerializeField] private Button angerDash1;
    [SerializeField] private Button angerDash2;
    [SerializeField] private Button angerDash3;
    [SerializeField] private Button sadnessDash1;
    [SerializeField] private Button sadnessDash2;
    [SerializeField] private Button sadnessDash3;
    [SerializeField] private Button loveDash1;
    [SerializeField] private Button loveDash2;
    [SerializeField] private Button loveDash3;
    [SerializeField] private Button fearDash1;
    [SerializeField] private Button fearDash2;
    [SerializeField] private Button fearDash3;

    [Header("Other Buttons")]
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button closeButton;

    [Header("Tabs")]
    [SerializeField] private Button upgradeTab;
    [SerializeField] private Button synergyTab;


    void Awake()
    {
        FindUpgradeHolder();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        meleeUpgrades = GameObject.Find("UpgradeHolder").GetComponent<MeleeUpgrades>();
    }

    void OnEnable()
    {
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

        // Skill Trees
        if (meleeTreeButton != null) meleeTreeButton.onClick.AddListener(OnMeleeTreeClicked);
        if (rangeTreeButton != null) rangeTreeButton.onClick.AddListener(OnRangeTreeClicked);
        if (dashTreeButton != null) dashTreeButton.onClick.AddListener(OnDashTreeClicked);
        // Melee 1
        if (joyMelee1 != null) joyMelee1.onClick.AddListener(OnJoyMelee1Clicked);
        if (angerMelee1 != null) angerMelee1.onClick.AddListener(OnAngerMelee1Clicked);
        if (sadnessMelee1 != null) sadnessMelee1.onClick.AddListener(OnSadnessMelee1Clicked);
        if (loveMelee1 != null) loveMelee1.onClick.AddListener(OnLoveMelee1Clicked);
        if (fearMelee1 != null) fearMelee1.onClick.AddListener(OnFearMelee1Clicked);
        // Range 1
        if (joyRange1 != null) joyRange1.onClick.AddListener(OnJoyRange1Clicked);
        if (angerRange1 != null) angerRange1.onClick.AddListener(OnAngerRange1Clicked);
        if (sadnessRange1 != null) sadnessRange1.onClick.AddListener(OnSadnessRange1Clicked);
        if (loveRange1 != null) loveRange1.onClick.AddListener(OnLoveRange1Clicked);
        if (fearRange1 != null) fearRange1.onClick.AddListener(OnFearRange1Clicked);
        // Dash 1
        if (joyDash1 != null) joyDash1.onClick.AddListener(OnJoyDash1Clicked);
        if (angerDash1 != null) angerDash1.onClick.AddListener(OnAngerDash1Clicked);
        if (sadnessDash1 != null) sadnessDash1.onClick.AddListener(OnSadnessDash1Clicked);
        if (loveDash1 != null) loveDash1.onClick.AddListener(OnLoveDash1Clicked);
        if (fearDash1 != null) fearDash1.onClick.AddListener(OnFearDash1Clicked);

        // Melee 2
        if (joyMelee2 != null) joyMelee2.onClick.AddListener(OnJoyMelee2Clicked);
        if (angerMelee2 != null) angerMelee2.onClick.AddListener(OnAngerMelee2Clicked);
        if (sadnessMelee2 != null) sadnessMelee2.onClick.AddListener(OnSadnessMelee2Clicked);
        if (loveMelee2 != null) loveMelee2.onClick.AddListener(OnLoveMelee2Clicked);
        if (fearMelee2 != null) fearMelee2.onClick.AddListener(OnFearMelee2Clicked);
        // Range 2
        if (joyRange2 != null) joyRange2.onClick.AddListener(OnJoyRange2Clicked);
        if (angerRange2 != null) angerRange2.onClick.AddListener(OnAngerRange2Clicked);
        if (sadnessRange2 != null) sadnessRange2.onClick.AddListener(OnSadnessRange2Clicked);
        if (loveRange2 != null) loveRange2.onClick.AddListener(OnLoveRange2Clicked);
        if (fearRange2 != null) fearRange2.onClick.AddListener(OnFearRange2Clicked);
        // Dash 2
        if (joyDash2 != null) joyDash2.onClick.AddListener(OnJoyDash2Clicked);
        if (angerDash2 != null) angerDash2.onClick.AddListener(OnAngerDash2Clicked);
        if (sadnessDash2 != null) sadnessDash2.onClick.AddListener(OnSadnessDash2Clicked);
        if (loveDash2 != null) loveDash2.onClick.AddListener(OnLoveDash2Clicked);
        if (fearDash2 != null) fearDash2.onClick.AddListener(OnFearDash2Clicked);

        // Melee 3
        if (joyMelee3 != null) joyMelee3.onClick.AddListener(OnJoyMelee3Clicked);
        if (angerMelee3 != null) angerMelee3.onClick.AddListener(OnAngerMelee3Clicked);
        if (sadnessMelee3 != null) sadnessMelee3.onClick.AddListener(OnSadnessMelee3Clicked);
        if (loveMelee3 != null) loveMelee3.onClick.AddListener(OnLoveMelee3Clicked);
        if (fearMelee3 != null) fearMelee3.onClick.AddListener(OnFearMelee3Clicked);
        // Range 3
        if (joyRange3 != null) joyRange3.onClick.AddListener(OnJoyRange3Clicked);
        if (angerRange3 != null) angerRange3.onClick.AddListener(OnAngerRange3Clicked);
        if (sadnessRange3 != null) sadnessRange3.onClick.AddListener(OnSadnessRange3Clicked);
        if (loveRange3 != null) loveRange3.onClick.AddListener(OnLoveRange3Clicked);
        if (fearRange3 != null) fearRange3.onClick.AddListener(OnFearRange3Clicked);
        // Dash 3
        if (joyDash3 != null) joyDash3.onClick.AddListener(OnJoyDash3Clicked);
        if (sadnessDash3 != null) sadnessDash3.onClick.AddListener(OnSadnessDash3Clicked);
        if (loveDash3 != null) loveDash3.onClick.AddListener(OnLoveDash3Clicked);
        if (fearDash3 != null) fearDash3.onClick.AddListener(OnFearDash3Clicked);
        if (angerDash3 != null) angerDash3.onClick.AddListener(OnAngerDash3Clicked);

        if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchaseClicked);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseButtonClicked);

        if (upgradeTab != null) upgradeTab.onClick.AddListener(() =>
        {
            skillTree.SetActive(true);
            synergies.SetActive(false);
        });
        if (synergyTab != null) synergyTab.onClick.AddListener(() =>
        {
            skillTree.SetActive(false);
            synergies.SetActive(true);

            // NEW: refresh the combos card when the tab opens
            if (combosMenuUI != null) combosMenuUI.Refresh();
        });


    }

    void OnDisable()
    {
        // Skill Trees
        if (meleeTreeButton != null) meleeTreeButton.onClick.RemoveListener(OnMeleeTreeClicked);
        if (rangeTreeButton != null) rangeTreeButton.onClick.RemoveListener(OnRangeTreeClicked);
        if (dashTreeButton != null) dashTreeButton.onClick.RemoveListener(OnDashTreeClicked);
        // Melee 1
        if (joyMelee1 != null) joyMelee1.onClick.RemoveListener(OnJoyMelee1Clicked);
        if (angerMelee1 != null) angerMelee1.onClick.RemoveListener(OnAngerMelee1Clicked);
        if (sadnessMelee1 != null) sadnessMelee1.onClick.RemoveListener(OnSadnessMelee1Clicked);
        if (loveMelee1 != null) loveMelee1.onClick.RemoveListener(OnLoveMelee1Clicked);
        if (fearMelee1 != null) fearMelee1.onClick.RemoveListener(OnFearMelee1Clicked);
        // Range 1
        if (joyRange1 != null) joyRange1.onClick.RemoveListener(OnJoyRange1Clicked);
        if (angerRange1 != null) angerRange1.onClick.RemoveListener(OnAngerRange1Clicked);
        if (sadnessRange1 != null) sadnessRange1.onClick.RemoveListener(OnSadnessRange1Clicked);
        if (loveRange1 != null) loveRange1.onClick.RemoveListener(OnLoveRange1Clicked);
        if (fearRange1 != null) fearRange1.onClick.RemoveListener(OnFearRange1Clicked);
        // Dash 1
        if (joyDash1 != null) joyDash1.onClick.RemoveListener(OnJoyDash1Clicked);
        if (angerDash1 != null) angerDash1.onClick.RemoveListener(OnAngerDash1Clicked);
        if (sadnessDash1 != null) sadnessDash1.onClick.RemoveListener(OnSadnessDash1Clicked);
        if (loveDash1 != null) loveDash1.onClick.RemoveListener(OnLoveDash1Clicked);
        if (fearDash1 != null) fearDash1.onClick.RemoveListener(OnFearDash1Clicked);

        // Melee 2
        if (joyMelee2 != null) joyMelee2.onClick.RemoveListener(OnJoyMelee2Clicked);
        if (angerMelee2 != null) angerMelee2.onClick.RemoveListener(OnAngerMelee2Clicked);
        if (sadnessMelee2 != null) sadnessMelee2.onClick.RemoveListener(OnSadnessMelee2Clicked);
        if (loveMelee2 != null) loveMelee2.onClick.RemoveListener(OnLoveMelee2Clicked);
        if (fearMelee2 != null) fearMelee2.onClick.RemoveListener(OnFearMelee2Clicked);
        // Range 2
        if (joyRange2 != null) joyRange2.onClick.RemoveListener(OnJoyRange2Clicked);
        if (angerRange2 != null) angerRange2.onClick.RemoveListener(OnAngerRange2Clicked);
        if (sadnessRange2 != null) sadnessRange2.onClick.RemoveListener(OnSadnessRange2Clicked);
        if (loveRange2 != null) loveRange2.onClick.RemoveListener(OnLoveRange2Clicked);
        if (fearRange2 != null) fearRange2.onClick.RemoveListener(OnFearRange2Clicked);
        // Dash 2
        if (joyDash2 != null) joyDash2.onClick.RemoveListener(OnJoyDash2Clicked);
        if (angerDash2 != null) angerDash2.onClick.RemoveListener(OnAngerDash2Clicked);
        if (sadnessDash2 != null) sadnessDash2.onClick.RemoveListener(OnSadnessDash2Clicked);
        if (loveDash2 != null) loveDash2.onClick.RemoveListener(OnLoveDash2Clicked);
        if (fearDash2 != null) fearDash2.onClick.RemoveListener(OnFearDash2Clicked);

        // Melee 3
        if (joyMelee3 != null) joyMelee3.onClick.RemoveListener(OnJoyMelee3Clicked);
        if (angerMelee3 != null) angerMelee3.onClick.RemoveListener(OnAngerMelee3Clicked);
        if (sadnessMelee3 != null) sadnessMelee3.onClick.RemoveListener(OnSadnessMelee3Clicked);
        if (loveMelee3 != null) loveMelee3.onClick.RemoveListener(OnLoveMelee3Clicked);
        if (fearMelee3 != null) fearMelee3.onClick.RemoveListener(OnFearMelee3Clicked);
        // Range 3
        if (joyRange3 != null) joyRange3.onClick.RemoveListener(OnJoyRange3Clicked);
        if (angerRange3 != null) angerRange3.onClick.RemoveListener(OnAngerRange3Clicked);
        if (sadnessRange3 != null) sadnessRange3.onClick.RemoveListener(OnSadnessRange3Clicked);
        if (loveRange3 != null) loveRange3.onClick.RemoveListener(OnLoveRange3Clicked);
        if (fearRange3 != null) fearRange3.onClick.RemoveListener(OnFearRange3Clicked);
        // Dash 3
        if (joyDash3 != null) joyDash3.onClick.RemoveListener(OnJoyDash3Clicked);
        if (sadnessDash3 != null) sadnessDash3.onClick.RemoveListener(OnSadnessDash3Clicked);
        if (loveDash3 != null) loveDash3.onClick.RemoveListener(OnLoveDash3Clicked);
        if (fearDash3 != null) fearDash3.onClick.RemoveListener(OnFearDash3Clicked);
        if (angerDash3 != null) angerDash3.onClick.RemoveListener(OnAngerDash3Clicked);

        if (purchaseButton != null) purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
        if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseButtonClicked);
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
    void SetRangeSadnessLevel(int level)
    {
        ZeroRange();
        for (int i = 0; i < level; i++) upgradeHandler.RangedSadnessUp();
    }
    void SetRangeFearLevel(int level)
    {
        ZeroRange();
        for (int i = 0; i < level; i++) upgradeHandler.RangedFearUp();
    }
    void SetRangeLoveLevel(int level)
    {
        ZeroRange();
        for (int i = 0; i < level; i++) upgradeHandler.RangedLoveUp();
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
        ZeroDash(); // mutually exclusive
        for (int i = 0; i < level; i++) upgradeHandler.DashSadnessUp();
    }
    void SetDashAngerLevel(int level)
    {
        ZeroDash(); // mutually exclusive 
        for (int i = 0; i < level; i++) upgradeHandler.DashAngerUp();
    }
    void SetDashFearLevel(int level)
    {
        ZeroDash(); // mutually exclusive
        for (int i = 0; i < level; i++) upgradeHandler.DashFearUp();
    }
    void SetDashLoveLevel(int level)
    {
        ZeroDash(); // mutually exclusive
        for (int i = 0; i < level; i++) upgradeHandler.DashLoveUp();
    }
    void SetDashJoyLevel(int level)
    {
        ZeroDash(); // mutually exclusive
        for (int i = 0; i < level; i++) upgradeHandler.DashJoyUp();
    }

    // ====== Swap Skill Trees ======
    private void OnMeleeTreeClicked()
    {
        meleeTree.SetActive(true);
        rangeTree.SetActive(false);
        dashTree.SetActive(false);

        UpdateTabButtonNavigation(targetMeleeButton);

        Debug.Log("melee tree click");
    }

    private void OnRangeTreeClicked()
    {
        meleeTree.SetActive(false);
        rangeTree.SetActive(true);
        dashTree.SetActive(false);

        UpdateTabButtonNavigation(targetRangeButton);

        Debug.Log("range tree click");
    }

    private void OnDashTreeClicked()
    {
        meleeTree.SetActive(false);
        rangeTree.SetActive(false);
        dashTree.SetActive(true);

        UpdateTabButtonNavigation(targetDashButton);

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
        SetRangeJoyLevel(3);
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
        SetRangeAngerLevel(3);
        Debug.Log("Anger Range Upgrade 3 Clicked!");
    }

    // RANGE (SADNESS)
    private void OnSadnessRange1Clicked()
    {
        SetRangeSadnessLevel(1);
        Debug.Log("Sadness Range Upgrade 1 Clicked!");
    }
    private void OnSadnessRange2Clicked()
    {
        SetRangeSadnessLevel(2);
        Debug.Log("Sadness Range Upgrade 2 Clicked!");
    }
    private void OnSadnessRange3Clicked()
    {
        SetRangeSadnessLevel(3);
        Debug.Log("Sadness Range Upgrade 3 Clicked!");
    }

    // RANGE (LOVE)
    private void OnLoveRange1Clicked()
    {
        SetRangeLoveLevel(1);
        Debug.Log("Love Range Upgrade 1 Clicked!");
    }
    private void OnLoveRange2Clicked()
    {
        SetRangeLoveLevel(2);
        Debug.Log("Love Range Upgrade 2 Clicked!");
    }
    private void OnLoveRange3Clicked()
    {
        SetRangeLoveLevel(3);
        Debug.Log("Love Range Upgrade 3 Clicked!");
    }

    // RANGE (FEAR)
    private void OnFearRange1Clicked()
    {
        SetRangeFearLevel(1);
        Debug.Log("Fear Range Upgrade 1 Clicked!");
    }
    private void OnFearRange2Clicked()
    {
        SetRangeFearLevel(2);
        Debug.Log("Fear Range Upgrade 2 Clicked!");
    }
    private void OnFearRange3Clicked()
    {
        SetRangeFearLevel(3);
        Debug.Log("Fear Range Upgrade 3 Clicked!");
    }

    // DASH (JOY)
    private void OnJoyDash1Clicked()
    {
        SetDashJoyLevel(1);
        Debug.Log("Joy Dash Upgrade 1 Clicked!");
    }
    private void OnJoyDash2Clicked()
    {
        SetDashJoyLevel(2);
        Debug.Log("Joy Dash Upgrade 2 Clicked!");
    }
    private void OnJoyDash3Clicked()
    {
        SetDashJoyLevel(3);
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
        SetDashAngerLevel(3);
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
        SetDashSadLevel(3);
        Debug.Log("Sad Dash Upgrade 3 Clicked!");
    }

    // DASH (LOVE)
    private void OnLoveDash1Clicked()
    {
        SetDashLoveLevel(1);
        Debug.Log("Love Dash Upgrade 1 Clicked!");
    }
    private void OnLoveDash2Clicked()
    {
        SetDashLoveLevel(2);
        Debug.Log("Love Dash Upgrade 2 Clicked!");
    }
    private void OnLoveDash3Clicked()
    {
        SetDashLoveLevel(3);
        Debug.Log("Love Dash Upgrade 3 Clicked!");
    }

    // DASH (FEAR)
    private void OnFearDash1Clicked()
    {
        SetDashFearLevel(1);
        Debug.Log("Fear Dash Upgrade 1 Clicked!");
    }
    private void OnFearDash2Clicked()
    {
        SetDashFearLevel(2);
        Debug.Log("Fear Dash Upgrade 2 Clicked!");
    }
    private void OnFearDash3Clicked()
    {
        SetDashFearLevel(3);
        Debug.Log("Fear Dash Upgrade 3 Clicked!");
    }

    private void OnPurchaseClicked()
    {
        Debug.Log("Purchase Button Clicked!");
    }

    private void UpdateTabButtonNavigation(Button targetButton)
    {
        if (meleeTreeButton != null)
        {
            Navigation nav = meleeTreeButton.navigation;
            nav.selectOnDown = targetButton;
            meleeTreeButton.navigation = nav;
        }

        if (rangeTreeButton != null)
        {
            Navigation nav = rangeTreeButton.navigation;
            nav.selectOnDown = targetButton;
            rangeTreeButton.navigation = nav;
        }

        if (dashTreeButton != null)
        {
            Navigation nav = dashTreeButton.navigation;
            nav.selectOnDown = targetButton;
            dashTreeButton.navigation = nav;
        }
    }

    public void UpdateDescriptionPanel(string upgradeName, string description)
    {
        if (upgradeNameText != null)
        {
            upgradeNameText.text = upgradeName;
        }

        if (upgradeDescriptionText != null)
        {
            upgradeDescriptionText.text = description;
        }
    }

    public void SelectUpgrade(UpgradeButton button)
    {
        if (currentlySelectedButton != null)
        {
            currentlySelectedButton.SetSelected(false);
        }

        currentlySelectedButton = button;
        currentlySelectedButton.SetSelected(true);
    }

    private void OnCloseButtonClicked()
    {
        GetComponentInParent<InteractableUI>().ToggleUpgradeUI();
    }

    private void FindUpgradeHolder()
    {
        MeleeUpgrades foundMeleeUpgrades = null;

        if (PersistentUpgradeHolder.Instance != null)
        {
            foundMeleeUpgrades = PersistentUpgradeHolder.Instance.GetComponent<MeleeUpgrades>();
            Debug.Log("UpgradeUIScript: Using PersistentUpgradeHolder.Instance");
        }
        else
        {
            GameObject upgradeHolderObj = GameObject.Find("UpgradeHolder");
            if (upgradeHolderObj != null)
            {
                foundMeleeUpgrades = upgradeHolderObj.GetComponent<MeleeUpgrades>();
                Debug.Log("UpgradeUIScript: Found UpgradeHolder via GameObject.Find");
            }
        }

        if (foundMeleeUpgrades != null)
        {
            meleeUpgrades = foundMeleeUpgrades;
            Debug.Log("UpgradeUIScript: Successfully found MeleeUpgrades component");
        }
        else
        {
            Debug.LogWarning("UpgradeUIScript: Could not find UpgradeHolder or MeleeUpgrades component");
        }
    }
}
