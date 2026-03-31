using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Start Setup")]
    [SerializeField] private Vector2Int startGridPosition = new Vector2Int(10, 10);
    [SerializeField] private Vector2Int startDirection = Vector2Int.right;

    private GameManager gameManager;
    private Vector2Int currentGridPosition;
    private Vector2Int currentDirection;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        ResetToStart();
    }

    public void ResetToStart()
    {
        currentGridPosition = startGridPosition;
        currentDirection = startDirection;

        // For now we place the root object at the starting grid position.
        transform.position = new Vector3(currentGridPosition.x, currentGridPosition.y, 0f);
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
