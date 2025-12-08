using System.Collections.Generic;
using UnityEngine;

public class CollectableUITarget : MonoBehaviour
{
    private static Dictionary<string, CollectableUITarget> targets = new Dictionary<string, CollectableUITarget>();

    [Header("Configuration")]
    public string targetID = "default";
    public Transform targetTransform;

    private void Awake()
    {
        if (targetTransform == null)
        {
            targetTransform = transform;
        }

        if (!targets.ContainsKey(targetID))
        {
            targets[targetID] = this;
        }
        else
        {
            Debug.LogWarning($"[CollectableUITarget] Duplicate target ID: {targetID}");
        }
    }

    private void OnDestroy()
    {
        if (targets.ContainsKey(targetID) && targets[targetID] == this)
        {
            targets.Remove(targetID);
        }
    }

    public static CollectableUITarget GetTarget(string id)
    {
        if (targets.TryGetValue(id, out CollectableUITarget target))
        {
            return target;
        }

        if (targets.TryGetValue("default", out CollectableUITarget defaultTarget))
        {
            return defaultTarget;
        }

        Debug.LogWarning($"[CollectableUITarget] Target not found: {id}");
        return null;
    }

    public static void ClearTargets()
    {
        targets.Clear();
    }
}