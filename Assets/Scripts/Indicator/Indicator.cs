using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Indicator : MonoBehaviour
{
    [SerializeField] bool isEnabled = false;
    [SerializeField] private Animator anim;
    [SerializeField] Transform target;
    [SerializeField] Vector3 initialPos, targetPos;
    
    private void Awake()
    {
        anim.enabled = false;
        target.localPosition = initialPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isEnabled)
            {
                StartCoroutine(SequenceAndWait_ScaleUp());
                isEnabled = true;
            }
        }
    }

    IEnumerator SequenceAndWait_ScaleUp()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOScale(1f, 0.2f));
        seq.Join(target.DOLocalMove(targetPos, 0.2f));
        yield return seq.WaitForCompletion();
        
        anim.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isEnabled)
            {
                StartCoroutine(SequenceAndWait_ScaleDown());
                isEnabled = false;
            }
        }
    }
    
    IEnumerator SequenceAndWait_ScaleDown()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOScale(0f, 0.1f));
        seq.Join(target.DOLocalMove(initialPos, 0.1f));
        
        yield return seq.WaitForCompletion();
        
        anim.enabled = false;
    }
}
