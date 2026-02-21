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
    // 计划中的危险来源（支持按来源刷新）
    private readonly List<SourceWarning> scheduledWarnings = new List<SourceWarning>();

    private class WarningEntry
    {
        public GameObject instance;
        public int executeBeat;
    }

    private class SourceWarning
    {
        public Object source;
        public Vector2Int gridPos;
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
        BeatManager.OnBeatStart += OnBeatStart;
    }

    private void OnDisable()
    {
        BeatManager.OnBeatStart -= OnBeatStart;
    }

    // 匿名入口：上报下一拍将危险的格子
    public static bool TryReportNextBeatWarning(Vector2Int gridPos)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportAnonymousWarning(gridPos, BeatManager.BeatIndex + 1);
        return true;
    }

    // 匿名入口：按指定拍号上报预警
    public static bool TryReportWarning(Vector2Int gridPos, int executeBeat)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportAnonymousWarning(gridPos, executeBeat);
        return true;
    }

    // 来源入口：同一来源会覆盖旧预警
    public static bool TryReportNextBeatWarning(Object source, Vector2Int gridPos)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportSourceWarning(source, gridPos, BeatManager.BeatIndex + 1);
        return true;
    }

    // 来源入口：按指定拍号上报，同一来源会覆盖旧预警
    public static bool TryReportWarning(Object source, Vector2Int gridPos, int executeBeat)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportSourceWarning(source, gridPos, executeBeat);
        return true;
    }

    // 来源入口：同一来源一次上报多格，覆盖该来源旧预警
    public static bool TryReportNextBeatWarnings(Object source, IReadOnlyList<Vector2Int> gridPositions)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportSourceWarnings(source, gridPositions, BeatManager.BeatIndex + 1);
        return true;
    }

    // 来源入口：按指定拍号一次上报多格
    public static bool TryReportWarnings(Object source, IReadOnlyList<Vector2Int> gridPositions, int executeBeat)
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.ReportSourceWarnings(source, gridPositions, executeBeat);
        return true;
    }

    private void ReportAnonymousWarning(Vector2Int gridPos, int executeBeat)
    {
        if (!CanAcceptWarning(gridPos, executeBeat))
        {
            return;
        }

        scheduledWarnings.Add(new SourceWarning
        {
            source = null,
            gridPos = gridPos,
            executeBeat = executeBeat
        });

        RebuildVisualWarnings();
    }

    // 同一来源仅保留一条：刷新时删除旧标记并立即生成新标记
    private void ReportSourceWarning(Object source, Vector2Int gridPos, int executeBeat)
    {
        if (source == null)
        {
            ReportAnonymousWarning(gridPos, executeBeat);
            return;
        }

        scheduledWarnings.RemoveAll(w => w.source == source);

        if (!CanAcceptWarning(gridPos, executeBeat))
        {
            RebuildVisualWarnings();
            return;
        }

        scheduledWarnings.Add(new SourceWarning
        {
            source = source,
            gridPos = gridPos,
            executeBeat = executeBeat
        });

        RebuildVisualWarnings();
    }

    // 同一来源可上报多格：适用于远程直线等连续危险区
    private void ReportSourceWarnings(Object source, IReadOnlyList<Vector2Int> gridPositions, int executeBeat)
    {
        if (source == null)
        {
            return;
        }

        scheduledWarnings.RemoveAll(w => w.source == source);

        if (gridPositions == null || executeBeat <= BeatManager.BeatIndex)
        {
            RebuildVisualWarnings();
            return;
        }

        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector2Int gridPos = gridPositions[i];
            if (!CanAcceptWarning(gridPos, executeBeat))
            {
                continue;
            }

            scheduledWarnings.Add(new SourceWarning
            {
                source = source,
                gridPos = gridPos,
                executeBeat = executeBeat
            });
        }

        RebuildVisualWarnings();
    }

    private bool CanAcceptWarning(Vector2Int gridPos, int executeBeat)
    {
        if (warningPrefab == null || gridManager == null)
        {
            return false;
        }

        if (!gridManager.IsValidPosition(gridPos))
        {
            return false;
        }

        if (executeBeat <= BeatManager.BeatIndex)
        {
            return false;
        }

        return true;
    }

    // 根据计划预警重建可见 Warning（同格合并为一个）
    private void RebuildVisualWarnings()
    {
        if (warningPrefab == null || gridManager == null)
        {
            return;
        }

        Dictionary<Vector2Int, int> aggregated = new Dictionary<Vector2Int, int>();

        foreach (SourceWarning warning in scheduledWarnings)
        {
            if (warning.executeBeat <= BeatManager.BeatIndex)
            {
                continue;
            }

            if (aggregated.TryGetValue(warning.gridPos, out int existingBeat))
            {
                aggregated[warning.gridPos] = Mathf.Min(existingBeat, warning.executeBeat);
            }
            else
            {
                aggregated[warning.gridPos] = warning.executeBeat;
            }
        }

        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, WarningEntry> pair in activeWarnings)
        {
            if (!aggregated.ContainsKey(pair.Key))
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

        foreach (KeyValuePair<Vector2Int, int> pair in aggregated)
        {
            if (activeWarnings.TryGetValue(pair.Key, out WarningEntry existingEntry))
            {
                existingEntry.executeBeat = pair.Value;
                continue;
            }

            Vector3 worldPos = gridManager.GridToWorld(pair.Key);
            GameObject warningInstance = Instantiate(warningPrefab, worldPos, Quaternion.identity, transform);

            activeWarnings[pair.Key] = new WarningEntry
            {
                instance = warningInstance,
                executeBeat = pair.Value
            };
        }
    }

    // 每拍开始时，移除本拍即将结算的预警
    private void OnBeatStart()
    {
        if (scheduledWarnings.Count == 0 && activeWarnings.Count == 0)
        {
            return;
        }

        scheduledWarnings.RemoveAll(w => w.executeBeat <= BeatManager.BeatIndex);
        RebuildVisualWarnings();
    }
}