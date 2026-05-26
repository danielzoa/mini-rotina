namespace TimerInfantilWindows.Models;

public sealed class ScheduleSetting
{
    public ScheduleSetting(string activityKey, bool isEnabled, TimeSpan time)
    {
        ActivityKey = activityKey;
        IsEnabled = isEnabled;
        Time = time;
    }

    public string ActivityKey { get; }

    public bool IsEnabled { get; }

    public TimeSpan Time { get; }
}
