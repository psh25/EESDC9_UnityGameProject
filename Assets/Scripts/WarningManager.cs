using System.Collections.Generic;
using UnityEngine;

public class WarningManager : MonoBehaviour
{
    [Header("Warning Settings")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private GridManager gridManager;

    public static WarningManager Instance { get; private set; }

    // 当前场上预警（按格子去重）
    private readonly Dictionary<Vector2Int, WarningEntry> activeWarnings = new Dictionary<Vector2Int, WarningEntry>();

    // 预警显示对象与其生效拍号
    private class WarningEntry
    {
        public GameObject instance;
        public int executeBeat;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    private void OnEnable()
    {
        // 在“拍开始”阶段清理将要结算的预警
        BeatManager.OnBeatStart += OnBeatStart;
    }

    private void OnDisable()
    {
        BeatManager.OnBeatStart -= OnBeatStart;
    }

    // 对外静态入口：上报下一拍将危险的格子
    public static bool TryReportNextBeatWarning(Vector2Int gridPos)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportWarning(gridPos, BeatManager.BeatIndex + 1);
        return true;
    }

    // 对外静态入口：按指定拍号上报预警
    public static bool TryReportWarning(Vector2Int gridPos, int executeBeat)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportWarning(gridPos, executeBeat);
        return true;
    }

    // 核心逻辑：创建或合并同格预警
    public void ReportWarning(Vector2Int gridPos, int executeBeat)
    {
        if (warningPrefab == null || gridManager == null)
        {
            return;
        }

        if (!gridManager.IsValidPosition(gridPos))
        {
            return;
        }

        // 只接受未来拍预警
        if (executeBeat <= BeatManager.BeatIndex)
        {
            return;
        }

        if (activeWarnings.TryGetValue(gridPos, out WarningEntry existingEntry))
        {
            // 同格已有预警时，保留更早结算的危险
            existingEntry.executeBeat = Mathf.Min(existingEntry.executeBeat, executeBeat);
            return;
        }

        Vector3 worldPos = gridManager.GridToWorld(gridPos);
        GameObject warningInstance = Instantiate(warningPrefab, worldPos, Quaternion.identity, transform);

        activeWarnings[gridPos] = new WarningEntry
        {
            instance = warningInstance,
            executeBeat = executeBeat
        };
    }

    // 每拍开始时，移除本拍即将结算的预警
    private void OnBeatStart()
    {
        if (activeWarnings.Count == 0)
        {
            return;
        }

        List<Vector2Int> toRemove = new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, WarningEntry> pair in activeWarnings)
        {
            if (pair.Value.executeBeat <= BeatManager.BeatIndex)
            {
                if (pair.Value.instance != null)
                {
                    Destroy(pair.Value.instance);
                }

                toRemove.Add(pair.Key);
            }
        }

        foreach (Vector2Int key in toRemove)
        {
            activeWarnings.Remove(key);
        }
    }
}
