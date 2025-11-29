using Unity.Behavior;
using UnityEngine;

public class PrintCompositeMethods : MonoBehaviour
{
    private void Start()
    {
        var type = typeof(Composite);
        foreach (var m in type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            Debug.Log(m.Name);
        }
    }
}
