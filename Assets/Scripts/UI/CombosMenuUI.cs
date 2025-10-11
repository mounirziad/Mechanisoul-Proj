using UnityEngine;
using UnityEngine.UIElements;

public class CombosMenuUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UpgradeHandler handler;
    [SerializeField] private UIDocument uiDoc;

    [Header("UXML element names")]
    [SerializeField] private string combosRootName = "CombosEL"; // container already in your UI
    [SerializeField] private string titleName = "ComboTitle";
    [SerializeField] private string bodyName = "ComboBody";
    [SerializeField] private string chipLeftName = "ChipLeft";   // optional squares
    [SerializeField] private string chipRightName = "ChipRight";

    [Header("Locked view")]
    [SerializeField] private string lockedTitle = "Combo Locked";
    [SerializeField, TextArea]
    private string lockedBody =
        "Discover by upgrading Melee: Anger and Ranged: Joy (any level).";

    [Header("Unlocked view (after first discovery)")]
    [SerializeField] private string unlockedTitle = "Rage + Joy: Pop Shot";
    [SerializeField, TextArea]
    private string unlockedBodyTemplate =
        "Unlocked! Ranged hits spawn a JOY burst.\nCurrent: Anger L{0}, Joy L{1}.";

    [Header("Chip colors")]
    [SerializeField] private Color angerColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color joyColor = new Color(1.00f, 0.90f, 0.15f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    // internal
    const string PrefKey = "Combo_Discovered_AngerMelee_JoyRanged";
    bool discovered;

    // cached UI
    VisualElement root, combosRoot, chipLeft, chipRight;
    Label title, body;

    void OnEnable()
    {
        if (!uiDoc) uiDoc = GetComponent<UIDocument>();
        if (!handler) handler = FindObjectOfType<UpgradeHandler>();

        if (!uiDoc || !handler)
        {
            Debug.LogWarning("[CombosMenuUI] Missing UIDocument or UpgradeHandler.");
            return;
        }

        root = uiDoc.rootVisualElement;
        combosRoot = root.Q<VisualElement>(combosRootName);
        title = root.Q<Label>(titleName);
        body = root.Q<Label>(bodyName);
        chipLeft = root.Q<VisualElement>(chipLeftName);
        chipRight = root.Q<VisualElement>(chipRightName);

        discovered = PlayerPrefs.GetInt(PrefKey, 0) == 1;

        handler.LevelsChanged += Refresh;
        Refresh(); // initial
    }

    void OnDisable()
    {
        if (handler != null) handler.LevelsChanged -= Refresh;
    }

    public void Refresh()
    {
        if (root == null || handler == null) return;

        // If combo conditions are met now, mark as discovered permanently
        if (!discovered && handler.HasCombo_AngerMelee_JoyRanged)
        {
            discovered = true;
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }

        bool showUnlocked = discovered;

        if (title != null) title.text = showUnlocked ? unlockedTitle : lockedTitle;

        if (body != null)
        {
            if (showUnlocked)
                body.text = string.Format(unlockedBodyTemplate, handler.MeleeAngerLevel, handler.RangedJoyLevel);
            else
                body.text = lockedBody;
        }

        if (chipLeft != null) chipLeft.style.backgroundColor = new StyleColor(showUnlocked ? angerColor : lockedColor);
        if (chipRight != null) chipRight.style.backgroundColor = new StyleColor(showUnlocked ? joyColor : lockedColor);
    }
}
