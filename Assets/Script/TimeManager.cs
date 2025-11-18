using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    // 던전에 밤낮은 없지만, 일단 밤/낮으로 표현
    [SerializeField] float dayDuration = 240f;
    [SerializeField] float nightDuration = 360f;

    public event Action<bool> OnWaveChanged;   // true: 낮, false: 밤
    public event Action<float> OnCycleProgress;  // 0~1 진행도 (원판 회전에 사용)
    public event Action<float> OnDayRatioChanged;   // DayRatio 변경 시 호출

    double elapsed; // 누적 시간
    bool isDay = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        elapsed += dt;

        float fullCycle = dayDuration + nightDuration;
        float t = (float)(elapsed % fullCycle);
        float progress = t / fullCycle;

        OnCycleProgress?.Invoke(progress);

        bool changed = isDay != (t < dayDuration);

        if (changed)
        {
            isDay = !isDay;
            OnWaveChanged?.Invoke(isDay);
        }
    }

    public float DayRatio => dayDuration / (dayDuration + nightDuration);

    public void SetDuration(float dd, float nd)
    {
        dayDuration = Mathf.Max(0.01f, dd);
        nightDuration = Mathf.Max(0.01f, nd);

        OnDayRatioChanged?.Invoke(DayRatio);
    }

    /*
    활용 예시
    void OnEnable()
    {
        TimeManager.Instance.OnWaveChanged += ApplyWave;
        TimeManager.Instance.OnCycleProgress += ApplyCycleProgress;
        TimeManager.Instance.OnDayRatioChanged += ApplyRatioChange;
    }
    void OnDisable()
    {
        TimeManager.Instance.OnWaveChanged -= ApplyWave;
        TimeManager.Instance.OnCycleProgress -= ApplyCycleProgress;
        TimeManager.Instance.OnDayRatioChanged -= ApplyRatioChange;
    }

    void ApplyWave(bool isDay)
    {
        if (isDay) { }
        else { }
        TimeManager.Instance.SetDuration(5f, 10f);
    }

    void ApplyCycleProgress(float progress)
    {
        Debug.Log(progress);
    }

    void ApplyRatioChange(float ratio)
    {
        if (ratio < TimeManager.Instance.DayRatio) Debug.Log("밤이 길어집니다.");
    }
    */

}
