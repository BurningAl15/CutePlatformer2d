using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.instance != null)
        {
            if(particleSystem != null)
                particleSystem.Play();
            
            StartCoroutine(GameManager.instance.VictoryWithAbduction());
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Victory");
        }
    }
}