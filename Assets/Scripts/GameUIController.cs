using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreTextTMP;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private string bestScorePrefix = "Best: ";
    [SerializeField] private string scoreSeparator = "   ";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameManager gameManager;
    private int currentScore;
    private int bestScore;
    private string statusMessage;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        currentScore = 0;
        bestScore = 0;
        statusMessage = string.Empty;

        ConfigureScoreLabelsForStatus();
        UpdateScoreLabel();
        ShowGameOver(false);
    }

    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreLabel();
    }

    public void SetBestScore(int score)
    {
        bestScore = Mathf.Max(0, score);
        UpdateScoreLabel();
    }

    public void SetStatusMessage(string message)
    {
        statusMessage = message ?? string.Empty;
        UpdateScoreLabel();
    }

    public void ClearStatusMessage()
    {
        statusMessage = string.Empty;
        UpdateScoreLabel();
    }

    private void UpdateScoreLabel()
    {
        string scoreLabel = scorePrefix + currentScore + scoreSeparator + bestScorePrefix + bestScore;
        if (!string.IsNullOrWhiteSpace(statusMessage))
        {
            scoreLabel += "\n" + statusMessage;
        }

        if (scoreTextTMP != null)
        {
            scoreTextTMP.text = scoreLabel;
        }

        if (scoreText != null)
        {
            scoreText.text = scoreLabel;
        }
    }

    private void ConfigureScoreLabelsForStatus()
    {
        if (scoreTextTMP != null)
        {
            scoreTextTMP.enableWordWrapping = false;
            scoreTextTMP.overflowMode = TextOverflowModes.Overflow;
        }

        if (scoreText != null)
        {
            scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
            scoreText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    public void ShowGameOver(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }
    }

    public void RestartGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void BackToMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("GameUIController: mainMenuSceneName is empty.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public GameManager GetGameManager()
    {
        return gameManager;
    }
}
