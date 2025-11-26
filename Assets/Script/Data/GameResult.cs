public struct GameResult
{
    public bool IsClear;
    public float PlayTime;
    public int LevelAchieved;
    public int MonsterKills;

    public GameResult(bool isClear, float playTime, int levelAchieved, int monsterKills)
    {
        IsClear = isClear;
        PlayTime = playTime;
        LevelAchieved = levelAchieved;
        MonsterKills = monsterKills;
    }
}
