using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
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

        if (turnCounter == 0)
        {
            ChooseDirection();  //抬手阶段
            turnCounter = 1;
        }
        else  //行动阶段
        {
            if (pendingDirection.HasValue)
            {
                Vector2Int targetPos = GridPosition + pendingDirection.Value;

                if (!GridManager.IsValidPosition(targetPos))
                {
                    pendingDirection = null;
                    turnCounter = 0;
                    return;
                }

                Entity occupant = GridManager.GetOccupant(targetPos);
                if (occupant is Player player)
                {
                    //Todo:造成伤害
                    Debug.Log("近战敌人攻击了玩家");
                }
                else if (occupant == null)  // 没有障碍，可以移动
                {
                    TryMove(pendingDirection.Value);
                }
                else
                {
                    // 前方有障碍，可能什么都不做
                    Debug.Log("近战敌人被阻挡");
                }

                pendingDirection = null;
            }
            turnCounter = 0;
        }
    }

    private void ChooseDirection()  //选择方向
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        System.Collections.Generic.List<Vector2Int> validDirs = new System.Collections.Generic.List<Vector2Int>();  //存储有效方向

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = GridPosition + dir;  //计算邻居位置
            if (GridManager.IsValidPosition(neighbor) && (GridManager.GetOccupant(neighbor) == null||GridManager.GetOccupant(neighbor) is Player))  // 只有空格或玩家才算有效方向
                validDirs.Add(dir);  //将有效方向添加到列表
        }

        if (validDirs.Count > 0)
        {
            pendingDirection = validDirs[Random.Range(0, validDirs.Count)];  //随机选择一个有效方向
            Debug.Log("抬手方向：" + pendingDirection);
        }
        else
        {
            pendingDirection = null;  //没有有效方向，保持原地不动
             Debug.Log("没有有效方向，近战敌人保持原地");
        }
    }
}