using UnityEngine;

public class StarCollectable : CollectableBase
{
    [Header("Star Specific")]
    [SerializeField] private ParticleSystem collectParticles;
    
    protected override void OnBeforeCollect()
    {
        base.OnBeforeCollect();
        
        if (collectParticles != null)
        {
            collectParticles.Play();
        }
    }

    protected override void OnAfterCollect()
    {
        base.OnAfterCollect();
        
        Debug.Log($"[StarCollectable] {name} collected!");
    }
}