using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombosMenuUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ComboUpgrades comboUpgrades;
    
    [Header("UI References")]
    [SerializeField] private Transform comboCardsContainer;
    [SerializeField] private GameObject comboCardPrefab;

    [Header("Combo Card Elements (if using single card)")]
    [SerializeField] private GameObject singleComboCard;
    [SerializeField] private TextMeshProUGUI comboTitle;
    [SerializeField] private TextMeshProUGUI comboBody;
    [SerializeField] private Image chipLeft;
    [SerializeField] private Image chipRight;

    [Header("Locked view")]
    [SerializeField] private string lockedTitle = "Combo Locked";
    [SerializeField, TextArea]
    private string lockedBody = "Discover by upgrading Melee: Anger and Ranged: Joy (any level).";

    [Header("Unlocked view (after first discovery)")]
    [SerializeField] private string unlockedTitle = "Rage + Joy: Pop Shot";
    [SerializeField, TextArea]
    private string unlockedBodyTemplate = "Unlocked! Ranged hits spawn a JOY burst.\nCurrent: Anger L{0}, Joy L{1}.";

    [Header("Chip colors")]
    [SerializeField] private Color angerColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color joyColor = new Color(1.00f, 0.90f, 0.15f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private const string PREF_KEY = "Combo_Discovered_AngerMelee_JoyRanged";
    private bool discovered;

    private void Awake()
    {
        if (!comboUpgrades)
        {
            comboUpgrades = FindObjectOfType<ComboUpgrades>();
        }

        discovered = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        
        AutoAssignReferencesIfNeeded();
    }

    private void AutoAssignReferencesIfNeeded()
    {
        if (singleComboCard == null && comboCardsContainer != null && comboCardsContainer.childCount > 0)
        {
            singleComboCard = comboCardsContainer.GetChild(0).gameObject;
        }

        if (singleComboCard != null)
        {
            if (comboTitle == null)
            {
                comboTitle = FindTextInChildren(singleComboCard.transform, "Title");
                if (comboTitle == null)
                    comboTitle = singleComboCard.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (comboBody == null)
            {
                comboBody = FindTextInChildren(singleComboCard.transform, "Body");
                if (comboBody == null && comboTitle != null)
                {
                    TextMeshProUGUI[] texts = singleComboCard.GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length > 1)
                        comboBody = texts[1];
                }
            }

            if (chipLeft == null)
            {
                chipLeft = FindImageInChildren(singleComboCard.transform, "ChipLeft");
            }

            if (chipRight == null)
            {
                chipRight = FindImageInChildren(singleComboCard.transform, "ChipRight");
            }
        }
    }

    private TextMeshProUGUI FindTextInChildren(Transform parent, string partialName)
    {
        foreach (TextMeshProUGUI text in parent.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name.Contains(partialName))
                return text;
        }
        return null;
    }

    private Image FindImageInChildren(Transform parent, string partialName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains(partialName))
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                    return img;
            }
        }
        return null;
    }

    private void OnEnable()
    {
        if (comboUpgrades == null)
        {
            Debug.LogWarning("[CombosMenuUI] Missing ComboUpgades.");
            return;
        }

        comboUpgrades.ComboAdded += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (comboUpgrades != null)
        {
            comboUpgrades.ComboAdded -= Refresh;
        }
    }

    public void Refresh()
    {
        if (comboUpgrades == null) return;

        if (!discovered && comboUpgrades.HasCombo())
        {
            discovered = true;
            PlayerPrefs.SetInt(PREF_KEY, 1);
            PlayerPrefs.Save();
        }

        bool showUnlocked = discovered;

        UpdateComboCard(showUnlocked);
    }

    private void UpdateComboCard(bool showUnlocked)
    {
        if (comboTitle != null)
        {
            comboTitle.text = showUnlocked ? unlockedTitle : lockedTitle;
        }

        if (comboBody != null)
        {
            if (showUnlocked)
            {
                comboBody.text = string.Format(
                    unlockedBodyTemplate, 
                    comboUpgrades.GetMeleeAngerLevel(), 
                    comboUpgrades.GetRangedJoyLevel()
                );
            }
            else
            {
                comboBody.text = lockedBody;
            }
        }

        if (chipLeft != null)
        {
            chipLeft.color = showUnlocked ? angerColor : lockedColor;
        }

        if (chipRight != null)
        {
            chipRight.color = showUnlocked ? joyColor : lockedColor;
        }
    }
}
