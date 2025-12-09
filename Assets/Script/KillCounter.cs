using UnityEngine;
using System;
using System.Collections;


public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }


    public int TotalMonsterKills { get; private set; }
    public int TotalBossKills { get; private set; }

    public int TotalKills => TotalMonsterKills + TotalBossKills;

    // KillCountUI와 연결(킬 수 변경 시 알림)
    public event Action<int> OnKillCountChanged;
    // KillCountUI와 연결(KillCounter 싱글톤 생성 시 알림)
    public static event Action<KillCounter> OnCreated;

    [SerializeField]
    private float bossEndDelay = 3f; // 보스 죽고 엔딩까지 기다릴 시간(초)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        OnCreated?.Invoke(this);
    }

    // 게임 재실행 시 카운트 초기화
    private void OnEnable()
    {
        ResetCounter();
    }

    public void AddMonsterKill()
    {
        TotalMonsterKills++;
        OnKillCountChanged?.Invoke(TotalKills);
    }

    public void AddBossKill()
    {
        TotalBossKills++;
        OnKillCountChanged?.Invoke(TotalKills);

        // 보스 처치 직후 즉시 승리 조건 검사 -> 엔딩 대기 플래그 설정(다른 방 진입 차단)
        if (GameManager.Instance != null && GameManager.Instance.IsWinConditionMet())
        {
            GameManager.Instance.MarkGameEnding();
            Debug.Log("엔딩 조건 충족: 엔딩 대기 상태로 전환됨. 방 입장 무시됨.");
        }

        // 여기서 바로 끝내지 말고, 코루틴으로 지연 실행하여 엔딩 연출 대기
        StartCoroutine(DelayEndGameCoroutine());
    }

    private IEnumerator DelayEndGameCoroutine()
    {
        yield return new WaitForSeconds(bossEndDelay);

        GameManager.Instance.CheckAndEndGame();
    }

    public void ResetCounter()
    {
        TotalMonsterKills = 0;
        TotalBossKills = 0;
        OnKillCountChanged?.Invoke(TotalKills);
    }

}
