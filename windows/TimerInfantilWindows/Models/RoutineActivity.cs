namespace TimerInfantilWindows.Models;

public sealed class RoutineActivity
{
    public RoutineActivity(string key, string name, TimeSpan duration, bool isFreePlay)
    {
        Key = key;
        Name = name;
        Duration = duration;
        IsFreePlay = isFreePlay;
    }

    public string Key { get; }

    public string Name { get; }

    public TimeSpan Duration { get; }

    public bool IsFreePlay { get; }
}
