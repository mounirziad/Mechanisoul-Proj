using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections;


public class UpgradeUIScript : MonoBehaviour
{
    private CombosMenuUI combosMenuUI;

    public UpgradeHandler upgradeHandler;
    public MeleeUpgrades meleeUpgrades;
    private bool isSynergyActive = false;
    [SerializeField] Button firstSynergyButton;
    private bool canPurchaseUpgrades = true;

    [Header("Menus")]
    public GameObject skillMenu;
    [SerializeField] private GameObject skillTree;
    [SerializeField] private GameObject synergies;

    [Header("Description Panel")]
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeDescriptionText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
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

    [Header("Pending Selections")]
    private Emotions pendingMeleeEmotion = Emotions.None;
    private int pendingMeleeLevel = 0;

    private Emotions pendingRangedEmotion = Emotions.None;
    private int pendingRangedLevel = 0;

    private Emotions pendingDashEmotion = Emotions.None;
    private int pendingDashLevel = 0;

    [Header("Current Purchases")]
    private Emotions purchasedMeleeEmotion = Emotions.None;
    private int purchasedMeleeLevel = 0;

    private Emotions purchasedRangedEmotion = Emotions.None;
    private int purchasedRangedLevel = 0;

    private Emotions purchasedDashEmotion = Emotions.None;
    private int purchasedDashLevel = 0;

    [Header("Upgrade Points")]
    [SerializeField] private XPManager xpManager;
    [SerializeField] private TextMeshProUGUI pointsText;

    // cost per tier: 1, 2, 3 by requirement
    [SerializeField] private int level1Cost = 1;
    [SerializeField] private int level2Cost = 2;
    [SerializeField] private int level3Cost = 3;

    // track last known for cheap UI refresh
    int lastKnownPoints = -1;

    // snapshot struct for whole tree state
    [System.Serializable]
    public struct UpgradeTreeState
    {
        public Emotions meleeEmotion;
        public int meleeLevel;

        public Emotions rangedEmotion;
        public int rangedLevel;

        public Emotions dashEmotion;
        public int dashLevel;
    }

    void Awake()
    {
        meleeUpgrades = GameObject.Find("UpgradeHolder").GetComponent<MeleeUpgrades>();
        CreateBordersForAllButtons();

        // find XPManager if not wired in inspector
        if (xpManager == null && XPManager.Instance != null)
        {
            xpManager = XPManager.Instance;
        }

        UpdatePointsUI();
    }

    void Update()
    {
        if (xpManager == null || pointsText == null) return;

        int currentPoints = xpManager.CurrentSkillPoints;
        if (currentPoints != lastKnownPoints)
        {
            UpdatePointsUI();
        }
    }

    void UpdatePointsUI()
    {
        if (xpManager == null || pointsText == null) return;

        lastKnownPoints = xpManager.CurrentSkillPoints;
        pointsText.text = "Points: " + lastKnownPoints;
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
            isSynergyActive = false;
        });
        if (synergyTab != null) synergyTab.onClick.AddListener(() =>
        {
            skillTree.SetActive(false);
            synergies.SetActive(true);
            isSynergyActive = true;

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

    public Button GetFirstButtonForCurrentTab()
    {
        if (isSynergyActive && firstSynergyButton != null)
        {
            return firstSynergyButton;
        }
        return null;
    }

    // expose purchased state to other systems (snapshot manager)
    public UpgradeTreeState GetPurchasedState()
    {
        UpgradeTreeState state;
        state.meleeEmotion = purchasedMeleeEmotion;
        state.meleeLevel = purchasedMeleeLevel;
        state.rangedEmotion = purchasedRangedEmotion;
        state.rangedLevel = purchasedRangedLevel;
        state.dashEmotion = purchasedDashEmotion;
        state.dashLevel = purchasedDashLevel;
        return state;
    }

    // allow snapshot restore to re-apply upgrades
    public void ApplyPurchasedState(UpgradeTreeState state)
    {
        purchasedMeleeEmotion = state.meleeEmotion;
        purchasedMeleeLevel = state.meleeLevel;

        purchasedRangedEmotion = state.rangedEmotion;
        purchasedRangedLevel = state.rangedLevel;

        purchasedDashEmotion = state.dashEmotion;
        purchasedDashLevel = state.dashLevel;

        // clear runtime state and re-apply levels
        if (meleeUpgrades != null)
        {
            meleeUpgrades.Respec();

            if (purchasedMeleeEmotion != Emotions.None && purchasedMeleeLevel > 0)
            {
                meleeUpgrades.SelectEmotion(purchasedMeleeEmotion);
                meleeUpgrades.SetLevel(purchasedMeleeLevel);
            }
        }

        ZeroRange();
        if (purchasedRangedEmotion != Emotions.None && purchasedRangedLevel > 0)
        {
            switch (purchasedRangedEmotion)
            {
                case Emotions.Joy: SetRangeJoyLevel(purchasedRangedLevel); break;
                case Emotions.Anger: SetRangeAngerLevel(purchasedRangedLevel); break;
                case Emotions.Sadness: SetRangeSadnessLevel(purchasedRangedLevel); break;
                case Emotions.Love: SetRangeLoveLevel(purchasedRangedLevel); break;
                case Emotions.Fear: SetRangeFearLevel(purchasedRangedLevel); break;
            }
        }

        ZeroDash();
        if (purchasedDashEmotion != Emotions.None && purchasedDashLevel > 0)
        {
            switch (purchasedDashEmotion)
            {
                case Emotions.Joy: SetDashJoyLevel(purchasedDashLevel); break;
                case Emotions.Anger: SetDashAngerLevel(purchasedDashLevel); break;
                case Emotions.Sadness: SetDashSadLevel(purchasedDashLevel); break;
                case Emotions.Love: SetDashLoveLevel(purchasedDashLevel); break;
                case Emotions.Fear: SetDashFearLevel(purchasedDashLevel); break;
            }
        }

        // clear pending
        pendingMeleeEmotion = Emotions.None;
        pendingMeleeLevel = 0;
        pendingRangedEmotion = Emotions.None;
        pendingRangedLevel = 0;
        pendingDashEmotion = Emotions.None;
        pendingDashLevel = 0;

        UpdateButtonVisuals();
    }

    int GetTierCost(int level)
    {
        switch (level)
        {
            case 1: return level1Cost; // default 1
            case 2: return level2Cost; // default 2
            case 3: return level3Cost; // default 3
            default: return 0;
        }
    }

    // total extra cost to go from currentLevel -> targetLevel
    int GetTotalCostForTargetLevel(int currentLevel, int targetLevel)
    {
        if (targetLevel <= currentLevel) return 0;

        int cost = 0;
        for (int i = currentLevel + 1; i <= targetLevel; i++)
        {
            cost += GetTierCost(i);
        }
        return cost;
    }

    void ZeroRange()
    {
        for (int i = 0; i < 10; i++)
        {
            upgradeHandler.RangedJoyDown();
            upgradeHandler.RangedAngerDown();
            upgradeHandler.RangedSadnessDown();
            upgradeHandler.RangedFearDown();
            upgradeHandler.RangedLoveDown();
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
            upgradeHandler.DashFearDown();
            upgradeHandler.DashLoveDown();
            upgradeHandler.DashJoyDown();
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
        SetPendingMeleeEmotion(Emotions.Joy, 1);
        Debug.Log("Joy Melee Upgrade 1 Clicked!");
    }
    private void OnJoyMelee2Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Joy, 2);
        Debug.Log("Joy Melee Upgrade 2 Clicked!");
    }
    private void OnJoyMelee3Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Joy, 3);
        Debug.Log("Joy Melee Upgrade 3 Clicked!");
    }

    // MELEE (ANGER)
    private void OnAngerMelee1Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Anger, 1);
        Debug.Log("Anger Melee Upgrade 1 Clicked!");
    }
    private void OnAngerMelee2Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Anger, 2);
        Debug.Log("Anger Melee Upgrade 2 Clicked!");
    }
    private void OnAngerMelee3Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Anger, 3);
        Debug.Log("Anger Melee Upgrade 3 Clicked!");
    }

    // MELEE (SADNESS)
    private void OnSadnessMelee1Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Sadness, 1);
        Debug.Log("Sadness Melee Upgrade 1 Clicked!");
    }
    private void OnSadnessMelee2Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Sadness, 2);
        Debug.Log("Sadness Melee Upgrade 2 Clicked!");
    }
    private void OnSadnessMelee3Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Sadness, 3);
        Debug.Log("Sadness Melee Upgrade 3 Clicked!");
    }

    // MELEE (LOVE)
    private void OnLoveMelee1Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Love, 1);
        Debug.Log("Love Melee Upgrade 1 Clicked!");
    }
    private void OnLoveMelee2Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Love, 2);
        Debug.Log("Love Melee Upgrade 2 Clicked!");
    }
    private void OnLoveMelee3Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Love, 3);
        Debug.Log("Love Melee Upgrade 3 Clicked!");
    }

    // MELEE (FEAR)
    private void OnFearMelee1Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Fear, 1);
        Debug.Log("Fear Melee Upgrade 1 Clicked!");
    }
    private void OnFearMelee2Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Fear, 2);
        Debug.Log("Fear Melee Upgrade 2 Clicked!");
    }
    private void OnFearMelee3Clicked()
    {
        SetPendingMeleeEmotion(Emotions.Fear, 3);
        Debug.Log("Fear Melee Upgrade 3 Clicked!");
    }

    // RANGE (JOY)
    private void OnJoyRange1Clicked()
    {
        SetPendingRangedEmotion(Emotions.Joy, 1);
        Debug.Log("Joy Range Upgrade 1 Clicked!");
    }
    private void OnJoyRange2Clicked()
    {
        SetPendingRangedEmotion(Emotions.Joy, 2);
        Debug.Log("Joy Range Upgrade 2 Clicked!");
    }
    private void OnJoyRange3Clicked()
    {
        SetPendingRangedEmotion(Emotions.Joy, 3);
        Debug.Log("Joy Range Upgrade 3 Clicked!");
    }

    // RANGE (ANGER)
    private void OnAngerRange1Clicked()
    {
        SetPendingRangedEmotion(Emotions.Anger, 1);
        Debug.Log("Anger Range Upgrade 1 Clicked!");
    }
    private void OnAngerRange2Clicked()
    {
        SetPendingRangedEmotion(Emotions.Anger, 2);
        Debug.Log("Anger Range Upgrade 2 Clicked!");
    }
    private void OnAngerRange3Clicked()
    {
        SetPendingRangedEmotion(Emotions.Anger, 3);
        Debug.Log("Anger Range Upgrade 3 Clicked!");
    }

    // RANGE (SADNESS)
    private void OnSadnessRange1Clicked()
    {
        SetPendingRangedEmotion(Emotions.Sadness, 1);
        Debug.Log("Sadness Range Upgrade 1 Clicked!");
    }
    private void OnSadnessRange2Clicked()
    {
        SetPendingRangedEmotion(Emotions.Sadness, 2);
        Debug.Log("Sadness Range Upgrade 2 Clicked!");
    }
    private void OnSadnessRange3Clicked()
    {
        SetPendingRangedEmotion(Emotions.Sadness, 3);
        Debug.Log("Sadness Range Upgrade 3 Clicked!");
    }

    // RANGE (LOVE)
    private void OnLoveRange1Clicked()
    {
        SetPendingRangedEmotion(Emotions.Love, 1);
        Debug.Log("Love Range Upgrade 1 Clicked!");
    }
    private void OnLoveRange2Clicked()
    {
        SetPendingRangedEmotion(Emotions.Love, 2);
        Debug.Log("Love Range Upgrade 2 Clicked!");
    }
    private void OnLoveRange3Clicked()
    {
        SetPendingRangedEmotion(Emotions.Love, 3);
        Debug.Log("Love Range Upgrade 3 Clicked!");
    }

    // RANGE (FEAR)
    private void OnFearRange1Clicked()
    {
        SetPendingRangedEmotion(Emotions.Fear, 1);
        Debug.Log("Fear Range Upgrade 1 Clicked!");
    }
    private void OnFearRange2Clicked()
    {
        SetPendingRangedEmotion(Emotions.Fear, 2);
        Debug.Log("Fear Range Upgrade 2 Clicked!");
    }
    private void OnFearRange3Clicked()
    {
        SetPendingRangedEmotion(Emotions.Fear, 3);
        Debug.Log("Fear Range Upgrade 3 Clicked!");
    }

    // DASH (JOY)
    private void OnJoyDash1Clicked()
    {
        SetPendingDashEmotion(Emotions.Joy, 1);
        Debug.Log("Joy Dash Upgrade 1 Clicked!");
    }
    private void OnJoyDash2Clicked()
    {
        SetPendingDashEmotion(Emotions.Joy, 2);
        Debug.Log("Joy Dash Upgrade 2 Clicked!");
    }
    private void OnJoyDash3Clicked()
    {
        SetPendingDashEmotion(Emotions.Joy, 3);
        Debug.Log("Joy Dash Upgrade 3 Clicked!");
    }

    // DASH (ANGER)
    private void OnAngerDash1Clicked()
    {
        SetPendingDashEmotion(Emotions.Anger, 1);
        Debug.Log("Anger Dash Upgrade 1 Clicked!");
    }
    private void OnAngerDash2Clicked()
    {
        SetPendingDashEmotion(Emotions.Anger, 2);
        Debug.Log("Anger Dash Upgrade 2 Clicked!");
    }
    private void OnAngerDash3Clicked()
    {
        SetPendingDashEmotion(Emotions.Anger, 3);
        Debug.Log("Anger Dash Upgrade 3 Clicked!");
    }

    // DASH (SADNESS)
    private void OnSadnessDash1Clicked()
    {
        SetPendingDashEmotion(Emotions.Sadness, 1);
        Debug.Log("Sad Dash Upgrade 1 Clicked!");
    }
    private void OnSadnessDash2Clicked()
    {
        SetPendingDashEmotion(Emotions.Sadness, 2);
        Debug.Log("Sad Dash Upgrade 2 Clicked!");
    }
    private void OnSadnessDash3Clicked()
    {
        SetPendingDashEmotion(Emotions.Sadness, 3);
        Debug.Log("Sad Dash Upgrade 3 Clicked!");
    }

    // DASH (LOVE)
    private void OnLoveDash1Clicked()
    {
        SetPendingDashEmotion(Emotions.Love, 1);
        Debug.Log("Love Dash Upgrade 1 Clicked!");
    }
    private void OnLoveDash2Clicked()
    {
        SetPendingDashEmotion(Emotions.Love, 2);
        Debug.Log("Love Dash Upgrade 2 Clicked!");
    }
    private void OnLoveDash3Clicked()
    {
        SetPendingDashEmotion(Emotions.Love, 3);
        Debug.Log("Love Dash Upgrade 3 Clicked!");
    }

    // DASH (FEAR)
    private void OnFearDash1Clicked()
    {
        SetPendingDashEmotion(Emotions.Fear, 1);
        Debug.Log("Fear Dash Upgrade 1 Clicked!");
    }
    private void OnFearDash2Clicked()
    {
        SetPendingDashEmotion(Emotions.Fear, 2);
        Debug.Log("Fear Dash Upgrade 2 Clicked!");
    }
    private void OnFearDash3Clicked()
    {
        SetPendingDashEmotion(Emotions.Fear, 3);
        Debug.Log("Fear Dash Upgrade 3 Clicked!");
    }

    #region PURCHASING UPGRADES
    public void SetPurchaseMode(bool canPurchase)
    {
        canPurchaseUpgrades = canPurchase;

        if (purchaseButton != null)
        {
            purchaseButton.gameObject.SetActive(canPurchase);
        }
    }
    private void OnPurchaseClicked()
    {
        if (!canPurchaseUpgrades)
        {
            Debug.Log("Cannot purchase upgrades in view-only mode.");
            return;
        }

        if (xpManager == null)
        {
            Debug.LogWarning("[UpgradeUI] No XPManager set. Applying upgrades without cost.");
            ApplyPurchasesWithoutCost();
            return;
        }

        int totalCost = 0;

        if (pendingMeleeEmotion != Emotions.None &&
            (pendingMeleeEmotion != purchasedMeleeEmotion || pendingMeleeLevel != purchasedMeleeLevel))
        {
            totalCost += GetTotalCostForTargetLevel(purchasedMeleeLevel, pendingMeleeLevel);
        }

        if (pendingRangedEmotion != Emotions.None &&
            (pendingRangedEmotion != purchasedRangedEmotion || pendingRangedLevel != purchasedRangedLevel))
        {
            totalCost += GetTotalCostForTargetLevel(purchasedRangedLevel, pendingRangedLevel);
        }

        if (pendingDashEmotion != Emotions.None &&
            (pendingDashEmotion != purchasedDashEmotion || pendingDashLevel != purchasedDashLevel))
        {
            totalCost += GetTotalCostForTargetLevel(purchasedDashLevel, pendingDashLevel);
        }

        if (totalCost <= 0)
        {
            Debug.Log("No new upgrades selected to purchase.");
            return;
        }

        if (!xpManager.TrySpendPoints(totalCost))
        {
            Debug.Log("Not enough points for these upgrades. Need " + totalCost +
                      ", have " + xpManager.CurrentSkillPoints);

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashButtonRed(purchaseButton));

            return;
        }


        bool purchaseMade = false;

        if (pendingMeleeEmotion != Emotions.None &&
            (pendingMeleeEmotion != purchasedMeleeEmotion || pendingMeleeLevel != purchasedMeleeLevel))
        {
            ApplyMeleePurchase();
            purchaseMade = true;
        }

        if (pendingRangedEmotion != Emotions.None &&
            (pendingRangedEmotion != purchasedRangedEmotion || pendingRangedLevel != purchasedRangedLevel))
        {
            ApplyRangedPurchase();
            purchaseMade = true;
        }

        if (pendingDashEmotion != Emotions.None &&
            (pendingDashEmotion != purchasedDashEmotion || pendingDashLevel != purchasedDashLevel))
        {
            ApplyDashPurchase();
            purchaseMade = true;
        }

        if (purchaseMade)
        {
            Debug.Log("Purchase successful. Spent " + totalCost + " points.");
            UpdateButtonVisuals();
            UpdatePointsUI();
        }
    }



    void ApplyPurchasesWithoutCost()
    {
        bool purchaseMade = false;

        if (pendingMeleeEmotion != Emotions.None &&
            (pendingMeleeEmotion != purchasedMeleeEmotion || pendingMeleeLevel != purchasedMeleeLevel))
        {
            ApplyMeleePurchase();
            purchaseMade = true;
        }

        if (pendingRangedEmotion != Emotions.None &&
            (pendingRangedEmotion != purchasedRangedEmotion || pendingRangedLevel != purchasedRangedLevel))
        {
            ApplyRangedPurchase();
            purchaseMade = true;
        }

        if (pendingDashEmotion != Emotions.None &&
            (pendingDashEmotion != purchasedDashEmotion || pendingDashLevel != purchasedDashLevel))
        {
            ApplyDashPurchase();
            purchaseMade = true;
        }

        if (purchaseMade)
        {
            UpdateButtonVisuals();
        }
    }


    private void ApplyMeleePurchase()
    {
        // Clear previous melee emotion if switching
        if (purchasedMeleeEmotion != Emotions.None && purchasedMeleeEmotion != pendingMeleeEmotion)
        {
            meleeUpgrades.Respec();
        }

        // Apply the new selection
        meleeUpgrades.SelectEmotion(pendingMeleeEmotion);
        meleeUpgrades.SetLevel(pendingMeleeLevel);

        // Update purchased state
        purchasedMeleeEmotion = pendingMeleeEmotion;
        purchasedMeleeLevel = pendingMeleeLevel;

        // Clear pending selection after purchase
        pendingMeleeEmotion = Emotions.None;
        pendingMeleeLevel = 0;

        // Deduct points (add later)
        // availablePoints -= (pendingMeleeLevel * pointsPerUpgrade);
    }

    private void ApplyRangedPurchase()
    {
        // Clear previous ranged upgrades if switching emotions
        if (purchasedRangedEmotion != Emotions.None && purchasedRangedEmotion != pendingRangedEmotion)
        {
            ZeroRange();
        }

        // Apply the new selection
        switch (pendingRangedEmotion)
        {
            case Emotions.Joy:
                SetRangeJoyLevel(pendingRangedLevel);
                break;
            case Emotions.Anger:
                SetRangeAngerLevel(pendingRangedLevel);
                break;
            case Emotions.Sadness:
                SetRangeSadnessLevel(pendingRangedLevel);
                break;
            case Emotions.Love:
                SetRangeLoveLevel(pendingRangedLevel);
                break;
            case Emotions.Fear:
                SetRangeFearLevel(pendingRangedLevel);
                break;
        }

        // Update purchased state
        purchasedRangedEmotion = pendingRangedEmotion;
        purchasedRangedLevel = pendingRangedLevel;

        // Clear pending selection after purchase
        pendingRangedEmotion = Emotions.None;
        pendingRangedLevel = 0;
    }

    private void ApplyDashPurchase()
    {
        // Clear previous dash if switching emotions
        if (purchasedDashEmotion != Emotions.None && purchasedDashEmotion != pendingDashEmotion)
        {
            ZeroDash();
        }

        // Apply the new selection
        switch (pendingDashEmotion)
        {
            case Emotions.Joy:
                SetDashJoyLevel(pendingDashLevel);
                break;
            case Emotions.Anger:
                SetDashAngerLevel(pendingDashLevel);
                break;
            case Emotions.Sadness:
                SetDashSadLevel(pendingDashLevel);
                break;
            case Emotions.Love:
                SetDashLoveLevel(pendingDashLevel);
                break;
            case Emotions.Fear:
                SetDashFearLevel(pendingDashLevel);
                break;
        }

        // Update purchased state
        purchasedDashEmotion = pendingDashEmotion;
        purchasedDashLevel = pendingDashLevel;

        // Clear pending selection after purchase
        pendingDashEmotion = Emotions.None;
        pendingDashLevel = 0;
    }
    #endregion

    #region HELPER METHODS FOR PENDING SELECTIONS
    private void SetPendingMeleeEmotion(Emotions emotion, int level)
    {
        pendingMeleeEmotion = emotion;
        pendingMeleeLevel = level;
        UpdateButtonVisuals();
    }

    private void SetPendingRangedEmotion(Emotions emotion, int level)
    {
        pendingRangedEmotion = emotion;
        pendingRangedLevel = level;
        UpdateButtonVisuals();
    }

    private void SetPendingDashEmotion(Emotions emotion, int level)
    {
        pendingDashEmotion = emotion;
        pendingDashLevel = level;
        UpdateButtonVisuals();
    }
    #endregion

    #region UPDATE BUTTON VISUAL HELPERS
    private void UpdateButtonVisuals()
    {
        UpdateMeleeButtonVisuals();
        UpdateRangedButtonVisuals();
        UpdateDashButtonVisuals();
    }

    private void UpdateMeleeButtonVisuals()
    {
        // Reset all melee buttons first
        ResetButtonState(joyMelee1);
        ResetButtonState(joyMelee2);
        ResetButtonState(joyMelee3);
        ResetButtonState(angerMelee1);
        ResetButtonState(angerMelee2);
        ResetButtonState(angerMelee3);
        ResetButtonState(sadnessMelee1);
        ResetButtonState(sadnessMelee2);
        ResetButtonState(sadnessMelee3);
        ResetButtonState(loveMelee1);
        ResetButtonState(loveMelee2);
        ResetButtonState(loveMelee3);
        ResetButtonState(fearMelee1);
        ResetButtonState(fearMelee2);
        ResetButtonState(fearMelee3);


        // Highlight purchased upgrades
        switch (purchasedMeleeEmotion)
        {
            case Emotions.Joy:
                if (purchasedMeleeLevel >= 1) SetButtonPurchased(joyMelee1);
                if (purchasedMeleeLevel >= 2) SetButtonPurchased(joyMelee2);
                if (purchasedMeleeLevel >= 3) SetButtonPurchased(joyMelee3);
                break;
            case Emotions.Anger:
                if (purchasedMeleeLevel >= 1) SetButtonPurchased(angerMelee1);
                if (purchasedMeleeLevel >= 2) SetButtonPurchased(angerMelee2);
                if (purchasedMeleeLevel >= 3) SetButtonPurchased(angerMelee3);
                break;
            case Emotions.Sadness:
                if (purchasedMeleeLevel >= 1) SetButtonPurchased(sadnessMelee1);
                if (purchasedMeleeLevel >= 2) SetButtonPurchased(sadnessMelee2);
                if (purchasedMeleeLevel >= 3) SetButtonPurchased(sadnessMelee3);
                break;
            case Emotions.Love:
                if (purchasedMeleeLevel >= 1) SetButtonPurchased(loveMelee1);
                if (purchasedMeleeLevel >= 2) SetButtonPurchased(loveMelee2);
                if (purchasedMeleeLevel >= 3) SetButtonPurchased(loveMelee3);
                break;
            case Emotions.Fear:
                if (purchasedMeleeLevel >= 1) SetButtonPurchased(fearMelee1);
                if (purchasedMeleeLevel >= 2) SetButtonPurchased(fearMelee2);
                if (purchasedMeleeLevel >= 3) SetButtonPurchased(fearMelee3);
                break;
        }

        // Highlight pending selection (different color)
        switch (pendingMeleeEmotion)
        {
            case Emotions.Joy:
                if (pendingMeleeLevel >= 1) SetButtonPending(joyMelee1);
                if (pendingMeleeLevel >= 2) SetButtonPending(joyMelee2);
                if (pendingMeleeLevel >= 3) SetButtonPending(joyMelee3);
                break;
            case Emotions.Anger:
                if (pendingMeleeLevel >= 1) SetButtonPending(angerMelee1);
                if (pendingMeleeLevel >= 2) SetButtonPending(angerMelee2);
                if (pendingMeleeLevel >= 3) SetButtonPending(angerMelee3);
                break;
            case Emotions.Sadness:
                if (pendingMeleeLevel >= 1) SetButtonPending(sadnessMelee1);
                if (pendingMeleeLevel >= 2) SetButtonPending(sadnessMelee2);
                if (pendingMeleeLevel >= 3) SetButtonPending(sadnessMelee3);
                break;
            case Emotions.Love:
                if (pendingMeleeLevel >= 1) SetButtonPending(loveMelee1);
                if (pendingMeleeLevel >= 2) SetButtonPending(loveMelee2);
                if (pendingMeleeLevel >= 3) SetButtonPending(loveMelee3);
                break;
            case Emotions.Fear:
                if (pendingMeleeLevel >= 1) SetButtonPending(fearMelee1);
                if (pendingMeleeLevel >= 2) SetButtonPending(fearMelee2);
                if (pendingMeleeLevel >= 3) SetButtonPending(fearMelee3);
                break;
        }
    }

    private void UpdateRangedButtonVisuals()
    {
        ResetButtonState(joyRange1);
        ResetButtonState(joyRange2);
        ResetButtonState(joyRange3);
        ResetButtonState(angerRange1);
        ResetButtonState(angerRange2);
        ResetButtonState(angerRange3);
        ResetButtonState(sadnessRange1);
        ResetButtonState(sadnessRange2);
        ResetButtonState(sadnessRange3);
        ResetButtonState(loveRange1);
        ResetButtonState(loveRange2);
        ResetButtonState(loveRange3);
        ResetButtonState(fearRange1);
        ResetButtonState(fearRange2);
        ResetButtonState(fearRange3);

        switch (purchasedRangedEmotion)
        {
            case Emotions.Joy:
                if (purchasedRangedLevel >= 1) SetButtonPurchased(joyRange1);
                if (purchasedRangedLevel >= 2) SetButtonPurchased(joyRange2);
                if (purchasedRangedLevel >= 3) SetButtonPurchased(joyRange3);
                break;
            case Emotions.Anger:
                if (purchasedRangedLevel >= 1) SetButtonPurchased(angerRange1);
                if (purchasedRangedLevel >= 2) SetButtonPurchased(angerRange2);
                if (purchasedRangedLevel >= 3) SetButtonPurchased(angerRange3);
                break;
            case Emotions.Sadness:
                if (purchasedRangedLevel >= 1) SetButtonPurchased(sadnessRange1);
                if (purchasedRangedLevel >= 2) SetButtonPurchased(sadnessRange2);
                if (purchasedRangedLevel >= 3) SetButtonPurchased(sadnessRange3);
                break;
            case Emotions.Love:
                if (purchasedRangedLevel >= 1) SetButtonPurchased(loveRange1);
                if (purchasedRangedLevel >= 2) SetButtonPurchased(loveRange2);
                if (purchasedRangedLevel >= 3) SetButtonPurchased(loveRange3);
                break;
            case Emotions.Fear:
                if (purchasedRangedLevel >= 1) SetButtonPurchased(fearRange1);
                if (purchasedRangedLevel >= 2) SetButtonPurchased(fearRange2);
                if (purchasedRangedLevel >= 3) SetButtonPurchased(fearRange3);
                break;
        }

        switch (pendingRangedEmotion)
        {
            case Emotions.Joy:
                if (pendingRangedLevel >= 1) SetButtonPending(joyRange1);
                if (pendingRangedLevel >= 2) SetButtonPending(joyRange2);
                if (pendingRangedLevel >= 3) SetButtonPending(joyRange3);
                break;
            case Emotions.Anger:
                if (pendingRangedLevel >= 1) SetButtonPending(angerRange1);
                if (pendingRangedLevel >= 2) SetButtonPending(angerRange2);
                if (pendingRangedLevel >= 3) SetButtonPending(angerRange3);
                break;
            case Emotions.Sadness:
                if (pendingRangedLevel >= 1) SetButtonPending(sadnessRange1);
                if (pendingRangedLevel >= 2) SetButtonPending(sadnessRange2);
                if (pendingRangedLevel >= 3) SetButtonPending(sadnessRange3);
                break;
            case Emotions.Love:
                if (pendingRangedLevel >= 1) SetButtonPending(loveRange1);
                if (pendingRangedLevel >= 2) SetButtonPending(loveRange2);
                if (pendingRangedLevel >= 3) SetButtonPending(loveRange3);
                break;
            case Emotions.Fear:
                if (pendingRangedLevel >= 1) SetButtonPending(fearRange1);
                if (pendingRangedLevel >= 2) SetButtonPending(fearRange2);
                if (pendingRangedLevel >= 3) SetButtonPending(fearRange3);
                break;
        }
    }

    private void UpdateDashButtonVisuals()
    {
        ResetButtonState(joyDash1);
        ResetButtonState(joyDash2);
        ResetButtonState(joyDash3);
        ResetButtonState(angerDash1);
        ResetButtonState(angerDash2);
        ResetButtonState(angerDash3);
        ResetButtonState(sadnessDash1);
        ResetButtonState(sadnessDash2);
        ResetButtonState(sadnessDash3);
        ResetButtonState(loveDash1);
        ResetButtonState(loveDash2);
        ResetButtonState(loveDash3);
        ResetButtonState(fearDash1);
        ResetButtonState(fearDash2);
        ResetButtonState(fearDash3);

        switch (purchasedDashEmotion)
        {
            case Emotions.Joy:
                if (purchasedDashLevel >= 1) SetButtonPurchased(joyDash1);
                if (purchasedDashLevel >= 2) SetButtonPurchased(joyDash2);
                if (purchasedDashLevel >= 3) SetButtonPurchased(joyDash3);
                break;
            case Emotions.Anger:
                if (purchasedDashLevel >= 1) SetButtonPurchased(angerDash1);
                if (purchasedDashLevel >= 2) SetButtonPurchased(angerDash2);
                if (purchasedDashLevel >= 3) SetButtonPurchased(angerDash3);
                break;
            case Emotions.Sadness:
                if (purchasedDashLevel >= 1) SetButtonPurchased(sadnessDash1);
                if (purchasedDashLevel >= 2) SetButtonPurchased(sadnessDash2);
                if (purchasedDashLevel >= 3) SetButtonPurchased(sadnessDash3);
                break;
            case Emotions.Love:
                if (purchasedDashLevel >= 1) SetButtonPurchased(loveDash1);
                if (purchasedDashLevel >= 2) SetButtonPurchased(loveDash2);
                if (purchasedDashLevel >= 3) SetButtonPurchased(loveDash3);
                break;
            case Emotions.Fear:
                if (purchasedDashLevel >= 1) SetButtonPurchased(fearDash1);
                if (purchasedDashLevel >= 2) SetButtonPurchased(fearDash2);
                if (purchasedDashLevel >= 3) SetButtonPurchased(fearDash3);
                break;
        }

        switch (pendingDashEmotion)
        {
            case Emotions.Joy:
                if (pendingDashLevel >= 1) SetButtonPending(joyDash1);
                if (pendingDashLevel >= 2) SetButtonPending(joyDash2);
                if (pendingDashLevel >= 3) SetButtonPending(joyDash3);
                break;
            case Emotions.Anger:
                if (pendingDashLevel >= 1) SetButtonPending(angerDash1);
                if (pendingDashLevel >= 2) SetButtonPending(angerDash2);
                if (pendingDashLevel >= 3) SetButtonPending(angerDash3);
                break;
            case Emotions.Sadness:
                if (pendingDashLevel >= 1) SetButtonPending(sadnessDash1);
                if (pendingDashLevel >= 2) SetButtonPending(sadnessDash2);
                if (pendingDashLevel >= 3) SetButtonPending(sadnessDash3);
                break;
            case Emotions.Love:
                if (pendingDashLevel >= 1) SetButtonPending(loveDash1);
                if (pendingDashLevel >= 2) SetButtonPending(loveDash2);
                if (pendingDashLevel >= 3) SetButtonPending(loveDash3);
                break;
            case Emotions.Fear:
                if (pendingDashLevel >= 1) SetButtonPending(fearDash1);
                if (pendingDashLevel >= 2) SetButtonPending(fearDash2);
                if (pendingDashLevel >= 3) SetButtonPending(fearDash3);
                break;
        }
    }
    #endregion
    private void CreateBordersForAllButtons()
    {
        Button[] allButtons = new Button[]
        {
        joyMelee1, joyMelee2, joyMelee3,
        angerMelee1, angerMelee2, angerMelee3,
        sadnessMelee1, sadnessMelee2, sadnessMelee3,
        loveMelee1, loveMelee2, loveMelee3,
        fearMelee1, fearMelee2, fearMelee3,
        joyRange1, joyRange2, joyRange3,
        angerRange1, angerRange2, angerRange3,
        sadnessRange1, sadnessRange2, sadnessRange3,
        loveRange1, loveRange2, loveRange3,
        fearRange1, fearRange2, fearRange3,
        joyDash1, joyDash2, joyDash3,
        angerDash1, angerDash2, angerDash3,
        sadnessDash1, sadnessDash2, sadnessDash3,
        loveDash1, loveDash2, loveDash3,
        fearDash1, fearDash2, fearDash3
        };

        foreach (Button button in allButtons)
        {
            if (button != null)
            {
                CreateBorderForButton(button);
                CreateOutlineForButton(button);
            }
        }
    }

    private void CreateBorderForButton(Button button)
    {
        Transform existingBorder = button.transform.Find("Border");
        if (existingBorder != null) return;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null || buttonImage.sprite == null) return;

        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(button.transform, false);
        borderObj.transform.SetAsFirstSibling();

        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.anchoredPosition = Vector2.zero;

        float borderSize = 10f;
        borderRect.offsetMin = new Vector2(-borderSize, -borderSize);
        borderRect.offsetMax = new Vector2(borderSize, borderSize);

        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.sprite = buttonImage.sprite;
        borderImage.type = buttonImage.type;
        borderImage.color = Color.white;
        borderImage.enabled = false;
        borderImage.raycastTarget = false;
    }


    private void CreateOutlineForButton(Button button)
    {
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(10, 10);
        outline.enabled = false;
    }

    private void ResetButtonState(Button button)
    {
        if (button != null)
        {
            Transform border = button.transform.Find("Border");
            if (border != null)
            {
                Image borderImage = border.GetComponent<Image>();
                if (borderImage != null)
                {
                    borderImage.enabled = false;
                }
            }

            Outline outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

    private void SetButtonPurchased(Button button)
    {
        if (button != null)
        {
            Transform border = button.transform.Find("Border");
            if (border != null)
            {
                Image borderImage = border.GetComponent<Image>();
                if (borderImage != null)
                {
                    borderImage.enabled = false;
                }
            }

            Outline outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
            }
        }
    }

    private void SetButtonPending(Button button)
    {
        if (button != null)
        {
            Outline outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }

            Transform border = button.transform.Find("Border");
            if (border != null)
            {
                Image borderImage = border.GetComponent<Image>();
                if (borderImage != null)
                {
                    borderImage.enabled = true;
                }
            }
        }
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

    public void UpdateDescriptionPanel(string upgradeName, string description, string cost)
    {
        if (upgradeNameText != null)
        {
            upgradeNameText.text = upgradeName;
        }

        if (upgradeDescriptionText != null)
        {
            upgradeDescriptionText.text = description;
        }

        if (upgradeCostText != null)
        {
            upgradeCostText.text = cost;
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

    private Coroutine flashCoroutine;
    private IEnumerator FlashButtonRed(Button button)
    {
        if (button == null) yield break;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null) yield break;

        Color greenColor = Color.green;
        Color redColor = Color.red;
        float flashDuration = 0.2f;

        buttonImage.color = redColor;
        yield return new WaitForSeconds(flashDuration);

        buttonImage.color = greenColor;

        flashCoroutine = null;
    }
}
