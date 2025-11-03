using UnityEngine;

[DisallowMultipleComponent]
public class DashUpgrades : Upgrade
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

        handler.dashAngerLvl = 0;
        handler.dashSadnessLvl = 0;
        handler.dashJoyLvl = 0;
        handler.dashLoveLvl = 0;
        handler.dashFearLvl = 0;

        int lvl = Mathf.Clamp(upgradeLevel, 0, 3);
        switch (selectedEmotion)
        {
            case Emotions.Joy:     
                handler.dashJoyLvl = lvl; break;
            case Emotions.Anger:   
                handler.dashAngerLvl = lvl; break;
            case Emotions.Sadness: 
                handler.dashSadnessLvl = lvl; break;
            case Emotions.Love:    
                handler.dashLoveLvl = lvl; break;
            case Emotions.Fear:    
                handler.dashFearLvl = lvl; break;
        }

        handler.PushAll();
    }
}
