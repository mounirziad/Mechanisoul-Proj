using System;
using UnityEngine;

[Serializable]
public class ComboWeightedAttack : MonoBehaviour, IWeightedAttack
{
    public float GetWeight(float distance)
    {
        if (distance <= 11f)
        {
            return 0.7f; //70% weight
        }

        return 0f;
    }
}

[Serializable]
public class LungeWeightedAttack : MonoBehaviour, IWeightedAttack
{
    public float GetWeight(float distance)
    {
        if (distance <= 11f)
        {
            return 0.3f; //close range: 30%
        }

        if (distance > 11f && distance < 18f)
        {
            return 0.65f; //mid range: 65%
        }

        if (distance >= 18f)
        {
            return 0.2f; //far range: 20%
        }

        return 0f;
    }
}

[Serializable]
public class LightningWeightedAttack : MonoBehaviour, IWeightedAttack
{
    public float GetWeight(float distance)
    {
        if (distance > 11f && distance < 18f)
        {
            return 0.35f; //mid range: 35%
        }

        if (distance >= 18f)
        {
            return 0.8f; //far range: 80%
        }

        return 0f;
    }
}
