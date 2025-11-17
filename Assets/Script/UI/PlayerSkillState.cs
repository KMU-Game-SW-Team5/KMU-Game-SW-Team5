public class PlayerSkillState
{
    public SkillData data; // 어떤 스킬인지
    public int level;      // 현재 레벨
    public bool unlocked;

    public PlayerSkillState(SkillData data, int level = 0, bool unlocked = false)
    {
        this.data = data;
        this.level = level;
        this.unlocked = unlocked;
    }
}
