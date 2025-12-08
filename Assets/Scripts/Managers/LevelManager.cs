using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [System.Serializable]
    public class CollectableCounter
    {
        public string collectableType;
        public int total;
        public int collected;

        public CollectableCounter(string type)
        {
            collectableType = type;
            total = 0;
            collected = 0;
        }
    }

    private Dictionary<string, CollectableCounter> counters = new Dictionary<string, CollectableCounter>();
    
    private int totalStars;
    private int collectedStars;
    
    public int TotalStars => totalStars;
    public int CollectedStars => collectedStars;
    public bool AllStarsCollected => collectedStars >= totalStars;

    [System.Obsolete("Use TotalStars instead")]
    public int TotalCollectables => totalStars;
    
    [System.Obsolete("Use CollectedStars instead")]
    public int CollectablesCollected => collectedStars;
    
    [System.Obsolete("Use AllStarsCollected instead")]
    public bool AllCollectablesCollected => collectedStars >= totalStars;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RegisterAllCollectables();
    }

    private void RegisterAllCollectables()
    {
        counters.Clear();
        
        Collectable[] oldCollectables = FindObjectsByType<Collectable>(FindObjectsSortMode.None);
        foreach (var collectable in oldCollectables)
        {
            RegisterCollectableByType("star");
        }
        
        CollectableBase[] newCollectables = FindObjectsByType<CollectableBase>(FindObjectsSortMode.None);
        foreach (var collectable in newCollectables)
        {
            string typeID = GetCollectableTypeID(collectable);
            RegisterCollectableByType(typeID);
        }
        
        totalStars = GetTotalByType("star");
        collectedStars = 0;

        UpdateUI();
        
        LogCollectablesSummary();
    }

    private string GetCollectableTypeID(CollectableBase collectable)
    {
        if (collectable is StarCollectable)
            return "star";
        else if (collectable is CharacterOrbCollectable)
            return "orb";
        else if (collectable is BadgeCollectable)
            return "badge";
        else
            return "unknown";
    }

    private void RegisterCollectableByType(string typeID)
    {
        if (!counters.ContainsKey(typeID))
        {
            counters[typeID] = new CollectableCounter(typeID);
        }
        
        counters[typeID].total++;
    }

    public void CollectItem(string collectableTypeID)
    {
        if (!counters.ContainsKey(collectableTypeID))
        {
            Debug.LogWarning($"[LevelManager] Unknown collectable type: {collectableTypeID}");
            return;
        }

        counters[collectableTypeID].collected++;

        if (collectableTypeID == "star")
        {
            collectedStars++;
            UpdateUI();
        }

        LogCollection(collectableTypeID);
    }

    public int GetTotalByType(string typeID)
    {
        return counters.ContainsKey(typeID) ? counters[typeID].total : 0;
    }

    public int GetCollectedByType(string typeID)
    {
        return counters.ContainsKey(typeID) ? counters[typeID].collected : 0;
    }

    private void UpdateUI()
    {
        if (CollectableUI.instance != null)
        {
            CollectableUI.instance.UpdateDisplay(collectedStars, totalStars);
        }
    }

    private void LogCollectablesSummary()
    {
        Debug.Log("=== [LevelManager] Collectables Summary ===");
        foreach (var kvp in counters)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value.total} total");
        }
    }

    private void LogCollection(string typeID)
    {
        if (counters.ContainsKey(typeID))
        {
            var counter = counters[typeID];
            Debug.Log($"[LevelManager] {typeID} collected: {counter.collected}/{counter.total}");
        }
    }

    public void RegisterCollectable(MonoBehaviour collectable)
    {
    }
}

