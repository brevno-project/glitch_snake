using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour
{
    [Header("Start Setup")]
    [SerializeField] private Vector2Int startGridPosition = new Vector2Int(10, 10);
    [SerializeField] private Vector2Int startDirection = Vector2Int.right;

    private GameManager gameManager;
    private Vector2Int currentGridPosition;
    private Vector2Int currentDirection;
    private Vector2Int nextDirection;
    private float moveTimer;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        ResetToStart();
    }

    public void ResetToStart()
    {
        currentGridPosition = startGridPosition;
        currentDirection = GetSafeStartDirection();
        nextDirection = currentDirection;
        moveTimer = 0f;

        // For now we place the root object at the starting grid position.
        transform.position = new Vector3(currentGridPosition.x, currentGridPosition.y, 0f);
    }

    private void Update()
    {
        if (gameManager == null || gameManager.IsGameOver())
        {
            return;
        }

        ReadDirectionInput();
        TickMovement();
    }

    private void ReadDirectionInput()
    {
        if (IsUpPressed())
        {
            TrySetNextDirection(Vector2Int.up);
        }
        else if (IsDownPressed())
        {
            TrySetNextDirection(Vector2Int.down);
        }
        else if (IsLeftPressed())
        {
            TrySetNextDirection(Vector2Int.left);
        }
        else if (IsRightPressed())
        {
            TrySetNextDirection(Vector2Int.right);
        }
    }

    private void TickMovement()
    {
        float interval = Mathf.Max(0.01f, gameManager.GetMoveInterval());
        moveTimer += Time.deltaTime;

        while (moveTimer >= interval)
        {
            moveTimer -= interval;
            MoveOneCell();
        }
    }

    private void MoveOneCell()
    {
        currentDirection = nextDirection;
        currentGridPosition += currentDirection;
        transform.position = new Vector3(currentGridPosition.x, currentGridPosition.y, 0f);
    }

    private void TrySetNextDirection(Vector2Int requestedDirection)
    {
        // Disallow instant 180-degree turns.
        if (requestedDirection == -currentDirection)
        {
            return;
        }

        nextDirection = requestedDirection;
    }

    private Vector2Int GetSafeStartDirection()
    {
        if (startDirection == Vector2Int.zero)
        {
            return Vector2Int.right;
        }

        return startDirection;
    }

    private bool IsUpPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame);
    }

    private bool IsDownPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame);
    }

    private bool IsLeftPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame);
    }

    private bool IsRightPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame);
    }

    public void Grow()
    {
        // Body growth will be implemented later.
    }

    public Vector2Int GetHeadGridPosition()
    {
        return currentGridPosition;
    }

    public Vector2Int GetCurrentDirection()
    {
        return currentDirection;
    }

    public GameManager GetGameManager()
    {
        return gameManager;
    }
}
