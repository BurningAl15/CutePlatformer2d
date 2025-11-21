using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private static readonly int IsActive = Animator.StringToHash("isActive");
    [SerializeField] private Vector3 respawnOffset = Vector3.zero;

    [SerializeField] Animator animator;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private bool isActive;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateCheckpoint(other.GetComponent<PlayerHealth>());
        }
    }

    private void ActivateCheckpoint(PlayerHealth playerHealth)
    {
        if (playerHealth == null) return;
        
        if (!isActive)
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX("Checkpoint");
            }
            
            if(particleSystem != null)
                particleSystem.Play();

            isActive = true;
        }
        
        if(animator != null)
            animator.SetBool(IsActive, isActive);

        DeactivateAllOtherWaypoints();

        playerHealth.SetCurrentWaypoint(this);
    }

    private void DeactivateAllOtherWaypoints()
    {
        Waypoint[] allWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
        foreach (Waypoint waypoint in allWaypoints)
        {
            if (waypoint != this)
            {
                waypoint.Deactivate();
            }
        }
    }

    private void Deactivate()
    {
        isActive = false;
        if(animator != null)
            animator.SetBool(IsActive, isActive);
    }

    public Vector3 GetRespawnPosition()
    {
        return transform.position + respawnOffset;
    }
}