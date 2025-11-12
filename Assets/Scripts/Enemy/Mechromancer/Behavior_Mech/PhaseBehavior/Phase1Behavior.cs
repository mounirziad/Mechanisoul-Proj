using UnityEngine;

public class Phase1Behavior : MonoBehaviour
{
    private MechBehaviorController controller;

    private void Start()
    {
        controller = GetComponent<MechBehaviorController>();
        //controller.OnPhase1 += EnterPhase;
    }

    private void OnDestroy()
    {
        //controller.OnPhase1 -= EnterPhase;
    }

    private void EnterPhase()
    {
        Debug.Log("Starting mech phase 1");
        //set up attacks, speed, cooldowns
        //controller.SetNavSpeed(3); example of changing speed in nav to player
    }
}
