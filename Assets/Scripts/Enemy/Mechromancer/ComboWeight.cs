using UnityEngine;

public class ComboWeight : MonoBehaviour, IWeightedAttack
{
    public float closeWeight = 70f;
    public float midWeight = 0f;
    public float farWeight = 0f;

    public float GetWeight(float distance)
    {
        if (distance <= 11f) return closeWeight;
        return 0;
    }
}
