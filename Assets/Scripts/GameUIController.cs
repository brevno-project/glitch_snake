using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string scorePrefix = "Score: ";

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

        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentScore;
        }
    }

    public void ShowGameOver(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }
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
