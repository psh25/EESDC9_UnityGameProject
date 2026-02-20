using UnityEngine;

public class Enemy : Entity
{
    public int health = 1;

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
        if (target is Box || target is Player || target is Firewall)
        {
            return;
        }

        if (target is Enemy enemy)
        {
            enemy.health --;
            health --;
            if (enemy.health <= 0)
            {
                enemy.Die();
            }
            if (health <= 0)
            {
                Die();
            }
            return;
        }

        TryMove(attackDirection);
    }
}
