using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    [Header("Upgrade Info")]
    public UpgradeData upgradeData;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;
    
    private UpgradeUIScript upgradeUIScript;
    private Button button;
    private bool isSelected;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    void Start()
    {
        upgradeUIScript = GetComponentInParent<UpgradeUIScript>();
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
        
        ResetVisuals();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            ShowUpgradeInfo();
            SetHoverVisuals();
        }
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (!isSelected)
        {
            ShowUpgradeInfo();
            SetHoverVisuals();
        }
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        if (!isSelected)
        {
            ResetVisuals();
        }
    }
    
    public void OnUpgradeClicked()
    {
        if (upgradeUIScript != null)
        {
            upgradeUIScript.SelectUpgrade(this);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (isSelected)
        {
            SetSelectedVisuals();
            ShowUpgradeInfo();
        }
        else
        {
            ResetVisuals();
        }
    }
    
    private void ShowUpgradeInfo()
    {
        if (upgradeUIScript != null && upgradeData != null)
        {
            upgradeUIScript.UpdateDescriptionPanel(upgradeData.upgradeName, upgradeData.description, upgradeData.cost.ToString());
        }
    }
    
    private void SetHoverVisuals()
    {
        if (iconImage != null)
        {
            iconImage.color = hoverColor;
        }
    }
    
    private void SetSelectedVisuals()
    {
        if (iconImage != null)
        {
            iconImage.color = selectedColor;
        }
    }
    
    private void ResetVisuals()
    {
        if (iconImage != null)
        {
            iconImage.color = normalColor;
        }
    }
}
