using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Start Setup")]
    [SerializeField] private Vector2Int startGridPosition = new Vector2Int(10, 10);
    [SerializeField] private Vector2Int startDirection = Vector2Int.right;

    private void Start()
    {
        // Snake setup will be implemented in a later step.
    }

    private void Update()
    {
        // Input and movement will be implemented in a later step.
    }

    public void Grow()
    {
        // Body growth will be implemented later.
    }
}
