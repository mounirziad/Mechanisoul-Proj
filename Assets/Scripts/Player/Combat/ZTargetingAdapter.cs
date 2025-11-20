using UnityEngine;

public class ZTargetingAdapter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZTargetingSystem zTargeting;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private LockOnLetterbox letterboxUI;
    
    [Header("Settings")]
    [SerializeField] private bool updateCombatTarget = true;
    [SerializeField] private bool controlLetterbox = true;
    
    private void Awake()
    {
        if (zTargeting == null)
            zTargeting = GetComponent<ZTargetingSystem>();
        
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();
    }
    
    private void OnEnable()
    {
        if (zTargeting != null)
        {
            zTargeting.OnTargetChanged += OnTargetChanged;
            zTargeting.OnLockStateChanged += OnLockStateChanged;
        }
    }
    
    private void OnDisable()
    {
        if (zTargeting != null)
        {
            zTargeting.OnTargetChanged -= OnTargetChanged;
            zTargeting.OnLockStateChanged -= OnLockStateChanged;
        }
    }
    
    private void OnTargetChanged(Transform newTarget, Transform oldTarget)
    {
        if (updateCombatTarget && playerCombat != null)
        {
            playerCombat.currentTarget = newTarget;
        }
    }
    
    private void OnLockStateChanged(bool isLocked)
    {
        if (controlLetterbox && letterboxUI != null)
        {
            if (isLocked)
            {
                letterboxUI.ShowBars();
            }
            else
            {
                letterboxUI.HideBars();
            }
        }
    }
    
    public bool IsLocked()
    {
        return zTargeting != null && zTargeting.IsLocked;
    }
    
    public Transform GetCurrentTarget()
    {
        return zTargeting != null ? zTargeting.CurrentTarget : null;
    }
    
    public void SetLetterboxUI(LockOnLetterbox newLetterbox)
    {
        letterboxUI = newLetterbox;
    }
    
    public LockOnLetterbox GetLetterboxUI()
    {
        return letterboxUI;
    }
}
