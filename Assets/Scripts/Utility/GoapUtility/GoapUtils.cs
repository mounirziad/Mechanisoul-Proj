using UnityEngine;

public static class GoapUtils
{
    public static bool InRangeOf(Vector3 posA, Vector3 posB, float range) 
        => Vector3.Distance(posA, posB) <= range;
}
