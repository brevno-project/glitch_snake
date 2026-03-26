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

    private void Start()
    {
        // Initialization flow will be implemented in a later step.
    }

    public void HandleFoodEaten()
    {
        // Score and growth handling will be implemented later.
    }

    public void TriggerGameOver()
    {
        // Game over flow will be implemented later.
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}
