using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TimerInfantilWindows.Models;
using TimerInfantilWindows.Rewards;
using TimerInfantilWindows.Settings;
using TimerInfantilWindows.Sounds;

namespace TimerInfantilWindows;

public partial class MainWindow : Window
{
    private static readonly ColorTheme PinkTheme = new(
        name: "pink",
        background: "#FFF0F6",
        textPrimary: "#352B34",
        textSecondary: "#6B5864",
        badgeBorder: "#F9A8D4",
        action: "#F472B6",
        secondaryAction: "#FDE2F0",
        progressBackground: "#FDE2F0",
        progressFill: "#F472B6",
        toggle: "#FFB3C7",
        activityColors: new[]
        {
            "#FBCFE8",
            "#CDEBFF",
            "#FFE4A3",
            "#D8C8FF",
            "#CFF5D2",
            "#BFD3FF",
            "#FFB3C7",
            "#FFD0E2"
        });

    private static readonly ColorTheme BlueTheme = new(
        name: "blue",
        background: "#EEF7FF",
        textPrimary: "#243447",
        textSecondary: "#526A80",
        badgeBorder: "#93C5FD",
        action: "#60A5FA",
        secondaryAction: "#D9EEFF",
        progressBackground: "#D9EEFF",
        progressFill: "#3B82F6",
        toggle: "#A7D8FF",
        activityColors: new[]
        {
            "#B9F3E4",
            "#A7D8FF",
            "#FFE39E",
            "#C9D8FF",
            "#BFE8C4",
            "#AABDE8",
            "#F8BBD0",
            "#B8E0FF"
        });

    private readonly DispatcherTimer countdownTimer;
    private readonly DispatcherTimer clockTimer;
    private readonly RewardStore rewardStore;
    private readonly AppSettingsStore settingsStore;
    private readonly List<RoutineActivity> activities = new();
    private readonly Dictionary<string, RoutineActivity> activitiesByKey = new();
    private readonly Dictionary<string, ScheduleRow> scheduleRows = new();
    private readonly Dictionary<string, DateTime> firedAlarmDates = new();

    private Dictionary<string, ScheduleSetting> schedules = new();
    private RoutineActivity? currentActivity;
    private TimeSpan totalTime;
    private TimeSpan remainingTime;
    private bool isPaused;
    private ColorTheme currentTheme = PinkTheme;
    private string currentAlarmSoundName = AlarmSoundPlayer.Soft;

    public MainWindow()
    {
        InitializeComponent();

        rewardStore = new RewardStore();
        settingsStore = new AppSettingsStore();

        countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        countdownTimer.Tick += OnCountdownTick;

        clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        clockTimer.Tick += OnClockTick;

        SetupActivities();
        SetupScheduleRows();
        SetupButtons();
        LoadSavedSchedules();
        LoadSavedAlarmSound();
        UpdateStars();
        LoadSavedTheme();
        UpdateClock();
        clockTimer.Start();
    }

    // Keeps activities in one place so names, keys and durations are easy to adjust.
    private void SetupActivities()
    {
        RegisterActivity(BrushTeethButton, new RoutineActivity("brush_teeth", "Escovar os dentes", TimeSpan.FromMinutes(2), false));
        RegisterActivity(SchoolButton, new RoutineActivity("school", "Ir para escola", TimeSpan.FromMinutes(10), false));
        RegisterActivity(CleanRoomButton, new RoutineActivity("clean_room", "Arrumar quarto", TimeSpan.FromMinutes(15), false));
        RegisterActivity(HomeworkButton, new RoutineActivity("homework", "Dever de casa", TimeSpan.FromMinutes(20), false));
        RegisterActivity(ReadingButton, new RoutineActivity("reading", "Leitura", TimeSpan.FromMinutes(10), false));
        RegisterActivity(SleepButton, new RoutineActivity("sleep", "Dormir", TimeSpan.FromMinutes(5), false));
        RegisterActivity(FreePlayButton, new RoutineActivity("free_play", "Brincadeira livre", TimeSpan.FromMinutes(30), true));
        RegisterActivity(OnlineGamesButton, new RoutineActivity("online_games", "Jogos online", TimeSpan.FromMinutes(20), false));
    }

    private void RegisterActivity(Button button, RoutineActivity activity)
    {
        activities.Add(activity);
        activitiesByKey[activity.Key] = activity;
        button.Tag = activity;
        button.Click += OnActivityClick;
    }

    private void SetupScheduleRows()
    {
        scheduleRows["brush_teeth"] = new ScheduleRow(BrushTeethScheduleCheckBox, BrushTeethScheduleTextBox);
        scheduleRows["school"] = new ScheduleRow(SchoolScheduleCheckBox, SchoolScheduleTextBox);
        scheduleRows["clean_room"] = new ScheduleRow(CleanRoomScheduleCheckBox, CleanRoomScheduleTextBox);
        scheduleRows["homework"] = new ScheduleRow(HomeworkScheduleCheckBox, HomeworkScheduleTextBox);
        scheduleRows["reading"] = new ScheduleRow(ReadingScheduleCheckBox, ReadingScheduleTextBox);
        scheduleRows["sleep"] = new ScheduleRow(SleepScheduleCheckBox, SleepScheduleTextBox);
        scheduleRows["free_play"] = new ScheduleRow(FreePlayScheduleCheckBox, FreePlayScheduleTextBox);
        scheduleRows["online_games"] = new ScheduleRow(OnlineGamesScheduleCheckBox, OnlineGamesScheduleTextBox);
    }

    private void SetupButtons()
    {
        StartTimerButton.Click += OnStartTimerClick;
        PauseButton.Click += OnPauseClick;
        BackButton.Click += OnBackClick;
        ThemeButton.Click += OnThemeButtonClick;
        TimerThemeButton.Click += OnThemeButtonClick;
        OpenScheduleButton.Click += OnOpenScheduleClick;
        SaveScheduleButton.Click += OnSaveScheduleClick;
        CloseScheduleButton.Click += OnCloseScheduleClick;
        TestAlarmButton.Click += OnTestAlarmClick;
    }

    private void LoadSavedSchedules()
    {
        schedules = settingsStore.GetSchedules();

        foreach ((string key, ScheduleRow row) in scheduleRows)
        {
            if (!schedules.TryGetValue(key, out ScheduleSetting? schedule))
            {
                continue;
            }

            row.CheckBox.IsChecked = schedule.IsEnabled;
            row.TextBox.Text = schedule.Time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }
    }

    private void LoadSavedTheme()
    {
        string themeName = settingsStore.GetThemeName();
        ApplyTheme(themeName.Equals("blue", StringComparison.OrdinalIgnoreCase) ? BlueTheme : PinkTheme, save: false);
    }

    private void LoadSavedAlarmSound()
    {
        currentAlarmSoundName = NormalizeAlarmSoundName(settingsStore.GetAlarmSoundName());
        AlarmSoundComboBox.SelectedValue = currentAlarmSoundName;

        if (AlarmSoundComboBox.SelectedItem == null)
        {
            currentAlarmSoundName = AlarmSoundPlayer.Soft;
            AlarmSoundComboBox.SelectedValue = currentAlarmSoundName;
        }
    }

    private void OnThemeButtonClick(object sender, RoutedEventArgs e)
    {
        ApplyTheme(currentTheme.Name == PinkTheme.Name ? BlueTheme : PinkTheme);
    }

    private void ApplyTheme(ColorTheme theme, bool save = true)
    {
        currentTheme = theme;

        if (save)
        {
            settingsStore.SaveThemeName(theme.Name);
        }

        Background = Brush(theme.Background);

        Brush textPrimary = Brush(theme.TextPrimary);
        Brush textSecondary = Brush(theme.TextSecondary);
        Brush action = Brush(theme.Action);
        Brush secondaryAction = Brush(theme.SecondaryAction);
        Brush toggle = Brush(theme.Toggle);

        HomeTitleText.Foreground = textPrimary;
        StarsText.Foreground = textPrimary;
        ClockText.Foreground = textPrimary;
        TimerActivityText.Foreground = textPrimary;
        TimerText.Foreground = textPrimary;
        ScheduleTitleText.Foreground = textPrimary;
        AlarmSoundLabelText.Foreground = textPrimary;

        HomeSubtitleText.Foreground = textSecondary;
        ClockLabelText.Foreground = textSecondary;
        FeedbackText.Foreground = textSecondary;
        ScheduleHelpText.Foreground = textSecondary;
        ScheduleStatusText.Foreground = textSecondary;

        StarsBadge.Background = Brushes.White;
        StarsBadge.BorderBrush = Brush(theme.BadgeBorder);
        ClockBadge.Background = Brushes.White;
        ClockBadge.BorderBrush = Brush(theme.BadgeBorder);
        AlarmSoundBadge.Background = Brushes.White;
        AlarmSoundBadge.BorderBrush = Brush(theme.BadgeBorder);

        StartTimerButton.Background = action;
        PauseButton.Background = secondaryAction;
        BackButton.Background = secondaryAction;
        SaveScheduleButton.Background = action;
        CloseScheduleButton.Background = secondaryAction;
        TestAlarmButton.Background = toggle;
        OpenScheduleButton.Background = toggle;
        ThemeButton.Background = toggle;
        TimerThemeButton.Background = toggle;

        TimerProgress.Background = Brush(theme.ProgressBackground);
        TimerProgress.Foreground = Brush(theme.ProgressFill);

        Button[] activityButtons =
        {
            BrushTeethButton,
            SchoolButton,
            CleanRoomButton,
            HomeworkButton,
            ReadingButton,
            SleepButton,
            FreePlayButton,
            OnlineGamesButton
        };

        for (int i = 0; i < activityButtons.Length; i++)
        {
            activityButtons[i].Background = Brush(theme.ActivityColors[i]);
            activityButtons[i].Foreground = textPrimary;
        }

        foreach (ScheduleRow row in scheduleRows.Values)
        {
            row.CheckBox.Foreground = textPrimary;
            row.TextBox.Foreground = textPrimary;
        }

        AlarmSoundComboBox.Foreground = textPrimary;
        AlarmSoundComboBox.Background = Brushes.White;

        Button[] commandButtons =
        {
            StartTimerButton,
            PauseButton,
            BackButton,
            SaveScheduleButton,
            CloseScheduleButton,
            TestAlarmButton,
            OpenScheduleButton,
            ThemeButton,
            TimerThemeButton
        };

        foreach (Button button in commandButtons)
        {
            button.Foreground = textPrimary;
        }

        string nextThemeLabel = theme.Name == PinkTheme.Name ? "Modo azul" : "Modo rosa";
        ThemeButton.Content = nextThemeLabel;
        TimerThemeButton.Content = nextThemeLabel;
    }

    private void OnActivityClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: RoutineActivity activity })
        {
            return;
        }

        OpenTaskGuide(activity, fromAlarm: false);
    }

    private void OpenTaskGuide(RoutineActivity activity, bool fromAlarm)
    {
        countdownTimer.Stop();

        currentActivity = activity;
        totalTime = activity.Duration;
        remainingTime = activity.Duration;
        isPaused = false;

        HomePanel.Visibility = Visibility.Collapsed;
        SchedulePanel.Visibility = Visibility.Collapsed;
        TimerPanel.Visibility = Visibility.Visible;

        TimerActivityText.Text = activity.Name;
        PauseButton.Content = "Pausar";
        PauseButton.IsEnabled = false;
        StartTimerButton.IsEnabled = true;

        FeedbackText.Text = fromAlarm
            ? $"Está na hora de {activity.Name}. Aperte Começar timer quando estiver pronto."
            : "Tudo pronto? Aperte Começar timer quando quiser começar.";

        UpdateTimerScreen();
    }

    private void OnStartTimerClick(object sender, RoutedEventArgs e)
    {
        if (currentActivity == null)
        {
            return;
        }

        StopAlarmSound();
        isPaused = false;
        StartTimerButton.IsEnabled = false;
        PauseButton.IsEnabled = true;
        PauseButton.Content = "Pausar";
        FeedbackText.Text = "Respire fundo. Vamos com calma.";
        countdownTimer.Start();
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        remainingTime -= TimeSpan.FromSeconds(1);

        if (remainingTime <= TimeSpan.Zero)
        {
            remainingTime = TimeSpan.Zero;
            FinishTimer();
            return;
        }

        UpdateTimerScreen();
    }

    private void OnClockTick(object? sender, EventArgs e)
    {
        UpdateClock();
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;
        ClockText.Text = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        CheckScheduledAlarms(now);
    }

    private void CheckScheduledAlarms(DateTime now)
    {
        if (countdownTimer.IsEnabled)
        {
            return;
        }

        foreach (ScheduleSetting schedule in schedules.Values)
        {
            if (!schedule.IsEnabled ||
                schedule.Time.Hours != now.Hour ||
                schedule.Time.Minutes != now.Minute)
            {
                continue;
            }

            if (firedAlarmDates.TryGetValue(schedule.ActivityKey, out DateTime lastDate) &&
                lastDate.Date == now.Date)
            {
                continue;
            }

            firedAlarmDates[schedule.ActivityKey] = now.Date;
            TriggerScheduledActivity(schedule);
            break;
        }
    }

    private void TriggerScheduledActivity(ScheduleSetting schedule)
    {
        if (!activitiesByKey.TryGetValue(schedule.ActivityKey, out RoutineActivity? activity))
        {
            return;
        }

        OpenTaskGuide(activity, fromAlarm: true);
        BringWindowToFront();
        PlayAlarmSoundForTwoMinutes();
    }

    private void BringWindowToFront()
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Show();
        Activate();
        Topmost = true;
        Topmost = false;
    }

    private void OnPauseClick(object sender, RoutedEventArgs e)
    {
        if (currentActivity == null || remainingTime <= TimeSpan.Zero)
        {
            return;
        }

        if (isPaused)
        {
            isPaused = false;
            FeedbackText.Text = "Respire fundo. Vamos com calma.";
            PauseButton.Content = "Pausar";
            countdownTimer.Start();
        }
        else
        {
            isPaused = true;
            FeedbackText.Text = "Tudo bem pausar um pouquinho.";
            PauseButton.Content = "Continuar";
            countdownTimer.Stop();
        }
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        StopAlarmSound();
        countdownTimer.Stop();
        TimerPanel.Visibility = Visibility.Collapsed;
        SchedulePanel.Visibility = Visibility.Collapsed;
        HomePanel.Visibility = Visibility.Visible;
        UpdateStars();
    }

    private void OnOpenScheduleClick(object sender, RoutedEventArgs e)
    {
        StopAlarmSound();
        countdownTimer.Stop();
        HomePanel.Visibility = Visibility.Collapsed;
        TimerPanel.Visibility = Visibility.Collapsed;
        SchedulePanel.Visibility = Visibility.Visible;
        ScheduleStatusText.Text = string.Empty;
    }

    private void OnCloseScheduleClick(object sender, RoutedEventArgs e)
    {
        SchedulePanel.Visibility = Visibility.Collapsed;
        TimerPanel.Visibility = Visibility.Collapsed;
        HomePanel.Visibility = Visibility.Visible;
    }

    private void OnSaveScheduleClick(object sender, RoutedEventArgs e)
    {
        List<ScheduleSetting> updatedSchedules = new();

        foreach ((string key, ScheduleRow row) in scheduleRows)
        {
            bool isEnabled = row.CheckBox.IsChecked == true;
            if (!TryReadScheduleTime(row.TextBox.Text, out TimeSpan time))
            {
                if (isEnabled)
                {
                    ScheduleStatusText.Text = "Confira os horários. Use algo como 07:30 ou 18:00.";
                    row.TextBox.Focus();
                    return;
                }

                time = TimeSpan.Zero;
            }

            row.TextBox.Text = time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
            updatedSchedules.Add(new ScheduleSetting(key, isEnabled, time));
        }

        settingsStore.SaveSchedules(updatedSchedules);
        currentAlarmSoundName = GetSelectedAlarmSoundName();
        settingsStore.SaveAlarmSoundName(currentAlarmSoundName);
        schedules = updatedSchedules.ToDictionary(schedule => schedule.ActivityKey);
        ScheduleStatusText.Text = "Horários e som do alarme salvos.";
    }

    private void OnTestAlarmClick(object sender, RoutedEventArgs e)
    {
        currentAlarmSoundName = GetSelectedAlarmSoundName();
        PlayAlarmSoundOnce();
        ScheduleStatusText.Text = "Teste do alarme tocando.";
    }

    private void FinishTimer()
    {
        countdownTimer.Stop();
        StartTimerButton.IsEnabled = false;
        PauseButton.IsEnabled = false;
        UpdateTimerScreen();
        PlayAlarmSoundOnce();

        if (currentActivity?.IsFreePlay == true)
        {
            FeedbackText.Text = "Espero que tenha se divertido \U0001F31F";
            return;
        }

        rewardStore.AddStar();
        UpdateStars();
        FeedbackText.Text = "Muito bem! Voc\u00EA completou essa atividade \U0001F31F";
    }

    private void UpdateTimerScreen()
    {
        TimerText.Text = remainingTime.ToString(@"mm\:ss", CultureInfo.InvariantCulture);

        double progress = 100;
        if (totalTime.TotalSeconds > 0)
        {
            progress = ((totalTime - remainingTime).TotalSeconds / totalTime.TotalSeconds) * 100;
        }

        TimerProgress.Value = Math.Clamp(progress, 0, 100);
    }

    private void UpdateStars()
    {
        StarsText.Text = $"Estrelas: {rewardStore.GetStars()}";
    }

    // Sounds are generated by code, so the app does not ship copyrighted audio files.
    private void PlayAlarmSoundOnce()
    {
        AlarmSoundPlayer.PlayOnce(currentAlarmSoundName);
    }

    private void PlayAlarmSoundForTwoMinutes()
    {
        AlarmSoundPlayer.PlayFor(currentAlarmSoundName, TimeSpan.FromMinutes(2));
    }

    private static void StopAlarmSound()
    {
        AlarmSoundPlayer.Stop();
    }

    private string GetSelectedAlarmSoundName()
    {
        string? selectedValue = AlarmSoundComboBox.SelectedValue as string;
        return NormalizeAlarmSoundName(selectedValue);
    }

    private static string NormalizeAlarmSoundName(string? soundName)
    {
        return soundName switch
        {
            AlarmSoundPlayer.Classic => AlarmSoundPlayer.Classic,
            AlarmSoundPlayer.Digital => AlarmSoundPlayer.Digital,
            AlarmSoundPlayer.Rising => AlarmSoundPlayer.Rising,
            _ => AlarmSoundPlayer.Soft
        };
    }

    private static bool TryReadScheduleTime(string text, out TimeSpan time)
    {
        string cleanText = text.Trim();
        string[] formats = { @"h\:mm", @"hh\:mm" };

        if (!TimeSpan.TryParseExact(cleanText, formats, CultureInfo.InvariantCulture, out time))
        {
            return false;
        }

        return time >= TimeSpan.Zero && time < TimeSpan.FromDays(1);
    }

    private static SolidColorBrush Brush(string colorText)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorText));
    }

    private sealed class ScheduleRow
    {
        public ScheduleRow(CheckBox checkBox, TextBox textBox)
        {
            CheckBox = checkBox;
            TextBox = textBox;
        }

        public CheckBox CheckBox { get; }

        public TextBox TextBox { get; }
    }

    private sealed class ColorTheme
    {
        public ColorTheme(
            string name,
            string background,
            string textPrimary,
            string textSecondary,
            string badgeBorder,
            string action,
            string secondaryAction,
            string progressBackground,
            string progressFill,
            string toggle,
            string[] activityColors)
        {
            Name = name;
            Background = background;
            TextPrimary = textPrimary;
            TextSecondary = textSecondary;
            BadgeBorder = badgeBorder;
            Action = action;
            SecondaryAction = secondaryAction;
            ProgressBackground = progressBackground;
            ProgressFill = progressFill;
            Toggle = toggle;
            ActivityColors = activityColors;
        }

        public string Name { get; }

        public string Background { get; }

        public string TextPrimary { get; }

        public string TextSecondary { get; }

        public string BadgeBorder { get; }

        public string Action { get; }

        public string SecondaryAction { get; }

        public string ProgressBackground { get; }

        public string ProgressFill { get; }

        public string Toggle { get; }

        public string[] ActivityColors { get; }
    }
}
