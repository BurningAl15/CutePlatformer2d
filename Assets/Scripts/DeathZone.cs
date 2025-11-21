using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[DeathZone] Player entered at: {other.transform.position}");
            
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }

        if (other.CompareTag("Key"))
        {
            Debug.Log("[DeathZone] Key fell into DeathZone");
            Grabbable2D grabbable = other.GetComponent<Grabbable2D>();
            if (grabbable != null)
                grabbable.ResetPositionToInitial();
        }
    }
}