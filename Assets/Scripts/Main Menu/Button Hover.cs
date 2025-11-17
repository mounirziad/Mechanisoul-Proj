using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MainMenuPlayer mainMenuPlayer;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mainMenuPlayer != null)
        {
            StopAllCoroutines();
            StartCoroutine(mainMenuPlayer.TurnOn());
        }
        Debug.Log("Button Hovered");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (mainMenuPlayer != null)
        {
            StopAllCoroutines();
            StartCoroutine(mainMenuPlayer.TurnOff());
        }
        Debug.Log("Button Hover Exit");
    }
}
