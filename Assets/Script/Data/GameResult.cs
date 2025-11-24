public struct GameResult
{
    public bool IsClear;
    public float PlayTimeSeconds;
    public int LevelAchieved;
    public int MonsterKills;

    public GameResult(bool isClear, float playTimeSeconds, int levelAchieved, int monsterKills)
    {
        IsClear = isClear;
        PlayTimeSeconds = playTimeSeconds;
        LevelAchieved = levelAchieved;
        MonsterKills = monsterKills;
    }
}
