using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player2DController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxLives = 3;
    private int currentLives = 3;

    [Header("Respawn Settings")]
    [SerializeField] private float invincibilityDuration = 6f;
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
            Debug.Log($"[PlayerHealth] Initial waypoint: {currentWaypoint.name}");
        }
    }

    public void SetCurrentWaypoint(Waypoint waypoint)
    {
        currentWaypoint = waypoint;
        Debug.Log($"[PlayerHealth] Waypoint changed to: {waypoint.name}");
    }

    public void TakeDamage(int damage = 1)
    {
        if (isInvincible || !IsAlive)
        {
            Debug.Log($"[PlayerHealth] Damage blocked - Invincible: {isInvincible}, Alive: {IsAlive}");
            return;
        }

        currentLives -= damage;
        currentLives = Mathf.Max(0, currentLives);

        Debug.Log($"[PlayerHealth] Took {damage} damage. Lives: {currentLives}");

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
        Debug.Log($"[PlayerHealth] Respawn - Waypoint: {currentWaypoint?.name ?? "NULL"}");
        
        if (currentWaypoint != null)
        {
            if (GameManager.instance != null)
            {
                StartCoroutine(RespawnAnimation());
            }
            else
            {
                Debug.LogError("[PlayerHealth] GameManager.instance is NULL!");
            }
        }
        else
        {
            Debug.LogError("[PlayerHealth] No waypoint for respawn!");
        }
    }

    IEnumerator RespawnAnimation()
    {
        yield return new WaitForSeconds(.1f);
        StartCoroutine(GameManager.instance.RespawnWithAbduction(currentWaypoint));
        StartInvincibility();
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died - Game Over");
        
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
        if (isInvincible)
        {
            Debug.Log("[PlayerHealth] Invincibility restarted");
            StopAllCoroutines();
        }
        
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        Debug.Log($"[PlayerHealth] Invincibility for {invincibilityDuration}s");
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
        Debug.Log("[PlayerHealth] Invincibility ended");
    }
}
