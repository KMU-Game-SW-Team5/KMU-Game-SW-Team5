using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    // 던전에 밤낮은 없지만, 일단 밤/낮으로 표현
    public float fullCycle = 600f;  //  하루: 600초

    public event Action<bool> OnWaveChanged;   // true: 낮, false: 밤
    public event Action<float> OnProgress;  // 0~1 진행도 (원판 회전에 사용)

    double elapsed; // 누적 시간
    int wave = 1;  // 0: 밤, 1: 낮
    float Half => fullCycle * 0.5f;

    void Update()
    {
        float dt = Time.deltaTime;
        elapsed += dt;

        float progress = (float)(elapsed % fullCycle) / fullCycle;
        OnProgress?.Invoke(progress);

        int newWave = ((int)Math.Floor((elapsed % fullCycle) / Half)) == 0 ? 1 : 0;
        if (newWave != wave)
        {
            wave = newWave;
            OnWaveChanged?.Invoke(wave == 1);
        }
    }
    public void AddProgressListener(Action<float> listener)
    {
        OnProgress -= listener;
        OnProgress += listener;
    }
    public void RemoveProgressListener(Action<float> listener)
    {
        OnProgress -= listener;
    }
    public void AddWaveChangedListener(Action<bool> listener)
    {
        OnWaveChanged -= listener; // 중복 방지
        OnWaveChanged += listener;
    }
    public void RemoveWaveChangedListener(Action<bool> listener)
    {
        OnWaveChanged -= listener;
    }
    
    /*
    활용 예시
    void OnEnable()  { timeManager.AddWaveChangedListener += ApplyWave; }
    void OnDisable() { timeManager.RemoveWaveChangedListener -= ApplyWave; }

    void ApplyWave(bool isDay)
    {
        if(isDay){}
        else {}
    }
    */
}
