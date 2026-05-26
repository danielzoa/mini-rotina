using System.IO;
using TimerInfantilWindows.Models;

namespace TimerInfantilWindows.Settings;

public sealed class AppSettingsStore
{
    private readonly string themeFilePath;
    private readonly string schedulesFilePath;
    private readonly string alarmSoundFilePath;

    public AppSettingsStore()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "TimerInfantilWindows");

        Directory.CreateDirectory(folder);
        themeFilePath = Path.Combine(folder, "theme.txt");
        schedulesFilePath = Path.Combine(folder, "schedules.txt");
        alarmSoundFilePath = Path.Combine(folder, "alarm-sound.txt");
    }

    // Keeps the chosen theme local and tiny for this MVP.
    public string GetThemeName()
    {
        if (!File.Exists(themeFilePath))
        {
            return "pink";
        }

        string themeName = File.ReadAllText(themeFilePath).Trim();
        return string.IsNullOrWhiteSpace(themeName) ? "pink" : themeName;
    }

    public void SaveThemeName(string themeName)
    {
        File.WriteAllText(themeFilePath, themeName);
    }

    public Dictionary<string, ScheduleSetting> GetSchedules()
    {
        Dictionary<string, ScheduleSetting> schedules = new();

        if (!File.Exists(schedulesFilePath))
        {
            return schedules;
        }

        foreach (string line in File.ReadAllLines(schedulesFilePath))
        {
            string[] parts = line.Split('|');
            if (parts.Length != 3)
            {
                continue;
            }

            bool isEnabled = parts[1].Equals("1", StringComparison.Ordinal);
            if (!TimeSpan.TryParse(parts[2], out TimeSpan time))
            {
                continue;
            }

            schedules[parts[0]] = new ScheduleSetting(parts[0], isEnabled, time);
        }

        return schedules;
    }

    public void SaveSchedules(IEnumerable<ScheduleSetting> schedules)
    {
        IEnumerable<string> lines = schedules.Select(schedule =>
            $"{schedule.ActivityKey}|{(schedule.IsEnabled ? "1" : "0")}|{schedule.Time:hh\\:mm}");

        File.WriteAllLines(schedulesFilePath, lines);
    }

    public string GetAlarmSoundName()
    {
        if (!File.Exists(alarmSoundFilePath))
        {
            return "soft";
        }

        string soundName = File.ReadAllText(alarmSoundFilePath).Trim();
        return string.IsNullOrWhiteSpace(soundName) ? "soft" : soundName;
    }

    public void SaveAlarmSoundName(string soundName)
    {
        File.WriteAllText(alarmSoundFilePath, soundName);
    }
}
