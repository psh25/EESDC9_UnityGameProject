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

    // 记录有效位置集合
    private readonly HashSet<Vector2Int> validPositions = new HashSet<Vector2Int>();
    // 记录每个格子上的当前实体
    private readonly Dictionary<Vector2Int, Entity> occupants = new Dictionary<Vector2Int, Entity>();

    private void Awake()
    {
        BuildGrid();
        CenterGridToScreen();
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
                    Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                }
            }
        }
    }

    // 网格中心位于屏幕中心（依赖主摄像机）
    private void CenterGridToScreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        // 计算网格中心的世界坐标偏移
        Vector3 gridWorldSize = new Vector3(width * cellSize, height * cellSize, 0f);
        Vector3 gridCenterOffset = new Vector3(gridWorldSize.x, gridWorldSize.y, 0f) * 0.5f;

        // 屏幕中心对应的世界坐标
        Vector3 screenCenterWorld = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, Mathf.Abs(cam.transform.position.z)));
        screenCenterWorld.z = 0f;

        // 将网格移动到屏幕中心
        transform.position = screenCenterWorld - gridCenterOffset + new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
    }

    // 将网格坐标转换为世界坐标
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0f);
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
}
