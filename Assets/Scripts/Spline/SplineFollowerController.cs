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

    [SerializeField] private Button playBtn, quitBtn;
    
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
        playBtn.interactable = false;
        quitBtn.interactable = false;
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
        onSequenceComplete?.Invoke();
    }
}
