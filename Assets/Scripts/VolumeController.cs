using UnityEngine;
using UnityEngine.UI;

// Audio controls only: music, toggle sound and mute.
// Panel show/hide lives in MenuController.
// Field names match the existing scene references, so keep them.
public class VolumeController : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider SFXSlider;
    [SerializeField] AudioSource music;
    [SerializeField] AudioSource UISound;     // Shared with MenuController.
    [SerializeField] AudioClip toggleClip;    // Plays on music and mute toggles.

    private void Start()
    {
        // Start each slider at its own source's volume.
        if (music != null) musicSlider.value = music.volume;
        if (UISound != null) SFXSlider.value = UISound.volume;
    }

    // Play/stop button for background music.
    public void OnPlayMusic()
    {
        PlayToggleSound();

        if (music == null) return;

        if (music.isPlaying)
            music.Stop();
        else
            music.Play();
    }

    // Hook to the Music slider (On Value Changed).
    public void OnMusicSliderChange()
    {
        if (music != null) music.volume = musicSlider.value;
    }

    // Hook to the SFX slider (On Value Changed).
    public void OnSFXSliderChange()
    {
        if (UISound != null) UISound.volume = SFXSlider.value;
    }

    // Mute button: pauses or resumes all audio.
    public void OnSoundToggle()
    {
        PlayToggleSound();
        AudioListener.pause = !AudioListener.pause;
    }

    private void PlayToggleSound()
    {
        if (UISound != null && toggleClip != null)
        {
            UISound.PlayOneShot(toggleClip);
        }
    }
}
