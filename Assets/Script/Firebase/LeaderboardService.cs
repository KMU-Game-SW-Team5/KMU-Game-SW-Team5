using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static LeaderboardService Instance { get; private set; }

    public LeaderboardEntry LastSubmittedEntry { get; private set; }

    private FirebaseFirestore db;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task<bool> SubmitResultAsync(GameResult result, string nickname)
    {
        var entry = new LeaderboardEntry
        {
            Nickname = nickname,
            Difficulty = SettingsService.GameDifficulty,
            LevelAchieved = result.LevelAchieved,
            MonsterKills = result.MonsterKills,
            PlayTime = result.PlayTime            
        };        

        try
        {
            var docRef = await db.Collection("leaderboard").AddAsync(entry);
            entry.Id = docRef.Id;
            LastSubmittedEntry = entry; // player entry 저장

            int rank = await GetGlobalRankAsync(entry);
            entry.Rank = rank;

            Debug.Log("리더보드 업로드 성공");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"리더보드 업로드 실패: {e}");
            return false;
        }
    }

    public async Task<List<LeaderboardEntry>> LoadTopAsync(int limit = 10)
    {
        var resultList = new List<LeaderboardEntry>();

        try
        {
            // 랭킹 기준: 플레이타임 오름차순 -> 레벨 내림차순 -> 킬수 내림차순
            var query = db.Collection("leaderboard")
                .OrderBy("PlayTime")
                .OrderByDescending("LevelAchieved")
                .OrderByDescending("MonsterKills")                
                .Limit(limit);

            var snapshot = await query.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var entry = doc.ConvertTo<LeaderboardEntry>();
                resultList.Add(entry);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"리더보드 로딩 실패: {e}");
        }

        return resultList;
    }

    public async Task<int> GetGlobalRankAsync(LeaderboardEntry me)
    {
        // 제일 간단한 방법(전체 카운트)
        var query = db.Collection("leaderboard")
            .OrderBy("PlayTime")
            .OrderByDescending("LevelAchieved")
            .OrderByDescending("MonsterKills");

        var snapshot = await query.GetSnapshotAsync();

        int rank = 1;
        foreach (var doc in snapshot.Documents)
        {
            var entry = doc.ConvertTo<LeaderboardEntry>();

            if (entry.PlayTime == me.PlayTime &&
                entry.LevelAchieved == me.LevelAchieved &&
                entry.MonsterKills == me.MonsterKills &&
                entry.Nickname == me.Nickname) // or Id 비교
            {
                return rank;
            }

            rank++;
        }

        return rank;
    }

}
