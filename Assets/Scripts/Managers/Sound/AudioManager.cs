using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;

    [Header("Scene Music Configuration")]
    [SerializeField] private SceneMusicData sceneMusicData;

    [Header("Fallback Music")]
    [SerializeField] private AudioClip defaultBackgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioClip victorySound;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Crossfade Settings")]
    [SerializeField] private float crossfadeDuration = 2f;
    [SerializeField] private bool crossfadeOnSceneChange = true;

    private AudioSource currentMusicSource;
    private AudioSource nextMusicSource;
    private Coroutine crossfadeCoroutine;
    private string currentSceneName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        PlayMusicForCurrentScene(false);
    }

    private void InitializeAudioSources()
    {
        if (musicSourceA == null)
        {
            musicSourceA = gameObject.AddComponent<AudioSource>();
            musicSourceA.loop = true;
            musicSourceA.playOnAwake = false;
        }

        if (musicSourceB == null)
        {
            musicSourceB = gameObject.AddComponent<AudioSource>();
            musicSourceB.loop = true;
            musicSourceB.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        currentMusicSource = musicSourceA;
        nextMusicSource = musicSourceB;

        UpdateVolumes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = scene.name;

        if (newSceneName != currentSceneName)
        {
            Debug.Log($"[AudioManager] Scene changed: {currentSceneName} → {newSceneName}");
            currentSceneName = newSceneName;
            PlayMusicForCurrentScene(crossfadeOnSceneChange);
        }
    }

    private void PlayMusicForCurrentScene(bool useCrossfade)
    {
        AudioClip newClip = null;
        float targetVolume = musicVolume;

        if (sceneMusicData != null)
        {
            newClip = sceneMusicData.GetMusicForScene(currentSceneName);
            targetVolume = sceneMusicData.GetVolumeForScene(currentSceneName);
        }

        if (newClip == null)
        {
            newClip = defaultBackgroundMusic;
        }

        if (newClip == null)
        {
            Debug.LogWarning($"[AudioManager] No music configured for scene: {currentSceneName}");
            return;
        }

        if (currentMusicSource.clip == newClip && currentMusicSource.isPlaying)
        {
            Debug.Log($"[AudioManager] Same music already playing: {newClip.name}");
            return;
        }

        if (useCrossfade && currentMusicSource.isPlaying)
        {
            CrossfadeToMusic(newClip, targetVolume);
        }
        else
        {
            PlayMusicImmediate(newClip, targetVolume);
        }
    }

    private void PlayMusicImmediate(AudioClip clip, float volume)
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        currentMusicSource.Stop();
        nextMusicSource.Stop();

        currentMusicSource.clip = clip;
        currentMusicSource.volume = volume;
        currentMusicSource.Play();

        Debug.Log($"[AudioManager] Playing music immediately: {clip.name}");
    }

    private void CrossfadeToMusic(AudioClip newClip, float targetVolume)
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }

        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip, targetVolume));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip, float targetVolume)
    {
        Debug.Log($"[AudioManager] Crossfading: {currentMusicSource.clip?.name} → {newClip.name} ({crossfadeDuration}s)");

        nextMusicSource.clip = newClip;
        nextMusicSource.volume = 0f;
        nextMusicSource.Play();

        float elapsed = 0f;
        float startVolumeA = currentMusicSource.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeDuration;

            currentMusicSource.volume = Mathf.Lerp(startVolumeA, 0f, t);
            nextMusicSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        currentMusicSource.volume = 0f;
        currentMusicSource.Stop();

        nextMusicSource.volume = targetVolume;

        AudioSource temp = currentMusicSource;
        currentMusicSource = nextMusicSource;
        nextMusicSource = temp;

        crossfadeCoroutine = null;

        Debug.Log($"[AudioManager] Crossfade complete: Now playing {newClip.name}");
    }

    public void PlayBackgroundMusic()
    {
        PlayMusicForCurrentScene(false);
    }

    public void PlayMusic(AudioClip clip, bool useCrossfade = true)
    {
        if (clip == null) return;

        if (useCrossfade && currentMusicSource.isPlaying)
        {
            CrossfadeToMusic(clip, musicVolume);
        }
        else
        {
            PlayMusicImmediate(clip, musicVolume);
        }
    }

    public void StopMusic(bool useFade = true)
    {
        if (useFade)
        {
            StartCoroutine(FadeOutMusic(crossfadeDuration));
        }
        else
        {
            currentMusicSource.Stop();
            nextMusicSource.Stop();
        }
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = currentMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentMusicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        currentMusicSource.Stop();
        currentMusicSource.volume = musicVolume;
    }

    public void PlaySFX(string soundName)
    {
        AudioClip clip = GetClipByName(soundName);
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX not found: {soundName}");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
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

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateVolumes()
    {
        if (musicSourceA != null)
        {
            musicSourceA.volume = musicVolume;
        }

        if (musicSourceB != null)
        {
            musicSourceB.volume = musicVolume;
        }
    }
}
