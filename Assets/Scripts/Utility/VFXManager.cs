using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Hit VFX Prefabs")]
    public GameObject defaultSparkVFX;
    public GameObject metalSparkVFX;
    public GameObject fleshHitVFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameObject SpawnHitVFX(GameObject vfxPrefab, Vector3 position, Vector3 direction, float destroyDelay = 3f)
    {
        if (vfxPrefab == null) return null;

        Quaternion rotation = Quaternion.LookRotation(-direction);
        GameObject vfxInstance = Instantiate(vfxPrefab, position, rotation);
        
        ParticleSystem particles = vfxInstance.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }
        
        Destroy(vfxInstance, destroyDelay);
        return vfxInstance;
    }

    public static GameObject SpawnDefaultSparkVFX(Vector3 position, Vector3 direction)
    {
        if (Instance != null && Instance.defaultSparkVFX != null)
        {
            return SpawnHitVFX(Instance.defaultSparkVFX, position, direction);
        }
        return null;
    }
}