using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private GridManager gridManager;

    [Header("Grid Position")]
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private bool snapToGridEachFrame = true;

    public Vector2Int GridPosition => gridPosition;
    protected GridManager GridManager => gridManager;

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
        if (snapToGridEachFrame)
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

        Vector2Int targetPos = gridPosition + direction;
        if (!gridManager.IsValidPosition(targetPos))
        {
            return false;
        }

        if (gridManager.GetOccupant(targetPos) != null)
        {
            return false;
        }

        gridManager.ClearOccupant(gridPosition);
        gridPosition = targetPos;
        gridManager.SetOccupant(gridPosition, this);
        SyncWorldPosition();
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

        gridManager.ClearOccupant(gridPosition);
        gridPosition = newPosition;
        gridManager.SetOccupant(gridPosition, this);
        SyncWorldPosition();
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

    // 被攻击时的默认响应（子类可重写）
    public virtual void Onhit(Vector2Int attackDirection)
    {
    }
}
