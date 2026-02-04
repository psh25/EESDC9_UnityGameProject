using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGridManager : MonoBehaviour
{
    [Header("网格设置")]
    public int gridWidth = 8;      // 网格宽度
    public int gridHeight = 8;     // 网格高度
    public float cellSize = 1f;    // 单元格大小

    [Header("瓷砖设置")]
    public Sprite defaultTileSprite;  // 默认瓷砖图片
    public Color defaultTileColor = Color.white; // 默认瓷砖颜色
    public bool showGridLines = true; // 是否显示网格边框
    public Color gridLineColor = new Color(0.3f, 0.3f, 0.3f, 1f); // 网格线颜色

    [Header("层级设置")]
    public int tileSortingOrder = 0;  // 瓷砖渲染层级
    public string tileSortingLayer = "Default"; // 瓷砖排序层

    private GameObject tilesContainer; // 瓷砖容器
    private GameObject[,] tileObjects; // 瓷砖对象数组
    private SpriteRenderer[,] tileRenderers; // 瓷砖渲染器数组

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        // 创建瓷砖容器
        tilesContainer = new GameObject("Tiles");
        tilesContainer.transform.SetParent(transform);
        tilesContainer.transform.localPosition = Vector3.zero;

        // 初始化数组
        tileObjects = new GameObject[gridWidth, gridHeight];
        tileRenderers = new SpriteRenderer[gridWidth, gridHeight];

        // 计算网格偏移（使网格中心在(0,0)）
        float offsetX = -(gridWidth - 1) * cellSize / 2f;
        float offsetY = -(gridHeight - 1) * cellSize / 2f;

        // 创建每个瓷砖
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // 创建瓷砖对象
                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(tilesContainer.transform);
                
                // 设置位置
                Vector3 position = new Vector3(
                    offsetX + x * cellSize,
                    offsetY + y * cellSize,
                    0
                );
                tile.transform.position = position;

                // 添加SpriteRenderer组件
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = defaultTileSprite;
                sr.color = defaultTileColor;
                sr.sortingOrder = tileSortingOrder;
                sr.sortingLayerName = tileSortingLayer;

                // 调整瓷砖大小以适应单元格
                if (defaultTileSprite != null)
                {
                    // 计算缩放以匹配cellSize
                    float spriteWidth = sr.sprite.bounds.size.x;
                    float spriteHeight = sr.sprite.bounds.size.y;
                    tile.transform.localScale = new Vector3(
                        cellSize / spriteWidth,
                        cellSize / spriteHeight,
                        1
                    );
                }

                // 如果显示网格线，添加边框
                if (showGridLines)
                {
                    AddTileBorder(tile, cellSize);
                }

                // 保存引用
                tileObjects[x, y] = tile;
                tileRenderers[x, y] = sr;
            }
        }
    }

    void AddTileBorder(GameObject tile, float size)
    {
        // 创建边框对象
        GameObject border = new GameObject("Border");
        border.transform.SetParent(tile.transform);
        border.transform.localPosition = Vector3.zero;

        LineRenderer lr = border.AddComponent<LineRenderer>();
        lr.positionCount = 5;
        lr.loop = true;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridLineColor;
        lr.endColor = gridLineColor;
        lr.sortingOrder = tileSortingOrder + 1;
        lr.useWorldSpace = false;

        // 设置边框四个角的位置
        float half = size / 2f;
        lr.SetPosition(0, new Vector3(-half, -half, 0));
        lr.SetPosition(1, new Vector3(half, -half, 0));
        lr.SetPosition(2, new Vector3(half, half, 0));
        lr.SetPosition(3, new Vector3(-half, half, 0));
        lr.SetPosition(4, new Vector3(-half, -half, 0));
    }

    /// <summary>
    /// 设置指定位置的瓷砖图片
    /// </summary>
    public void SetTileSprite(int x, int y, Sprite sprite)
    {
        if (IsValidPosition(x, y) && tileRenderers[x, y] != null)
        {
            tileRenderers[x, y].sprite = sprite;
            
            // 调整大小
            if (sprite != null)
            {
                float spriteWidth = sprite.bounds.size.x;
                float spriteHeight = sprite.bounds.size.y;
                tileObjects[x, y].transform.localScale = new Vector3(
                    cellSize / spriteWidth,
                    cellSize / spriteHeight,
                    1
                );
            }
        }
    }

    /// <summary>
    /// 设置指定位置的瓷砖颜色
    /// </summary>
    public void SetTileColor(int x, int y, Color color)
    {
        if (IsValidPosition(x, y) && tileRenderers[x, y] != null)
        {
            tileRenderers[x, y].color = color;
        }
    }

    /// <summary>
    /// 获取指定位置的瓷砖对象
    /// </summary>
    public GameObject GetTile(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            return tileObjects[x, y];
        }
        return null;
    }

    /// <summary>
    /// 获取指定位置的SpriteRenderer
    /// </summary>
    public SpriteRenderer GetTileRenderer(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            return tileRenderers[x, y];
        }
        return null;
    }

    /// <summary>
    /// 设置所有瓷砖的图片
    /// </summary>
    public void SetAllTilesSprite(Sprite sprite)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                SetTileSprite(x, y, sprite);
            }
        }
    }

    /// <summary>
    /// 设置所有瓷砖的颜色
    /// </summary>
    public void SetAllTilesColor(Color color)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                SetTileColor(x, y, color);
            }
        }
    }

    /// <summary>
    /// 显示/隐藏瓷砖
    /// </summary>
    public void SetTileVisible(int x, int y, bool visible)
    {
        if (IsValidPosition(x, y) && tileObjects[x, y] != null)
        {
            tileObjects[x, y].SetActive(visible);
        }
    }

    /// <summary>
    /// 检查位置是否有效
    /// </summary>
    bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    /// <summary>
    /// 重新生成网格（运行时更新）
    /// </summary>
    public void RegenerateGrid()
    {
        // 删除旧网格
        if (tilesContainer != null)
        {
            Destroy(tilesContainer);
        }

        // 创建新网格
        CreateGrid();
    }

    /// <summary>
    /// 清除所有瓷砖（设为默认）
    /// </summary>
    public void ClearAllTiles()
    {
        SetAllTilesSprite(defaultTileSprite);
        SetAllTilesColor(defaultTileColor);
    }
}
