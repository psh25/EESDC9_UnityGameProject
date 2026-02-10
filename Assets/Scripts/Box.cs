using UnityEngine;

public class Box : Entity
{
    // 被攻击时尝试沿攻击方向移动一格
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
            return;
        }

        if (target is Enemy enemy)
        {
            enemy.Die();
        }

        TryMove(attackDirection);
    }
}
