using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 节拍管理器：使用 DSP 时间驱动拍点，避免 deltaTime 累积误差
public class BeatManager : MonoBehaviour
{
    // 全局单例，跨场景保持唯一
    public static BeatManager Instance { get; private set; }

    [Header("Beat Settings")]
    [SerializeField] public float bpm = 122f;
    // 首拍偏移
    [SerializeField] private float firstBeatOffsetSeconds = 0.1f;
    // 无音乐时自动起拍
    [SerializeField] private bool autoStartWithoutMusic = false;

    // 当前歌曲已运行时间（秒）
    public float gametime = 0f;
    private float beatInterval;
    // 歌曲计划开始的 DSP 时间
    private double songStartDspTime;
    // 已处理的最后一拍（从 0 开始）
    private int lastProcessedBeat = -1;
    // 当前是否正在进行节拍驱动
    private bool songRunning = false;
    // 暂停时缓存已播放时长（秒），用于恢复后继续原节拍位置
    private float pausedElapsedSeconds = 0f;
    // 当前是否处于暂停状态
    private bool isSongPaused = false;

    // 对外拍号（从 1 开始）
    public static int BeatIndex { get; private set; } = 0;
    // 拍点开始事件
    public static event System.Action OnBeatStart;
    // 拍点通用事件
    public static event System.Action OnBeat;

    // 单例初始化：保留首个实例并跨场景存活
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 初始化拍长，可选在无音乐情况下自动起拍
    private void Start()
    {
        beatInterval = 60f / bpm;
        BeatIndex = 0;

        if (autoStartWithoutMusic)
        {
            StartSong(bpm, AudioSettings.dspTime + 0.05d, firstBeatOffsetSeconds);
        }
    }

    // 由外部（如 AudioManager）在音乐定时播放时调用，保证同一时间基准
    public void StartSong(double startDspTime)
    {
        StartSong(bpm, startDspTime, firstBeatOffsetSeconds);
    }

    // 由外部（如 AudioManager）在音乐定时播放时调用，保证同一时间基准
    public void StartSong(float targetBpm, double startDspTime, float beatOffsetSeconds = 0f)
    {
        bpm = targetBpm;
        beatInterval = 60f / bpm;
        firstBeatOffsetSeconds = beatOffsetSeconds;
        songStartDspTime = startDspTime;
        lastProcessedBeat = -1;
        BeatIndex = 0;
        gametime = 0f;
        songRunning = true;
        isSongPaused = false;
        pausedElapsedSeconds = 0f;
    }

    // 暂停节拍推进，保留当前拍点进度用于恢复
    public void PauseSong()
    {
        if (!songRunning)
        {
            return;
        }

        double elapsedSeconds = AudioSettings.dspTime - songStartDspTime - firstBeatOffsetSeconds;
        pausedElapsedSeconds = Mathf.Max(0f, (float)elapsedSeconds);
        gametime = pausedElapsedSeconds;
        songRunning = false;
        isSongPaused = true;
    }

    // 从暂停点恢复节拍推进，保持拍点连续
    public void ResumeSong()
    {
        if (!isSongPaused)
        {
            return;
        }

        songStartDspTime = AudioSettings.dspTime - pausedElapsedSeconds - firstBeatOffsetSeconds;
        songRunning = true;
        isSongPaused = false;
    }

    // 停止节拍并重置状态
    public void StopSong()
    {
        songRunning = false;
        isSongPaused = false;
        pausedElapsedSeconds = 0f;
        lastProcessedBeat = -1;
        BeatIndex = 0;
        gametime = 0f;
    }

    // 每帧根据 DSP 时间反推当前拍号，并补发可能跨帧丢失的拍点
    private void Update()
    {
        if (!songRunning)
        {
            return;
        }

        // elapsedSeconds < 0 表示还未到计划开始时间
        double elapsedSeconds = AudioSettings.dspTime - songStartDspTime - firstBeatOffsetSeconds;
        gametime = Mathf.Max(0f, (float)elapsedSeconds);

        if (elapsedSeconds < 0d)
        {
            return;
        }

        // 计算当前应处于第几拍（从 0 开始）
        int currentBeat = Mathf.FloorToInt((float)(elapsedSeconds / beatInterval));

        // 低帧率时可能一次跨过多拍，这里用 while 补齐事件
        while (lastProcessedBeat < currentBeat)
        {
            lastProcessedBeat++;

            // 对外拍号从 1 开始
            BeatIndex = lastProcessedBeat + 1;
            OnBeatStart?.Invoke();
            OnBeat?.Invoke();
        }
    }
}
