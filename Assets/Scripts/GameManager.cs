using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private int boardWidth = 20;
    [SerializeField] private int boardHeight = 20;
    [SerializeField] private float moveInterval = 0.15f;

    [Header("Camera")]
    [SerializeField] private bool fitCameraToBoardOnStart = true;
    [SerializeField] private float cameraPadding = 0.5f;

    [Header("Border Visual")]
    [SerializeField] private bool showBoardBorder = true;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private float borderWidth = 0.08f;
    [SerializeField] private int borderSortingOrder = 20;

    [Header("Scene References")]
    [SerializeField] private SnakeController snakeController;
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private GameUIController gameUIController;

    private bool isGameOver;
    private int score;
    private LineRenderer boardBorderLine;

    private void Start()
    {
        FitMainCameraToBoard();
        SetupBoardBorderVisual();
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

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        Vector2 offset = GetBoardWorldOffset();
        return new Vector3(gridPosition.x + offset.x, gridPosition.y + offset.y, 0f);
    }

    private Vector2 GetBoardWorldOffset()
    {
        // Keeps the logical board centered around world (0,0).
        return new Vector2(
            -((boardWidth - 1) * 0.5f),
            -((boardHeight - 1) * 0.5f)
        );
    }

    private void FitMainCameraToBoard()
    {
        if (!fitCameraToBoardOnStart)
        {
            return;
        }

        if (boardWidth <= 0 || boardHeight <= 0)
        {
            Debug.LogWarning("GameManager: board size must be greater than zero to fit camera.");
            return;
        }

        Camera targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogWarning("GameManager: no Main Camera found for board fitting.");
            return;
        }

        if (!targetCamera.orthographic)
        {
            Debug.LogWarning("GameManager: Main Camera must be Orthographic for board fitting.");
            return;
        }

        GetBoardWorldBounds(out float boardMinX, out float boardMaxX, out float boardMinY, out float boardMaxY);

        float halfWidth = ((boardMaxX - boardMinX) * 0.5f) + cameraPadding;
        float halfHeight = ((boardMaxY - boardMinY) * 0.5f) + cameraPadding;
        float aspect = Mathf.Max(0.01f, targetCamera.aspect);
        float sizeFromWidth = halfWidth / aspect;

        targetCamera.orthographicSize = Mathf.Max(halfHeight, sizeFromWidth);

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = (boardMinX + boardMaxX) * 0.5f;
        cameraPosition.y = (boardMinY + boardMaxY) * 0.5f;
        targetCamera.transform.position = cameraPosition;
    }

    private void SetupBoardBorderVisual()
    {
        if (!showBoardBorder || boardWidth <= 0 || boardHeight <= 0)
        {
            if (boardBorderLine != null)
            {
                boardBorderLine.enabled = false;
            }
            return;
        }

        if (boardBorderLine == null)
        {
            GameObject borderObject = new GameObject("BoardBorder");
            borderObject.transform.SetParent(transform, false);

            boardBorderLine = borderObject.AddComponent<LineRenderer>();
            boardBorderLine.useWorldSpace = true;
            boardBorderLine.loop = true;
            boardBorderLine.positionCount = 4;

            Shader lineShader = Shader.Find("Sprites/Default");
            if (lineShader != null)
            {
                boardBorderLine.material = new Material(lineShader);
            }
        }

        boardBorderLine.enabled = true;
        boardBorderLine.startWidth = borderWidth;
        boardBorderLine.endWidth = borderWidth;
        boardBorderLine.startColor = borderColor;
        boardBorderLine.endColor = borderColor;
        boardBorderLine.sortingOrder = borderSortingOrder;

        GetBoardWorldBounds(out float boardMinX, out float boardMaxX, out float boardMinY, out float boardMaxY);

        boardBorderLine.SetPosition(0, new Vector3(boardMinX, boardMinY, 0f));
        boardBorderLine.SetPosition(1, new Vector3(boardMaxX, boardMinY, 0f));
        boardBorderLine.SetPosition(2, new Vector3(boardMaxX, boardMaxY, 0f));
        boardBorderLine.SetPosition(3, new Vector3(boardMinX, boardMaxY, 0f));
    }

    private void GetBoardWorldBounds(out float boardMinX, out float boardMaxX, out float boardMinY, out float boardMaxY)
    {
        Vector3 minCellCenter = GridToWorldPosition(new Vector2Int(0, 0));
        Vector3 maxCellCenter = GridToWorldPosition(new Vector2Int(boardWidth - 1, boardHeight - 1));

        boardMinX = minCellCenter.x - 0.5f;
        boardMaxX = maxCellCenter.x + 0.5f;
        boardMinY = minCellCenter.y - 0.5f;
        boardMaxY = maxCellCenter.y + 0.5f;
    }

    private void SpawnFoodForCurrentSnake()
    {
        foodSpawner.SpawnFood(snakeController.GetOccupiedCells());
    }

    private void OnDrawGizmosSelected()
    {
        if (boardWidth <= 0 || boardHeight <= 0)
        {
            return;
        }

        GetBoardWorldBounds(out float boardMinX, out float boardMaxX, out float boardMinY, out float boardMaxY);
        Vector3 center = new Vector3((boardMinX + boardMaxX) * 0.5f, (boardMinY + boardMaxY) * 0.5f, 0f);
        Vector3 size = new Vector3(boardMaxX - boardMinX, boardMaxY - boardMinY, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}
