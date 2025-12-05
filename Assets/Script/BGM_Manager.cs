using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM_Manager : MonoBehaviour
{
    // 싱글톤
    public static BGM_Manager Instance { get; private set; }

    [Header("BGM Clips")]
    [SerializeField] private AudioClip normal;
    [SerializeField] private AudioClip combat;
    [SerializeField] private AudioClip boss;
    [SerializeField] private AudioClip victory;
    [SerializeField] private AudioClip defeat;

    [Header("Transition")]
    [SerializeField] private float transitionTime = 1f;

    // 내부 오디오 소스 2개로 크로스페이드 처리
    private AudioSource sourceA;
    private AudioSource sourceB;
    private bool useA = true;

    // 전환 코루틴 참조 (중복 실행 방지)
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            // 필요하면 씬 전환 시에도 유지: 주석 해제 가능
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 준비
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        InitializeAudioSource(sourceA);
        InitializeAudioSource(sourceB);
    }

    // 씬이 로드되었을 때 기본 BGM 재생
    private void Start()
    {
        if (normal != null)
        {
            PlayNormal();
        }
    }

    private void InitializeAudioSource(AudioSource src)
    {
        src.playOnAwake = false;
        src.loop = true;
        src.volume = 1f;
    }

    // 외부에서 호출하는 전환 함수
    public void TransitionMusic(AudioClip clip, float transitionTime)
    {
        if (clip == null) return;

        // 같은 곡이면 아무 것도 하지 않음
        var current = GetCurrentSource();
        if (current.clip == clip && current.isPlaying) return;

        // 이전 전환 취소
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionCoroutine(clip, transitionTime));
    }

    // 편의 함수: 기본 transitionTime 사용
    public void TransitionMusic(AudioClip clip)
    {
        TransitionMusic(clip, this.transitionTime);
    }

    // 편의 호출용: 미리 정의된 클립 재생
    public void PlayNormal() => TransitionMusic(normal, transitionTime);
    public void PlayCombat() => TransitionMusic(combat, transitionTime);
    public void PlayBoss() => TransitionMusic(boss, transitionTime);
    public void PlayVictory()
    {
        TransitionMusic(victory, 0f);
        Debug.Log("Victory BGM played.");
    }
    public void PlayDefeat()
    {
        TransitionMusic(defeat, 0f);
        Debug.Log("Defeat BGM played.");
    }
    private IEnumerator TransitionCoroutine(AudioClip clip, float tTime)
    {
        var current = GetCurrentSource();
        var other = GetOtherSource();

        if (tTime <= 0f)
        {
            // 즉시 전환
            current.Stop();
            other.clip = clip;
            other.volume = 1f;
            other.Play();
            useA = !useA;
            yield break;
        }

        other.clip = clip;
        other.volume = 0f;
        other.Play();

        float timer = 0f;
        float startVolCurrent = current.isPlaying ? current.volume : 0f;

        while (timer < tTime)
        {
            timer += Time.deltaTime;
            float p = Mathf.Clamp01(timer / tTime);

            other.volume = Mathf.Lerp(0f, 1f, p);
            current.volume = Mathf.Lerp(startVolCurrent, 0f, p);

            yield return null;
        }

        // 전환 완료
        other.volume = 1f;
        current.Stop();
        current.clip = null;

        useA = !useA;
        transitionCoroutine = null;
    }

    private AudioSource GetCurrentSource() => useA ? sourceA : sourceB;
    private AudioSource GetOtherSource() => useA ? sourceB : sourceA;

    public bool IsPlayingEndingMusic()
    {
        var current = GetCurrentSource();
        return current.clip == victory || current.clip == defeat;
    }
}