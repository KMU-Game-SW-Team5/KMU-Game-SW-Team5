using Firebase.Firestore;

[FirestoreData]
public class LeaderboardEntry
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Nickname { get; set; } // 유저가 입력할 예정 8글자 정도

    [FirestoreProperty]
    public int Difficulty { get; set; }   // 플레이 난이도

    [FirestoreProperty]
    public int LevelAchieved { get; set; }   // 달성 레벨

    [FirestoreProperty]
    public int MonsterKills { get; set; }    // 처치 수

    [FirestoreProperty]
    public float PlayTime { get; set; }   // 플레이타임    

    public int Rank { get; set; }   // 순위
}
