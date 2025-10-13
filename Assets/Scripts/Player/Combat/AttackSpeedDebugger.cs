using UnityEngine;
using UnityEngine.UI;

public class AttackSpeedDebugger : MonoBehaviour
{
    [Header("Debug UI")]
    public Text debugText;
    public bool showDebugInConsole = true;
    public bool showDebugOnScreen = false;
    
    [Header("Testing")]
    public KeyCode testJoyUpgradeKey = KeyCode.J;
    public KeyCode testJoyDowngradeKey = KeyCode.K;
    public KeyCode testTempModifierKey = KeyCode.T;
    
    private PlayerManager playerManager;
    private AttackAnimationManager attackAnimManager;
    private UpgradeHandler upgradeHandler;
    
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        attackAnimManager = GetComponent<AttackAnimationManager>();
        upgradeHandler = FindObjectOfType<UpgradeHandler>();
        
        if (showDebugOnScreen && debugText == null)
        {
            // Try to find a debug text component
            debugText = FindObjectOfType<Text>();
        }
    }
    
    void Update()
    {
        HandleDebugInput();
        UpdateDebugDisplay();
    }
    
    void HandleDebugInput()
    {
        // Test Joy upgrade increases
        if (Input.GetKeyDown(testJoyUpgradeKey) && upgradeHandler != null)
        {
            upgradeHandler.MeleeJoyUp();
            Debug.Log("Joy upgrade increased!");
        }
        
        // Test Joy upgrade decreases
        if (Input.GetKeyDown(testJoyDowngradeKey) && upgradeHandler != null)
        {
            upgradeHandler.MeleeJoyDown();
            Debug.Log("Joy upgrade decreased!");
        }
        
        // Test temporary modifier
        if (Input.GetKeyDown(testTempModifierKey))
        {
            AttackSpeedUtility.AddTemporarySpeedModifier("TestBuff", 0.5f, 5f);
            Debug.Log("Added temporary +50% attack speed for 5 seconds!");
        }
    }
    
    void UpdateDebugDisplay()
    {
        if (playerManager == null || attackAnimManager == null) return;
        
        string debugInfo = GetDebugInfo();
        
        if (showDebugInConsole && attackAnimManager.IsApplyingAttackSpeed())
        {
            Debug.Log(debugInfo);
        }
        
        if (showDebugOnScreen && debugText != null)
        {
            debugText.text = debugInfo;
        }
    }
    
    string GetDebugInfo()
    {
        string info = "=== ATTACK SPEED DEBUG ===\n";
        
        if (upgradeHandler != null)
        {
            info += $"Joy Upgrade Level: {upgradeHandler.MeleeAngerLevel}\n";
        }
        
        if (playerManager != null)
        {
            info += $"Current Attack Speed Buff: +{playerManager.GetAttackSpeedBuff():F2} ({(playerManager.GetAttackSpeedBuff() * 100):F0}%)\n";
        }
        
        if (attackAnimManager != null)
        {
            info += $"Animation Manager Active: {attackAnimManager.IsApplyingAttackSpeed()}\n";
            info += $"Current Speed Multiplier: {attackAnimManager.GetCurrentSpeedMultiplier():F2}\n";
            
            if (attackAnimManager.GetCurrentAttackData() != null)
            {
                var attackData = attackAnimManager.GetCurrentAttackData();
                info += $"Current Attack: {attackData.name}\n";
                info += $"Base Animation Speed: {attackData.baseAnimationSpeed:F2}\n";
                info += $"Affected by Upgrades: {attackData.affectedByAttackSpeedUpgrades}\n";
            }
        }
        
        info += "\n=== CONTROLS ===\n";
        info += $"Press [{testJoyUpgradeKey}] to increase Joy upgrade\n";
        info += $"Press [{testJoyDowngradeKey}] to decrease Joy upgrade\n";
        info += $"Press [{testTempModifierKey}] to add temporary speed buff\n";
        
        return info;
    }
    
    void OnGUI()
    {
        if (!showDebugOnScreen || debugText != null) return;
        
        // Fallback GUI display if no Text component is assigned
        GUI.Label(new Rect(10, 10, 400, 300), GetDebugInfo());
    }
}