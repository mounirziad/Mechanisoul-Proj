using UnityEngine;

public abstract class Upgrade : MonoBehaviour
{
    int upgradeLevel;
    Emotions selectedEmotion;

    private void Awake()
    {
        upgradeLevel = 0;
        selectedEmotion = Emotions.None;
    }

    public void SelectEmotion(Emotions emotion)
    {
        selectedEmotion = emotion;
        UpgradeEmotion();
    }

    void DeselectEmotion()
    {
        selectedEmotion = Emotions.None;
    }

    public void UpgradeEmotion()
    {
        upgradeLevel++;
        LevelChange();
    }

    public void DowngradeEmotion()
    {
        upgradeLevel--;
        LevelChange();

        if (upgradeLevel == 0)
            DeselectEmotion();
    }

    void LevelChange()
    {
        switch (selectedEmotion)
        {
            case Emotions.Joy:
                JoyChange(); break;
            case Emotions.Anger:
                AngerChange(); break;
            case Emotions.Sadness:
                SadnessChange(); break;
            case Emotions.Love:
                LoveChange(); break;
            case Emotions.Fear:
                FearChange(); break;
            case Emotions.None:
                Debug.LogWarning("No emotion selected for level change");
                break;
            default:
                Debug.LogError("selected emotion for level change out of bounds");
                break;
        }
    }

    //funcs below for updating the values for upgrades, overridden by child class
    void JoyChange()
    {
        
    }

    void AngerChange()
    {
        
    }

    void SadnessChange()
    {

    }

    void LoveChange()
    {

    }

    void FearChange()
    {

    }

    //send the updated values to player, overridden by child class
    void UpdatePlayer()
    {

    }
}
