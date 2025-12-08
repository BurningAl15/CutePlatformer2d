using UnityEngine;
using UnityEngine.Events;

public class CharacterOrbCollectable : CollectableBase
{
    [Header("Character Orb Specific")]
    [SerializeField] private string characterID;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] UnityEvent onCollect;
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        if (visualEffect != null)
        {
            visualEffect.enablePulse = true;
            visualEffect.pulseSpeed = 2f;
            visualEffect.pulseAmount = 0.2f;
        }
    }

    protected override void OnBeforeCollect()
    {
        base.OnBeforeCollect();
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
        
        UnlockCharacter();
    }

    private void UnlockCharacter()
    {
        Debug.Log($"[CharacterOrbCollectable] Character unlocked: {characterID}");
        onCollect?.Invoke();
    }
}