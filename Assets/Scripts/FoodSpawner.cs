using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public enum SpawnedFoodType
    {
        None,
        Normal,
        Fake
    }

    [Header("Food")]
    [SerializeField] private GameObject foodPrefab;

    private GameManager gameManager;
    private int boardWidth;
    private int boardHeight;

    private readonly List<FoodEntry> activeFoodEntries = new List<FoodEntry>();

    private sealed class FoodEntry
    {
        public GameObject Instance;
        public Vector2Int GridPosition;
        public SpawnedFoodType Type;
    }

    public void Initialize(GameManager manager, int width, int height)
    {
        gameManager = manager;
        boardWidth = width;
        boardHeight = height;

        ClearFood();
    }

    public void SpawnFood(IReadOnlyList<Vector2Int> occupiedCells)
    {
        SpawnNormalFood(occupiedCells);
    }

    public bool SpawnNormalFood(IReadOnlyList<Vector2Int> occupiedCells)
    {
        return SpawnFoodInternal(occupiedCells, SpawnedFoodType.Normal);
    }

    public int EnsureNormalFoodCount(IReadOnlyList<Vector2Int> occupiedCells, int targetCount)
    {
        targetCount = Mathf.Max(0, targetCount);
        int currentNormalCount = GetFoodCount(SpawnedFoodType.Normal);
        int spawnedCount = 0;

        while (currentNormalCount < targetCount)
        {
            if (!SpawnNormalFood(occupiedCells))
            {
                break;
            }

            currentNormalCount++;
            spawnedCount++;
        }

        return spawnedCount;
    }

    public bool SpawnFakeFood(IReadOnlyList<Vector2Int> occupiedCells)
    {
        if (HasFoodType(SpawnedFoodType.Fake))
        {
            return false;
        }

        return SpawnFoodInternal(occupiedCells, SpawnedFoodType.Fake);
    }

    private bool SpawnFoodInternal(IReadOnlyList<Vector2Int> occupiedCells, SpawnedFoodType foodType)
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("FoodSpawner: Food prefab is not assigned yet.");
            return false;
        }

        if (!TryGetRandomFreeCell(occupiedCells, out Vector2Int spawnCell))
        {
            Debug.LogWarning("FoodSpawner: Could not find a free cell for food.");
            return false;
        }

        GameObject foodInstance = Instantiate(
            foodPrefab,
            gameManager.GridToWorldPosition(spawnCell),
            Quaternion.identity,
            transform
        );

        if (foodInstance.GetComponent<FoodVisualAnimator>() == null)
        {
            foodInstance.AddComponent<FoodVisualAnimator>();
        }

        activeFoodEntries.Add(new FoodEntry
        {
            Instance = foodInstance,
            GridPosition = spawnCell,
            Type = foodType
        });

        return true;
    }

    public void ClearFood()
    {
        for (int i = 0; i < activeFoodEntries.Count; i++)
        {
            if (activeFoodEntries[i].Instance != null)
            {
                Destroy(activeFoodEntries[i].Instance);
            }
        }

        activeFoodEntries.Clear();
    }

    public void ClearFoodType(SpawnedFoodType foodType)
    {
        for (int i = activeFoodEntries.Count - 1; i >= 0; i--)
        {
            if (activeFoodEntries[i].Type != foodType)
            {
                continue;
            }

            if (activeFoodEntries[i].Instance != null)
            {
                Destroy(activeFoodEntries[i].Instance);
            }

            activeFoodEntries.RemoveAt(i);
        }
    }

    public bool TryConsumeFoodAtPosition(Vector2Int gridPosition, out SpawnedFoodType consumedFoodType)
    {
        consumedFoodType = SpawnedFoodType.None;
        for (int i = 0; i < activeFoodEntries.Count; i++)
        {
            FoodEntry entry = activeFoodEntries[i];
            if (entry.GridPosition != gridPosition)
            {
                continue;
            }

            consumedFoodType = entry.Type;
            if (entry.Instance != null)
            {
                Destroy(entry.Instance);
            }

            activeFoodEntries.RemoveAt(i);
            return true;
        }

        return false;
    }

    public bool HasFood()
    {
        return activeFoodEntries.Count > 0;
    }

    public bool HasFoodType(SpawnedFoodType foodType)
    {
        for (int i = 0; i < activeFoodEntries.Count; i++)
        {
            if (activeFoodEntries[i].Type == foodType)
            {
                return true;
            }
        }

        return false;
    }

    public int GetFoodCount(SpawnedFoodType foodType)
    {
        int count = 0;
        for (int i = 0; i < activeFoodEntries.Count; i++)
        {
            if (activeFoodEntries[i].Type == foodType)
            {
                count++;
            }
        }

        return count;
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
        int maxAttempts = Mathf.Max(10, boardWidth * boardHeight * 3);
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int candidate = new Vector2Int(
                Random.Range(0, boardWidth),
                Random.Range(0, boardHeight)
            );

            if (!IsCellOccupied(candidate, occupiedCells) && !IsCellOccupiedByFood(candidate))
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
                if (!IsCellOccupied(candidate, occupiedCells) && !IsCellOccupiedByFood(candidate))
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

    private bool IsCellOccupiedByFood(Vector2Int cell)
    {
        for (int i = 0; i < activeFoodEntries.Count; i++)
        {
            if (activeFoodEntries[i].GridPosition == cell)
            {
                return true;
            }
        }

        return false;
    }

}
