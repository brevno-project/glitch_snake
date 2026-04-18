using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour
{
    [Header("Start Setup")]
    [SerializeField] private Vector2Int startGridPosition = new Vector2Int(10, 10);
    [SerializeField] private Vector2Int startDirection = Vector2Int.right;
    [SerializeField] [Min(1)] private int startLength = 1;

    [Header("Body Visuals")]
    [SerializeField] private bool tintBodySegments = false;
    [SerializeField] private Color bodySegmentColor = new Color(0.13333334f, 0.827451f, 0.93333334f, 1f);
    [SerializeField] private Sprite bodySpriteOverride;
    [SerializeField] private float bodySegmentScale = 1f;
    [SerializeField] private float tailSegmentScale = 0.78f;
    [SerializeField] private bool taperBodyTowardsTail = true;
    [SerializeField] private int bodySortingOrderOffset = -1;
    [SerializeField] [Min(1f)] private float cornerSegmentScaleMultiplier = 1.02f;
    [SerializeField] private bool autoNormalizeBodyThickness = true;
    [SerializeField] private bool useDirectionalSprites = true;
    [SerializeField] private Sprite bodyHorizontalSprite;
    [SerializeField] private Sprite bodyVerticalSprite;
    [SerializeField] private Sprite bodyTopLeftSprite;
    [SerializeField] private Sprite bodyTopRightSprite;
    [SerializeField] private Sprite bodyBottomLeftSprite;
    [SerializeField] private Sprite bodyBottomRightSprite;
    [SerializeField] private Sprite tailUpSprite;
    [SerializeField] private Sprite tailDownSprite;
    [SerializeField] private Sprite tailLeftSprite;
    [SerializeField] private Sprite tailRightSprite;

    [Header("Head Visuals")]
    [SerializeField] private Sprite headUpSprite;
    [SerializeField] private Sprite headDownSprite;
    [SerializeField] private Sprite headLeftSprite;
    [SerializeField] private Sprite headRightSprite;
    [SerializeField] private bool orientHeadToDirection = true;
    [SerializeField] private float headTurnSmoothing = 20f;
    [SerializeField] private bool autoNormalizeHeadSize = true;
    [SerializeField] [Min(0.1f)] private float headScaleMultiplier = 1.08f;
    [SerializeField] private bool pulseHead = true;
    [SerializeField] private float headPulseAmplitude = 0.05f;
    [SerializeField] private float headPulseSpeed = 7f;
    [SerializeField] private Sprite blinkHeadSprite;
    [SerializeField] private float minBlinkInterval = 2.2f;
    [SerializeField] private float maxBlinkInterval = 4.8f;
    [SerializeField] private float blinkDuration = 0.09f;

    private GameManager gameManager;
    private Vector2Int currentGridPosition;
    private Vector2Int currentDirection;
    private Vector2Int nextDirection;
    private float moveTimer;

    private readonly List<Vector2Int> occupiedCells = new List<Vector2Int>();
    private readonly List<Transform> bodySegments = new List<Transform>();
    private readonly List<SpriteRenderer> bodySegmentRenderers = new List<SpriteRenderer>();
    private SpriteRenderer headSpriteRenderer;
    private Vector3 baseHeadScale;
    private Sprite defaultHeadSprite;
    private float bodyThicknessReference = 1f;
    private float bodyLengthReference = 1f;
    private float gridCellWorldSize = 1f;
    private float currentHeadNormalization = 1f;
    private float headFacingAngle;
    private float blinkEndsAtTime;
    private float nextBlinkStartTime;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        headSpriteRenderer = GetComponent<SpriteRenderer>();
        if (headSpriteRenderer == null)
        {
            headSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        baseHeadScale = transform.localScale;
        defaultHeadSprite = headSpriteRenderer != null ? headSpriteRenderer.sprite : null;
        RefreshBodyThicknessReference();
        RefreshGridCellWorldSize();
        RefreshHeadNormalization();
        headFacingAngle = DirectionToAngle(startDirection);
        ScheduleNextBlink();

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
        int initialLength = Mathf.Max(1, startLength);
        for (int i = 1; i < initialLength; i++)
        {
            occupiedCells.Add(currentGridPosition - currentDirection * i);
        }

        transform.localScale = baseHeadScale;
        headFacingAngle = DirectionToAngle(currentDirection);
        transform.rotation = UseDirectionalHeadSprites() ? Quaternion.identity : Quaternion.Euler(0f, 0f, headFacingAngle);
        ApplyHeadSprite(currentDirection);
        transform.position = GridToWorld(currentGridPosition);
        UpdateBodySegments();
    }

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        UpdateHeadVisual();

        if (!gameManager.IsGameplayActive())
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
        if (gameManager != null && gameManager.AreControlsReversed())
        {
            requestedDirection = -requestedDirection;
        }

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
            bodySegmentRenderers.RemoveAt(lastIndex);
        }

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector2Int bodyCell = occupiedCells[i + 1];
            bodySegments[i].position = GridToWorld(bodyCell);
            ApplyBodySegmentSprite(i);

            float targetScale = Mathf.Max(0.01f, bodySegmentScale);
            if (taperBodyTowardsTail && bodySegments.Count > 1)
            {
                float tailLerp = (float)i / (bodySegments.Count - 1);
                targetScale = Mathf.Max(0.01f, Mathf.Lerp(bodySegmentScale, tailSegmentScale, tailLerp));
            }

            int cellIndex = i + 1;
            if (IsCornerCell(cellIndex))
            {
                targetScale *= Mathf.Max(0.01f, cornerSegmentScaleMultiplier);
            }

            Sprite segmentSprite = i < bodySegmentRenderers.Count && bodySegmentRenderers[i] != null
                ? bodySegmentRenderers[i].sprite
                : null;
            float normalization = GetBodyThicknessNormalization(segmentSprite);
            bodySegments[i].localScale = Vector3.one * targetScale * normalization;

            if (tintBodySegments && i < bodySegmentRenderers.Count && bodySegmentRenderers[i] != null)
            {
                Color segmentColor = bodySegmentColor;
                if (bodySegments.Count > 1)
                {
                    float tailLerp = (float)i / (bodySegments.Count - 1);
                    segmentColor.a = Mathf.Lerp(bodySegmentColor.a, bodySegmentColor.a * 0.78f, tailLerp);
                }
                bodySegmentRenderers[i].color = segmentColor;
            }
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

        SpriteRenderer segmentRenderer = null;

        if (headSpriteRenderer != null)
        {
            segmentRenderer = segmentObject.AddComponent<SpriteRenderer>();
            segmentRenderer.sprite = GetFallbackBodySprite();
            segmentRenderer.color = tintBodySegments ? bodySegmentColor : headSpriteRenderer.color;
            segmentRenderer.sortingLayerID = headSpriteRenderer.sortingLayerID;
            segmentRenderer.sortingOrder = headSpriteRenderer.sortingOrder + bodySortingOrderOffset;
        }

        bodySegmentRenderers.Add(segmentRenderer);
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
        bodySegmentRenderers.Clear();
    }

    private void UpdateHeadVisual()
    {
        if (headSpriteRenderer == null)
        {
            return;
        }

        Vector2Int facingDirection = currentDirection != Vector2Int.zero ? currentDirection : startDirection;
        bool usesDirectionalHead = ApplyHeadSprite(facingDirection);

        if (usesDirectionalHead)
        {
            transform.rotation = Quaternion.identity;
        }
        else if (orientHeadToDirection)
        {
            float targetAngle = DirectionToAngle(facingDirection);
            float smoothing = Mathf.Max(0.01f, headTurnSmoothing);
            float interpolation = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            headFacingAngle = Mathf.LerpAngle(headFacingAngle, targetAngle, interpolation);
            transform.rotation = Quaternion.Euler(0f, 0f, headFacingAngle);
        }

        if (pulseHead)
        {
            float amplitude = Mathf.Max(0f, headPulseAmplitude);
            float speed = Mathf.Max(0f, headPulseSpeed);
            float pulse = 1f + Mathf.Sin(Time.time * speed) * amplitude;
            transform.localScale = baseHeadScale * currentHeadNormalization * Mathf.Max(0.1f, headScaleMultiplier) * pulse;
        }
        else
        {
            transform.localScale = baseHeadScale * currentHeadNormalization * Mathf.Max(0.1f, headScaleMultiplier);
        }

        if (usesDirectionalHead || blinkHeadSprite == null)
        {
            return;
        }

        float currentTime = Time.time;
        if (currentTime >= nextBlinkStartTime)
        {
            blinkEndsAtTime = currentTime + Mathf.Max(0.01f, blinkDuration);
            ScheduleNextBlink();
        }

        bool isBlinking = currentTime < blinkEndsAtTime;
        headSpriteRenderer.sprite = isBlinking ? blinkHeadSprite : defaultHeadSprite;
    }

    private void ScheduleNextBlink()
    {
        float minInterval = Mathf.Max(0.1f, minBlinkInterval);
        float maxInterval = Mathf.Max(minInterval, maxBlinkInterval);
        nextBlinkStartTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    private bool ApplyHeadSprite(Vector2Int direction)
    {
        if (headSpriteRenderer == null)
        {
            return false;
        }

        if (!TryGetDirectionalHeadSprite(direction, out Sprite directionalHeadSprite))
        {
            headSpriteRenderer.sprite = defaultHeadSprite;
            RefreshHeadNormalization();
            return false;
        }

        headSpriteRenderer.sprite = directionalHeadSprite;
        RefreshHeadNormalization();
        return true;
    }

    private void ApplyBodySegmentSprite(int segmentIndex)
    {
        if (segmentIndex < 0 || segmentIndex >= bodySegmentRenderers.Count)
        {
            return;
        }

        SpriteRenderer segmentRenderer = bodySegmentRenderers[segmentIndex];
        if (segmentRenderer == null)
        {
            return;
        }

        Sprite targetSprite = GetFallbackBodySprite();
        int cellIndex = segmentIndex + 1;

        if (useDirectionalSprites)
        {
            bool isTail = cellIndex == occupiedCells.Count - 1;
            if (isTail)
            {
                if (TryGetTailSprite(cellIndex, out Sprite tailSprite))
                {
                    targetSprite = tailSprite;
                }
            }
            else if (TryGetBodySprite(cellIndex, out Sprite bodySprite))
            {
                targetSprite = bodySprite;
            }
        }

        segmentRenderer.sprite = targetSprite;
    }

    private bool TryGetDirectionalHeadSprite(Vector2Int direction, out Sprite sprite)
    {
        sprite = null;
        if (!UseDirectionalHeadSprites())
        {
            return false;
        }

        if (direction == Vector2Int.up)
        {
            sprite = headUpSprite;
        }
        else if (direction == Vector2Int.down)
        {
            sprite = headDownSprite;
        }
        else if (direction == Vector2Int.left)
        {
            sprite = headLeftSprite;
        }
        else
        {
            sprite = headRightSprite;
        }

        return sprite != null;
    }

    private bool TryGetTailSprite(int cellIndex, out Sprite sprite)
    {
        sprite = null;
        if (cellIndex <= 0 || cellIndex >= occupiedCells.Count)
        {
            return false;
        }

        Vector2Int tailCell = occupiedCells[cellIndex];
        Vector2Int neighborCell = occupiedCells[cellIndex - 1];
        Vector2Int tailDirection = tailCell - neighborCell;

        // During growth the new last cell can overlap the previous tail cell for one frame.
        // In that case infer the tail direction from the previous segment direction.
        if (tailDirection == Vector2Int.zero && cellIndex >= 2)
        {
            Vector2Int previousNeighborCell = occupiedCells[cellIndex - 2];
            tailDirection = neighborCell - previousNeighborCell;
        }

        if (tailDirection == Vector2Int.up)
        {
            sprite = tailUpSprite;
        }
        else if (tailDirection == Vector2Int.down)
        {
            sprite = tailDownSprite;
        }
        else if (tailDirection == Vector2Int.left)
        {
            sprite = tailLeftSprite;
        }
        else if (tailDirection == Vector2Int.right)
        {
            sprite = tailRightSprite;
        }

        return sprite != null;
    }

    private bool TryGetBodySprite(int cellIndex, out Sprite sprite)
    {
        sprite = null;
        if (cellIndex <= 0 || cellIndex >= occupiedCells.Count - 1)
        {
            return false;
        }

        Vector2Int currentCell = occupiedCells[cellIndex];
        Vector2Int previousCell = occupiedCells[cellIndex - 1];
        Vector2Int nextCell = occupiedCells[cellIndex + 1];

        Vector2Int toPrevious = previousCell - currentCell;
        Vector2Int toNext = nextCell - currentCell;

        // Handle temporary duplicated tail cells on growth so segment type stays coherent.
        if (toPrevious == Vector2Int.zero && toNext != Vector2Int.zero)
        {
            toPrevious = -toNext;
        }
        else if (toNext == Vector2Int.zero && toPrevious != Vector2Int.zero)
        {
            toNext = -toPrevious;
        }

        if (toPrevious == -toNext)
        {
            sprite = toPrevious.x != 0 ? bodyHorizontalSprite : bodyVerticalSprite;
            return sprite != null;
        }

        bool hasUp = toPrevious == Vector2Int.up || toNext == Vector2Int.up;
        bool hasDown = toPrevious == Vector2Int.down || toNext == Vector2Int.down;
        bool hasLeft = toPrevious == Vector2Int.left || toNext == Vector2Int.left;
        bool hasRight = toPrevious == Vector2Int.right || toNext == Vector2Int.right;

        if (hasUp && hasRight)
        {
            sprite = bodyTopRightSprite;
        }
        else if (hasUp && hasLeft)
        {
            sprite = bodyTopLeftSprite;
        }
        else if (hasDown && hasRight)
        {
            sprite = bodyBottomRightSprite;
        }
        else if (hasDown && hasLeft)
        {
            sprite = bodyBottomLeftSprite;
        }

        return sprite != null;
    }

    private bool IsCornerCell(int cellIndex)
    {
        if (cellIndex <= 0 || cellIndex >= occupiedCells.Count - 1)
        {
            return false;
        }

        Vector2Int currentCell = occupiedCells[cellIndex];
        Vector2Int previousCell = occupiedCells[cellIndex - 1];
        Vector2Int nextCell = occupiedCells[cellIndex + 1];

        Vector2Int toPrevious = previousCell - currentCell;
        Vector2Int toNext = nextCell - currentCell;

        if (toPrevious == Vector2Int.zero || toNext == Vector2Int.zero)
        {
            return false;
        }

        // Straight segment if directions are opposite.
        if (toPrevious == -toNext)
        {
            return false;
        }

        return true;
    }

    private Sprite GetFallbackBodySprite()
    {
        if (bodySpriteOverride != null)
        {
            return bodySpriteOverride;
        }

        if (bodyHorizontalSprite != null)
        {
            return bodyHorizontalSprite;
        }

        if (bodyVerticalSprite != null)
        {
            return bodyVerticalSprite;
        }

        if (headSpriteRenderer != null && headSpriteRenderer.sprite != null)
        {
            return headSpriteRenderer.sprite;
        }

        return defaultHeadSprite;
    }

    private void RefreshBodyThicknessReference()
    {
        float horizontalThickness = GetExpectedStraightThickness(bodyHorizontalSprite, true);
        float verticalThickness = GetExpectedStraightThickness(bodyVerticalSprite, false);
        float horizontalLength = GetExpectedStraightLength(bodyHorizontalSprite, true);
        float verticalLength = GetExpectedStraightLength(bodyVerticalSprite, false);

        if (horizontalThickness > 0f && verticalThickness > 0f)
        {
            bodyThicknessReference = (horizontalThickness + verticalThickness) * 0.5f;
        }
        else if (horizontalThickness > 0f)
        {
            bodyThicknessReference = horizontalThickness;
        }
        else if (verticalThickness > 0f)
        {
            bodyThicknessReference = verticalThickness;
        }
        else
        {
            bodyThicknessReference = 1f;
        }

        if (horizontalLength > 0f && verticalLength > 0f)
        {
            bodyLengthReference = (horizontalLength + verticalLength) * 0.5f;
        }
        else if (horizontalLength > 0f)
        {
            bodyLengthReference = horizontalLength;
        }
        else if (verticalLength > 0f)
        {
            bodyLengthReference = verticalLength;
        }
        else
        {
            bodyLengthReference = 1f;
        }
    }

    private void RefreshGridCellWorldSize()
    {
        if (gameManager == null)
        {
            gridCellWorldSize = 1f;
            return;
        }

        Vector3 origin = gameManager.GridToWorldPosition(Vector2Int.zero);
        Vector3 right = gameManager.GridToWorldPosition(Vector2Int.right);
        float step = Vector3.Distance(origin, right);
        gridCellWorldSize = step > 0.0001f ? step : 1f;
    }

    private float GetExpectedStraightThickness(Sprite sprite, bool horizontal)
    {
        if (sprite == null)
        {
            return 0f;
        }

        Vector2 size = sprite.bounds.size;
        if (size.x <= 0.0001f || size.y <= 0.0001f)
        {
            return 0f;
        }

        return horizontal ? size.y : size.x;
    }

    private float GetExpectedStraightLength(Sprite sprite, bool horizontal)
    {
        if (sprite == null)
        {
            return 0f;
        }

        Vector2 size = sprite.bounds.size;
        if (size.x <= 0.0001f || size.y <= 0.0001f)
        {
            return 0f;
        }

        return horizontal ? size.x : size.y;
    }

    private float GetBodyThicknessNormalization(Sprite sprite)
    {
        if (!autoNormalizeBodyThickness || sprite == null)
        {
            return 1f;
        }

        Vector2 size = sprite.bounds.size;
        if (size.x <= 0.0001f || size.y <= 0.0001f)
        {
            return 1f;
        }

        float spriteThickness = Mathf.Min(size.x, size.y);
        if (spriteThickness <= 0.0001f)
        {
            return 1f;
        }

        float spriteLength = Mathf.Max(size.x, size.y);
        if (spriteLength <= 0.0001f)
        {
            return bodyThicknessReference / spriteThickness;
        }

        float targetLength = Mathf.Max(0.0001f, gridCellWorldSize);
        float lengthScale = targetLength / spriteLength;

        float thicknessRatio = bodyLengthReference > 0.0001f
            ? bodyThicknessReference / bodyLengthReference
            : 1f;
        float targetThickness = targetLength * thicknessRatio;
        float thicknessScale = bodyThicknessReference / spriteThickness;
        if (targetThickness > 0.0001f)
        {
            thicknessScale = targetThickness / spriteThickness;
        }

        // Prefer a slightly larger scale so segments overlap cleanly without visible gaps.
        return Mathf.Max(thicknessScale, lengthScale);
    }

    private void RefreshHeadNormalization()
    {
        currentHeadNormalization = 1f;

        if (!autoNormalizeHeadSize || headSpriteRenderer == null || headSpriteRenderer.sprite == null)
        {
            return;
        }

        Vector2 size = headSpriteRenderer.sprite.bounds.size;
        if (size.x <= 0.0001f || size.y <= 0.0001f)
        {
            return;
        }

        float spriteLength = Mathf.Max(size.x, size.y);
        if (spriteLength <= 0.0001f)
        {
            return;
        }

        float targetLength = Mathf.Max(0.0001f, gridCellWorldSize);
        currentHeadNormalization = targetLength / spriteLength;
    }

    private bool UseDirectionalHeadSprites()
    {
        return headUpSprite != null
            && headDownSprite != null
            && headLeftSprite != null
            && headRightSprite != null;
    }

    private float DirectionToAngle(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
        {
            return 90f;
        }

        if (direction == Vector2Int.left)
        {
            return 180f;
        }

        if (direction == Vector2Int.down)
        {
            return -90f;
        }

        return 0f;
    }

}
