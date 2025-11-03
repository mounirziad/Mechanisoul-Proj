using UnityEngine;

[RequireComponent(typeof(Collider))]

public class Terminal : MonoBehaviour
{
    [SerializeField] bool inRange;

    private void Awake() => inRange = false;

    private void Update()
    {
        if (inRange)
        {
            //show visual ui for terminal "press e to open terminal"
            //Debug.Log("in terminal range");

            //if interact key is pressed, open terminal
        }
    }
    private void OnTriggerEnter(Collider other) => inRange = other.CompareTag("Player") ? true : false;
    private void OnTriggerExit(Collider other) => inRange = other.CompareTag("Player") ? false : true;
}
