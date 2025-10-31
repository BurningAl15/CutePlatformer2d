using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioClip victorySound;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        UpdateVolumes();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlaySFX(string soundName)
    {
        AudioClip clip = GetClipByName(soundName);
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    private AudioClip GetClipByName(string name)
    {
        switch (name.ToLower())
        {
            case "collect":
                return collectSound;
            case "damage":
                return damageSound;
            case "death":
                return deathSound;
            case "checkpoint":
                return checkpointSound;
            case "victory":
                return victorySound;
            default:
                return null;
        }
    }

    private void UpdateVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
}
