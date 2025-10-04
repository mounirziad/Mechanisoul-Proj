using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource soundEffectSource;
    [SerializeField] private AudioSource musicSource;
    
    [Header("Combat Sound Effects")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip dashSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    
    [Header("Audio Settings")]
    [SerializeField] private float pitchVariation = 0.1f;
    [SerializeField] private float globalHitSoundCooldown = 0.15f;
    
    private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();
    private float lastGlobalHitTime = -999f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundLibrary();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (soundEffectSource == null)
        {
            soundEffectSource = GetComponent<AudioSource>();
            if (soundEffectSource == null)
            {
                soundEffectSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (musicSource == null && soundEffectSource != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupAudioSources();
    }
    
    private void SetupAudioSources()
    {
        if (soundEffectSource != null)
        {
            soundEffectSource.playOnAwake = false;
            soundEffectSource.loop = false;
            soundEffectSource.volume = sfxVolume * masterVolume;
        }
        
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
        }
    }
    
    private void InitializeSoundLibrary()
    {
        if (dashSound != null)
            soundLibrary["dash"] = dashSound;
            
        for (int i = 0; i < attackSounds.Length; i++)
        {
            if (attackSounds[i] != null)
                soundLibrary[$"attack_{i}"] = attackSounds[i];
        }
        
        for (int i = 0; i < hitSounds.Length; i++)
        {
            if (hitSounds[i] != null)
                soundLibrary[$"hit_{i}"] = hitSounds[i];
        }
    }
    
    public void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0)
        {
            AudioClip randomAttackSound = attackSounds[Random.Range(0, attackSounds.Length)];
            PlaySoundEffect(randomAttackSound);
        }
    }
    
    public void PlayHitSound()
    {
        // Global cooldown check to prevent rapid-fire hit sounds
        if (Time.time - lastGlobalHitTime < globalHitSoundCooldown)
        {
            return;
        }
        
        if (hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip randomHitSound = hitSounds[Random.Range(0, hitSounds.Length)];
            PlaySoundEffect(randomHitSound);
            lastGlobalHitTime = Time.time;
        }
    }
    
    public void PlayDashSound()
    {
        if (dashSound != null)
        {
            PlaySoundEffect(dashSound);
        }
    }
    
    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null && soundEffectSource != null)
        {
            float originalPitch = soundEffectSource.pitch;
            soundEffectSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
            soundEffectSource.PlayOneShot(clip, sfxVolume * masterVolume);
            soundEffectSource.pitch = originalPitch;
        }
    }
    
    public void PlaySound(string soundName)
    {
        if (soundLibrary.ContainsKey(soundName))
        {
            PlaySoundEffect(soundLibrary[soundName]);
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in sound library!");
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }
    
    private void UpdateAudioSourceVolumes()
    {
        if (soundEffectSource != null)
            soundEffectSource.volume = sfxVolume * masterVolume;
            
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }
    
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }
}