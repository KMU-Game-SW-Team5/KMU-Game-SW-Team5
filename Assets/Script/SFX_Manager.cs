using UnityEngine;

public class SFX_Manager : MonoBehaviour
{
    public static SFX_Manager Instance { get; private set; }

    [Header("UI SFX Clips")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip paperClip;
    [SerializeField] private AudioClip selectClip;

    [Header("Settings")]
    [SerializeField] private float volume = 1f;
    [SerializeField] private AudioSource audioSource; // Inspector에 할당하거나 Awake에서 추가

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 필요하면 씬 전환 유지
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    public void PlayClick() => PlayOneShot(clickClip);
    public void PlayHover() => PlayOneShot(hoverClip);
    public void PlayOpen() => PlayOneShot(openClip);
    public void PlayClose() => PlayOneShot(closeClip);

    public void PlayPaper() => PlayOneShot(openClip);
    public void PlaySelect() => PlayOneShot(selectClip);

    public void PlayOneShot(AudioClip clip, float vol = 1f)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, Mathf.Clamp01(vol * volume));
    }

    // 범용 재생
    public void Play(AudioClip clip) => PlayOneShot(clip);
}