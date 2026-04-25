using UnityEngine;

public class RoverAudioSystem : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmSource; // For the radio
    public AudioSource asmrSource; // For tire tracks/dust sounds

    [Header("BGM Settings")]
    [Tooltip("If true, starts playing the assigned clip immediately.")]
    public bool playOnAwake = true;

    [Header("ASMR Settings (For Later)")]
    public Rigidbody rb;
    public float maxAsmrVolume = 0.5f;
    public float asmrFadeSpeed = 2f;

    private void Start()
    {
        if (bgmSource != null && playOnAwake)
        {
            bgmSource.Play();
        }
    }

    private void Update()
    {
        // ASMR logic will be expanded here when we have the tire sound effects.
        if (asmrSource != null && rb != null && asmrSource.clip != null)
        {
            float speed = rb.linearVelocity.magnitude;
            float targetVolume = speed > 0.2f ? maxAsmrVolume : 0f;
            asmrSource.volume = Mathf.Lerp(asmrSource.volume, targetVolume, Time.deltaTime * asmrFadeSpeed);
        }
    }

    // Public method to be called when discovering a new cassette tape
    public void PlayTrack(AudioClip track)
    {
        if (bgmSource != null)
        {
            bgmSource.clip = track;
            bgmSource.Play();
        }
    }
}
