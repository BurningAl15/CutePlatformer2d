using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class SoundEffect
    {
        public string soundName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
    }

    public SoundEffect[] soundEffects;
    public AudioClip backgroundMusic;

    public SoundEffect GetSound(string soundName)
    {
        foreach (SoundEffect sound in soundEffects)
        {
            if (sound.soundName == soundName)
            {
                return sound;
            }
        }
        return null;
    }
}