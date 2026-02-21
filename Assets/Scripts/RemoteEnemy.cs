using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteEnemy : Enemy
{
    private int turnCounter = 0;               // 0:抬手回合, 1:行动回合
    private Vector2Int? pendingDirection = null;
    private int pendingWarningExecuteBeat = -1;

    public override void PerformAction()
    {
        if (GridManager == null)
            return;

        if (turnCounter == 0)  //抬手回合
        {
            ChooseDirection();
            ReportNextBeatWarnings(); // 抬手后立刻上报下一拍预警
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
                        player.Onhit(pendingDirection.Value);
                    }
                    
                    currentHit += pendingDirection.Value;  // 继续向前检查
                    Debug.Log("当前激光位置：" + currentHit);
                }

                pendingWarningExecuteBeat = -1;
                pendingDirection = null;
                turnCounter = 0;
            }
        }
    }

    protected override void OnMovedByTryMove(Vector2Int oldPos, Vector2Int newPos)
    {
        base.OnMovedByTryMove(oldPos, newPos);

        // 抬手阶段内被迫移动：整条预警线立刻刷新到新位置，结算拍号保持不变
        if (turnCounter != 1 || !pendingDirection.HasValue)
        {
            return;
        }

        if (pendingWarningExecuteBeat <= BeatManager.BeatIndex)
        {
            return;
        }

        ReportPendingWarningsAtCurrentPosition();
    }

    // 上报远程敌人下一拍的危险格（沿抬手方向直到被阻挡）
    private void ReportNextBeatWarnings()
    {
        pendingWarningExecuteBeat = BeatManager.BeatIndex + 1;
        ReportPendingWarningsAtCurrentPosition();
    }

    private void ReportPendingWarningsAtCurrentPosition()
    {
        if (GridManager == null || !pendingDirection.HasValue)
        {
            return;
        }

        List<Vector2Int> warningCells = new List<Vector2Int>();
        Vector2Int current = GridPosition + pendingDirection.Value;
        while (GridManager.IsValidPosition(current))
        {
            warningCells.Add(current);
            current += pendingDirection.Value;
        }

        WarningManager.TryReportWarnings(this, warningCells, pendingWarningExecuteBeat);
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
