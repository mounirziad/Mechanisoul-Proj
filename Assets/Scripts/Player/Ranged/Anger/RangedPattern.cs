using UnityEngine;

// helper to generate shotgun directions; use in PlayerCombat when anger is active
public static class RangedPattern
{
    public static Vector3[] GetPelletDirections(
        Vector3 forward, Vector3 right, Vector3 up, int pelletCount, float spreadAngleDegrees)
    {
        if (pelletCount <= 1) return new Vector3[] { forward.normalized };
        Vector3[] dirs = new Vector3[pelletCount];
        for (int i = 0; i < pelletCount; i++)
        {
            float yaw = Random.Range(-spreadAngleDegrees, spreadAngleDegrees);
            float pitch = Random.Range(-spreadAngleDegrees, spreadAngleDegrees);
            Quaternion rot = Quaternion.AngleAxis(yaw, up) * Quaternion.AngleAxis(pitch, right);
            dirs[i] = (rot * forward).normalized;
        }
        return dirs;
    }
}
