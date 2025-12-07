using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectAction : MonoBehaviour
{
    private static readonly int Play = Animator.StringToHash("Play");
    [SerializeField] private Animator animator;
    [SerializeField] private float extraWaitTime = 0.1f;
    [SerializeField] private bool hasFinished = false;
    
    public void OnSplineComplete()
    {
        StartCoroutine(GoToNextScene());
    }

    IEnumerator GoToNextScene()
    {
        PlayAnimation();
        
        yield return new WaitForSeconds(0.1f);
        
        yield return new WaitUntil(() => hasFinished);
        
        yield return new WaitForSeconds(extraWaitTime);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void FinishAnimation()
    {
        hasFinished = true;
    }
    
    public void PlayAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(Play);
        }
    }
}