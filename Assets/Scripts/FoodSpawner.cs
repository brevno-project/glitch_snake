using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    [SerializeField] private GameObject foodPrefab;

    private GameManager gameManager;
    private GameObject currentFoodInstance;
    private int boardWidth;
    private int boardHeight;

    public void Initialize(GameManager manager, int width, int height)
    {
        gameManager = manager;
        boardWidth = width;
        boardHeight = height;

        ClearFood();
    }

    public void SpawnFood()
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("FoodSpawner: Food prefab is not assigned yet.");
            return;
        }

        // Random free-cell spawning will be implemented in a later step.
        Debug.Log("FoodSpawner: SpawnFood() is ready to be implemented in the next step.");
    }

    public void ClearFood()
    {
        if (currentFoodInstance != null)
        {
            Destroy(currentFoodInstance);
            currentFoodInstance = null;
        }
    }

    public int GetBoardWidth()
    {
        return boardWidth;
    }

    public int GetBoardHeight()
    {
        return boardHeight;
    }

    public GameManager GetGameManager()
    {
        return gameManager;
    }
}
