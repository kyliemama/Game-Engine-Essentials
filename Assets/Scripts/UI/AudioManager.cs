using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string name;    
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Sources")]
    [SerializeField] AudioSource musicSource; 
    [SerializeField] AudioSource sfxSource;   
    [SerializeField] AudioSource loopSource;   

    [Header("Music clips")]
    [SerializeField] AudioClip normalMusic;    
    [SerializeField] AudioClip enemyMusic;    

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

    // mute and unmute
    public void ToggleMusic()
    {
        if (musicSource == null) return;

        if (musicSource.isPlaying)
            musicSource.Stop();
        else
            musicSource.Play();
    }

    // play sound with name
    public void PlaySFX(string soundName)
    {
        PlayClip(FindSound(soundName));
    }

    // play click
    public void PlayClick()
    {
        PlaySFX("Click");
    }

    // can choose what clip to play
    public void PlayClip(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Plays a clip on the SFX source at the given volume (0 to 1), so the SFX slider applies.
    public void PlayClip(AudioClip clip, float volume)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

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

    //battle music and normal music
    private void SwapMusic(AudioClip clip) 
    {
        if (musicSource == null || clip == null || musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }
}
