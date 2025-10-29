using System;

public static class EventManager
{

    public static event Action OnMonsterHit;

    public static void MonsterHit()
    {
        OnMonsterHit?.Invoke();
    }
}