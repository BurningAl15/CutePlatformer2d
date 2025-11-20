using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { Initialize, Load, Playing, Paused, GameOver, Victory }
    
    private GameState currentState = GameState.Initialize;
    
    public GameState CurrentState => currentState;

    [SerializeField] private AbductionEffect abductionEffect;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        Debug.Log("[GameManager] Awake");
    }

    private void Start()
    {
        StartCoroutine(CallCore());
    }

    private IEnumerator CallCore()
    {
        Debug.Log("[GameManager] Starting CallCore sequence");
        yield return StartCoroutine(Initialize());
        yield return null;
        yield return StartCoroutine(Load());
        yield return null;
        currentState = GameState.Playing;
        Debug.Log("[GameManager] Game state: Playing");
    }
    
    private IEnumerator Initialize()
    {
        Debug.Log("[GameManager] Initialize");
        currentState = GameState.Initialize;
        abductionEffect.Initialize();
        yield return null;
    }
    
    private IEnumerator Load()
    {
        Debug.Log("[GameManager] Load - Starting initial abduction");
        currentState = GameState.Load;
        yield return StartCoroutine(abductionEffect.AbductionAnim_Land());
        Debug.Log("[GameManager] Initial abduction complete");
        yield return null;
    }

    public IEnumerator RespawnWithAbduction(Waypoint waypoint)
    {
        Debug.Log($"[GameManager] === RESPAWN SEQUENCE START === Waypoint: {waypoint.name}");
        currentState = GameState.Load;
        
        yield return StartCoroutine(abductionEffect.AbductionSequence_Respawn(waypoint.transform));
        
        currentState = GameState.Playing;
        Debug.Log("[GameManager] === RESPAWN SEQUENCE END === State: Playing");
    }

    public IEnumerator VictoryWithAbduction()
    {
        Debug.Log("[GameManager] === VICTORY SEQUENCE START ===");
        currentState = GameState.Load;
        
        yield return StartCoroutine(abductionEffect.AbductionSequence_Victory());
        
        SetGameState(GameState.Victory);
        Debug.Log("[GameManager] === VICTORY SEQUENCE END === State: Victory");
    }

    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"[GameManager] State change: {currentState} -> {newState}");
        currentState = newState;

        switch (currentState)
        {
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.Victory:
                Time.timeScale = 0f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
        }

        if (PopupManager.instance != null)
        {
            PopupManager.instance.OnGameStateChanged(currentState);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
