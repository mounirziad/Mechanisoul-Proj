using UnityEngine;

public class BreakRoomDoor : MonoBehaviour
{
    [SerializeField] private GameObject doorToOpen;
    [SerializeField] private bool showDebugLogs = true;

    void Start()
    {
        OpenDoor();
    }
    
    public void OpenDoor()
    {
        if (doorToOpen != null)
        {
            Animator doorAnimator = doorToOpen.GetComponent<Animator>();
            if (doorAnimator != null)
            {
                doorAnimator.SetBool("IsDoorOpen", true);
                doorToOpen.GetComponent<DoorOutline>().EnableOutline();

                if (showDebugLogs)
                {
                    Debug.Log($"Door animation triggered: {doorToOpen.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No Animator found on door: {doorToOpen.name}");
            }
        }
    }
}
