public static class InputBlocker
{
    public static bool IsInputBlocked { get; private set; }

    public static void Block() => IsInputBlocked = true;
    public static void Unblock() => IsInputBlocked = false;
}
