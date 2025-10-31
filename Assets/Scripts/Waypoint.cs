using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private static readonly int IsActive = Animator.StringToHash("isActive");
    [SerializeField] private Vector3 respawnOffset = Vector3.zero;

    [SerializeField] Animator animator;

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
        
        if(animator != null)
            animator.SetBool(IsActive, true);

        DeactivateAllOtherWaypoints();

        playerHealth.SetCurrentWaypoint(this);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Checkpoint");
        }
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

    public void Deactivate()
    {
        if(animator != null)
            animator.SetBool(IsActive, false);
    }

    public Vector3 GetRespawnPosition()
    {
        return transform.position + respawnOffset;
    }
}