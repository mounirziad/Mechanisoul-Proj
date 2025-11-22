using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
public class AudioLibraryManager : ScriptableObject
{
    [Header("Combat Sounds")]
    public AudioClip[] lightAttackSounds;
    public AudioClip[] heavyAttackSounds;
    public AudioClip[] criticalHitSounds;
    public AudioClip[] blockSounds;
    public AudioClip[] dodgeSounds;
    
    [Header("Movement Sounds")]
    public AudioClip[] footstepSounds;
    public AudioClip[] jumpSounds;
    public AudioClip[] landingSounds;
    
    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;
    
    public AudioClip GetRandomSound(AudioClip[] soundArray)
    {
        if (soundArray == null || soundArray.Length == 0)
            return null;
            
        return soundArray[Random.Range(0, soundArray.Length)];
    }
    
    public AudioClip GetLightAttackSound() => GetRandomSound(lightAttackSounds);
    public AudioClip GetHeavyAttackSound() => GetRandomSound(heavyAttackSounds);
    public AudioClip GetCriticalHitSound() => GetRandomSound(criticalHitSounds);
    public AudioClip GetBlockSound() => GetRandomSound(blockSounds);
    public AudioClip GetDodgeSound() => GetRandomSound(dodgeSounds);
    public AudioClip GetFootstepSound() => GetRandomSound(footstepSounds);
    public AudioClip GetJumpSound() => GetRandomSound(jumpSounds);
    public AudioClip GetLandingSound() => GetRandomSound(landingSounds);
}