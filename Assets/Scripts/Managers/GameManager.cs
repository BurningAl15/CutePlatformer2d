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
    }

    private void Start()
    {
        StartCoroutine(CallCore());
    }

    private IEnumerator CallCore()
    {
        yield return StartCoroutine(Initialize());
        yield return null;
        yield return StartCoroutine(Load());
        yield return null;
        currentState = GameState.Playing;
    }
    
    private IEnumerator Initialize()
    {
        currentState = GameState.Initialize;
        abductionEffect.Initialize();
        yield return null;
    }
    
    private IEnumerator Load()
    {
        currentState = GameState.Load;
        yield return StartCoroutine(abductionEffect.AbductionAnim_Land());
        yield return null;
    }

    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;
        
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