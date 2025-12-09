using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class CollectableBase : MonoBehaviour, ICollectable
{
    [Header("Configuration")]
    [SerializeField] protected CollectableData data;
    [SerializeField] protected bool autoRegister = true;
    
    [Header("Components")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Collider2D collectableCollider;
    [SerializeField] protected CollectableVisualEffect visualEffect;
    [SerializeField] protected CollectableMoveToUIEffect moveToUIEffect;
    
    protected bool isCollected = false;

    public bool IsCollected => isCollected;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (collectableCollider == null)
            collectableCollider = GetComponent<Collider2D>();
        
        if (visualEffect == null)
            visualEffect = GetComponent<CollectableVisualEffect>();
        
        if (moveToUIEffect == null)
            moveToUIEffect = GetComponent<CollectableMoveToUIEffect>();
    }

    protected virtual void Start()
    {
        if (autoRegister && LevelManager.instance != null)
        {
            LevelManager.instance.RegisterCollectable(this);
        }
    
        SetupVisualEffects();
        OnInitialize();
    }

    protected virtual void SetupVisualEffects()
    {
        if (visualEffect != null && data != null)
        {
            visualEffect.enableRotation = data.hasRotation;
            visualEffect.rotationSpeed = data.rotationSpeed;
            visualEffect.enableBobbing = data.hasBobbing;
            visualEffect.bobSpeed = data.bobSpeed;
            visualEffect.bobHeight = data.bobHeight;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            if (Player2DController.instance != null && Player2DController.instance.IsAbducted)
            {
                return;
            }

            Collect();
        }
    }


    public virtual void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        if (collectableCollider != null)
            collectableCollider.enabled = false;
        
        if (visualEffect != null)
            visualEffect.StopAllEffects();

        PlayCollectSound();
        
        OnBeforeCollect();
        
        StartCoroutine(CollectSequence());
    }

    protected virtual IEnumerator CollectSequence()
    {
        if (data != null && data.moveToUI && moveToUIEffect != null)
        {
            yield return MoveToUISequence();
        }
        else if (moveToUIEffect != null)
        {
            yield return moveToUIEffect.DefaultCollectAnimation();
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        NotifyCollection();
        OnAfterCollect();
        
        Destroy(gameObject);
    }

    protected virtual IEnumerator MoveToUISequence()
    {
        CollectableUITarget uiTarget = FindUITarget();

        if (uiTarget == null || uiTarget.targetTransform == null)
        {
            Debug.LogWarning($"[{name}] UI Target not found, using default animation");
            
            if (moveToUIEffect != null)
            {
                yield return moveToUIEffect.DefaultCollectAnimation();
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }
            
            yield break;
        }

        yield return moveToUIEffect.MoveToTarget(uiTarget.targetTransform);
    }

    protected virtual CollectableUITarget FindUITarget()
    {
        if (data != null && !string.IsNullOrEmpty(data.uiTargetID))
        {
            return CollectableUITarget.GetTarget(data.uiTargetID);
        }
        
        return CollectableUITarget.GetTarget("default");
    }

    protected virtual void NotifyCollection()
    {
        if (LevelManager.instance != null && data != null && data.countsForLevelCompletion)
        {
            LevelManager.instance.CollectItem(data.collectableID);
        }
    }

    protected virtual void PlayCollectSound()
    {
        if (AudioManager.instance != null && data != null && !string.IsNullOrEmpty(data.collectSoundName))
        {
            AudioManager.instance.PlaySFX(data.collectSoundName);
        }
    }

    public virtual int GetPointValue()
    {
        return data != null ? data.pointValue : 0;
    }

    protected virtual void OnInitialize() { }
    
    protected virtual void OnBeforeCollect() { }
    
    protected virtual void OnAfterCollect() { }

    protected virtual void OnDrawGizmosSelected()
    {
        if (data != null && data.hasBobbing)
        {
            Gizmos.color = Color.yellow;
            Vector3 pos = transform.position;
            Gizmos.DrawWireSphere(pos + Vector3.up * data.bobHeight, 0.1f);
            Gizmos.DrawWireSphere(pos - Vector3.up * data.bobHeight, 0.1f);
            Gizmos.DrawLine(pos + Vector3.up * data.bobHeight, pos - Vector3.up * data.bobHeight);
        }
    }
}
