using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject creditsPanel;

    void Start()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Credits Panel is not assigned in the inspector.");
        }
    }
    public void OnPlay()
    {
        SceneManager.LoadScene("TutorialCutscene");
    }

    public void OnCredits()
    {
        if (creditsPanel != null)
        {
            if (creditsPanel.activeSelf)
                creditsPanel.SetActive(false);
            else
                creditsPanel.SetActive(true);
        }
    }

    public void OnQuit()
    {
        
    }
}
