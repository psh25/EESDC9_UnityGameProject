using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteEnemy : Enemy
{
    [Header("Action Settings")]
    [SerializeField] private float actionCooldown = 1f;  // 行动间隔（秒）
    private float nextActionTime;

    private int turnCounter = 0;               // 0:抬手回合, 1:行动回合
    private Vector2Int? pendingDirection = null;

    private void Update()
    {
        if (Time.time < nextActionTime)
            return;

        PerformAction();
        nextActionTime = Time.time + actionCooldown;
    }

    private void PerformAction()
    {
        if (GridManager == null)
            return;

        if (turnCounter == 0)  //抬手回合
        {
            ChooseDirection();
            turnCounter = 1;
        }
        else  //行动回合
        {
            if (pendingDirection.HasValue)
            {
                Vector2Int currentHit = GridPosition + pendingDirection.Value;
                while (GridManager.IsValidPosition(currentHit))
                {
                    Entity occupant = GridManager.GetOccupant(currentHit);
                    if (occupant is Player player)
                    {
                        //Todo:造成伤害
                        Debug.Log("远程敌人攻击玩家");
                        break;
                    }
                    else if (occupant != null)
                    {
                        // 遇到其他实体（如墙），停止攻击
                        Debug.Log("远程敌人攻击被阻挡");
                        break;
                    }
                    currentHit += pendingDirection.Value;  // 继续向前检查
                    Debug.Log("当前激光位置：" + currentHit);
                }
                turnCounter = 0;
            }
        }
    }

    private void ChooseDirection()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        System.Collections.Generic.List<Vector2Int> validDirs = new System.Collections.Generic.List<Vector2Int>();  // 存储有效方向

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = GridPosition + dir;
            if (GridManager.IsValidPosition(neighbor) && (GridManager.GetOccupant(neighbor) == null || GridManager.GetOccupant(neighbor) is Player))  // 只有空格或玩家才算有效方向
                validDirs.Add(dir);
        }

        if (validDirs.Count > 0)
        {
            pendingDirection = validDirs[Random.Range(0, validDirs.Count)];  // 从有效方向中随机选择一个
            Debug.Log("抬手方向：" + pendingDirection);
        }
        else
        {
            pendingDirection = null;  // 没有有效方向，远程敌人将不会攻击
            Debug.Log("远程敌人没有有效方向可抬手");
        }
    }
}
