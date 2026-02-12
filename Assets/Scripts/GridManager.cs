using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 8;
    public int height = 8;
    public float cellSize = 1f;

    [Header("Visuals")]
    public GameObject tilePrefab;

    [Header("Gizmos")]
    public bool showSpawnPoints = true;
    public Color spawnPointColor = new Color(0.2f, 0.9f, 0.4f, 0.9f);
    public float spawnPointRadius = 0.08f;

    // 记录有效位置集合
    private readonly HashSet<Vector2Int> validPositions = new HashSet<Vector2Int>();
    // 记录每个格子上的当前实体
    private readonly Dictionary<Vector2Int, Entity> occupants = new Dictionary<Vector2Int, Entity>();

    // 网格在世界坐标中的原点（左下角格子的中心）
    private Vector3 gridOriginWorld = Vector3.zero;

    private void Awake()
    {
        CenterGridToWorld();
        BuildGrid();
    }

    // 建立坐标系并生成瓷砖
    private void BuildGrid()
    {
        validPositions.Clear();
        occupants.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var gridPos = new Vector2Int(x, y);
                validPositions.Add(gridPos);

                // 在有效位置显示瓷砖贴图
                if (tilePrefab != null)
                {
                    var worldPos = GridToWorld(gridPos);
                    var tileInstance = Instantiate(tilePrefab, worldPos, Quaternion.identity);
                    tileInstance.transform.SetParent(transform, true);
                }
            }
        }
    }

    // 网格中心位于世界中心（不移动自身物体）
    private void CenterGridToWorld()
    {
        gridOriginWorld = CalculateGridOriginWorld();
    }

    private Vector3 CalculateGridOriginWorld()
    {
        Vector3 gridWorldSize = new Vector3(width * cellSize, height * cellSize, 0f);
        Vector3 gridCenterOffset = gridWorldSize * 0.5f;
        Vector3 halfCell = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);

        // 世界中心为 (0,0,0)
        return -gridCenterOffset + halfCell;
    }

    // 将网格坐标转换为世界坐标
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return gridOriginWorld + new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0f);
    }

    // 判断坐标是否有效
    public bool IsValidPosition(Vector2Int gridPos)
    {
        return validPositions.Contains(gridPos);
    }

    // 获取指定坐标上的物体
    public Entity GetOccupant(Vector2Int gridPos)
    {
        occupants.TryGetValue(gridPos, out Entity entity);
        return entity;
    }

    // 设置指定坐标上的物体
    public void SetOccupant(Vector2Int gridPos, Entity entity)
    {
        if (!IsValidPosition(gridPos))
        {
            return;
        }

        if (entity == null)
        {
            occupants.Remove(gridPos);
            return;
        }

        occupants[gridPos] = entity;
    }

    // 清空指定坐标上的物体
    public void ClearOccupant(Vector2Int gridPos)
    {
        occupants.Remove(gridPos);
    }


    // 寻找距离给定世界坐标最近的可用位置
    public bool TryFindNearestAvailablePosition(Vector3 worldPosition, out Vector2Int nearestPosition)
    {
        nearestPosition = default;

        if (validPositions.Count == 0)
        {
            return false;
        }

        bool found = false;
        float bestDistanceSqr = float.MaxValue;

        foreach (Vector2Int gridPos in validPositions)
        {
            if (GetOccupant(gridPos) != null)
            {
                continue;
            }

            Vector3 cellWorldPos = GridToWorld(gridPos);
            float distanceSqr = (worldPosition - cellWorldPos).sqrMagnitude;

            if (!found || distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                nearestPosition = gridPos;
                found = true;
            }
        }

        return found;
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnPoints || width <= 0 || height <= 0 || cellSize <= 0f)
        {
            return;
        }

        Vector3 originWorld = CalculateGridOriginWorld();
        Gizmos.color = spawnPointColor;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 worldPos = originWorld + new Vector3(x * cellSize, y * cellSize, 0f);
                Gizmos.DrawSphere(worldPos, spawnPointRadius);
            }
        }
    }

}
