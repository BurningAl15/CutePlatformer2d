using UnityEngine;

[RequireComponent(typeof(Player2DController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxLives = 3;
    private int currentLives = 3;

    [Header("Respawn Settings")]
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float blinkInterval = 0.1f;

    private bool isInvincible = false;
    private Waypoint currentWaypoint;
    private Player2DController playerController;
    private SpriteRenderer spriteRenderer;

    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public bool IsAlive => currentLives > 0;

    private void Awake()
    {
        playerController = GetComponent<Player2DController>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        currentLives = maxLives;
        
        if (HealthUI.instance != null)
        {
            HealthUI.instance.UpdateDisplay(currentLives, maxLives);
        }

        FindInitialWaypoint();
    }

    private void FindInitialWaypoint()
    {
        Waypoint[] waypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
        if (waypoints.Length > 0)
        {
            currentWaypoint = waypoints[0];
        }
    }

    public void SetCurrentWaypoint(Waypoint waypoint)
    {
        currentWaypoint = waypoint;
    }

    public void TakeDamage(int damage = 1)
    {
        if (isInvincible || !IsAlive) return;

        currentLives -= damage;
        currentLives = Mathf.Max(0, currentLives);

        if (HealthUI.instance != null)
        {
            HealthUI.instance.UpdateDisplay(currentLives, maxLives);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Damage");
        }

        if (IsAlive)
        {
            Respawn();
        }
        else
        {
            Die();
        }
    }

    private void Respawn()
    {
        if (currentWaypoint != null)
        {
            transform.position = currentWaypoint.GetRespawnPosition();
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        StartInvincibility();
    }

    private void Die()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetGameState(GameManager.GameState.GameOver);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Death");
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    public void AddLife(int amount = 1)
    {
        currentLives += amount;
        currentLives = Mathf.Min(currentLives, maxLives);
        
        if (HealthUI.instance != null)
        {
            HealthUI.instance.UpdateDisplay(currentLives, maxLives);
        }
    }

    private void StartInvincibility()
    {
        if (isInvincible) return;
        
        StopAllCoroutines();
        StartCoroutine(InvincibilityRoutine());
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibilityDuration)
        {
            visible = !visible;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }
}
