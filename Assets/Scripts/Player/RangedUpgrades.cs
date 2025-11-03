using UnityEngine;

[DisallowMultipleComponent]
public class RangedUpgrades : Upgrade
{
    [SerializeField] UpgradeHandler handler;

    protected override void Awake()
    {
        base.Awake();
        if (!handler) handler = GetComponent<UpgradeHandler>();
        if (!handler)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) handler = p.GetComponent<UpgradeHandler>();
        }
    }

    protected override void JoyChange()     { UpdatePlayer(); }
    protected override void AngerChange()   { UpdatePlayer(); }
    protected override void SadnessChange() { UpdatePlayer(); }
    protected override void LoveChange()    { UpdatePlayer(); }
    protected override void FearChange()    { UpdatePlayer(); }

    protected override void UpdatePlayer()
    {
        if (!handler) return;

        handler.rangedJoyLvl = 0;
        handler.rangedAngerLvl = 0;
        handler.rangedSadnessLvl = 0;
        handler.rangedLoveLvl = 0;
        handler.rangedFearLvl = 0;

        switch (selectedEmotion)
        {
            case Emotions.Joy:     
                handler.rangedJoyLvl = Mathf.Clamp(upgradeLevel, 0, 3); break;
            case Emotions.Anger:   
                handler.rangedAngerLvl = Mathf.Clamp(upgradeLevel, 0, 3); break;
            case Emotions.Sadness: 
                handler.rangedSadnessLvl = Mathf.Clamp(upgradeLevel, 0, 3); break;
            case Emotions.Love:    
                handler.rangedLoveLvl = Mathf.Clamp(upgradeLevel, 0, 3); break;
            case Emotions.Fear:    
                handler.rangedFearLvl = Mathf.Clamp(upgradeLevel, 0, 3); break;
        }

        handler.PushRanged();
    }
}
