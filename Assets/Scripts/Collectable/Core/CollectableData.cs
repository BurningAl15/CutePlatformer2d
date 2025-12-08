using UnityEngine;

[CreateAssetMenu(fileName = "CollectableData", menuName = "Game/Collectable Data")]
public class CollectableData : ScriptableObject
{
    [Header("Identification")]
    public string collectableID = "star";
    public string displayName;

    [Header("Gameplay")]
    public int pointValue = 1;
    public bool countsForLevelCompletion = true;
    
    [Header("Visual Effects")]
    public bool hasRotation = true;
    public float rotationSpeed = 100f;
    
    public bool hasBobbing = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    
    [Header("Collection Animation")]
    public bool moveToUI = true;
    public float moveToUIDuration = 0.8f;
    public bool scaleOnMove = true;
    public float targetScale = 0.5f;
    
    [Header("Audio")]
    public string collectSoundName = "Collect";
    
    [Header("UI")]
    public Sprite uiIcon;
    public string uiTargetID = "default";
}