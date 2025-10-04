using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class EnhancedSoundManager : MonoBehaviour
{
    public static EnhancedSoundManager Instance;
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup voiceMixerGroup;
    
    [Header("Audio Libraries")]
    [SerializeField] private AudioLibraryManager audioLibrary;
    
    [Header("Dynamic Audio Sources")]
    [SerializeField] private int maxAudioSources = 10;
    private AudioSource[] audioSourcePool;
    private int currentAudioSourceIndex = 0;
    
    [Header("3D Audio Settings")]
    [SerializeField] private float maxAudioDistance = 50f;
    [SerializeField] private AnimationCurve distanceRolloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSourcePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudioSourcePool()
    {
        audioSourcePool = new AudioSource[maxAudioSources];
        
        for (int i = 0; i < maxAudioSources; i++)
        {
            GameObject audioObject = new GameObject($"AudioSource_{i}");
            audioObject.transform.SetParent(transform);
            audioSourcePool[i] = audioObject.AddComponent<AudioSource>();
            audioSourcePool[i].playOnAwake = false;
        }
    }
    
    public void PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetNextAudioSource();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.outputAudioMixerGroup = sfxMixerGroup;
        
        if (position != default)
        {
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D audio
            source.maxDistance = maxAudioDistance;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, distanceRolloff);
        }
        else
        {
            source.spatialBlend = 0f; // 2D audio
        }
        
        source.Play();
        
        // Return audio source to pool after clip finishes
        StartCoroutine(ReturnAudioSourceToPool(source, clip.length));
    }
    
    public void PlaySFXWithRandomPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f, Vector3 position = default)
    {
        float randomPitch = Random.Range(minPitch, maxPitch);
        PlaySFX(clip, position, 1f, randomPitch);
    }
    
    public void PlayCombatSound(string soundType, Vector3 position = default)
    {
        if (audioLibrary == null) return;
        
        AudioClip clip = soundType.ToLower() switch
        {
            "lightattack" => audioLibrary.GetLightAttackSound(),
            "heavyattack" => audioLibrary.GetHeavyAttackSound(),
            "criticalhit" => audioLibrary.GetCriticalHitSound(),
            "block" => audioLibrary.GetBlockSound(),
            "dodge" => audioLibrary.GetDodgeSound(),
            "footstep" => audioLibrary.GetFootstepSound(),
            "jump" => audioLibrary.GetJumpSound(),
            "landing" => audioLibrary.GetLandingSound(),
            _ => null
        };
        
        if (clip != null)
        {
            PlaySFXWithRandomPitch(clip, 0.95f, 1.05f, position);
        }
    }
    
    private AudioSource GetNextAudioSource()
    {
        AudioSource source = audioSourcePool[currentAudioSourceIndex];
        currentAudioSourceIndex = (currentAudioSourceIndex + 1) % maxAudioSources;
        return source;
    }
    
    private IEnumerator ReturnAudioSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (source != null)
        {
            source.Stop();
            source.clip = null;
            source.transform.position = transform.position;
            source.spatialBlend = 0f;
        }
    }
    
    public void SetMixerVolume(string parameterName, float volume)
    {
        if (sfxMixerGroup != null && sfxMixerGroup.audioMixer != null)
        {
            float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            sfxMixerGroup.audioMixer.SetFloat(parameterName, dbValue);
        }
    }
}