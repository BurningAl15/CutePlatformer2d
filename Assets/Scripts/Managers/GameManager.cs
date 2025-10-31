using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { Playing, Paused, GameOver, Victory }
    
    private GameState currentState = GameState.Playing;
    
    public GameState CurrentState => currentState;

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