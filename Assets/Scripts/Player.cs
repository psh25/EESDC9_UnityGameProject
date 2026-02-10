using UnityEngine;

public class Player : Entity
{
    [Header("Move Settings")]
    [SerializeField] private float moveCooldown = 0.2f;

    private float nextMoveTime;

    private void Update()
    {
        if (Time.time < nextMoveTime)
        {
            return;
        }

        if (!TryGetInputDirection(out Vector2Int direction))
        {
            return;
        }

        if (GridManager == null)
        {
            return;
        }

        Vector2Int targetPos = GridPosition + direction;
        if (!GridManager.IsValidPosition(targetPos))
        {
            return;
        }

        Entity target = GridManager.GetOccupant(targetPos);
        if (target != null)
        {
            // 目标格子有实体则攻击
            target.Onhit(direction);
            nextMoveTime = Time.time + moveCooldown;
            return;
        }

        if (TryMove(direction))
        {
            nextMoveTime = Time.time + moveCooldown;
        }
    }

    // 获取输入方向（WASD 与方向键）
    private bool TryGetInputDirection(out Vector2Int direction)
    {
        direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector2Int.up;
            return true;
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector2Int.down;
            return true;
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector2Int.left;
            return true;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector2Int.right;
            return true;
        }

        return false;
    }
}
