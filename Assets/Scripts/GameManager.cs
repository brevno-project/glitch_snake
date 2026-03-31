using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private int boardWidth = 20;
    [SerializeField] private int boardHeight = 20;
    [SerializeField] private float moveInterval = 0.15f;

    [Header("Scene References")]
    [SerializeField] private SnakeController snakeController;
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private GameUIController gameUIController;

    private bool isGameOver;
    private int score;

    private void Start()
    {
        InitializeSceneFoundation();
    }

    private void InitializeSceneFoundation()
    {
        if (!ValidateReferences())
        {
            return;
        }

        isGameOver = false;
        score = 0;

        snakeController.Initialize(this);
        foodSpawner.Initialize(this, boardWidth, boardHeight);
        gameUIController.Initialize(this);

        gameUIController.SetScore(score);
        gameUIController.ShowGameOver(false);

        SpawnFoodForCurrentSnake();
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (snakeController == null)
        {
            Debug.LogError("GameManager: SnakeController reference is missing.");
            isValid = false;
        }

        if (foodSpawner == null)
        {
            Debug.LogError("GameManager: FoodSpawner reference is missing.");
            isValid = false;
        }

        if (gameUIController == null)
        {
            Debug.LogError("GameManager: GameUIController reference is missing.");
            isValid = false;
        }

        return isValid;
    }

    public void HandleFoodEaten()
    {
        if (isGameOver)
        {
            return;
        }

        score += 1;
        gameUIController.SetScore(score);
    }

    public void HandleSnakeMoved(Vector2Int headGridPosition)
    {
        if (isGameOver)
        {
            return;
        }

        if (!foodSpawner.IsFoodAtPosition(headGridPosition))
        {
            return;
        }

        snakeController.Grow();
        HandleFoodEaten();
        SpawnFoodForCurrentSnake();
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        gameUIController.ShowGameOver(true);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetBoardWidth()
    {
        return boardWidth;
    }

    public int GetBoardHeight()
    {
        return boardHeight;
    }

    public float GetMoveInterval()
    {
        return moveInterval;
    }

    private void SpawnFoodForCurrentSnake()
    {
        foodSpawner.SpawnFood(snakeController.GetOccupiedCells());
    }
}
