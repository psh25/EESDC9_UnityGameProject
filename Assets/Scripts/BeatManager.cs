using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public float bpm = 120f;
    private float beatInterval;
    private float beatTimer = 0f;
    public static event System.Action OnBeat;

    // Start is called before the first frame update
    void Start()
    {
        beatInterval = 60f / bpm;//计算每拍的时间间隔
    }

    // Update is called once per frame
    void Update()
    {
        //每拍触发OnBeat事件
        beatTimer += Time.deltaTime;
        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval;
            OnBeat?.Invoke();
        }
    }
}
