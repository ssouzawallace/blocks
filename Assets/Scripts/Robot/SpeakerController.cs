using UnityEngine;

/// <summary>
/// Controls an audio speaker on the robot.
/// Attach to a GameObject with an AudioSource component.
/// Provides methods to play sounds, beeps, and manage audio playback.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SpeakerController : MonoBehaviour
{
    [Tooltip("Default beep sound clip to play when no specific clip is provided.")]
    [SerializeField] private AudioClip defaultBeepClip;

    [Tooltip("Volume level from 0 to 1.")]
    [SerializeField] [Range(0f, 1f)] private float volume = 0.8f;

    private AudioSource audioSource;

    /// <summary>
    /// Returns whether the speaker is currently playing audio.
    /// </summary>
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    /// <summary>
    /// Plays the default beep sound.
    /// </summary>
    public void Beep()
    {
        PlayClip(defaultBeepClip);
    }

    /// <summary>
    /// Plays a specific audio clip through the speaker.
    /// </summary>
    public void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    /// <summary>
    /// Stops any currently playing audio.
    /// </summary>
    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Sets the speaker volume.
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Generates and plays a tone at the specified frequency for a given duration.
    /// </summary>
    public void PlayTone(float frequency, float duration)
    {
        if (audioSource == null)
            return;

        int sampleRate = AudioSettings.outputSampleRate;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        AudioClip toneClip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate);
        }
        toneClip.SetData(samples, 0);

        PlayClip(toneClip);
    }
}
