using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Scene sound controls: sliders, mute buttons and buttons. This is per scene.
// The sound itself lives in AudioManager, which persists across scenes.
// Field names match the existing scene references, so keep them.
public class VolumeController : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider SFXSlider;
    [SerializeField] AudioMixer mixer;     // MainMixer asset.

    [Header("Mute icons")]
    [SerializeField] Image musicMute;      // Image on the music mute button.
    [SerializeField] Image sfxMute;        // Image on the SFX mute button.
    [SerializeField] Sprite normalSprite;  // Speaker with sound.
    [SerializeField] Sprite muteSprite;    // Speaker with a cross.

    float savedMusicVolume = -1f;
    float savedSFXVolume = -1f;

    private void OnEnable()
    {
        // Each time the panel opens, show the volume the mixer already has.
        // This keeps the sliders matching the main menu after a scene change.
        musicSlider.SetValueWithoutNotify(GetLinearVolume("MusicVolume"));
        SFXSlider.SetValueWithoutNotify(GetLinearVolume("SFXVolume"));
    }

    private void Update()
    {
        // Icons follow the sliders, so dragging to 0 also changes the icon.
        UpdateIcon(musicMute, musicSlider);
        UpdateIcon(sfxMute, SFXSlider);
    }

    // Reads a mixer parameter in decibels and returns it as a 0-1 slider value.
    private float GetLinearVolume(string parameter)
    {
        if (mixer == null || !mixer.GetFloat(parameter, out float db)) return 1f;
        return Mathf.Pow(10f, db / 20f);
    }

    // Music play/stop button.
    public void OnPlayMusic()
    {
        if (AudioManager.instance != null) AudioManager.instance.ToggleMusic();
    }

    // Music mute button (On Click). Mutes to 0, or restores the saved volume.
    public void ToggleMusicMute()
    {
        ToggleMute(musicSlider, ref savedMusicVolume);
    }

    // SFX mute button (On Click). Mutes to 0, or restores the saved volume.
    public void ToggleSFXMute()
    {
        ToggleMute(SFXSlider, ref savedSFXVolume);
    }

    // Hook to the Music slider (On Value Changed).
    public void OnMusicSliderChange()
    {
        SetMixerVolume("MusicVolume", musicSlider.value);
    }

    // Hook to the SFX slider (On Value Changed).
    public void OnSFXSliderChange()
    {
        SetMixerVolume("SFXVolume", SFXSlider.value);
    }

    // Mute button: pauses or resumes all audio.
    public void OnSoundToggle()
    {
        AudioListener.pause = !AudioListener.pause;
    }

    // Button click sound from AudioManager's Click entry. Works in any scene.
    public void PlayClick()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlayClick();
    }
    private void ToggleMute(Slider slider, ref float savedVolume)
    {
        if (slider == null) return;

        if (IsMuted(slider))
        {
            // Setting the value fires On Value Changed, so the mixer updates too.
            slider.value = savedVolume > 0f ? savedVolume : 1f;
        }
        else
        {
            savedVolume = slider.value;
            slider.value = 0f;
        }
    }

    private void UpdateIcon(Image icon, Slider slider)
    {
        if (icon == null || slider == null) return;

        Sprite wanted = IsMuted(slider) ? muteSprite : normalSprite;
        if (wanted != null && icon.sprite != wanted)
        {
            icon.sprite = wanted;
        }
    }

    private bool IsMuted(Slider slider)
    {
        return slider.value <= 0.0001f;
    }

    private void SetMixerVolume(string parameter, float linear)
    {
        if (mixer == null) return;

        // The mixer uses decibels. Clamp to avoid log of zero.
        float db = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        mixer.SetFloat(parameter, db);
    }
}
