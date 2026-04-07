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
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameManager gameManager;
    private int currentScore;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        currentScore = 0;

        ShowGameOver(false);
    }

    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreLabel();
    }

    private void UpdateScoreLabel()
    {
        string scoreLabel = scorePrefix + currentScore;

        if (scoreTextTMP != null)
        {
            scoreTextTMP.text = scoreLabel;
        }

        if (scoreText != null)
        {
            scoreText.text = scoreLabel;
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
