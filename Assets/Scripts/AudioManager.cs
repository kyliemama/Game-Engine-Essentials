using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string name;    // Name you call from code, such as Walk, Attack or Hit.
    public AudioClip clip;
}

// Music and sound effects for the whole game. Keeps playing across scenes.
// Put on a root GameObject named AudioManager. Put the audio sources on child objects.
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Sources")]
    [SerializeField] AudioSource musicSource;  // Loop on, Play On Awake off. Output: MainMixer > Music.
    [SerializeField] AudioSource sfxSource;    // Play On Awake off. Output: MainMixer > SFX. One-shots.
    [SerializeField] AudioSource loopSource;   // Loop on, Play On Awake off. Output: MainMixer > SFX. Walking, etc.

    [Header("Music clips")]
    [SerializeField] AudioClip normalMusic;    // Plays most of the time.
    [SerializeField] AudioClip enemyMusic;     // Plays while an enemy sees the player.

    [Header("Sound effect clips")]
    [SerializeField] SoundEffect[] soundEffects;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Only the copy that survived starts the music.
        if (instance != this) return;

        if (musicSource != null && normalMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = normalMusic;
            musicSource.Play();
        }
    }

    public void PlayNormalMusic()
    {
        SwapMusic(normalMusic);
    }

    public void PlayEnemyMusic()
    {
        SwapMusic(enemyMusic);
    }

    // Starts or stops the background music.
    public void ToggleMusic()
    {
        if (musicSource == null) return;

        if (musicSource.isPlaying)
            musicSource.Stop();
        else
            musicSource.Play();
    }

    // Plays a named sound effect once, such as "Attack".
    public void PlaySFX(string soundName)
    {
        PlayClip(FindSound(soundName));
    }

    // Button click sound. Uses the Sound Effect Clips entry named Click.
    public void PlayClick()
    {
        PlaySFX("Click");
    }

    // Plays a clip once. Used by buttons that pass a clip in the Inspector.
    public void PlayClip(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Loops a named sound, such as "Walk". Pass an empty name to stop it.
    public void SetLoop(string soundName)
    {
        if (loopSource == null) return;

        if (string.IsNullOrEmpty(soundName))
        {
            loopSource.Stop();
            return;
        }

        AudioClip clip = FindSound(soundName);
        if (clip == null) return;
        if (loopSource.clip == clip && loopSource.isPlaying) return;

        loopSource.clip = clip;
        loopSource.Play();
    }

    private AudioClip FindSound(string soundName)
    {
        foreach (SoundEffect effect in soundEffects)
        {
            if (effect.name == soundName) return effect.clip;
        }

        Debug.LogWarning("No sound effect named " + soundName + " in AudioManager.");
        return null;
    }

    private void SwapMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null || musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }
}
