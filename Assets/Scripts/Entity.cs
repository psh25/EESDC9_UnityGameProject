using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private GridManager gridManager;

    [Header("Grid Position")]
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private bool snapToGridEachFrame = true;

    [Header("Movement")]
    [SerializeField] private bool useSmoothMove = false;
    [SerializeField] private float moveDuration = 0.15f;

    public Vector2Int GridPosition => gridPosition;
    protected GridManager GridManager => gridManager;

    private Coroutine smoothMoveRoutine;
    private bool isMoving;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    private void Start()
    {
        RegisterAtCurrentPosition();
        SyncWorldPosition();
    }

    private void LateUpdate()
    {
        if (snapToGridEachFrame && !isMoving)
        {
            SyncWorldPosition();
        }
    }

    // 向上移动一格
    public bool MoveUp()
    {
        return TryMove(Vector2Int.up);
    }

    // 向下移动一格
    public bool MoveDown()
    {
        return TryMove(Vector2Int.down);
    }

    // 向左移动一格
    public bool MoveLeft()
    {
        return TryMove(Vector2Int.left);
    }

    // 向右移动一格
    public bool MoveRight()
    {
        return TryMove(Vector2Int.right);
    }

    // 尝试按方向移动一格
    public bool TryMove(Vector2Int direction)
    {
        if (gridManager == null)
        {
            return false;
        }

        if (isMoving)
        {
            return false;
        }

        Vector2Int targetPos = gridPosition + direction;
        if (!gridManager.IsValidPosition(targetPos))
        {
            return false;
        }

        if (gridManager.GetOccupant(targetPos) != null)
        {
            return false;
        }

        ApplyMoveToGridPosition(targetPos);
        return true;
    }

    // 设置网格坐标并更新占用信息
    public void SetGridPosition(Vector2Int newPosition)
    {
        if (gridManager == null)
        {
            gridPosition = newPosition;
            SyncWorldPosition();
            return;
        }

        if (!gridManager.IsValidPosition(newPosition))
        {
            return;
        }

        ApplyMoveToGridPosition(newPosition);
    }

    // 注册实体到当前坐标
    private void RegisterAtCurrentPosition()
    {
        if (gridManager == null)
        {
            return;
        }

        if (!gridManager.IsValidPosition(gridPosition))
        {
            return;
        }

        gridManager.SetOccupant(gridPosition, this);
    }

    // 根据坐标同步世界位置
    private void SyncWorldPosition()
    {
        if (gridManager == null)
        {
            return;
        }

        transform.SetParent(gridManager.transform, true);
        transform.localPosition = gridManager.GridToWorld(gridPosition);
    }

    private void ApplyMoveToGridPosition(Vector2Int targetPos)
    {
        gridManager.ClearOccupant(gridPosition);
        gridPosition = targetPos;
        gridManager.SetOccupant(gridPosition, this);

        if (useSmoothMove && moveDuration > 0f)
        {
            StartSmoothMove();
        }
        else
        {
            SyncWorldPosition();
        }
    }

    private void StartSmoothMove()
    {
        if (gridManager == null)
        {
            return;
        }

        if (smoothMoveRoutine != null)
        {
            StopCoroutine(smoothMoveRoutine);
        }

        smoothMoveRoutine = StartCoroutine(SmoothMoveRoutine(gridManager.GridToWorld(gridPosition), moveDuration));
    }

    private IEnumerator SmoothMoveRoutine(Vector3 targetLocalPosition, float duration)
    {
        isMoving = true;
        transform.SetParent(gridManager.transform, true);

        Vector3 startLocalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, t);
            yield return null;
        }

        transform.localPosition = targetLocalPosition;
        isMoving = false;
        smoothMoveRoutine = null;
    }

    // 被攻击时的默认响应（子类可重写）
    public virtual void Onhit(Vector2Int attackDirection)
    {
    }
}
