using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour
{
    [Header("Start Setup")]
    [SerializeField] private Vector2Int startGridPosition = new Vector2Int(10, 10);
    [SerializeField] private Vector2Int startDirection = Vector2Int.right;

    [Header("Body Visuals")]
    [SerializeField] private bool tintBodySegments = false;
    [SerializeField] private Color bodySegmentColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private float bodySegmentScale = 1f;
    [SerializeField] private int bodySortingOrderOffset = -1;

    private GameManager gameManager;
    private Vector2Int currentGridPosition;
    private Vector2Int currentDirection;
    private Vector2Int nextDirection;
    private float moveTimer;

    private readonly List<Vector2Int> occupiedCells = new List<Vector2Int>();
    private readonly List<Transform> bodySegments = new List<Transform>();
    private SpriteRenderer headSpriteRenderer;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        headSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ResetToStart();
    }

    public void ResetToStart()
    {
        currentGridPosition = startGridPosition;
        currentDirection = GetSafeStartDirection();
        nextDirection = currentDirection;
        moveTimer = 0f;

        ClearBodySegments();
        occupiedCells.Clear();
        occupiedCells.Add(currentGridPosition);

        transform.position = GridToWorld(currentGridPosition);
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

            if (gameManager.IsGameOver())
            {
                return;
            }
        }
    }

    private void MoveOneCell()
    {
        currentDirection = nextDirection;

        Vector2Int nextHeadPosition = currentGridPosition + currentDirection;
        if (IsOutsideBoard(nextHeadPosition))
        {
            gameManager.TriggerGameOver();
            return;
        }

        currentGridPosition = nextHeadPosition;

        occupiedCells.Insert(0, currentGridPosition);
        occupiedCells.RemoveAt(occupiedCells.Count - 1);

        transform.position = GridToWorld(currentGridPosition);
        UpdateBodySegments();

        if (IsSelfCollision())
        {
            gameManager.TriggerGameOver();
            return;
        }

        gameManager.HandleSnakeMoved(currentGridPosition);
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
        if (occupiedCells.Count == 0)
        {
            return;
        }

        Vector2Int tailCell = occupiedCells[occupiedCells.Count - 1];
        occupiedCells.Add(tailCell);
        UpdateBodySegments();
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

    public IReadOnlyList<Vector2Int> GetOccupiedCells()
    {
        return occupiedCells;
    }

    private bool IsOutsideBoard(Vector2Int gridPosition)
    {
        return gridPosition.x < 0
            || gridPosition.x >= gameManager.GetBoardWidth()
            || gridPosition.y < 0
            || gridPosition.y >= gameManager.GetBoardHeight();
    }

    private bool IsSelfCollision()
    {
        for (int i = 1; i < occupiedCells.Count; i++)
        {
            if (occupiedCells[i] == currentGridPosition)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateBodySegments()
    {
        int requiredCount = Mathf.Max(0, occupiedCells.Count - 1);

        while (bodySegments.Count < requiredCount)
        {
            bodySegments.Add(CreateBodySegment(bodySegments.Count + 1));
        }

        while (bodySegments.Count > requiredCount)
        {
            int lastIndex = bodySegments.Count - 1;
            Destroy(bodySegments[lastIndex].gameObject);
            bodySegments.RemoveAt(lastIndex);
        }

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector2Int bodyCell = occupiedCells[i + 1];
            bodySegments[i].position = GridToWorld(bodyCell);
        }
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return gameManager.GridToWorldPosition(gridPosition);
    }

    private Transform CreateBodySegment(int index)
    {
        GameObject segmentObject = new GameObject("BodySegment_" + index);
        segmentObject.transform.SetParent(transform.parent);
        segmentObject.transform.localScale = Vector3.one * Mathf.Max(0.01f, bodySegmentScale);

        if (headSpriteRenderer != null)
        {
            SpriteRenderer segmentRenderer = segmentObject.AddComponent<SpriteRenderer>();
            segmentRenderer.sprite = headSpriteRenderer.sprite;
            segmentRenderer.color = tintBodySegments ? bodySegmentColor : headSpriteRenderer.color;
            segmentRenderer.sortingLayerID = headSpriteRenderer.sortingLayerID;
            segmentRenderer.sortingOrder = headSpriteRenderer.sortingOrder + bodySortingOrderOffset;
        }

        return segmentObject.transform;
    }

    private void ClearBodySegments()
    {
        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i] != null)
            {
                Destroy(bodySegments[i].gameObject);
            }
        }

        bodySegments.Clear();
    }
}
