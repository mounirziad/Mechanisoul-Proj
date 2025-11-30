using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private MainMenuPlayer mainMenuPlayer;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        TriggerTurnOn();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TriggerTurnOff();
    }

    public void OnSelect(BaseEventData eventData)
    {
        TriggerTurnOn();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        TriggerTurnOff();
    }

    private void TriggerTurnOn()
    {
        if (mainMenuPlayer != null)
        {
            StopAllCoroutines();
            StartCoroutine(mainMenuPlayer.TurnOn());
        }
        Debug.Log("Button Selected/Hovered");
    }

    private void TriggerTurnOff()
    {
        if (mainMenuPlayer != null)
        {
            StopAllCoroutines();
            StartCoroutine(mainMenuPlayer.TurnOff());
        }
        Debug.Log("Button Deselected/Exit Hover");
    }
}
