using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Lock : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] List<GameObject> linkedLocks = new List<GameObject>();

    private bool hasBeenUnlocked = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Key"))
        {
            if (!hasBeenUnlocked)
            {
                if (other.GetComponent<Grabbable2D>().ID == id)
                {
                    StartCoroutine(GroupSequence(other.gameObject));
                }
                hasBeenUnlocked = true;
            }
        }
    }

    private IEnumerator SequenceAndWait(Transform block)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.5f, 0.5f));
        seq.Append(transform.DOScale(0f, 0.5f));

        yield return seq.WaitForCompletion();
        
        block.gameObject.SetActive(false);

        Debug.Log("Secuencia terminada");
    }

    private IEnumerator GroupSequence(GameObject key)
    {
        foreach (var locks in linkedLocks)
        {
            StartCoroutine(SequenceAndWait(locks.transform));
            yield return new WaitForSeconds(0.1f);
        }
        key.SetActive(false);
        StartCoroutine(SequenceAndWait(gameObject.transform));
    }
}
