using UnityEngine;

[CreateAssetMenu(fileName = "SceneMusicData", menuName = "Audio/Scene Music Data")]
public class SceneMusicData : ScriptableObject
{
    [System.Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public AudioClip musicClip;
        [Range(0f, 1f)] public float volume = 0.7f;
    }

    public SceneMusicEntry[] sceneMusicEntries;

    public AudioClip GetMusicForScene(string sceneName)
    {
        foreach (var entry in sceneMusicEntries)
        {
            if (entry.sceneName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry.musicClip;
            }
        }

        return null;
    }

    public float GetVolumeForScene(string sceneName)
    {
        foreach (var entry in sceneMusicEntries)
        {
            if (entry.sceneName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry.volume;
            }
        }

        return 0.7f;
    }
}