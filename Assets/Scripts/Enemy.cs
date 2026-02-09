using UnityEngine;

public class Enemy : Entity
{
    // 死亡处理：删除自身
    public void Die()
    {
        if (GridManager != null)
        {
            GridManager.ClearOccupant(GridPosition);
        }

        Destroy(gameObject);
    }

    // 被攻击时的响应
    public override void Onhit(Vector2Int attackDirection)
    {
        if (GridManager == null)
        {
            return;
        }

        Vector2Int targetPos = GridPosition + attackDirection;
        if (!GridManager.IsValidPosition(targetPos))
        {
            return;
        }

        Entity target = GridManager.GetOccupant(targetPos);
        if (target is Box || target is Player)
        {
            Die();
            return;
        }

        if (target is Enemy enemy)
        {
            enemy.Die();
            Die();
            return;
        }

        TryMove(attackDirection);
    }
}
