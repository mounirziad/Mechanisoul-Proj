using UnityEngine;

public static class RangedDebug
{
    public static bool logs = true;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(string msg)
    {
        if (logs) Debug.Log(msg);
    }
}
