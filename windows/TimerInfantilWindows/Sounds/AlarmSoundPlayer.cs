using System.Media;

namespace TimerInfantilWindows.Sounds;

public static class AlarmSoundPlayer
{
    public const string Soft = "soft";
    public const string Classic = "classic";
    public const string Digital = "digital";
    public const string Rising = "rising";

    private static readonly object SyncRoot = new();
    private static CancellationTokenSource? activeLoop;

    public static void PlayOnce(string soundName)
    {
        _ = Task.Run(() =>
        {
            try
            {
                PlayPattern(soundName, CancellationToken.None);
            }
            catch
            {
                SystemSounds.Asterisk.Play();
            }
        });
    }

    public static void PlayFor(string soundName, TimeSpan duration)
    {
        Stop();

        CancellationTokenSource loopSource = new();

        lock (SyncRoot)
        {
            activeLoop = loopSource;
        }

        _ = Task.Run(() =>
        {
            DateTime endTime = DateTime.Now.Add(duration);

            try
            {
                while (!loopSource.IsCancellationRequested && DateTime.Now < endTime)
                {
                    PlayPattern(soundName, loopSource.Token);
                    Pause(450, loopSource.Token);
                }
            }
            catch
            {
                if (!loopSource.IsCancellationRequested)
                {
                    SystemSounds.Asterisk.Play();
                }
            }
            finally
            {
                lock (SyncRoot)
                {
                    if (ReferenceEquals(activeLoop, loopSource))
                    {
                        activeLoop = null;
                    }
                }

                loopSource.Dispose();
            }
        });
    }

    public static void Stop()
    {
        lock (SyncRoot)
        {
            activeLoop?.Cancel();
            activeLoop = null;
        }
    }

    private static void PlayPattern(string soundName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        switch (soundName)
        {
            case Classic:
                PlayClassic(token);
                break;
            case Digital:
                PlayDigital(token);
                break;
            case Rising:
                PlayRising(token);
                break;
            default:
                PlaySoft(token);
                break;
        }
    }

    // Calm two-note reminder, good for younger children.
    private static void PlaySoft(CancellationToken token)
    {
        Beep(660, 180, token);
        Pause(80, token);
        Beep(880, 220, token);
    }

    // Familiar short repeated beeps used by many alarm-style interfaces.
    private static void PlayClassic(CancellationToken token)
    {
        for (int i = 0; i < 3; i++)
        {
            Beep(900, 180, token);
            Pause(120, token);
        }
    }

    // Alternating tones, similar to a simple digital alarm pattern.
    private static void PlayDigital(CancellationToken token)
    {
        for (int i = 0; i < 2; i++)
        {
            Beep(780, 140, token);
            Pause(70, token);
            Beep(1040, 140, token);
            Pause(90, token);
        }
    }

    // Gentle rising sequence that feels like a wake-up prompt.
    private static void PlayRising(CancellationToken token)
    {
        Beep(520, 140, token);
        Pause(70, token);
        Beep(660, 140, token);
        Pause(70, token);
        Beep(780, 160, token);
        Pause(70, token);
        Beep(1040, 220, token);
    }

    private static void Beep(int frequency, int duration, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        Console.Beep(frequency, duration);
    }

    private static void Pause(int milliseconds, CancellationToken token)
    {
        if (token.WaitHandle.WaitOne(milliseconds))
        {
            token.ThrowIfCancellationRequested();
        }
    }
}
