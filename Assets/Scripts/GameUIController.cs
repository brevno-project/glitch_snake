using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;

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
        // Score text binding will be added in a later step.
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
