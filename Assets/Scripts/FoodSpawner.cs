using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    [SerializeField] private GameObject foodPrefab;

    private GameManager gameManager;
    private GameObject currentFoodInstance;
    private int boardWidth;
    private int boardHeight;
    private Vector2Int currentFoodGridPosition;
    private bool hasFood;

    public void Initialize(GameManager manager, int width, int height)
    {
        gameManager = manager;
        boardWidth = width;
        boardHeight = height;

        ClearFood();
    }

    public void SpawnFood(IReadOnlyList<Vector2Int> occupiedCells)
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("FoodSpawner: Food prefab is not assigned yet.");
            return;
        }

        if (!TryGetRandomFreeCell(occupiedCells, out Vector2Int spawnCell))
        {
            Debug.LogWarning("FoodSpawner: Could not find a free cell for food.");
            ClearFood();
            return;
        }

        ClearFood();

        currentFoodGridPosition = spawnCell;
        currentFoodInstance = Instantiate(
            foodPrefab,
            new Vector3(spawnCell.x, spawnCell.y, 0f),
            Quaternion.identity,
            transform
        );
        hasFood = true;
    }

    public void ClearFood()
    {
        if (currentFoodInstance != null)
        {
            Destroy(currentFoodInstance);
            currentFoodInstance = null;
        }

        hasFood = false;
    }

    public bool IsFoodAtPosition(Vector2Int gridPosition)
    {
        return hasFood && currentFoodGridPosition == gridPosition;
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

    private bool TryGetRandomFreeCell(IReadOnlyList<Vector2Int> occupiedCells, out Vector2Int freeCell)
    {
        int maxAttempts = Mathf.Max(10, boardWidth * boardHeight * 2);
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int candidate = new Vector2Int(
                Random.Range(0, boardWidth),
                Random.Range(0, boardHeight)
            );

            if (!IsCellOccupied(candidate, occupiedCells))
            {
                freeCell = candidate;
                return true;
            }
        }

        // Fallback full scan so we still find a free cell if random attempts miss.
        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                Vector2Int candidate = new Vector2Int(x, y);
                if (!IsCellOccupied(candidate, occupiedCells))
                {
                    freeCell = candidate;
                    return true;
                }
            }
        }

        freeCell = Vector2Int.zero;
        return false;
    }

    private bool IsCellOccupied(Vector2Int cell, IReadOnlyList<Vector2Int> occupiedCells)
    {
        if (occupiedCells == null)
        {
            return false;
        }

        for (int i = 0; i < occupiedCells.Count; i++)
        {
            if (occupiedCells[i] == cell)
            {
                return true;
            }
        }

        return false;
    }
}
