using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private int boardWidth = 20;
    [SerializeField] private int boardHeight = 20;
    [SerializeField] private float moveInterval = 0.15f;
    
    [Header("Speed Progression")]
    [SerializeField] private bool increaseSpeedWithScore = true;
    [SerializeField] [Min(1)] private int foodPerSpeedStep = 5;
    [SerializeField] [Min(0f)] private float moveIntervalDecreasePerStep = 0.008f;
    [SerializeField] [Min(0.02f)] private float minMoveInterval = 0.08f;

    [Header("Flow")]
    [SerializeField] private GameMode gameMode = GameMode.Classic;
    [SerializeField] private bool waitForInputToStart = true;
    [SerializeField] private string startStatusMessage = "Press any key to start";
    [SerializeField] private string pausedStatusMessage = "Paused (P/Esc)";

    [Header("Glitch Mode")]
    [SerializeField] private float reverseControlsEverySeconds = 8f;
    [SerializeField] private float reverseControlsDuration = 4f;
    [SerializeField] private string glitchModeStatusMessage = "Glitch Mode";
    [SerializeField] private string reversedControlsStatusMessage = "GLITCH: REVERSED";

    [Header("Camera")]
    [SerializeField] private bool fitCameraToBoardOnStart = true;
    [SerializeField] private float cameraPadding = 0.5f;

    [Header("Border Visual")]
    [SerializeField] private bool showBoardBorder = true;
    [SerializeField] private Color borderColor = new Color(0.39215687f, 0.45490196f, 0.54509807f, 1f);
    [SerializeField] private float borderWidth = 0.08f;
    [SerializeField] private int borderSortingOrder = 20;
    [SerializeField] [Range(0, 8)] private int borderCornerVertices = 2;
    [SerializeField] [Range(0, 8)] private int borderCapVertices = 2;

    [Header("Scene References")]
    [SerializeField] private SnakeController snakeController;
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private GameUIController gameUIController;

    private bool isGameOver;
    private bool hasGameStarted;
    private bool isPaused;
    private bool areControlsReversed;
    private float nextReverseControlsTime;
    private float reverseControlsEndTime;
    private int score;
    private int bestScore;
    private LineRenderer boardBorderLine;

    private const string BestScorePrefsKey = "GlitchSnake_BestScore";

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
        hasGameStarted = !waitForInputToStart;
        isPaused = false;
        if (GameModeSelection.HasSelectedMode)
        {
            gameMode = GameModeSelection.SelectedMode;
        }
        areControlsReversed = false;
        ResetNextReverseControlsTime();
        score = 0;
        bestScore = LoadBestScore();

        snakeController.Initialize(this);
        foodSpawner.Initialize(this, boardWidth, boardHeight);
        gameUIController.Initialize(this);

        gameUIController.SetScore(score);
        gameUIController.SetBestScore(bestScore);
        UpdateStatusLabel();
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
        if (score > bestScore)
        {
            bestScore = score;
            SaveBestScore();
            gameUIController.SetBestScore(bestScore);
        }

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
        isPaused = false;
        areControlsReversed = false;
        UpdateStatusLabel();
        gameUIController.ShowGameOver(true);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool IsGameplayActive()
    {
        return hasGameStarted && !isPaused && !isGameOver;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetBestScore()
    {
        return bestScore;
    }

    public bool HasGameStarted()
    {
        return hasGameStarted;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public bool IsGlitchMode()
    {
        return gameMode == GameMode.Glitch;
    }

    public bool AreControlsReversed()
    {
        return IsGlitchMode() && areControlsReversed;
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
        float targetMoveInterval = moveInterval;
        if (increaseSpeedWithScore && foodPerSpeedStep > 0)
        {
            int speedSteps = score / foodPerSpeedStep;
            targetMoveInterval -= speedSteps * moveIntervalDecreasePerStep;
        }

        float safeMinInterval = Mathf.Max(0.01f, minMoveInterval);
        return Mathf.Max(safeMinInterval, targetMoveInterval);
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
        boardBorderLine.numCornerVertices = borderCornerVertices;
        boardBorderLine.numCapVertices = borderCapVertices;
        boardBorderLine.alignment = LineAlignment.View;
        boardBorderLine.textureMode = LineTextureMode.Stretch;

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

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (!hasGameStarted)
        {
            if (keyboard.anyKey.wasPressedThisFrame)
            {
                hasGameStarted = true;
                isPaused = false;
                ResetNextReverseControlsTime();
                UpdateStatusLabel();
            }

            return;
        }

        if (keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
        {
            isPaused = !isPaused;
            UpdateStatusLabel();
            return;
        }

        if (!isPaused && IsGlitchMode() && keyboard.gKey.wasPressedThisFrame)
        {
            StartReverseControlsGlitch();
        }

        UpdateGlitchTimers();
    }

    private int LoadBestScore()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(BestScorePrefsKey, 0));
    }

    private void SaveBestScore()
    {
        PlayerPrefs.SetInt(BestScorePrefsKey, bestScore);
        PlayerPrefs.Save();
    }

    private void UpdateGlitchTimers()
    {
        if (!IsGlitchMode() || !IsGameplayActive())
        {
            return;
        }

        if (areControlsReversed)
        {
            if (Time.time >= reverseControlsEndTime)
            {
                areControlsReversed = false;
                ResetNextReverseControlsTime();
                UpdateStatusLabel();
            }

            return;
        }

        if (Time.time >= nextReverseControlsTime)
        {
            StartReverseControlsGlitch();
        }
    }

    private void StartReverseControlsGlitch()
    {
        if (!IsGlitchMode())
        {
            return;
        }

        areControlsReversed = true;
        reverseControlsEndTime = Time.time + Mathf.Max(0.1f, reverseControlsDuration);
        UpdateStatusLabel();
    }

    private void ResetNextReverseControlsTime()
    {
        nextReverseControlsTime = Time.time + Mathf.Max(0.1f, reverseControlsEverySeconds);
    }

    private void UpdateStatusLabel()
    {
        if (gameUIController == null)
        {
            return;
        }

        if (isGameOver)
        {
            gameUIController.ClearStatusMessage();
            return;
        }

        string statusMessage = string.Empty;

        if (!hasGameStarted)
        {
            statusMessage = startStatusMessage;
        }
        else if (isPaused)
        {
            statusMessage = pausedStatusMessage;
        }

        if (IsGlitchMode())
        {
            string glitchStatus = areControlsReversed ? reversedControlsStatusMessage : glitchModeStatusMessage;
            statusMessage = string.IsNullOrWhiteSpace(statusMessage)
                ? glitchStatus
                : statusMessage + "\n" + glitchStatus;
        }

        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            gameUIController.ClearStatusMessage();
        }
        else
        {
            gameUIController.SetStatusMessage(statusMessage);
        }
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
