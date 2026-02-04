using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("网格设置")]
    public int gridWidth = 8;      // 网格宽度
    public int gridHeight = 8;     // 网格高度
    public float cellSize = 1f;    // 单元格大小

    [Header("移动设置")]
    public float moveSpeed = 5f;   // 移动速度（用于平滑移动）
    public bool smoothMove = true; // 是否平滑移动

    [Header("当前位置")]
    public int gridX = 0;          // 当前网格X坐标
    public int gridY = 0;          // 当前网格Y坐标

    [Header("渲染设置")]
    public int playerSortingOrder = 2; // 玩家渲染层级（高于瓷砖）

    private Vector3 targetPosition; // 目标世界坐标
    private bool isMoving = false;  // 是否正在移动中
    private SpriteRenderer spriteRenderer; // 玩家的SpriteRenderer

    void Start()
    {
        // Start is called before the first frame update
        // 初始化位置（网格中心）
        gridX = gridWidth / 2;
        gridY = gridHeight / 2;
        
        // 设置玩家渲染层级
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = playerSortingOrder;
        }
        
        // 设置初始世界坐标
        UpdatePosition();
        transform.position = targetPosition;
    }

    void Update()
    {
        // Update is called once per frame
        // 如果正在移动且使用平滑移动，不接受新输入
        if (isMoving && smoothMove)
        {
            SmoothMoveToTarget();
            return;
        }

        // 检测输入
        HandleInput();

        // 如果使用平滑移动
        if (smoothMove && isMoving)
        {
            SmoothMoveToTarget();
        }
    }

    void HandleInput()
    {
        int newX = gridX;
        int newY = gridY;

        // 检测四向输入
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            newY++; // 向上
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            newY--; // 向下
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            newX--; // 向左
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            newX++; // 向右
        }

        // 检查是否有移动输入
        if (newX != gridX || newY != gridY)
        {
            // 边界检测
            if (IsValidPosition(newX, newY))
            {
                gridX = newX;
                gridY = newY;
                UpdatePosition();
                isMoving = true;

                // 如果不使用平滑移动，直接瞬移
                if (!smoothMove)
                {
                    transform.position = targetPosition;
                    isMoving = false;
                }
            }
        }
    }

    void UpdatePosition()
    {
        // 将网格坐标转换为世界坐标
        // 假设网格中心在世界坐标(0, 0)
        float offsetX = -(gridWidth - 1) * cellSize / 2f;
        float offsetY = -(gridHeight - 1) * cellSize / 2f;
        
        targetPosition = new Vector3(
            offsetX + gridX * cellSize,
            offsetY + gridY * cellSize,
            0
        );
    }

    void SmoothMoveToTarget()
    {
        // 平滑移动到目标位置
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // 检查是否到达目标
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    bool IsValidPosition(int x, int y)
    {
        // 检查是否在网格范围内
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    // 可视化网格（仅在编辑器中显示）
    void OnDrawGizmos()
    {
        // 计算网格偏移
        float offsetX = -(gridWidth - 1) * cellSize / 2f;
        float offsetY = -(gridHeight - 1) * cellSize / 2f;

        // 绘制网格线
        Gizmos.color = Color.gray;
        
        // 竖线
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(offsetX + x * cellSize - cellSize/2, offsetY - cellSize/2, 0);
            Vector3 end = new Vector3(offsetX + x * cellSize - cellSize/2, offsetY + gridHeight * cellSize - cellSize/2, 0);
            Gizmos.DrawLine(start, end);
        }
        
        // 横线
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = new Vector3(offsetX - cellSize/2, offsetY + y * cellSize - cellSize/2, 0);
            Vector3 end = new Vector3(offsetX + gridWidth * cellSize - cellSize/2, offsetY + y * cellSize - cellSize/2, 0);
            Gizmos.DrawLine(start, end);
        }

        // 绘制当前位置（红色方块）
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(targetPosition, Vector3.one * cellSize * 0.9f);
        }
    }
}
