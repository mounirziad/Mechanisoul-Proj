using UnityEngine;

public class SoundCycleOnHit : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip[] hitSounds;      // Sounds to cycle through
    public AudioSource audioSource;    // Audio source to play sounds

    private int currentSoundIndex = 0;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if hit by weapon (tag can be set to "Weapon")
        if (other.CompareTag("Weapon"))
        {
            PlayNextSound();
        }
    }

    private void PlayNextSound()
    {
        if (hitSounds == null || hitSounds.Length == 0) return;

        audioSource.PlayOneShot(hitSounds[currentSoundIndex]);

        currentSoundIndex++;
        if (currentSoundIndex >= hitSounds.Length)
        {
            currentSoundIndex = 0; // Loop back to start
        }
    }
}
