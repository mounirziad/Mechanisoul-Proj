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
    
    [Header("Projectile Sound Effects")]
    [SerializeField] private AudioClip[] laserShootSounds;
    [SerializeField] private AudioClip[] projectileHitSounds;
    
    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] walkFootsteps;
    [SerializeField] private AudioClip[] runFootsteps;
    [SerializeField] private AudioClip[] sprintFootsteps;
    [SerializeField] private AudioClip[] jumpLandSounds;
    
    [Header("Footstep Settings")]
    [SerializeField] private float footstepCooldown = 0.3f;
    [SerializeField] private float walkFootstepVolume = 0.4f;
    [SerializeField] private float runFootstepVolume = 0.6f;
    [SerializeField] private float sprintFootstepVolume = 0.8f;
    [SerializeField] private float footstepPitchVariation = 0.05f;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    
    [Header("Audio Settings")]
    [SerializeField] private float pitchVariation = 0.1f;
    [SerializeField] private float globalHitSoundCooldown = 0.15f;
    
    private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();
    private float lastGlobalHitTime = -999f;
    private float lastFootstepTime = -999f;

    private void Awake()
    {
        Instance = this;
        InitializeSoundLibrary();
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
        
        for (int i = 0; i < laserShootSounds.Length; i++)
        {
            if (laserShootSounds[i] != null)
                soundLibrary[$"laser_{i}"] = laserShootSounds[i];
        }
        
        for (int i = 0; i < projectileHitSounds.Length; i++)
        {
            if (projectileHitSounds[i] != null)
                soundLibrary[$"projectile_hit_{i}"] = projectileHitSounds[i];
        }
        
        for (int i = 0; i < walkFootsteps.Length; i++)
        {
            if (walkFootsteps[i] != null)
                soundLibrary[$"walk_{i}"] = walkFootsteps[i];
        }
        
        for (int i = 0; i < runFootsteps.Length; i++)
        {
            if (runFootsteps[i] != null)
                soundLibrary[$"run_{i}"] = runFootsteps[i];
        }
        
        for (int i = 0; i < sprintFootsteps.Length; i++)
        {
            if (sprintFootsteps[i] != null)
                soundLibrary[$"sprint_{i}"] = sprintFootsteps[i];
        }
        
        for (int i = 0; i < jumpLandSounds.Length; i++)
        {
            if (jumpLandSounds[i] != null)
                soundLibrary[$"land_{i}"] = jumpLandSounds[i];
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
    
    public void PlayLaserSound()
    {
        if (laserShootSounds != null && laserShootSounds.Length > 0)
        {
            AudioClip randomLaserSound = laserShootSounds[Random.Range(0, laserShootSounds.Length)];
            PlaySoundEffect(randomLaserSound);
        }
    }
    
    public void PlayProjectileHitSound()
    {
        if (projectileHitSounds != null && projectileHitSounds.Length > 0)
        {
            AudioClip randomProjectileHit = projectileHitSounds[Random.Range(0, projectileHitSounds.Length)];
            PlaySoundEffect(randomProjectileHit);
        }
    }
    
    public void PlayFootstep(float movementSpeed, bool isGrounded)
    {
        if (!isGrounded)
            return;
            
        if (Time.time - lastFootstepTime < footstepCooldown)
            return;
        
        AudioClip[] footstepArray;
        float volume;
        
        if (movementSpeed >= 6.5f)
        {
            footstepArray = sprintFootsteps;
            volume = sprintFootstepVolume;
        }
        else if (movementSpeed >= 4.5f)
        {
            footstepArray = runFootsteps;
            volume = runFootstepVolume;
        }
        else if (movementSpeed >= 0.5f)
        {
            footstepArray = walkFootsteps;
            volume = walkFootstepVolume;
        }
        else
        {
            return;
        }
        
        if (footstepArray != null && footstepArray.Length > 0)
        {
            AudioClip randomFootstep = footstepArray[Random.Range(0, footstepArray.Length)];
            if (randomFootstep != null && soundEffectSource != null)
            {
                float originalPitch = soundEffectSource.pitch;
                soundEffectSource.pitch = Random.Range(1f - footstepPitchVariation, 1f + footstepPitchVariation);
                soundEffectSource.PlayOneShot(randomFootstep, volume * sfxVolume * masterVolume);
                soundEffectSource.pitch = originalPitch;
                lastFootstepTime = Time.time;
            }
        }
    }
    
    public void PlayWalkFootstep()
    {
        PlayFootstepByType(walkFootsteps, walkFootstepVolume);
    }
    
    public void PlayRunFootstep()
    {
        PlayFootstepByType(runFootsteps, runFootstepVolume);
    }
    
    public void PlaySprintFootstep()
    {
        PlayFootstepByType(sprintFootsteps, sprintFootstepVolume);
    }
    
    public void PlayLandSound()
    {
        if (jumpLandSounds != null && jumpLandSounds.Length > 0)
        {
            AudioClip randomLandSound = jumpLandSounds[Random.Range(0, jumpLandSounds.Length)];
            if (randomLandSound != null && soundEffectSource != null)
            {
                soundEffectSource.PlayOneShot(randomLandSound, runFootstepVolume * sfxVolume * masterVolume);
            }
        }
    }
    
    private void PlayFootstepByType(AudioClip[] footstepArray, float volume)
    {
        if (Time.time - lastFootstepTime < footstepCooldown)
            return;
            
        if (footstepArray != null && footstepArray.Length > 0)
        {
            AudioClip randomFootstep = footstepArray[Random.Range(0, footstepArray.Length)];
            if (randomFootstep != null && soundEffectSource != null)
            {
                float originalPitch = soundEffectSource.pitch;
                soundEffectSource.pitch = Random.Range(1f - footstepPitchVariation, 1f + footstepPitchVariation);
                soundEffectSource.PlayOneShot(randomFootstep, volume * sfxVolume * masterVolume);
                soundEffectSource.pitch = originalPitch;
                lastFootstepTime = Time.time;
            }
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