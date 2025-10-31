using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private int totalCollectablesInLevel;
    private int collectablesCollected = 0;
    private List<Collectable> allCollectables = new List<Collectable>();

    public int TotalCollectables => totalCollectablesInLevel;
    public int CollectablesCollected => collectablesCollected;
    public bool AllCollectablesCollected => collectablesCollected >= totalCollectablesInLevel;

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
        allCollectables.Clear();
        allCollectables.AddRange(FindObjectsByType<Collectable>(FindObjectsSortMode.None));
        totalCollectablesInLevel = allCollectables.Count;
        collectablesCollected = 0;

        if (CollectableUI.instance != null)
        {
            CollectableUI.instance.UpdateDisplay(collectablesCollected, totalCollectablesInLevel);
        }
    }

    public void RegisterCollectable(Collectable collectable)
    {
        if (!allCollectables.Contains(collectable))
        {
            allCollectables.Add(collectable);
            totalCollectablesInLevel = allCollectables.Count;
        }
    }

    public void CollectItem()
    {
        collectablesCollected++;

        if (CollectableUI.instance != null)
        {
            CollectableUI.instance.UpdateDisplay(collectablesCollected, totalCollectablesInLevel);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Collect");
        }
    }
}

