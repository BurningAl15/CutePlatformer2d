using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Events;
using UnityEngine.UI;

public class SplineFollowerController : MonoBehaviour
{
    [Header("Spline Settings")] 
    public SplineAnimate animate;
    
    [Header("Events")]
    public UnityEvent onSequenceComplete;
    
    private bool hasCompleted = false;
    private bool wasPlaying = false;

    [SerializeField] private CanvasGroup canvasGroup;
    
    void Update()
    {
        if (animate == null || hasCompleted) return;

        bool isCurrentlyPlaying = animate.IsPlaying;

        if (wasPlaying && !isCurrentlyPlaying)
        {
            hasCompleted = true;
            OnSequenceComplete();
        }

        wasPlaying = isCurrentlyPlaying;
    }

    public void StartMovement()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        if (animate == null) return;
        
        hasCompleted = false;
        wasPlaying = false;
        animate.Restart(true);
    }

    public void StopMovement()
    {
        if (animate != null)
        {
            animate.Pause();
        }
    }

    private void OnSequenceComplete()
    {
        Debug.Log("Secuencia del spline completada!");
        StartCoroutine(CanvasGroupFade());
        onSequenceComplete?.Invoke();
    }

    IEnumerator CanvasGroupFade()
    {
        canvasGroup.alpha = 1;
        float duration = .2f;
        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            canvasGroup.alpha = 1 - i/duration;
            yield return null;
        }
        canvasGroup.alpha = 0;
        yield return null;
    }
}
