using UnityEngine;

public abstract class Upgrade : MonoBehaviour
{
    protected int upgradeLevel;
    protected Emotions selectedEmotion;

    protected PlayerManager playerManager;

    protected virtual void Awake()
    {
        upgradeLevel = 0;
        selectedEmotion = Emotions.None;

        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    public void SelectEmotion(Emotions emotion)
    {
        selectedEmotion = emotion;
        UpgradeEmotion();
    }

    void DeselectEmotion() => selectedEmotion = Emotions.None;

    public void UpgradeEmotion()
    {
        upgradeLevel++;
        LevelChange();
    }

    public void DowngradeEmotion()
    {
        upgradeLevel--;
        LevelChange();
        if (upgradeLevel == 0) DeselectEmotion();
    }

    // let external systems set level directly (UI calls in most cases)
    public void SetLevel(int level)
    {
        upgradeLevel = Mathf.Max(0, level);
        LevelChange();
        if (upgradeLevel == 0) DeselectEmotion();
        
    }

    void LevelChange()
    {
        switch (selectedEmotion)
        {
            case Emotions.Joy:     JoyChange();    break;
            case Emotions.Anger:   AngerChange();  break;
            case Emotions.Sadness: SadnessChange();break;
            case Emotions.Love:    LoveChange();   break;
            case Emotions.Fear:    FearChange();   break;
            case Emotions.None:
                Debug.LogWarning("No emotion selected for level change");
                break;
            default:
                Debug.LogError("selected emotion for level change out of bounds");
                break;
        }
        UpdatePlayer(); // allow pushing to player/handlers
    }

    public void Respec()
    {
        upgradeLevel = 0;
        LevelChange();
        DeselectEmotion();
        
    }

    // overridables for futureproofing
    protected virtual void JoyChange()    {}
    protected virtual void AngerChange()  {}
    protected virtual void SadnessChange(){}
    protected virtual void LoveChange()   {}
    protected virtual void FearChange()   {}
    protected virtual void UpdatePlayer() {}
}
