using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    [Header("Popup Panels")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Lose Panel")]
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private Button loseExitButton;
    [SerializeField] private TextMeshProUGUI loseCollectableText;

    [Header("Victory Panel")]
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryExitButton;
    [SerializeField] private TextMeshProUGUI victoryCollectableText;
    [SerializeField] private GameObject allCollectablesIcon;

    [Header("Pause Panel")]
    [SerializeField] private Button pauseResumeButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseExitButton;

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
        HideAllPanels();
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(OnRestart);
        if (loseExitButton != null)
            loseExitButton.onClick.AddListener(OnExit);

        if (victoryRestartButton != null)
            victoryRestartButton.onClick.AddListener(OnRestart);
        if (victoryExitButton != null)
            victoryExitButton.onClick.AddListener(OnExit);

        if (pauseResumeButton != null)
            pauseResumeButton.onClick.AddListener(OnResume);
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(OnRestart);
        if (pauseExitButton != null)
            pauseExitButton.onClick.AddListener(OnExit);
    }

    public void OnGameStateChanged(GameManager.GameState newState)
    {
        HideAllPanels();

        switch (newState)
        {
            case GameManager.GameState.GameOver:
                ShowLosePanel();
                break;
            case GameManager.GameState.Victory:
                ShowVictoryPanel();
                break;
            case GameManager.GameState.Paused:
                ShowPausePanel();
                break;
        }
    }

    private void HideAllPanels()
    {
        if (losePanel != null) losePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            UpdateCollectableText(loseCollectableText);
        }
    }

    private void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            bool hasAll = UpdateCollectableText(victoryCollectableText);
            
            if (allCollectablesIcon != null)
            {
                allCollectablesIcon.SetActive(hasAll);
            }
        }
    }

    private void ShowPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    private bool UpdateCollectableText(TextMeshProUGUI textComponent)
    {
        if (LevelManager.instance == null || textComponent == null) return false;

        int current = LevelManager.instance.CollectedStars;
        int total = LevelManager.instance.TotalStars;
        bool hasAll = current >= total;

        textComponent.text = $"{current}/{total}";

        return hasAll;
    }

    private void OnRestart()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartLevel();
        }
    }

    private void OnResume()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetGameState(GameManager.GameState.Playing);
        }
    }

    private void OnExit()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.QuitGame();
        }
    }

    public void TogglePause()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.CurrentState == GameManager.GameState.Playing)
        {
            GameManager.instance.SetGameState(GameManager.GameState.Paused);
        }
        else if (GameManager.instance.CurrentState == GameManager.GameState.Paused)
        {
            GameManager.instance.SetGameState(GameManager.GameState.Playing);
        }
    }
}
