using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Lock : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] List<GameObject> linkedLocks = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Key"))
        {
            
            if (other.GetComponent<Grabbable2D>().ID == id)
            {
                foreach (var locks in linkedLocks)
                {
                    locks.gameObject.SetActive(false);
                }
                
                other.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }
}
