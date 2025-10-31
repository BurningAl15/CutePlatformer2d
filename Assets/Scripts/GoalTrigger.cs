using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.instance != null)
        {
            GameManager.instance.SetGameState(GameManager.GameState.Victory);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Victory");
        }
    }
}