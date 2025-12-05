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

    [Header("Volume")]
    [Tooltip("BGM의 전체 볼륨(0.0 ~ 1.0). 인스펙터에서 조정 가능하며 런타임에 SetVolume으로 변경하세요.")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

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

    private void InitializeAudioSource(AudioSource src)
    {
        src.playOnAwake = false;
        src.loop = true;
        src.volume = bgmVolume;
    }

    private void Start()
    {
        // 초기 BGM 재생 (원하는 곡으로 변경 가능)
        TransitionMusic(normal, 1f);
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
            other.volume = bgmVolume;
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

            other.volume = Mathf.Lerp(0f, bgmVolume, p);
            current.volume = Mathf.Lerp(startVolCurrent, 0f, p);

            yield return null;
        }

        // 전환 완료
        other.volume = bgmVolume;
        current.Stop();
        current.clip = null;

        useA = !useA;
        transitionCoroutine = null;
    }

    private AudioSource GetCurrentSource() => useA ? sourceA : sourceB;
    private AudioSource GetOtherSource() => useA ? sourceB : sourceA;

    // 외부에서 BGM 볼륨을 변경할 때 호출
    public void SetVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        bgmVolume = v;

        if (sourceA != null)
        {
            sourceA.volume = sourceA.isPlaying ? bgmVolume : bgmVolume;
        }
        if (sourceB != null)
        {
            sourceB.volume = sourceB.isPlaying ? bgmVolume : bgmVolume;
        }
    }

    // 현재 엔딩 음악을 재생하고 있는지
    public bool IsPlayingEndingMusic()
    {
        var current = GetCurrentSource();
        return current.clip == victory || current.clip == defeat;
    }

    // 현재 설정된 볼륨 반환
    public float GetVolume() => bgmVolume;
}