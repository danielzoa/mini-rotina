using System.Globalization;
using System.IO;

namespace TimerInfantilWindows.Rewards;

public sealed class RewardStore
{
    private readonly string filePath;

    public RewardStore()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "TimerInfantilWindows");

        Directory.CreateDirectory(folder);
        filePath = Path.Combine(folder, "rewards.txt");
    }

    // Arquivo local simples: suficiente para o MVP e facil de apagar durante testes.
    public int GetStars()
    {
        if (!File.Exists(filePath))
        {
            return 0;
        }

        string savedText = File.ReadAllText(filePath);
        return int.TryParse(savedText, NumberStyles.None, CultureInfo.InvariantCulture, out int stars)
            ? stars
            : 0;
    }

    public int AddStar()
    {
        int stars = GetStars() + 1;
        File.WriteAllText(filePath, stars.ToString(CultureInfo.InvariantCulture));
        return stars;
    }
}
